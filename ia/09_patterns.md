# 09 — Patrones Verificados de Código

> **Última actualización:** 2026-05-07
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

---

## PATTERN-04: Desnormalización `group_id` para PowerSync (offline-first)

**Contexto:** PowerSync requiere `SELECT * FROM tabla WHERE columna = bucket.param` — tabla única, sin JOINs. Si una entidad hija no tiene acceso directo al `group_id` del bucket, agregar la columna desnormalizada es la solución correcta.

**Ejemplo:** `grades` no tiene `group_id` directamente (solo `activity_id`). Se agrega `group_id` como columna desnormalizada.

**Entidad:**
```csharp
public Guid GroupId { get; set; }   // ← desnormalizado para PowerSync
public Guid ActivityId { get; set; }
```

**Configuración EF:**
```csharp
builder.Property(x => x.GroupId).HasColumnName("group_id");
builder.HasIndex(x => x.GroupId).HasDatabaseName("ix_grades_group_id");
```

**Endpoint upsert — poblar al insertar:**
```csharp
db.Grades.Add(new Grade
{
    GroupId = grupoId,   // ← tomar del parámetro de ruta
    ActivityId = actividadId,
    StudentId = item.StudentId,
    Score = item.Score,
});
```

**Sync rule resultante (simple, sin JOINs):**
```yaml
- SELECT * FROM grades WHERE group_id = bucket.group_id
```

**Regla:** Siempre poblar `GroupId` desde el parámetro de ruta `grupoId` — nunca derivarlo de la actividad en memoria (evita inconsistencias en updates concurrentes).

---

## PATTERN-05: Job Hangfire — Tipo de cambio USD/CRC del API BCCR

**Contexto:** SINPE Móvil opera exclusivamente en CRC. Para mostrar el equivalente en colones del precio en USD y registrar el `amount_crc` de cada `PaymentRequest`, el sistema consulta el tipo de cambio de venta oficial del Banco Central de Costa Rica (BCCR) mediante su API SOAP pública (sin costo, requiere token gratuito). El job se ejecuta una vez por día hábil a las 6am hora CR con Hangfire.

```csharp
// Features/Jobs/UpdateExchangeRateJob.cs
public class UpdateExchangeRateJob(AulaIADbContext db, IOptions<SinpeOptions> opts, ILogger<UpdateExchangeRateJob> log)
{
    // Endpoint SOAP público del BCCR:
    // https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx
    // Indicador 318 = tipo de cambio de VENTA del USD (el que usa quien paga en USD → CRC)
    // Indicador 317 = tipo de cambio de COMPRA (no usar para SINPE)
    private const string BccrSoapUrl = "https://gee.bccr.fi.cr/Indicadores/Suscripciones/WS/wsindicadoreseconomicos.asmx";

    public async Task Execute(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica")));

        // Idempotente: no duplicar si ya existe registro del día
        if (await db.ExchangeRates.AnyAsync(e => e.Date == today, ct))
        {
            log.LogInformation("TC ya registrado para {Date}", today);
            return;
        }

        var soapEnvelope = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                           xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                           xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body>
                <ObtenerIndicadoresEconomicos xmlns="http://ws.sdde.bccr.fi.cr">
                  <Indicador>318</Indicador>
                  <FechaInicio>{today:dd/MM/yyyy}</FechaInicio>
                  <FechaFinal>{today:dd/MM/yyyy}</FechaFinal>
                  <Nombre>AulaIA</Nombre>
                  <SubNiveles>N</SubNiveles>
                  <CorreoElectronico>{opts.Value.BccrEmail}</CorreoElectronico>
                  <Token>{opts.Value.BccrApiToken}</Token>
                </ObtenerIndicadoresEconomicos>
              </soap:Body>
            </soap:Envelope>
            """;

        using var http = new HttpClient();
        using var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", "\"\"");

        var response = await http.PostAsync(BccrSoapUrl, content, ct);
        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(ct);
        var doc = XDocument.Parse(xml);
        XNamespace ns = "http://ws.sdde.bccr.fi.cr";

        // El BCCR devuelve el valor en el nodo <NUM_VALOR> del XML anidado
        var valorStr = doc.Descendants("NUM_VALOR").FirstOrDefault()?.Value
            ?? throw new InvalidOperationException("BCCR no devolvió NUM_VALOR");
        var sellRate = decimal.Parse(valorStr, CultureInfo.InvariantCulture);

        db.ExchangeRates.Add(new ExchangeRate
        {
            Date = today,
            SellRate = sellRate,
            FetchedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(ct);
        log.LogInformation("TC BCCR actualizado: {Rate} CRC/USD para {Date}", sellRate, today);
    }
}
```

**Fallback — si el job falla (fin de semana, feriado, error de red):**
```csharp
// Servicio que resuelve el TC del día para crear un PaymentRequest
public async Task<decimal> GetCurrentSellRateAsync(CancellationToken ct)
    => await db.ExchangeRates
           .OrderByDescending(e => e.Date)
           .Select(e => e.SellRate)
           .FirstOrDefaultAsync(ct)
       ?? throw new InvalidOperationException("No hay tipo de cambio disponible");
```

**AppSettings.json correspondiente:**
```json
"Sinpe": {
  "SinpeNumber": "88001234",
  "BccrEmail": "correo@registrado.bccr.fi.cr",
  "BccrApiToken": "TOKEN-OBTENIDO-EN-GEE-BCCR"
}
```

**Reglas:**
- El job es idempotente: si ya existe un registro para el día, no hace nada.
- Usar siempre **indicador 318** (venta), no 317 (compra) — SINPE cobra al usuario que convierte USD a CRC.
- Guardar `exchange_rate_used` en `PaymentRequest` en el momento de creación, no calcularlo despues (auditoría).
- El token BCCR es gratuito; registrarse en `https://gee.bccr.fi.cr` con el correo del proyecto.
- Configurar las credenciales BCCR en Key Vault, no en `appsettings.json`.

---

## PATTERN-06: Next.js `output: export` con rutas dinámicas `[param]` y `'use client'`

**Contexto:** Next.js 16 con `output: 'export'` requiere `generateStaticParams()` en toda ruta dinámica. Las páginas `'use client'` no pueden exportar `generateStaticParams()`. La solución es un **server wrapper** que actúa como `page.tsx` y delega a un `PageClient.tsx`.

**Problema con `return []` vacío:** Turbopack 16 ignora `generateStaticParams` si retorna array vacío y lanza `"Page X is missing generateStaticParams()"`. Necesita al menos un elemento placeholder.

**Estructura de carpeta:**
```
app/feature/[grupoId]/
├── page.tsx        ← Server Component (wrapper) — SIN 'use client'
└── PageClient.tsx  ← Client Component real — CON 'use client'
```

**`page.tsx` (Server Component wrapper):**
```tsx
import { Suspense } from 'react';
import PageClient from './PageClient';

export function generateStaticParams() {
  return [{ grupoId: '_' }];   // ← placeholder obligatorio; no puede ser []
}

export default function Page({ params }: { params: Promise<{ grupoId: string }> }) {
  return (
    <Suspense fallback={null}>
      <PageClient params={params} />
    </Suspense>
  );
}
```

**`PageClient.tsx` (Client Component):**
```tsx
'use client';
import { use } from 'react';

export default function FeaturePage({ params }: { params: Promise<{ grupoId: string }> }) {
  const { grupoId } = use(params);
  // ... lógica normal con hooks, Auth0, useEffect, etc.
}
```

**Por qué funciona en runtime:** ASP.NET Core sirve `index.html` como SPA fallback para todas las rutas no encontradas. El HTML del placeholder `/_` nunca llega al usuario — el router client-side de Next.js intercepta la navegación y `use(params)` resuelve el `grupoId` real de la URL del browser.

**El `<Suspense>` es obligatorio** porque Next.js 16 pre-renderiza SSG en build time e intenta resolver `useSearchParams()` (u otros hooks de cliente) sincrónicamente sin él, lanzando `"useSearchParams() should be wrapped in a suspense boundary"`.

**Reglas:**
- El array de `generateStaticParams` NUNCA puede ser `[]` con `output: export` — usar `[{ grupoId: '_' }]`.
- El `fallback={null}` en `<Suspense>` es intencional — evita flash de contenido durante hidratación.
- No agregar `'use client'` ni ningún import de React al `page.tsx` wrapper — debe permanecer Server Component.
- Si el `PageClient` usa `useSearchParams()`, también necesita estar envuelto en `<Suspense>` en su propio render o heredarlo del wrapper.
