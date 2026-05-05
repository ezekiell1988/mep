namespace AulaIA.Api.Shared.Domain;

/// <summary>Institución educativa (colegio / escuela)</summary>
public sealed class Institution
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? CircuitCode { get; set; }
    public string? RegionCode { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ICollection<User> Users { get; init; } = [];
    public ICollection<Group> Groups { get; init; } = [];
}
