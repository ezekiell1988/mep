using System.Text.Json.Serialization;
using AulaIA.Tests.Infrastructure;

namespace AulaIA.Tests;

/// <summary>
/// Tests para el módulo Curriculum.
///
/// Flujo completo:
///   1. POST /api/curriculum/upload — sube un PDF → recibe jobId + blobUrl
///   2. (Hangfire procesa el job en background: descarga PDF → GPT-5.5 → graba CurriculumUnits)
///   3. GET  /api/curriculum?asignatura=... → lista unidades extraídas
///   4. POST /api/curriculum/{id}/validate  → marca la unidad como validada
///   5. GET  /api/curriculum?asignatura=... → confirma que la unidad aparece validada
///
/// Prerequisito: la API debe estar corriendo y el token debe tener rol "admin".
/// Para subir un PDF real: AULAIA_TEST_PdfPath=/ruta/al/programa.pdf dotnet test
/// </summary>
public class CurriculumTests : IntegrationTestBase
{
    // ── Config ────────────────────────────────────────────────────────────────
    private static readonly string? PdfPath =
        Environment.GetEnvironmentVariable("AULAIA_TEST_PdfPath");

    private const string Asignatura = "Artes Plasticas";
    private const string Ciclo = "III Ciclo";
    private static readonly TimeSpan JobTimeout = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "POST /api/curriculum/upload → 202 Accepted con jobId")]
    public async Task Upload_PdfValido_Returns202ConJobId()
    {
        using var content = BuildPdfMultipart();
        var response = await Api.PostAsync(
            $"/api/curriculum/upload?asignatura={Uri.EscapeDataString(Asignatura)}&ciclo={Uri.EscapeDataString(Ciclo)}",
            content);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var body = await ReadJsonAsync<UploadResponse>(response);
        body.JobId.Should().NotBeNullOrWhiteSpace("el endpoint debe devolver un jobId");
        body.BlobUrl.Should().NotBeNullOrWhiteSpace("el endpoint debe devolver la URL del blob");
    }

    [Fact(DisplayName = "POST /api/curriculum/upload archivo que no es PDF → 400 Bad Request")]
    public async Task Upload_ArchivoNoEsPdf_Returns400()
    {
        using var multipart = new MultipartFormDataContent();
        var textContent = new ByteArrayContent("esto no es un pdf"u8.ToArray());
        textContent.Headers.ContentType = new("text/plain");
        multipart.Add(textContent, "file", "noespdf.txt");

        var response = await Api.PostAsync(
            $"/api/curriculum/upload?asignatura={Uri.EscapeDataString(Asignatura)}&ciclo={Uri.EscapeDataString(Ciclo)}",
            multipart);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Flujo completo: upload → esperar job → listar unidades → validar")]
    public async Task FlujoCompleto_UploadValidarListar()
    {
        // Requiere un PDF real con texto para que GPT-5.5 extraiga unidades curriculares.
        // Sin AULAIA_TEST_PdfPath configurado el job no produciría unidades — se omite.
        if (string.IsNullOrWhiteSpace(PdfPath) || !File.Exists(PdfPath)) return;

        // 1. Upload
        using var content = BuildPdfMultipart();
        var uploadResp = await Api.PostAsync(
            $"/api/curriculum/upload?asignatura={Uri.EscapeDataString(Asignatura)}&ciclo={Uri.EscapeDataString(Ciclo)}",
            content);

        uploadResp.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var upload = await ReadJsonAsync<UploadResponse>(uploadResp);
        upload.JobId.Should().NotBeNullOrWhiteSpace();

        // 2. Polling hasta que Hangfire procese el job
        List<CurriculumUnitDto>? units = null;
        var deadline = DateTimeOffset.UtcNow + JobTimeout;

        while (DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(PollInterval);

            var listResp = await Api.GetAsync(
                $"/api/curriculum?asignatura={Uri.EscapeDataString(Asignatura)}");

            if (listResp.StatusCode == HttpStatusCode.OK)
            {
                units = await listResp.Content.ReadFromJsonAsync<List<CurriculumUnitDto>>(
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (units is { Count: > 0 }) break;
            }
        }

        units.Should().NotBeNullOrEmpty(
            because: $"deben aparecer unidades curriculares en menos de {JobTimeout.TotalMinutes} min. " +
                     "Revisa los logs de Hangfire en /hangfire.");

        // 3. Validar la primera unidad
        var firstUnit = units![0];
        firstUnit.Id.Should().NotBe(Guid.Empty);

        var validateResp = await Api.PostAsync(
            $"/api/curriculum/{firstUnit.Id}/validate",
            new StringContent(""));

        validateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 4. Confirmar ValidatedAt
        var afterResp = await Api.GetAsync(
            $"/api/curriculum?asignatura={Uri.EscapeDataString(Asignatura)}");
        afterResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var validated = await afterResp.Content.ReadFromJsonAsync<List<CurriculumUnitDto>>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var validatedUnit = validated!.FirstOrDefault(u => u.Id == firstUnit.Id);
        validatedUnit.Should().NotBeNull();
        validatedUnit!.ValidatedAt.Should().NotBeNull(
            because: "la unidad debe tener ValidatedAt después de llamar a /validate");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static MultipartFormDataContent BuildPdfMultipart()
    {
        byte[] pdfBytes;
        string fileName;

        if (!string.IsNullOrWhiteSpace(PdfPath) && File.Exists(PdfPath))
        {
            pdfBytes = File.ReadAllBytes(PdfPath);
            fileName = Path.GetFileName(PdfPath);
        }
        else
        {
            pdfBytes = CreateMinimalPdf();
            fileName = "programa-artes-plasticas-test.pdf";
        }

        var multipart = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(pdfBytes);
        fileContent.Headers.ContentType = new("application/pdf");
        multipart.Add(fileContent, "file", fileName);
        return multipart;
    }

    /// <summary>PDF mínimo válido — el endpoint acepta el Content-Type; el job fallará sin texto real.</summary>
    private static byte[] CreateMinimalPdf()
    {
        const string pdf = "%PDF-1.4\n" +
                           "1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj\n" +
                           "2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj\n" +
                           "3 0 obj<</Type/Page/MediaBox[0 0 612 792]/Parent 2 0 R/Resources<<>>>>endobj\n" +
                           "xref\n0 4\n0000000000 65535 f\n0000000009 00000 n\n" +
                           "0000000058 00000 n\n0000000115 00000 n\n" +
                           "trailer<</Size 4/Root 1 0 R>>\nstartxref\n218\n%%EOF";
        return System.Text.Encoding.ASCII.GetBytes(pdf);
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    private record UploadResponse(
        [property: JsonPropertyName("jobId")] string JobId,
        [property: JsonPropertyName("blobUrl")] string BlobUrl);

    private record CurriculumUnitDto(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("asignatura")] string Asignatura,
        [property: JsonPropertyName("nivel")] int Nivel,
        [property: JsonPropertyName("validatedAt")] DateTimeOffset? ValidatedAt);
}
