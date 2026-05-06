# 09 — Patrones Verificados de Código

> **Última actualización:** 2026-05-06
> Registrar aquí solo patrones que ya funcionan en producción/dev y deben replicarse.

---

## PATTERN-01: Encabezado/Detalle con Cascade Delete (EF Core + PostgreSQL)

**Contexto:** Cuando un job de IA genera N registros hijos para un mismo "lote" (PDF, archivo, sesión), usar una entidad encabezado que agrupe los metadatos del lote y referencie los hijos con FK + cascade delete.

**Ejemplo:** `CurriculumExtraction` (1) → `CurriculumUnit[]` (N)

**Entidad encabezado:**
```csharp
public sealed class CurriculumExtraction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Asignatura { get; set; }
    public required string Ciclo { get; set; }
    public required string PdfSourceUrl { get; set; }
    public required string ModelUsed { get; set; }   // ← tomado de AiOptions.DeploymentChat
    public int TotalTokensUsed { get; set; }
    public int UnidadCount { get; set; }
    public DateTimeOffset ExtractedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<CurriculumUnit> Units { get; set; } = [];
}
```

**Configuración EF:**
```csharp
builder.HasMany(x => x.Units)
       .WithOne(u => u.Extraction)
       .HasForeignKey(u => u.ExtractionId)
       .OnDelete(DeleteBehavior.Cascade);
```

**Patrón de re-procesamiento en el job (eliminar encabezado previo no validado → cascade borra hijos):**
```csharp
var extraccionPrevia = await db.CurriculumExtractions
    .Where(e => e.Asignatura == asignatura && e.Ciclo == ciclo)
    .Where(e => !e.Units.Any(u => u.ValidatedAt != null))
    .FirstOrDefaultAsync(ct);
if (extraccionPrevia is not null)
    db.CurriculumExtractions.Remove(extraccionPrevia);

var extraction = new CurriculumExtraction
{
    Asignatura = asignatura, Ciclo = ciclo,
    PdfSourceUrl = blobUrl,
    ModelUsed = aiOpts.Value.DeploymentChat,
    TotalTokensUsed = tokensUsed
};
db.CurriculumExtractions.Add(extraction);

var seen = new HashSet<(int, int, int)>();
foreach (var unit in units)
{
    if (!seen.Add((unit.Nivel, unit.Trimestre, unit.UnidadNumero))) continue;
    unit.ExtractionId = extraction.Id;
    db.CurriculumUnits.Add(unit);
}
extraction.UnidadCount = seen.Count;
await db.SaveChangesAsync(ct);
```

**Reglas:**
- `ModelUsed` siempre se toma de `AiOptions.DeploymentChat` — nunca hardcodeado.
- La deduplicación de hijos va con `HashSet<(int,int,int)>` antes del `SaveChangesAsync`.
- Solo eliminar el encabezado previo si **ninguna** de sus unidades está validada (`ValidatedAt != null`).

---

## PATTERN-02: Hangfire Job con Blob Storage privado

**Contexto:** Los blobs en el contenedor `curriculum` son privados. El job recibe la URL pública pero debe autenticarse con connection string para descargarla.

```csharp
// ❌ NO funciona con blob privado:
// var response = await http.GetAsync(blobUrl);

// ✅ Autenticar con connection string:
var blobUri = new Uri(blobUrl);
var blobName = string.Join("/", blobUri.AbsolutePath.TrimStart('/').Split('/').Skip(1));
var blobClient = new BlobClient(storageOpts.Value.ConnectionString, "curriculum", blobName);
using var stream = await blobClient.OpenReadAsync(cancellationToken: ct);
```

---

## PATTERN-03: ILlmAuditService — Logging de jobs IA

**Contexto:** Todos los jobs de IA deben logear inicio, resultado y errores con `ILlmAuditService`. El contexto JSON permite búsqueda posterior.

```csharp
// Al inicio:
audit.LogEvent("NombreJob", "Iniciando",
    $"asignatura={asignatura} ciclo={ciclo}",
    new { blobUrl });

// Al completar:
audit.LogEvent("NombreJob", "Completado",
    $"✅ {count} items guardados (tokens: {tokensUsed:N0})",
    new { asignatura, ciclo, count, tokensUsed, encabezadoId = extraction.Id });

// En el catch:
audit.LogError("NombreJob", $"❌ Falló para {asignatura}", ex);
```
