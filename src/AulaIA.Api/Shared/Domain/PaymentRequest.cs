namespace AulaIA.Api.Shared.Domain;

public enum PaymentRequestStatus { Pending, Approved, Rejected }

/// <summary>
/// Solicitud de pago SINPE Móvil. El flujo es:
/// 1. Usuario POST /api/suscripcion/pago → se crea con Status=Pending y un reference_code único.
/// 2. Usuario transfiere el monto al número SINPE de AulaIA y opcionalmente sube comprobante.
/// 3. Admin aprueba → activa/renueva Subscription. Admin rechaza → notifica usuario.
/// </summary>
public sealed class PaymentRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    public SubscriptionPlan Plan { get; set; }
    public decimal AmountUsd { get; set; }
    public decimal AmountCrc { get; set; }
    public decimal ExchangeRateUsed { get; set; }

    /// <summary>Código único para que el admin identifique la transferencia. Formato: AUI-YYYYMMDD-XXXX.</summary>
    public string ReferenceCode { get; set; } = "";

    public PaymentRequestStatus Status { get; set; } = PaymentRequestStatus.Pending;

    /// <summary>Ruta del blob en el contenedor "pagos" (null si el usuario no subió comprobante).</summary>
    public string? VoucherBlobPath { get; set; }

    public string? AdminNote { get; set; }
    public string? ReviewedByAuth0Sub { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; init; }
}
