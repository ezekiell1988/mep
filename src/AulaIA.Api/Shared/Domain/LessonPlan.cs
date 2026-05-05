namespace AulaIA.Api.Shared.Domain;

public sealed class LessonPlan
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid GroupId { get; set; }
    public Group? Group { get; set; }

    public required string TeacherSub { get; set; }

    // Parámetros del planeamiento
    public required string Asignatura { get; set; }
    public int Nivel { get; set; }
    public int Trimestre { get; set; }
    public int AnioLectivo { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int LeccionesPorSemana { get; set; }

    // Contenido generado por IA (Markdown)
    public string? ContenidoGenerado { get; set; }

    // URL del PDF/DOCX guardado en Blob Storage
    public string? ArchivoBlobUrl { get; set; }

    public LessonPlanStatus Status { get; set; } = LessonPlanStatus.Pending;
    public string? ErrorMessage { get; set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? GeneratedAt { get; set; }
}

public enum LessonPlanStatus
{
    Pending = 0,
    Generating = 1,
    Ready = 2,
    Failed = 3
}
