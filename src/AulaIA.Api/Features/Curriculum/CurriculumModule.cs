using AulaIA.Api.Features.Curriculum.Jobs;
using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using Azure.Storage.Blobs;
using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AulaIA.Api.Features.Curriculum;

public static class CurriculumModule
{
    public static IServiceCollection AddCurriculumModule(this IServiceCollection services)
    {
        services.AddScoped<ExtractCurriculumJob>();
        services.AddScoped<SyncCurriculumJob>();

        // HttpClient dedicado para llamadas al sitio del MEP (HEAD + GET de PDFs)
        services.AddHttpClient("mep", c =>
        {
            c.BaseAddress = new Uri("https://www.mep.go.cr");
            c.DefaultRequestHeaders.UserAgent.ParseAdd("AulaIA-SyncBot/1.0");
        })
        .AddStandardResilienceHandler(opts =>
        {
            // PDFs pueden ser grandes — ajustar timeouts por encima del default (30 s)
            opts.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(120);
            opts.AttemptTimeout.Timeout      = TimeSpan.FromSeconds(90);
            opts.Retry.MaxRetryAttempts      = 2;
        });

        return services;
    }

    public static IEndpointRouteBuilder MapCurriculumEndpoints(this IEndpointRouteBuilder app)
    {
        var curriculum = app.MapGroup("/api/curriculum")
                            .WithTags("Curriculum")
                            .RequireAuthorization("admin");

        // POST /api/curriculum/upload
        // Sube el PDF del programa MEP a Blob Storage y encola el job de extracción
        curriculum.MapPost("/upload", async Task<Results<Accepted<UploadResponse>, BadRequest<string>>> (
            IFormFile file,
            [FromQuery] string asignatura,
            [FromQuery] string ciclo,
            IOptions<StorageOptions> storageOpts,
            IBackgroundJobClient jobs,
            CancellationToken ct) =>
        {
            if (file.ContentType != "application/pdf")
                return TypedResults.BadRequest("Solo se aceptan archivos PDF.");

            if (file.Length > 50 * 1024 * 1024)
                return TypedResults.BadRequest("El archivo no puede superar 50 MB.");

            var blobName = $"{asignatura.ToLowerInvariant().Replace(" ", "-")}/{Guid.NewGuid()}.pdf";
            var client = new BlobContainerClient(
                storageOpts.Value.ConnectionString,
                storageOpts.Value.ContainerCurriculum);

            await client.CreateIfNotExistsAsync(cancellationToken: ct);

            await using var stream = file.OpenReadStream();
            await client.UploadBlobAsync(blobName, stream, ct);

            var blobUrl = client.GetBlobClient(blobName).Uri.ToString();

            var jobId = jobs.Enqueue<ExtractCurriculumJob>(
                "curriculum",
                j => j.ExecuteAsync(blobUrl, asignatura, ciclo, CancellationToken.None));

            return TypedResults.Accepted("/api/curriculum/jobs", new UploadResponse(jobId, blobUrl));
        })
        .WithName("UploadCurriculum")
        .DisableAntiforgery();

        // GET /api/curriculum — lista unidades validadas por asignatura + nivel
        curriculum.MapGet("/", async Task<Ok<List<CurriculumUnit>>> (
            [FromQuery] string asignatura,
            [FromQuery] int? nivel,
            AulaIADbContext db,
            CancellationToken ct) =>
        {
            var query = db.CurriculumUnits
                .Where(u => u.Asignatura == asignatura && u.ValidatedAt != null);

            if (nivel.HasValue)
                query = query.Where(u => u.Nivel == nivel.Value);

            var units = await query
                .OrderBy(u => u.Nivel)
                .ThenBy(u => u.Trimestre)
                .ThenBy(u => u.UnidadNumero)
                .ToListAsync(ct);

            return TypedResults.Ok(units);
        })
        .WithName("ListCurriculumUnits");

        // POST /api/curriculum/{id}/validate — admin confirma que la extracción es correcta
        curriculum.MapPost("/{id:guid}/validate", async Task<Results<NoContent, NotFound>> (
            Guid id,
            HttpContext ctx,
            AulaIADbContext db,
            CancellationToken ct) =>
        {
            var unit = await db.CurriculumUnits.FindAsync([id], ct);
            if (unit is null) return TypedResults.NotFound();

            unit.ValidatedBy = ctx.User.FindFirst("sub")?.Value;
            unit.ValidatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);

            return TypedResults.NoContent();
        })
        .WithName("ValidateCurriculumUnit");

        return app;
    }

    public record UploadResponse(string JobId, string BlobUrl);
}
