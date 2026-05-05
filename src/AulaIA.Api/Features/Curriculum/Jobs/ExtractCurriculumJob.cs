using System.Text;
using System.Text.Json;
using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using Hangfire;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using UglyToad.PdfPig;

namespace AulaIA.Api.Features.Curriculum.Jobs;

public sealed class ExtractCurriculumJob(
    AulaIADbContext db,
    IOptions<AiOptions> aiOpts,
    IOptions<StorageOptions> storageOpts,
    ILogger<ExtractCurriculumJob> logger)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    [Queue("curriculum")]
    [AutomaticRetry(Attempts = 2)]
    public async Task ExecuteAsync(string blobUrl, string asignatura, string ciclo, CancellationToken ct)
    {
        logger.LogInformation("Iniciando extracción de currículo: {Asignatura} — {BlobUrl}", asignatura, blobUrl);

        var pdfText = await DownloadAndExtractTextAsync(blobUrl, ct);
        if (string.IsNullOrWhiteSpace(pdfText))
        {
            logger.LogWarning("PDF vacío o sin texto extraíble: {BlobUrl}", blobUrl);
            return;
        }

        var units = await ExtractUnitsWithAiAsync(pdfText, asignatura, ciclo, ct);
        if (units.Count == 0)
        {
            logger.LogWarning("La IA no extrajo unidades del programa: {Asignatura}", asignatura);
            return;
        }

        foreach (var unit in units)
        {
            unit.PdfSourceUrl = blobUrl;
            db.CurriculumUnits.Add(unit);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Extracción completada: {Count} unidades guardadas para {Asignatura}", units.Count, asignatura);
    }

    private async Task<string> DownloadAndExtractTextAsync(string blobUrl, CancellationToken ct)
    {
        var blobClient = new BlobClient(new Uri(blobUrl));
        var response = await blobClient.DownloadContentAsync(ct);
        var bytes = response.Value.Content.ToArray();

        var sb = new StringBuilder();
        using var pdf = PdfDocument.Open(bytes);
        foreach (var page in pdf.GetPages())
            sb.AppendLine(page.Text);

        return sb.ToString();
    }

    private async Task<List<CurriculumUnit>> ExtractUnitsWithAiAsync(
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

        // El texto del PDF puede ser largo; tomamos los primeros 120k caracteres para no superar el contexto
        var truncatedText = pdfText.Length > 120_000 ? pdfText[..120_000] : pdfText;

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage($"Asignatura: {asignatura}\nCiclo: {ciclo}\n\n---\n{truncatedText}")
        };

        var chatResult = await chatClient.CompleteChatAsync(messages, cancellationToken: ct);
        var json = chatResult.Value.Content[0].Text.Trim();

        try
        {
            var extracted = JsonSerializer.Deserialize<List<ExtractedUnit>>(json, JsonOpts) ?? [];
            return extracted.Select(e => new CurriculumUnit
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
            }).ToList();
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error parseando JSON de la IA. Respuesta recibida: {Json}", json[..Math.Min(500, json.Length)]);
            return [];
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
