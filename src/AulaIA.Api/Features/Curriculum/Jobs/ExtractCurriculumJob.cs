using System.Text;
using System.Text.Json;
using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using AulaIA.Api.Shared.Services;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using UglyToad.PdfPig;

namespace AulaIA.Api.Features.Curriculum.Jobs;

public sealed class ExtractCurriculumJob(
    AulaIADbContext db,
    IOptions<AiOptions> aiOpts,
    IOptions<StorageOptions> storageOpts,
    ILlmAuditService audit,
    ILogger<ExtractCurriculumJob> logger)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    [Queue("curriculum")]
    [AutomaticRetry(Attempts = 2)]
    public async Task ExecuteAsync(string blobUrl, string asignatura, string ciclo, PerformContext? ctx = null, CancellationToken ct = default)
    {
        ctx.WriteLine($"🚀 ExtractCurriculumJob: {asignatura} ({ciclo})");
        audit.LogEvent("ExtractCurriculumJob", "Iniciando",
            $"asignatura={asignatura} ciclo={ciclo}",
            new { blobUrl });

        try
        {
            ctx.WriteLine("📥 Descargando PDF desde Blob Storage...");
            var pdfText = await DownloadAndExtractTextAsync(blobUrl, ct);
            if (string.IsNullOrWhiteSpace(pdfText))
            {
                ctx.WriteLine("⚠️ PDF sin texto extraíble (puede ser escaneado sin OCR)");
                audit.LogEvent("ExtractCurriculumJob", "PDF sin texto",
                    "⚠️ PdfPig no extrajo texto — puede ser PDF escaneado sin OCR",
                    new { blobUrl });
                logger.LogWarning("PDF vacío o sin texto extraíble: {BlobUrl}", blobUrl);
                return;
            }

            ctx.WriteLine($"📄 PDF extraído: {pdfText.Length:N0} caracteres");
            audit.LogEvent("ExtractCurriculumJob", "PDF extraído",
                $"✅ {pdfText.Length:N0} caracteres — enviando a GPT-5.5");

            ctx.WriteLine($"🤖 Enviando a {aiOpts.Value.DeploymentName}...");
            var (units, tokensUsed) = await ExtractUnitsWithAiAsync(pdfText, asignatura, ciclo, ct);

            if (units.Count == 0)
            {
                ctx.WriteLine("⚠️ La IA no encontró unidades curriculares en el documento");
                audit.LogEvent("ExtractCurriculumJob", "Sin unidades",
                    "⚠️ GPT-5.5 no encontró unidades curriculares en el documento — " +
                    "verificar que el PDF es el programa oficial del MEP y no otro documento");
                logger.LogWarning("La IA no extrajo unidades del programa: {Asignatura}", asignatura);
                return;
            }

            ctx.WriteLine($"✅ {units.Count} unidades extraídas ({tokensUsed:N0} tokens) — guardando en BD...");

            // Eliminar extracción previa no validada para esta asignatura/ciclo (re-extracción)
            var extraccionPrevia = await db.CurriculumExtractions
                .Where(e => e.Asignatura == asignatura && e.Ciclo == ciclo)
                .Where(e => !e.Units.Any(u => u.ValidatedAt != null))
                .FirstOrDefaultAsync(ct);
            if (extraccionPrevia is not null)
                db.CurriculumExtractions.Remove(extraccionPrevia); // cascade borra las units

            // Crear encabezado de extracción
            var extraction = new CurriculumExtraction
            {
                Asignatura = asignatura,
                Ciclo = ciclo,
                PdfSourceUrl = blobUrl,
                ModelUsed = aiOpts.Value.DeploymentChat,
                TotalTokensUsed = tokensUsed
            };
            db.CurriculumExtractions.Add(extraction);

            // Deduplicar por (Nivel, Trimestre, UnidadNumero) por si GPT devuelve duplicados
            var seen = new HashSet<(int, int, int)>();
            foreach (var unit in units)
            {
                if (!seen.Add((unit.Nivel, unit.Trimestre, unit.UnidadNumero)))
                    continue;
                unit.ExtractionId = extraction.Id;
                db.CurriculumUnits.Add(unit);
            }

            extraction.UnidadCount = seen.Count;
            await db.SaveChangesAsync(ct);

            ctx.WriteLine($"📊 {seen.Count} unidades guardadas en curriculum_units ({tokensUsed:N0} tokens)");
            audit.LogEvent("ExtractCurriculumJob", "Completado",
                $"✅ {seen.Count} unidades guardadas en curriculum_units (tokens: {tokensUsed:N0})",
                new { asignatura, ciclo, count = seen.Count, tokensUsed, extractionId = extraction.Id });

            logger.LogInformation("Extracción completada: {Count} unidades guardadas para {Asignatura}", seen.Count, asignatura);
        }
        catch (Exception ex)
        {
            ctx.WriteLine($"❌ Error: {ex.Message}");
            audit.LogError("ExtractCurriculumJob", $"Falló la extracción para {asignatura}", ex);
            logger.LogError(ex, "Error en ExtractCurriculumJob para {Asignatura}", asignatura);
            throw;
        }
    }

    private async Task<string> DownloadAndExtractTextAsync(string blobUrl, CancellationToken ct)
    {
        // Usar connection string para acceder al container privado
        var blobUri = new Uri(blobUrl);
        var blobName = string.Join("/", blobUri.AbsolutePath.TrimStart('/').Split('/').Skip(1));
        var blobClient = new BlobClient(
            storageOpts.Value.ConnectionString,
            storageOpts.Value.ContainerCurriculum,
            blobName);

        var response = await blobClient.DownloadContentAsync(ct);
        var bytes = response.Value.Content.ToArray();

        var sb = new StringBuilder();
        using var pdf = PdfDocument.Open(bytes);
        foreach (var page in pdf.GetPages())
            sb.AppendLine(page.Text);

        return sb.ToString();
    }

    private async Task<(List<CurriculumUnit> Units, int TokensUsed)> ExtractUnitsWithAiAsync(
        string pdfText, string asignatura, string ciclo, CancellationToken ct)
    {
        var opts = aiOpts.Value;
        var azureClient = new AzureOpenAIClient(new Uri(opts.Endpoint), new Azure.AzureKeyCredential(opts.ApiKey ?? ""));
        var chatClient = azureClient.GetChatClient(opts.DeploymentName);

        var systemPrompt = """
            Eres un extractor de programas de estudio del MEP de Costa Rica.
            Dado el texto completo de un programa oficial, extrae TODAS las unidades didácticas
            con su información estructurada. Devuelve ÚNICAMENTE JSON válido con este schema exacto,
            sin texto adicional, sin markdown, sin bloques de código:

            [
              {
                "nivel": 7,
                "trimestre": 1,
                "unidadNumero": 1,
                "unidadNombre": "nombre de la unidad",
                "aprendizajesEsperados": ["...", "..."],
                "indicadoresEvaluacion": ["...", "..."],
                "contenidoConceptual": ["...", "..."],
                "contenidoProcedimental": ["...", "..."],
                "contenidoActitudinal": ["...", "..."],
                "estrategiasSugeridas": ["...", "..."]
              }
            ]

            Reglas críticas:
            - Copia textualmente del programa, NO parafrasees ni inventes contenido.
            - Si un campo no está en el programa, usa array vacío [].
            - El campo "nivel" es el año escolar (7, 8, 9, 10, 11).
            - El campo "trimestre" es el período lectivo (1, 2, 3).
            """;

        // El texto del PDF puede ser largo; tomamos los primeros 350k caracteres (~87k tokens) para no superar el contexto
        var truncatedText = pdfText.Length > 350_000 ? pdfText[..350_000] : pdfText;

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage($"Asignatura: {asignatura}\nCiclo: {ciclo}\n\n---\n{truncatedText}")
        };

        var chatResult = await chatClient.CompleteChatAsync(messages, cancellationToken: ct);
        var json = chatResult.Value.Content[0].Text.Trim();
        var totalTokens = chatResult.Value.Usage?.TotalTokenCount ?? 0;

        try
        {
            var extracted = JsonSerializer.Deserialize<List<ExtractedUnit>>(json, JsonOpts) ?? [];
            return (extracted.Select(e => new CurriculumUnit
            {
                Asignatura = asignatura,
                Ciclo = ciclo,
                Nivel = e.Nivel,
                Trimestre = e.Trimestre,
                UnidadNumero = e.UnidadNumero,
                UnidadNombre = e.UnidadNombre,
                AprendizajesEsperados = e.AprendizajesEsperados,
                IndicadoresEvaluacion = e.IndicadoresEvaluacion,
                ContenidoConceptual = e.ContenidoConceptual,
                ContenidoProcedimental = e.ContenidoProcedimental,
                ContenidoActitudinal = e.ContenidoActitudinal,
                EstrategiasSugeridas = e.EstrategiasSugeridas
            }).ToList(), totalTokens);
        }
        catch (JsonException ex)
        {
            var preview = json[..Math.Min(500, json.Length)];
            audit.LogError("ExtractCurriculumJob", $"JSON inválido devuelto por GPT-5.5. Preview: {preview}", ex);
            logger.LogError(ex, "Error parseando JSON de la IA. Respuesta recibida: {Json}", preview);
            return ([], totalTokens);
        }
    }

    private sealed record ExtractedUnit(
        int Nivel,
        int Trimestre,
        int UnidadNumero,
        string UnidadNombre,
        List<string> AprendizajesEsperados,
        List<string> IndicadoresEvaluacion,
        List<string> ContenidoConceptual,
        List<string> ContenidoProcedimental,
        List<string> ContenidoActitudinal,
        List<string> EstrategiasSugeridas);
}
