using AulaIA.Api.Features.Adecuaciones.Services;
using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Hangfire;

namespace AulaIA.Api.Features.Adecuaciones.Jobs;

public sealed class GenerarAdecuacionJob(
    AulaIADbContext db,
    AdecuacionAiService aiService,
    ILogger<GenerarAdecuacionJob> logger)
{
    [Queue("default")]
    [AutomaticRetry(Attempts = 1)]
    public async Task ExecuteAsync(Guid accommodationId, CancellationToken ct)
    {
        var acc = await db.Accommodations.FindAsync([accommodationId], ct);
        if (acc is null)
        {
            logger.LogWarning("Accommodation {Id} no encontrada", accommodationId);
            return;
        }

        acc.Status = AccommodationStatus.Generating;
        acc.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        try
        {
            acc.PropuestaGenerada = await aiService.GenerarPropuestaAsync(acc, ct);
            acc.Status = AccommodationStatus.Ready;
            acc.GeneratedAt = DateTimeOffset.UtcNow;
            logger.LogInformation("Propuesta adecuación {Id} generada correctamente", accommodationId);
        }
        catch (Exception ex)
        {
            acc.Status = AccommodationStatus.Failed;
            acc.ErrorMessage = ex.Message;
            logger.LogError(ex, "Error generando propuesta adecuación {Id}", accommodationId);
        }

        acc.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
}
