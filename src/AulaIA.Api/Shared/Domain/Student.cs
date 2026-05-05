namespace AulaIA.Api.Shared.Domain;

public sealed class Student
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string FullName { get; set; }
    public required string StudentCode { get; set; }
    public Guid GroupId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public Group? Group { get; init; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; init; } = [];
    public ICollection<Grade> Grades { get; init; } = [];
}
