namespace AulaIA.Api.Shared.Domain;

public sealed class Grade
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ActivityId { get; set; }
    public Guid StudentId { get; set; }
    public decimal Score { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public EvaluationActivity? Activity { get; init; }
    public Student? Student { get; init; }
}
