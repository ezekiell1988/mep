namespace AulaIA.Api.Shared.Options;

public sealed class LlmAuditOptions
{
    public const string Section = "LlmAudit";

    /// <summary>Activa o desactiva el sistema de audit completo.</summary>
    public bool Enabled { get; init; } = false;

    /// <summary>Ruta del archivo markdown local (relativa al CWD del proceso).</summary>
    public string LogPath { get; init; } = "logs/llm-audit.md";

    /// <summary>Tamaño máximo del archivo antes de truncarlo.</summary>
    public int MaxFileSizeKb { get; init; } = 2048;

    /// <summary>
    /// Persiste cada entrada también en la tabla llm_audit_entries de PostgreSQL.
    /// Útil para debug de contenedor remoto donde el archivo no es accesible.
    /// ⚠️ Desactivar en producción para no llenar la BD de basura.
    /// Activar con env var: LlmAudit__PersistToDb=true
    /// </summary>
    public bool PersistToDb { get; init; } = false;

    /// <summary>Máximo de entradas en la tabla. 0 = sin límite. Solo aplica si PersistToDb=true.</summary>
    public int MaxDbEntries { get; init; } = 5000;
}
