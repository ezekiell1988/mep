namespace AulaIA.Api.Shared.Domain;

/// <summary>
/// Código de referido de un usuario (normalmente generado para Adriana y otros referidores).
/// Se distribuye como ?ref=CODIGO en el link de registro.
/// </summary>
public sealed class ReferralCode
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    /// <summary>Código único legible (e.g. "ADRIANA2026"). Máx 32 chars, mayúsculas.</summary>
    public string Code { get; set; } = "";

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public User? User { get; init; }
    public ICollection<Commission> Commissions { get; init; } = [];
}
