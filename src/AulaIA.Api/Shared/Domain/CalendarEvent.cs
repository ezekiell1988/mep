namespace AulaIA.Api.Shared.Domain;

public sealed class CalendarEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Null = feriado nacional (visible a todos los grupos). No-null = evento del grupo.</summary>
    public Guid? GroupId { get; set; }
    public Group? Group { get; set; }

    public DateOnly Date { get; set; }

    /// <summary>Para eventos de varios días (ej. semana del deporte). Null = un solo día.</summary>
    public DateOnly? EndDate { get; set; }

    public required string Title { get; set; }

    public CalendarEventType Type { get; set; } = CalendarEventType.Other;

    public int SchoolYear { get; set; } = DateTime.UtcNow.Year;

    public string? CreatedByAuth0Sub { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public enum CalendarEventType
{
    Holiday = 0,          // Feriado nacional
    Exam = 1,             // Exámenes
    TeacherMeeting = 2,   // Consejo de profesores
    SportWeek = 3,        // FEA / semana del deporte
    Civic = 4,            // Acto cívico
    Institutional = 5,    // Día institucional
    Other = 6
}
