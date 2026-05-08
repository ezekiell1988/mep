namespace AulaIA.Api.Shared.Domain;

/// <summary>
/// Registro de una fuente de curriculum MEP: un PDF oficial por asignatura + ciclo.
/// Se usa para detectar actualizaciones comparando ETag / Last-Modified del servidor del MEP.
/// </summary>
public sealed class CurriculumSource
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Nombre de la asignatura en la nomenclatura de AulaIA (ej. "Artes Musicales").</summary>
    public required string Asignatura { get; set; }

    /// <summary>Ciclo correspondiente (ej. "III Ciclo", "I y II Ciclo").</summary>
    public required string Ciclo { get; set; }

    /// <summary>URL completa del PDF en el sitio del MEP.</summary>
    public required string MepUrl { get; set; }

    /// <summary>ETag HTTP devuelto por el MEP en la última sincronización exitosa.</summary>
    public string? LastEtag { get; set; }

    /// <summary>Last-Modified HTTP devuelto por el MEP en la última sincronización exitosa.</summary>
    public DateTimeOffset? LastModifiedMep { get; set; }

    /// <summary>Fecha/hora UTC de la última vez que se sincronizó (con o sin cambio).</summary>
    public DateTimeOffset? LastSyncedAt { get; set; }

    /// <summary>Si false, el job no procesa esta fuente.</summary>
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
