namespace AulaIA.Api.Shared.Domain;

public enum CommissionStatus { Pending, Paid }

/// <summary>
/// Comisión mensual generada por CalculateCommissionsJob para cada usuario referido activo.
/// Regla ADR-008: 20% del ingreso neto (bruto − costo infra) durante 12 meses desde suscripción.
/// </summary>
public sealed class Commission
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ReferralCodeId { get; set; }
    public Guid ReferredUserId { get; set; }

    /// <summary>Período en formato YYYYMM (e.g. 202606 = junio 2026).</summary>
    public int Month { get; set; }

    public decimal GrossRevenueCrc { get; set; }
    public decimal InfraCostCrc { get; set; }

    /// <summary>Base de comisión = GrossRevenueCrc − InfraCostCrc.</summary>
    public decimal BaseAmountCrc { get; set; }

    /// <summary>Tasa de comisión. Actualmente 0.20 (20%) según ADR-008.</summary>
    public decimal CommissionRate { get; set; } = 0.20m;
    public decimal CommissionAmountCrc { get; set; }

    public CommissionStatus Status { get; set; } = CommissionStatus.Pending;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ReferralCode? ReferralCode { get; init; }
    public User? ReferredUser { get; init; }
}
