using System.ComponentModel.DataAnnotations;

namespace AulaIA.Api.Shared.Options;

public sealed class SinpeOptions
{
    public const string Section = "Sinpe";

    /// <summary>Número SINPE Móvil de AulaIA al que el docente debe transferir. Configurable sin redeploy.</summary>
    [Required]
    public required string PhoneNumber { get; init; }

    /// <summary>Nombre del titular de la cuenta SINPE (para mostrar al usuario).</summary>
    [Required]
    public required string AccountName { get; init; }

    /// <summary>Precio del plan Básico en USD.</summary>
    public decimal PriceBasicUsd { get; init; } = 6m;

    /// <summary>Precio del plan Profesional en USD.</summary>
    public decimal PriceProfessionalUsd { get; init; } = 15m;

    /// <summary>Precio del plan Institucional en USD.</summary>
    public decimal PriceInstitutionalUsd { get; init; } = 100m;

    /// <summary>Duración del trial en días.</summary>
    public int TrialDays { get; init; } = 30;
}
