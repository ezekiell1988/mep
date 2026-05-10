# 09 — Patrones Verificados de Código

> **Última actualización:** 2026-05-08 (rev 2)
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

// ✅ Autenticar con connection string + decodificar AbsolutePath:
var blobUri = new Uri(blobUrl);
// Uri.AbsolutePath percent-encodea caracteres no-ASCII (acentos, etc.).
// El blob name es un path literal — se debe decodificar antes de usarlo.
var blobName = string.Join("/",
    Uri.UnescapeDataString(blobUri.AbsolutePath).TrimStart('/').Split('/').Skip(1));
var blobClient = new BlobClient(storageOpts.Value.ConnectionString, "curriculum", blobName);
using var stream = await blobClient.OpenReadAsync(cancellationToken: ct);
```

**Slug de blob sin acentos — `BlobSlugHelper.ToAsciiSlug()` (helper compartido):**
```csharp
// ✅ Usar BlobSlugHelper.ToAsciiSlug() en cualquier lugar que genere blob names
// Ubicación: AulaIA.Api/Shared/Extensions/BlobSlugHelper.cs
using AulaIA.Api.Shared.Extensions;

var blobName = $"{BlobSlugHelper.ToAsciiSlug(asignatura)}/{timestamp}.pdf";
// Resultado: "Educación Física" → "educacion-fisica"
//            "Artes Plásticas"  → "artes-plasticas"
```

**❌ NO usar nunca directamente:**
```csharp
// ❌ Preserva acentos → BlobNotFound en ExtractCurriculumJob (ISSUE-001, ISSUE-006)
asignatura.ToLowerInvariant().Replace(" ", "-")
```

**Regla crítica:** `Uri.AbsolutePath` **nunca** debe usarse directamente como blob name si el nombre puede tener caracteres no-ASCII. Siempre `Uri.UnescapeDataString(blobUri.AbsolutePath)` primero.

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

## PATTERN-06: AzureOpenAIClient como singleton en DI

**Contexto:** `AzureOpenAIClient` instanciado por llamada crea un `HttpClient` nuevo cada vez (sin pool). En Azure esto causa `NetworkTimeout` con PDFs grandes o respuestas largas del LLM. Registrar como singleton resuelve el problema y aumenta el timeout.

**Registro en DI (`OptionsExtensions.cs`):**
```csharp
services.AddSingleton(sp =>
{
    var ai = sp.GetRequiredService<IOptions<AiOptions>>().Value;
    var clientOptions = new AzureOpenAIClientOptions
    {
        NetworkTimeout = TimeSpan.FromMinutes(10)
    };
    return new AzureOpenAIClient(
        new Uri(ai.Endpoint),
        new ApiKeyCredential(ai.ApiKey ?? ""),
        clientOptions);
});
```

**Uso en job/servicio (inyección por constructor):**
```csharp
public sealed class ExtractCurriculumJob(
    AulaIADbContext db,
    AzureOpenAIClient aiClient,   // ← singleton inyectado
    IOptions<AiOptions> aiOpts,
    ...)
{
    // En el método que llama al LLM:
    var chatClient = aiClient.GetChatClient(aiOpts.Value.DeploymentName);
    var response = await chatClient.CompleteChatAsync(messages, options, ct);
}
```

**Reglas:**
- **Nunca** `new AzureOpenAIClient(...)` dentro de un método de job o servicio — usar DI siempre.
- El singleton comparte el `HttpClient` interno → connection pooling correcto.
- `NetworkTimeout = 10 min` cubre PDFs grandes (5+ MB) procesados por GPT-5.5 desde Azure.
- `AdecuacionAiService` y `PlaneamientoAiService` aún instancian el cliente por llamada (deuda técnica pendiente).

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

## PATTERN-06: Job Hangfire maestro con detección de cambios por ETag/Last-Modified

**Contexto:** Cuando se necesita sincronizar recursos externos (PDFs, archivos) y solo procesar si hubo cambio real, usar HEAD + comparación de ETag/Last-Modified contra BD antes de descargar el recurso completo. El job maestro orquesta y delega el procesamiento a jobs hijos.

**Ejemplo:** `SyncCurriculumJob` → detecta si un PDF del MEP cambió → encola `ExtractCurriculumJob`

**Entidad de estado por fuente:**
```csharp
public sealed class CurriculumSource
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Asignatura { get; set; }
    public required string Ciclo { get; set; }
    public required string MepUrl { get; set; }
    public string? LastEtag { get; set; }
    public DateTimeOffset? LastModifiedMep { get; set; }
    public DateTimeOffset? LastSyncedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

**Configuración EF — índice único por (asignatura, ciclo):**
```csharp
builder.HasIndex(x => new { x.Asignatura, x.Ciclo }).IsUnique();
```

**Job maestro — patrón de siembra + detección + delegación:**
```csharp
// 1. Sembrar catálogo si tabla vacía (auto-seed inicial)
if (!await db.CurriculumSources.AnyAsync(ct))
{
    foreach (var (asignatura, ciclo, url) in CatalogoInicial)
        db.CurriculumSources.Add(new CurriculumSource { Asignatura = asignatura, Ciclo = ciclo, MepUrl = url });
    await db.SaveChangesAsync(ct);
}

// 2. HEAD → detectar cambio
var (etag, lastModified) = await GetMepHeadAsync(http, fuente.MepUrl, ct);
var hayNuevaVersion = fuente.LastSyncedAt is null          // primera vez
    || (etag is not null && etag != fuente.LastEtag)       // ETag cambió
    || (lastModified.HasValue && lastModified > fuente.LastModifiedMep); // más nuevo

if (!hayNuevaVersion) { fuente.LastSyncedAt = DateTimeOffset.UtcNow; continue; }

// 3. Descargar + subir a Blob
var blobUrl = await DownloadAndUploadAsync(http, fuente, ct);

// 4. Encolar job hijo de procesamiento
var jobId = backgroundJobs.Enqueue<ExtractCurriculumJob>("curriculum",
    j => j.ExecuteAsync(blobUrl, fuente.Asignatura, fuente.Ciclo, CancellationToken.None));

// 5. Actualizar estado en BD
fuente.LastEtag        = etag;
fuente.LastModifiedMep = lastModified;
fuente.LastSyncedAt    = DateTimeOffset.UtcNow;
await db.SaveChangesAsync(ct);
```

**HttpClient con resilencia obligatoria (dotnet-10-csharp-14):**
```csharp
services.AddHttpClient("nombre", c => { c.BaseAddress = new Uri("https://..."); })
    .AddStandardResilienceHandler(opts =>
    {
        opts.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(120); // ajustar para archivos grandes
        opts.AttemptTimeout.Timeout      = TimeSpan.FromSeconds(90);
        opts.Retry.MaxRetryAttempts      = 2;
    });
```

**Reglas:**
- HEAD antes de GET siempre — evita descargar archivos que no cambiaron.
- Si el servidor no devuelve ETag ni Last-Modified, tratar como "nueva versión" solo la primera vez; las siguientes se saltan (no descargar ciegamente en cada ejecución).
- Errores por fuente se capturan individualmente — no abortar las demás fuentes si una falla.
- Si al final hubo errores, lanzar excepción para que Hangfire marque el job maestro como fallido y aparezca en el dashboard.
- Schedule imposible (`"0 0 30 2 *"` = 30 de febrero) para jobs que deben ejecutarse solo manualmente desde el dashboard de Hangfire.
- Paquete requerido: `Microsoft.Extensions.Http.Resilience` (no incluido en `Microsoft.Extensions.Http` base).

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

---

## PATTERN-07: Limpieza de `Program.cs` con C# 14 Extension Blocks

**Contexto:** En un monolito modular con muchos feature modules, `Program.cs` crece y se convierte en un archivo de 100+ líneas difícil de mantener. Extraer los bloques de registro/endpoints/jobs/migrate a extension blocks de C# 14 mantiene `Program.cs` en ~50 líneas y mejora la trazabilidad.

**Destino:** `Shared/Extensions/ModulesExtensions.cs`

**Patrón de extension blocks C# 14:**
```csharp
public static class ModulesExtensions
{
    extension(IServiceCollection services)
    {
        public void AddAulaIAModules() =>
            services
                .AddGruposModule()
                .AddEstudiantesModule()
                // ... todos los módulos
                .AddSuscripcionesModule();
    }

    extension(WebApplication app)
    {
        public void MapAulaIAEndpoints() =>
            app.MapGruposEndpoints()
               .MapEstudiantesEndpoints()
               // ... todos los endpoints
               .MapReferralsEndpoints();

        public void AddAulaIARecurringJobs()
        {
            var manager = app.Services.GetRequiredService<IRecurringJobManager>();
            manager.AddOrUpdate<UpdateExchangeRateJob>(...); // ← desde DI, no estático
            // ...
        }

        public async Task RunMigrationsAsync()
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AulaIADbContext>();
            await db.Database.MigrateAsync();
        }
    }
}
```

**`Program.cs` resultante (47 líneas):**
```csharp
using AulaIA.Api.Shared.Extensions;  // ← único using necesario

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAulaIAOptions();
builder.AddAulaIAPersistence();
builder.AddAulaIAAuth();
builder.AddAulaIACors();
builder.AddAulaIAHangfire();
builder.Services.AddOpenApi();
builder.AddLlmAuditServices();
builder.Services.AddAulaIAModules();  // ← todos los módulos en una línea

var app = builder.Build();

// Middleware (orden obligatorio)...
app.MapAulaIAEndpoints();             // ← todos los endpoints en una línea
app.MapFallbackToFile("index.html").ExcludeFromDescription();

await app.RunMigrationsAsync();       // ← migrate antes de Run()
app.AddAulaIARecurringJobs();
app.LogStartupFacts();
app.Run();
```

**Reglas:**
- Los extension blocks van en `Shared/Extensions/ModulesExtensions.cs`, nunca en `Program.cs`.
- `RunMigrationsAsync()` se llama con `await` **antes** de `app.Run()` — no dentro del pipeline de middlewares.
- `AddAulaIARecurringJobs()` se llama después de `RunMigrationsAsync()` — Hangfire necesita las tablas ya creadas.
- Usar `extension(IServiceCollection)` para DI y `extension(WebApplication)` para pipeline y runtime.

---

## PATTERN-08: Hangfire — Recurring Jobs + Console Logs

**Contexto:** Dos errores comunes al configurar recurring jobs en Hangfire con PostgreSQL: (1) usar `RecurringJob.AddOrUpdate` estático que depende de `JobStorage.Current` (puede no estar disponible en startup), y (2) no tener visibilidad de lo que hace el job en el dashboard. Este patrón cubre ambos.

### Registro de recurring jobs — siempre con `IRecurringJobManager` desde DI

```csharp
// ❌ NO: RecurringJob.AddOrUpdate estático — falla silenciosamente si JobStorage.Current
//        no está inicializado cuando se llama desde AddAulaIARecurringJobs()
RecurringJob.AddOrUpdate<MiJob>("mi-job", j => j.ExecuteAsync(CancellationToken.None), "0 12 * * *");

// ✅ SÍ: IRecurringJobManager desde el contenedor DI — siempre disponible post-Build()
public void AddAulaIARecurringJobs()
{
    var manager = app.Services.GetRequiredService<IRecurringJobManager>();
    manager.AddOrUpdate<UpdateExchangeRateJob>(
        "update-exchange-rate", j => j.ExecuteAsync(CancellationToken.None), "0 12 * * *");
    manager.AddOrUpdate<CheckExpiredSubscriptionsJob>(
        "check-expired-subscriptions", j => j.ExecuteAsync(CancellationToken.None), "0 8 * * *");
    manager.AddOrUpdate<SyncCurriculumJob>(
        "sync-curriculum-mep", j => j.ExecuteAsync(null, CancellationToken.None), "0 0 30 2 *");
}
```

### Logs en tiempo real en el dashboard — `Hangfire.Console`

**Paquete:** `Hangfire.Console` 1.4.2

**Registro en `AddHangfire()`:**
```csharp
builder.Services.AddHangfire(config => config
    .UseConsole()                                             // ← agregar
    .UseFilter(new AutomaticRetryAttribute { Attempts = 0 }) // retries globales = 0
    .UsePostgreSqlStorage(...));
```

**En el job — `PerformContext` como parámetro opcional al final:**
```csharp
using Hangfire.Console;
using Hangfire.Server;

[Queue("curriculum")]
[AutomaticRetry(Attempts = 1)] // override del global
public async Task ExecuteAsync(string blobUrl, string asignatura, string ciclo,
    PerformContext? ctx = null, CancellationToken ct = default)
{
    ctx.WriteLine($"🚀 ExtractCurriculumJob: {asignatura} ({ciclo})");
    // ... trabajo ...
    ctx.WriteLine($"✅ {count} unidades guardadas ({tokensUsed:N0} tokens)");
}
```

**Al encolar desde código — pasar `null` explícito para `PerformContext`:**
```csharp
// Hangfire inyecta el PerformContext real en runtime; pasar null al encolar
jobs.Enqueue<ExtractCurriculumJob>("curriculum",
    j => j.ExecuteAsync(blobUrl, asignatura, ciclo, null, CancellationToken.None));

// Recurring jobs — igual: null para PerformContext
manager.AddOrUpdate<SyncCurriculumJob>(
    "sync-curriculum-mep", j => j.ExecuteAsync(null, CancellationToken.None), "0 0 30 2 *");
```

**Reglas:**
- `PerformContext? ctx = null` **siempre al final** de la firma, antes de `CancellationToken` — o después si CT ya es el último.
- `ctx.WriteLine()` es extensión de `Hangfire.Console`; funciona aunque `ctx` sea `null` (el método es null-safe).
- Los retries globales se configuran con `.UseFilter(new AutomaticRetryAttribute { Attempts = 0 })` en `AddHangfire()`. Los atributos `[AutomaticRetry]` por clase ganan al global.
- Dashboard en dev: `LocalRequestsOnlyAuthorizationFilter` (sin JWT). En prod: `HangfireAdminAuthFilter` con JWT.
- Si el dashboard muestra 0 recurring jobs pero la BD tiene datos en `hangfire.hash`: ver skill `hangfire-reset`.

