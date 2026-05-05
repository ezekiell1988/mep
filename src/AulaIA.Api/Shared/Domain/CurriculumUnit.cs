namespace AulaIA.Api.Shared.Domain;

public sealed class CurriculumUnit
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Asignatura { get; set; }
    public required string Ciclo { get; set; }
    public int Nivel { get; set; }          // 7, 8, 9, 10, 11...
    public int Trimestre { get; set; }      // 1, 2, 3
    public int UnidadNumero { get; set; }
    public required string UnidadNombre { get; set; }

    // JSONB columns — almacenan arrays de strings del programa oficial
    public List<string> AprendizajesEsperados { get; set; } = [];
    public List<string> IndicadoresEvaluacion { get; set; } = [];
    public List<string> ContenidoConceptual { get; set; } = [];
    public List<string> ContenidoProcedimental { get; set; } = [];
    public List<string> ContenidoActitudinal { get; set; } = [];
    public List<string> EstrategiasSugeridas { get; set; } = [];

    public string? PdfSourceUrl { get; set; }
    public DateTimeOffset ExtractedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? ValidatedBy { get; set; }     // Auth0 sub del admin que validó
    public DateTimeOffset? ValidatedAt { get; set; }
}
