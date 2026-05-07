using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using AulaIA.Api.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Features.Suscripciones.Jobs;

/// <summary>
/// Revisa suscripciones vencidas y las marca como Expired.
/// Se ejecuta diariamente a las 8 AM UTC.
/// </summary>
public sealed class CheckExpiredSubscriptionsJob(
    AulaIADbContext db,
    ILlmAuditService audit,
    ILogger<CheckExpiredSubscriptionsJob> log)
{
    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        audit.LogEvent("CheckExpiredSubscriptionsJob", "Iniciando", $"utcNow={now:O}");

        try
        {
            var expired = await db.Subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active
                         && s.CurrentPeriodEnd < now)
                .ToListAsync(ct);

            foreach (var sub in expired)
            {
                sub.Status = SubscriptionStatus.Expired;
                sub.UpdatedAt = now;
                // Si era trial vencido lo degradamos, si era pago también lo marcamos expirado
                // El docente verá el banner de renovación en el frontend
            }

            if (expired.Count > 0)
            {
                await db.SaveChangesAsync(ct);
                audit.LogEvent("CheckExpiredSubscriptionsJob", "Completado",
                    $"✅ {expired.Count} suscripciones marcadas como Expired");
                log.LogInformation("{Count} suscripciones expiradas procesadas", expired.Count);
            }
            else
            {
                audit.LogEvent("CheckExpiredSubscriptionsJob", "Sin cambios", "0 suscripciones vencidas");
            }
        }
        catch (Exception ex)
        {
            audit.LogError("CheckExpiredSubscriptionsJob", "❌ Error procesando expirations", ex);
            log.LogError(ex, "Error en CheckExpiredSubscriptionsJob");
            throw;
        }
    }
}
