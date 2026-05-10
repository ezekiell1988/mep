using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Extensions;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using AulaIA.Api.Shared.Services;
using Azure.Storage.Blobs;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AulaIA.Api.Features.Curriculum.Jobs;

/// <summary>
/// Job maestro de sincronización de currículos del MEP.
///
/// Para cada fuente activa en <c>curriculum_sources</c>:
///   1. Hace HEAD al URL del MEP y compara ETag / Last-Modified con el último guardado.
///   2. Si hay nueva versión (o es la primera vez), descarga el PDF, lo sube a Blob Storage
///      y encola un <see cref="ExtractCurriculumJob"/> para la extracción con IA.
///   3. Actualiza ETag, Last-Modified y LastSyncedAt en BD.
///
/// Si la tabla de fuentes está vacía, la siembra con el catálogo oficial conocido.
///
/// Schedule: "0 0 30 2 *" (30 de febrero → nunca corre automáticamente).
/// Ejecutar manualmente desde el dashboard de Hangfire cuando sea necesario.
/// </summary>
public sealed class SyncCurriculumJob(
    AulaIADbContext db,
    IHttpClientFactory httpClientFactory,
    IOptions<StorageOptions> storageOpts,
    IBackgroundJobClient backgroundJobs,
    ILlmAuditService audit,
    ILogger<SyncCurriculumJob> logger)
{
    // ── Catálogo inicial ────────────────────────────────────────────────────
    // Fuente de verdad: skill mep-curriculum-pdfs §4.
    // Nombres de asignatura deben coincidir exactamente con el dropdown del UI.
    private static readonly (string Asignatura, string Ciclo, string MepUrl)[] CatalogoInicial =
    [
        ("Artes Plásticas",         "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/artesplasticas3cicloydiversificada.pdf"),
        ("Artes Musicales",         "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/musica3cicloydiversificada.pdf"),
        ("Educación para el Hogar", "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/educacion-vida-cotidiana.pdf"),
        ("Educación para el Hogar", "I y II Ciclo", "https://www.mep.go.cr/sites/default/files/media/vida-cotidiana1y2ciclos.pdf"),
        ("Educación Física",        "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/educfisica3cicloydiversificada.pdf"),
        ("Matemáticas",             "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/matematica.pdf"),
        ("Español",                 "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/espanol3ciclo_diversificada.pdf"),
        ("Ciencias",                "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/ciencias3ciclo.pdf"),
        ("Estudios Sociales",       "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/esociales3ciclo_diversificada.pdf"),
        ("Inglés",                  "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/ingles3ciclo_diversificada.pdf"),
        ("Inglés",                  "II Ciclo",     "https://www.mep.go.cr/sites/default/files/media/ingles_2ciclo.pdf"),
        ("Francés",                 "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/frances3ciclo_diversificada.pdf"),
        ("Orientación",             "III Ciclo",    "https://www.mep.go.cr/sites/default/files/media/orientacion-nuevo.pdf"),
    ];

    [Queue("curriculum")]
    [AutomaticRetry(Attempts = 1)]
    public async Task ExecuteAsync(PerformContext? ctx = null, CancellationToken ct = default)
    {
        ctx.WriteLine("🚀 SyncCurriculumJob iniciando...");
        logger.LogInformation("SyncCurriculumJob: iniciando");
        audit.LogEvent("SyncCurriculumJob", "Iniciando",
            "Verificando actualizaciones de programas en el sitio del MEP");

        // ── 1. Sembrar catálogo inicial si la tabla está vacía ──────────────
        var fuentes = await db.CurriculumSources
            .Where(s => s.IsActive)
            .ToListAsync(ct);

        if (fuentes.Count == 0)
        {
            ctx.WriteLine($"📋 Tabla vacía — sembrando {CatalogoInicial.Length} fuentes del catálogo inicial");
            logger.LogInformation("SyncCurriculumJob: tabla vacía — sembrando {Count} fuentes del catálogo inicial",
                CatalogoInicial.Length);

            foreach (var (asignatura, ciclo, url) in CatalogoInicial)
            {
                db.CurriculumSources.Add(new CurriculumSource
                {
                    Asignatura = asignatura,
                    Ciclo      = ciclo,
                    MepUrl     = url,
                });
            }

            await db.SaveChangesAsync(ct);

            fuentes = await db.CurriculumSources
                .Where(s => s.IsActive)
                .ToListAsync(ct);

            ctx.WriteLine($"✅ {fuentes.Count} fuentes insertadas en curriculum_sources");
            audit.LogEvent("SyncCurriculumJob", "Catálogo sembrado",
                $"✅ {fuentes.Count} fuentes insertadas en curriculum_sources");
        }
        else
        {
            ctx.WriteLine($"📂 {fuentes.Count} fuentes activas en BD");
        }

        // ── 2. Procesar cada fuente ─────────────────────────────────────────
        var http = httpClientFactory.CreateClient("mep");
        int actualizadas = 0, sinCambios = 0, errores = 0;

        for (int i = 0; i < fuentes.Count; i++)
        {
            var fuente = fuentes[i];
            ctx.WriteLine($"🔍 [{i + 1}/{fuentes.Count}] {fuente.Asignatura} ({fuente.Ciclo})");
            try
            {
                logger.LogInformation("SyncCurriculumJob: verificando {Asignatura} ({Ciclo})",
                    fuente.Asignatura, fuente.Ciclo);

                var (etag, lastModified) = await GetMepHeadAsync(http, fuente.MepUrl, ct);

                var esPrimera      = fuente.LastSyncedAt is null;
                var etagDistinto   = etag is not null && etag != fuente.LastEtag;
                var masNuevo       = lastModified.HasValue && lastModified > fuente.LastModifiedMep;
                var hayNuevaVersion = esPrimera || etagDistinto || masNuevo;

                if (!hayNuevaVersion)
                {
                    ctx.WriteLine($"   ↳ Sin cambios (ETag: {etag ?? "n/a"})");
                    logger.LogInformation(
                        "SyncCurriculumJob: {Asignatura} ({Ciclo}) sin cambios — ETag={ETag} LastModified={LastModified}",
                        fuente.Asignatura, fuente.Ciclo, etag, lastModified);

                    fuente.LastSyncedAt = DateTimeOffset.UtcNow;
                    await db.SaveChangesAsync(ct);
                    sinCambios++;
                    continue;
                }

                var razon = esPrimera ? "primera sincronización" : etagDistinto ? "ETag cambió" : "LastModified más nuevo";
                ctx.WriteLine($"   ↳ 🆕 Nueva versión ({razon}) — descargando PDF...");
                logger.LogInformation(
                    "SyncCurriculumJob: {Asignatura} ({Ciclo}) — nueva versión detectada " +
                    "(primera={EsPrimera} etagDistinto={EtagDistinto} masNuevo={MasNuevo}), descargando PDF...",
                    fuente.Asignatura, fuente.Ciclo, esPrimera, etagDistinto, masNuevo);

                // ── 3. Descargar y subir a Blob Storage ─────────────────────
                var blobUrl = await DownloadAndUploadAsync(http, fuente, ctx, ct);
                ctx.WriteLine($"   ↳ 📤 PDF subido a Blob");

                // ── 4. Encolar extracción con IA ─────────────────────────────
                var jobId = backgroundJobs.Enqueue<ExtractCurriculumJob>(
                    "curriculum",
                    j => j.ExecuteAsync(blobUrl, fuente.Asignatura, fuente.Ciclo, null, CancellationToken.None));

                ctx.WriteLine($"   ↳ 📌 ExtractCurriculumJob encolado (jobId: {jobId})");

                // ── 5. Actualizar estado en BD ───────────────────────────────
                fuente.LastEtag        = etag;
                fuente.LastModifiedMep = lastModified;
                fuente.LastSyncedAt    = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(ct);

                audit.LogEvent("SyncCurriculumJob", "PDF encolado",
                    $"✅ {fuente.Asignatura} ({fuente.Ciclo}) → ExtractCurriculumJob jobId={jobId}",
                    new { fuente.Asignatura, fuente.Ciclo, blobUrl, jobId, etag, lastModified });

                logger.LogInformation(
                    "SyncCurriculumJob: {Asignatura} ({Ciclo}) encolado correctamente — jobId={JobId}",
                    fuente.Asignatura, fuente.Ciclo, jobId);

                actualizadas++;
            }
            catch (Exception ex)
            {
                ctx.WriteLine($"   ↳ ❌ Error: {ex.Message}");
                audit.LogError("SyncCurriculumJob",
                    $"Error procesando {fuente.Asignatura} ({fuente.Ciclo})", ex);

                logger.LogError(ex,
                    "SyncCurriculumJob: fallo en {Asignatura} ({Ciclo})",
                    fuente.Asignatura, fuente.Ciclo);

                errores++;
                // No relanzar: continuar con las demás fuentes
            }
        }

        // ── 6. Resumen ───────────────────────────────────────────────────────
        var resumen = $"✅ {actualizadas} actualizadas · {sinCambios} sin cambios · {errores} errores";

        ctx.WriteLine($"\n📊 Resumen: {resumen}");
        audit.LogEvent("SyncCurriculumJob", "Completado", resumen,
            new { actualizadas, sinCambios, errores, total = fuentes.Count });

        logger.LogInformation("SyncCurriculumJob completado: {Resumen}", resumen);

        if (errores > 0)
            throw new InvalidOperationException(
                $"SyncCurriculumJob terminó con {errores} error(es). Revisar logs/llm-audit.md.");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Hace HEAD al URL del MEP y extrae ETag y Last-Modified.
    /// Retorna nulls si el servidor no los devuelve.
    /// </summary>
    private static async Task<(string? ETag, DateTimeOffset? LastModified)> GetMepHeadAsync(
        HttpClient http, string url, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Head, url);
        using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!resp.IsSuccessStatusCode)
        {
            // El sitio del MEP a veces devuelve 403 en HEAD pero acepta GET — no fallar aquí
            return (null, null);
        }

        var etag        = resp.Headers.ETag?.Tag;
        var lastModified = resp.Content.Headers.LastModified;

        return (etag, lastModified);
    }

    /// <summary>
    /// Descarga el PDF del MEP con GET y lo sube al contenedor Blob "curriculum".
    /// Retorna la URL del blob en Azure Storage.
    /// </summary>
    private async Task<string> DownloadAndUploadAsync(
        HttpClient http, CurriculumSource fuente, PerformContext? ctx, CancellationToken ct)
    {
        using var resp = await http.GetAsync(fuente.MepUrl, HttpCompletionOption.ResponseContentRead, ct);
        resp.EnsureSuccessStatusCode();

        var pdfBytes = await resp.Content.ReadAsByteArrayAsync(ct);
        ctx.WriteLine($"   ↳ 📄 PDF descargado: {pdfBytes.Length / 1024} KB");
        logger.LogInformation(
            "SyncCurriculumJob: PDF descargado — {Asignatura} {Ciclo} ({Kb} KB)",
            fuente.Asignatura, fuente.Ciclo, pdfBytes.Length / 1024);

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var blobName  = $"{BlobSlugHelper.ToAsciiSlug(fuente.Asignatura)}/{timestamp}.pdf";

        var container = new BlobContainerClient(
            storageOpts.Value.ConnectionString,
            storageOpts.Value.ContainerCurriculum);

        await container.CreateIfNotExistsAsync(cancellationToken: ct);
        await container.UploadBlobAsync(blobName, new BinaryData(pdfBytes), ct);

        var blobUrl = container.GetBlobClient(blobName).Uri.ToString();

        logger.LogInformation(
            "SyncCurriculumJob: PDF subido a Blob — {BlobUrl}", blobUrl);

        return blobUrl;
    }
}
