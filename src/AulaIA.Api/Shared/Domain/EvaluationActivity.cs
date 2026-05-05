namespace AulaIA.Api.Shared.Domain;

/// <summary>Actividad de evaluación (prueba, trabajo, etc.)</summary>
public sealed class EvaluationActivity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }  // ej. "Prueba", "Trabajo Cotidiano"
    public decimal MaxScore { get; set; } = 100;
    public decimal Percentage { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public Group? Group { get; init; }
    public ICollection<Grade> Grades { get; init; } = [];
}
