namespace AulaIA.Api.Shared.Domain;

/// <summary>
/// Entrada de audit log persistida en PostgreSQL.
/// Solo se guarda cuando LlmAudit:PersistToDb=true.
/// Útil para debug de contenedores remotos donde el archivo local no es accesible.
/// </summary>
public sealed class LlmAuditEntry
{
    public long Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>STARTUP | EVENT | DECISION | ERROR | FLOW | INTEGRATION</summary>
    public string Category { get; init; } = "";

    /// <summary>Componente o módulo que generó la entrada (primer parámetro de cada Log*).</summary>
    public string Component { get; init; } = "";

    /// <summary>Intención o acción en curso.</summary>
    public string Intent { get; init; } = "";

    /// <summary>Resultado o descripción de la entrada.</summary>
    public string Result { get; init; } = "";

    /// <summary>Contexto serializado en JSON (opcional).</summary>
    public string? ContextJson { get; init; }

    /// <summary>true si la entrada fue generada por LogError.</summary>
    public bool IsError { get; init; }
}
