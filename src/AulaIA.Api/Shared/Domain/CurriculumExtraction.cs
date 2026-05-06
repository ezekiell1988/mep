namespace AulaIA.Api.Shared.Domain;

/// <summary>
/// Encabezado de una extracción de curriculum: un registro por cada PDF procesado.
/// Las unidades curriculares extraídas referencian este registro.
/// </summary>
public sealed class CurriculumExtraction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Asignatura { get; set; }
    public required string Ciclo { get; set; }
    public required string PdfSourceUrl { get; set; }
    public required string ModelUsed { get; set; }
    public int TotalTokensUsed { get; set; }
    public int UnidadCount { get; set; }
    public DateTimeOffset ExtractedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navegación
    public List<CurriculumUnit> Units { get; set; } = [];
}
