namespace AulaIA.Api.Shared.Domain;

/// <summary>Grupo / sección de clase</summary>
public sealed class Group
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Level { get; set; }   // ej. "7° Año"
    public required string Subject { get; set; } // ej. "Matemáticas"
    public int SchoolYear { get; set; } = DateTime.UtcNow.Year;
    public Guid TeacherId { get; set; }
    /// Auth0 sub del docente — columna de texto para Sync Rules de PowerSync (evita casts de UUID).
    public string TeacherSub { get; set; } = "";
    public Guid InstitutionId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Ponderación configurable por grupo (en %). Suma debe ser 100. Defaults MEP secundaria.</summary>
    public decimal PctCotidiano  { get; set; } = 20m;
    public decimal PctPruebas    { get; set; } = 45m;
    public decimal PctExtraclase { get; set; } = 20m;
    public decimal PctOtros      { get; set; } = 15m;

    public User? Teacher { get; init; }
    public Institution? Institution { get; init; }
    public ICollection<Student> Students { get; init; } = [];
    public ICollection<AttendanceRecord> AttendanceRecords { get; init; } = [];
    public ICollection<EvaluationActivity> EvaluationActivities { get; init; } = [];
}
