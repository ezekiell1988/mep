namespace AulaIA.Api.Shared.Domain;

public enum AttendanceStatus { Present, Absent, Late, Justified }

public sealed class AttendanceRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid GroupId { get; set; }
    public Guid StudentId { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public Group? Group { get; init; }
    public Student? Student { get; init; }
}
