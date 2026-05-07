using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using AulaIA.Api.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Features.Suscripciones.Jobs;

/// <summary>
/// Calcula comisiones mensuales para todos los referidos activos cuya suscripción esté dentro
/// del período de 12 meses desde la activación. Requiere que el admin ingrese el costo de
/// infraestructura del mes antes de ejecutar.
/// Regla ADR-008: comisión = 20% × (ingresos brutos − costo infra Azure).
/// </summary>
public sealed class CalculateCommissionsJob(
    AulaIADbContext db,
    ILlmAuditService audit,
    ILogger<CalculateCommissionsJob> log)
{
    /// <param name="month">Período YYYYMM (e.g. 202606).</param>
    /// <param name="infraCostCrc">Costo de infraestructura Azure del mes en CRC, ingresado manualmente por el admin.</param>
    public async Task ExecuteAsync(int month, decimal infraCostCrc, CancellationToken ct = default)
    {
        audit.LogEvent("CalculateCommissionsJob", "Iniciando",
            $"month={month} infraCostCrc={infraCostCrc:N0}");

        try
        {
            // Cargamos todos los códigos de referido activos con sus usuarios referidos que:
            // 1. Tienen suscripción activa (Basic/Professional)
            // 2. Llevan menos de 12 meses desde la activación de su suscripción
            var cutoff = DateTime.UtcNow.AddMonths(-12);

            var referralData = await db.ReferralCodes
                .Where(rc => rc.IsActive)
                .Include(rc => rc.User)
                .ToListAsync(ct);

            // Usuarios que fueron referidos y tienen suscripción activa pagada
            var referredUsers = await db.Users
                .Where(u => u.ReferredByCode != null
                         && u.Subscription != null
                         && u.Subscription.Status == SubscriptionStatus.Active
                         && !u.Subscription.IsTrial
                         && u.Subscription.CreatedAt >= cutoff)
                .Include(u => u.Subscription)
                .ToListAsync(ct);

            int created = 0;
            decimal totalCommissionCrc = 0;

            foreach (var referredUser in referredUsers)
            {
                var code = referralData.FirstOrDefault(rc => rc.Code == referredUser.ReferredByCode);
                if (code is null) continue;

                // Evitar duplicados
                var exists = await db.Commissions
                    .AnyAsync(c => c.ReferralCodeId == code.Id
                                && c.ReferredUserId == referredUser.Id
                                && c.Month == month, ct);
                if (exists) continue;

                // Ingreso bruto de este usuario: precio del plan en CRC
                var planPrice = referredUser.Subscription!.Plan switch
                {
                    SubscriptionPlan.Basic => await GetPlanPriceCrcAsync(6m, ct),
                    SubscriptionPlan.Professional => await GetPlanPriceCrcAsync(15m, ct),
                    SubscriptionPlan.Institutional => await GetPlanPriceCrcAsync(100m, ct),
                    _ => 0m
                };

                if (planPrice <= 0) continue;

                // Proporcionar costo infra prorrateado (simple: dividir por total usuarios referidos activos)
                var infraProrateada = referredUsers.Count > 0
                    ? infraCostCrc / referredUsers.Count
                    : 0m;

                var baseAmount = Math.Max(planPrice - infraProrateada, 0m);
                var commissionAmount = Math.Round(baseAmount * 0.20m, 2);

                db.Commissions.Add(new Commission
                {
                    ReferralCodeId = code.Id,
                    ReferredUserId = referredUser.Id,
                    Month = month,
                    GrossRevenueCrc = planPrice,
                    InfraCostCrc = infraProrateada,
                    BaseAmountCrc = baseAmount,
                    CommissionRate = 0.20m,
                    CommissionAmountCrc = commissionAmount,
                    Status = CommissionStatus.Pending
                });

                created++;
                totalCommissionCrc += commissionAmount;
            }

            if (created > 0)
                await db.SaveChangesAsync(ct);

            audit.LogEvent("CalculateCommissionsJob", "Completado",
                $"✅ {created} comisiones creadas, total={totalCommissionCrc:N0} CRC",
                new { month, infraCostCrc, created, totalCommissionCrc });

            log.LogInformation("Comisiones calculadas: {Count} comisiones, total {Total:N0} CRC",
                created, totalCommissionCrc);
        }
        catch (Exception ex)
        {
            audit.LogError("CalculateCommissionsJob", $"❌ Falló para month={month}", ex);
            log.LogError(ex, "Error en CalculateCommissionsJob para período {Month}", month);
            throw;
        }
    }

    private async Task<decimal> GetPlanPriceCrcAsync(decimal priceUsd, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tc = await db.ExchangeRates
            .Where(r => r.Date <= today)
            .OrderByDescending(r => r.Date)
            .Select(r => r.UsdToCrc)
            .FirstOrDefaultAsync(ct);

        return tc > 0 ? Math.Round(priceUsd * tc, 2) : 0m;
    }
}
