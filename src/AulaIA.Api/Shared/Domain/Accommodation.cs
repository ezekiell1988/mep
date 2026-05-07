namespace AulaIA.Api.Shared.Domain;

/// <summary>
/// Adecuación curricular de un estudiante.
/// Marco legal: Ley 7600 — Igualdad de Oportunidades para Personas con Discapacidad.
/// Las adecuaciones son individuales (por estudiante), nunca masivas.
/// </summary>
public sealed class Accommodation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid StudentId { get; set; }

    /// <summary>Desnormalizado para ownership checks sin JOIN adicional.</summary>
    public Guid GroupId { get; set; }

    /// <summary>Tipo de adecuación según normativa MEP.</summary>
    public AccommodationType Type { get; set; } = AccommodationType.ANS;

    /// <summary>Diagnóstico clínico o educativo que fundamenta la adecuación.</summary>
    public required string Diagnostico { get; set; }

    /// <summary>Descripción adicional de la condición (opcional).</summary>
    public string? CondicionEspecial { get; set; }

    /// <summary>Estrategias de mediación propuestas por el docente o la IA.</summary>
    public string? EstrategiasMediacion { get; set; }

    /// <summary>Estrategias e instrumentos de evaluación diferenciados.</summary>
    public string? EstrategiasEvaluacion { get; set; }

    public string? Observaciones { get; set; }

    // ── Propuesta pedagógica generada por IA ──────────────────────────────
    public string? PropuestaGenerada { get; set; }
    public AccommodationStatus Status { get; set; } = AccommodationStatus.Draft;
    public DateTimeOffset? GeneratedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public string CreatedByAuth0Sub { get; set; } = "";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Student? Student { get; init; }
    public Group? Group { get; init; }
}

/// <summary>Tipos de adecuación curricular del MEP (Ley 7600).</summary>
public enum AccommodationType
{
    /// <summary>Adecuación Significativa — modifica el currículo. Requiere registro en SIMED.</summary>
    AS,
    /// <summary>Adecuación No Significativa — ajusta la forma de presentar el contenido sin modificar objetivos.</summary>
    ANS,
    /// <summary>Apoyo Académico — refuerzo sin modificación curricular formal.</summary>
    AA
}

public enum AccommodationStatus { Draft, Pending, Generating, Ready, Failed }
