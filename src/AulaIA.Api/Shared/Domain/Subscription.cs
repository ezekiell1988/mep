namespace AulaIA.Api.Shared.Domain;

public enum SubscriptionPlan { Trial, Basic, Professional, Institutional }
public enum SubscriptionStatus { Active, Expired, Cancelled }

/// <summary>
/// Suscripción activa de un usuario. Un usuario tiene como máximo una suscripción activa.
/// Al activar trial se crea con Plan=Trial, IsTrial=true, duración 30 días.
/// Cuando el admin aprueba un pago, se actualiza a Basic/Professional/Institutional.
/// </summary>
public sealed class Subscription
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Trial;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public bool IsTrial { get; set; } = true;
    public DateTime CurrentPeriodStart { get; set; } = DateTime.UtcNow;
    public DateTime CurrentPeriodEnd { get; set; } = DateTime.UtcNow.AddDays(30);

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; init; }
    public ICollection<PaymentRequest> PaymentRequests { get; init; } = [];
}
