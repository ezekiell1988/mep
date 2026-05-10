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
    AzureOpenAIClient aiClient,
    IOptions<AiOptions> aiOpts,
    IOptions<StorageOptions> storageOpts,
    ILlmAuditService audit,
    ILogger<ExtractCurriculumJob> logger)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    [Queue("curriculum")]
    [AutomaticRetry(Attempts = 1)]
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
            var (units, tokensUsed) = await ExtractUnitsWithAiAsync(pdfText, asignatura, ciclo, ctx, ct);

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

            foreach (var unit in units)
            {
                unit.ExtractionId = extraction.Id;
                db.CurriculumUnits.Add(unit);
            }

            extraction.UnidadCount = units.Count;
            await db.SaveChangesAsync(ct);

            ctx.WriteLine($"📊 {units.Count} unidades guardadas en curriculum_units ({tokensUsed:N0} tokens)");
            audit.LogEvent("ExtractCurriculumJob", "Completado",
                $"✅ {units.Count} unidades guardadas en curriculum_units (tokens: {tokensUsed:N0})",
                new { asignatura, ciclo, count = units.Count, tokensUsed, extractionId = extraction.Id });

            logger.LogInformation("Extracción completada: {Count} unidades guardadas para {Asignatura}", units.Count, asignatura);
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
        // Usar connection string para acceder al container privado.
        // Uri.AbsolutePath percent-encodea caracteres no-ASCII; se debe decodificar
        // antes de usarlo como blob name (que es un path literal, no una URL).
        var blobUri = new Uri(blobUrl);
        var blobName = string.Join("/",
            Uri.UnescapeDataString(blobUri.AbsolutePath).TrimStart('/').Split('/').Skip(1));
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

    private const int ChunkSize    = 300_000; // ~75k tokens por chunk
    private const int ChunkOverlap =  30_000; // overlap para no cortar unidades a mitad

    private async Task<(List<CurriculumUnit> Units, int TokensUsed)> ExtractUnitsWithAiAsync(
        string pdfText, string asignatura, string ciclo, PerformContext? ctx, CancellationToken ct)
    {
        var opts       = aiOpts.Value;
        var chatClient = aiClient.GetChatClient(opts.DeploymentName);

        var subjectHint = GetSubjectHint(asignatura, ciclo);

        var systemPrompt = """
            Eres un extractor de programas de estudio del MEP de Costa Rica.
            Dado el texto (completo o parcial) de un programa oficial, extrae TODAS las unidades
            didácticas que encuentres con su información estructurada.
            Devuelve ÚNICAMENTE JSON válido con este schema exacto,
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
            - NUNCA inventes trimestres: asigna el valor EXACTO indicado en las instrucciones de asignatura.
            - Si no hay unidades en este fragmento, devuelve [].
            """;

        // Para PDFs grandes: dividir en chunks con overlap para no perder unidades en los cortes
        var chunks = BuildChunks(pdfText);
        ctx?.WriteLine(chunks.Count == 1
            ? $"   ↳ Procesando en 1 bloque ({pdfText.Length:N0} chars)"
            : $"   ↳ PDF grande: procesando en {chunks.Count} bloques de ~{ChunkSize:N0} chars c/u");

        var allUnits   = new List<CurriculumUnit>();
        var totalTokens = 0;
        var seen       = new HashSet<(int, int, int)>(); // nivel, trimestre, unidadNumero

        for (int i = 0; i < chunks.Count; i++)
        {
            if (chunks.Count > 1)
                ctx?.WriteLine($"   ↳ 🤖 Bloque {i + 1}/{chunks.Count}...");

            var userContent = new StringBuilder();
            userContent.AppendLine($"Asignatura: {asignatura}");
            userContent.AppendLine($"Ciclo: {ciclo}");
            if (chunks.Count > 1) userContent.AppendLine($"Fragmento: {i + 1} de {chunks.Count}");
            if (!string.IsNullOrEmpty(subjectHint)) userContent.AppendLine(subjectHint);
            userContent.AppendLine($"\n---\n{chunks[i]}");

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userContent.ToString())
            };

            var chatResult  = await chatClient.CompleteChatAsync(messages, cancellationToken: ct);
            var json        = chatResult.Value.Content[0].Text.Trim();
            totalTokens    += chatResult.Value.Usage?.TotalTokenCount ?? 0;

            List<ExtractedUnit> extracted;
            try
            {
                extracted = JsonSerializer.Deserialize<List<ExtractedUnit>>(json, JsonOpts) ?? [];
            }
            catch (JsonException ex)
            {
                var preview = json[..Math.Min(500, json.Length)];
                audit.LogError("ExtractCurriculumJob",
                    $"JSON inválido en bloque {i + 1}. Preview: {preview}", ex);
                logger.LogError(ex, "Error parseando JSON bloque {Block}. Respuesta: {Json}", i + 1, preview);
                continue; // intentar el siguiente chunk
            }

            foreach (var e in extracted)
            {
                if (!seen.Add((e.Nivel, e.Trimestre, e.UnidadNumero)))
                    continue; // deduplicar (por overlap entre chunks)

                allUnits.Add(new CurriculumUnit
                {
                    Asignatura            = asignatura,
                    Ciclo                 = ciclo,
                    Nivel                 = e.Nivel,
                    Trimestre             = e.Trimestre,
                    UnidadNumero          = e.UnidadNumero,
                    UnidadNombre          = e.UnidadNombre,
                    AprendizajesEsperados = e.AprendizajesEsperados,
                    IndicadoresEvaluacion = e.IndicadoresEvaluacion,
                    ContenidoConceptual   = e.ContenidoConceptual,
                    ContenidoProcedimental = e.ContenidoProcedimental,
                    ContenidoActitudinal  = e.ContenidoActitudinal,
                    EstrategiasSugeridas  = e.EstrategiasSugeridas
                });
            }

            if (chunks.Count > 1)
                ctx?.WriteLine($"      ↳ {extracted.Count} unidades en este bloque ({totalTokens:N0} tokens acumulados)");
        }

        return (allUnits, totalTokens);
    }

    /// <summary>
    /// Retorna instrucciones adicionales para asignaturas cuyo PDF usa una estructura
    /// diferente al formato estándar (p.ej. Matemáticas usa áreas en vez de trimestres).
    /// </summary>
    private static string GetSubjectHint(string asignatura, string ciclo) =>
        (asignatura, ciclo) switch
        {
            ("Matemáticas", "III Ciclo") =>
                """

                INSTRUCCIÓN ESPECIAL — Matemáticas III Ciclo:
                El programa de Matemáticas NO usa trimestres explícitos.
                Está organizado por ÁREAS MATEMÁTICAS. Usa la siguiente distribución estándar del MEP:
                  • Área "Números"                    → trimestre=1, unidadNumero=1
                  • Área "Geometría"                  → trimestre=2, unidadNumero=2
                  • Área "Relaciones y Álgebra"        → trimestre=3, unidadNumero=3
                  • Área "Estadística y Probabilidad" → trimestre=3, unidadNumero=4
                Usa el nombre del área como "unidadNombre".
                Los años son 7°, 8°, 9° → nivel=7, nivel=8, nivel=9.
                NO inventes trimestres distintos a los indicados arriba.
                """,
            _ => string.Empty
        };

    private static List<string> BuildChunks(string text)
    {
        if (text.Length <= ChunkSize)
            return [text];

        var chunks = new List<string>();
        int pos = 0;
        while (pos < text.Length)
        {
            var end = Math.Min(pos + ChunkSize, text.Length);
            chunks.Add(text[pos..end]);
            pos += ChunkSize - ChunkOverlap; // avanzar con overlap
        }
        return chunks;
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
