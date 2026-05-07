namespace AulaIA.Api.Shared.Domain;

public enum UserRole { Teacher, Coordinator, Admin }

public sealed class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Auth0Sub { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public UserRole Role { get; set; } = UserRole.Teacher;
    public Guid InstitutionId { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Código de referido usado al registrarse (null si llegó orgánico).</summary>
    public string? ReferredByCode { get; set; }

    public Institution? Institution { get; init; }
    public ICollection<Group> Groups { get; init; } = [];
    public Subscription? Subscription { get; init; }
}
