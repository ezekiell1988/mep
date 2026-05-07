namespace AulaIA.Api.Shared.Domain;

/// <summary>
/// Tipo de cambio USD/CRC del BCCR (indicador 318 — venta).
/// Actualizado diariamente por UpdateExchangeRateJob (Hangfire).
/// </summary>
public sealed class ExchangeRate
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateOnly Date { get; set; }

    /// <summary>Tipo de cambio de venta CRC por 1 USD (indicador 318 BCCR).</summary>
    public decimal UsdToCrc { get; set; }

    public string Source { get; set; } = "BCCR";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
