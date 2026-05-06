---
name: mep-dotnet-ai-runtime-audit
description: "Instrumentación de apps del proyecto MEP (AulaIA) en runtime para que un LLM pueda observar su comportamiento real: startup facts, decisiones de configuración, flujo de endpoints y errores. Cubre: ILlmAuditService (escribe llm-audit.md), endpoints /diag/context y /diag/audit (solo Development), adaptado para AulaIA.Api (WebApplication estándar), aulaia-web (Next.js) y aulaia-app (React Native/Expo). También documenta la relación con AulaIA.Tests: qué tests puede correr un LLM autónomamente vs. cuáles requieren observabilidad del audit log para interpretar resultados. Triggers: 'quiero que el LLM vea los logs', 'analiza lo que pasó en startup', 'revisar comportamiento del servidor en runtime', 'debug con IA', 'instrumentar AulaIA para LLM', 'qué tests puede correr el LLM solo'."
---

# mep-dotnet-ai-runtime-audit — Instrumentación AulaIA en runtime para LLMs

Sistema liviano para que un LLM analice el comportamiento de la app AulaIA en tiempo de ejecución, sin ruido de logs tradicionales.

## Arquitectura

```
AulaIA.Api (runtime :8000)
  ├── ILlmAuditService  ──escribe──► logs/llm-audit.md   (Markdown semántico)
  ├── GET /diag/context ──retorna──► JSON snapshot del estado de la app
  ├── GET /diag/audit   ──retorna──► contenido de llm-audit.md
  ├── DELETE /diag/audit ─────────► limpia el archivo (nueva sesión)
  └── POST /diag/audit-event ─────► recibe eventos desde aulaia-web o aulaia-app
```

> Implementaciones de referencia: [`reference/aulaia-api.md`](reference/aulaia-api.md) · [`reference/aulaia-web.md`](reference/aulaia-web.md) · [`reference/aulaia-app.md`](reference/aulaia-app.md) · [`reference/aulaia-tests.md`](reference/aulaia-tests.md)

> Puerto API en dev: **http://localhost:8000**  
> Los endpoints `/diag/*` **solo existen en Development** (`IsDevelopment()`).  
> `LlmAudit.Enabled` es `false` en producción por defecto.

---

## Archivos del sistema

| Archivo | Propósito |
|---------|-----------|
| `Options/LlmAuditOptions.cs` | Configuración (ruta, habilitado, tamaño máximo) |
| `Services/LlmAuditService.cs` | Interface + implementación del writer |
| `Extensions/LlmAuditExtensions.cs` | `AddLlmAuditServices()` + `MapLlmDiagEndpoints()` |

---

## Configuración

**appsettings.json** (producción — desactivado):
```json
"LlmAudit": {
  "Enabled": false,
  "LogPath": "logs/llm-audit.md",
  "MaxFileSizeKb": 2048
}
```

**appsettings.Development.json** (dev — activado):
```json
"LlmAudit": {
  "Enabled": true,
  "LogPath": "logs/llm-audit.md"
}
```

> **Ruta física**: `LogPath` es relativa al CWD del proceso. Con `dotnet run` desde `src/AulaIA.Api/`, el archivo queda en **`src/AulaIA.Api/logs/llm-audit.md`**.  
> Para confirmar: `GET /api/diag/context` devuelve `llmAudit.logPath` como ruta absoluta.

---

## ILlmAuditService — API de escritura

```csharp
public interface ILlmAuditService
{
    // Hechos de startup: componente registrado, configuración cargada
    void LogStartup(string component, IEnumerable<string> facts);

    // Evento con intención y resultado
    void LogEvent(string category, string intent, string result, object? context = null);

    // Decisión de diseño / branching logic
    void LogDecision(string area, string decision, string rationale);

    // Error con contexto de excepción opcional
    void LogError(string category, string message, Exception? ex = null);

    // Limpia el archivo (nueva sesión de debug)
    void Clear();
}
```

### Inyección

El servicio es **Singleton**. Inyectar en constructor primary:

```csharp
public class MyService(ILlmAuditService audit)
{
    public void DoWork()
    {
        audit.LogEvent("MyService", "Procesando pedido", "✅ completado", new { grupoId = 42 });
    }
}
```

---

## Registro DI — Program.cs

```csharp
// En el bloque de builder:
builder.AddLlmAuditServices();

// En el bloque de app, dentro de IsDevelopment():
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(CorsExtensions.DevPolicy);
    app.MapLlmDiagEndpoints();   // ← agregar aquí
}
```

---

## Ejemplos de uso

### Startup de un módulo

```csharp
public static IServiceCollection AddGruposModule(this IServiceCollection services, IConfiguration configuration)
{
    // ... registro de servicios ...

    var audit = services.BuildServiceProvider().GetService<ILlmAuditService>();
    audit?.LogStartup("GruposModule", [
        $"Repositorio registrado: GruposRepository",
        $"Endpoint base: /grupos"
    ]);

    return services;
}
```

### Decisión de negocio

```csharp
if (grupo.EsActivo)
{
    audit.LogDecision(
        area: "Asistencia",
        decision: "Permitir toma de lista",
        rationale: "Grupo activo con estudiantes inscritos"
    );
}
```

### Error manejado

```csharp
catch (DbUpdateException ex)
{
    audit.LogError("Database", "Fallo al guardar asistencia", ex);
}
```

---

## Flujo LLM — cómo usarlo con GitHub Copilot

### Opción A: leer el archivo directamente

1. Correr `AulaIA.Api` en Development (`dotnet run --launch-profile http`)
2. Ejecutar los pasos que quieres auditar
3. Leer el archivo con el LLM:

```
Lee el archivo src/AulaIA.Api/logs/llm-audit.md y dime si el startup
cumplió con los requisitos del módulo de Grupos
```

### Opción B: llamar al endpoint /diag/audit

```bash
curl http://localhost:8000/diag/audit
```

El LLM recibe el Markdown y puede:
- Verificar cumplimiento vs. una spec
- Detectar errores o advertencias
- Sugerir cambios basados en lo que realmente ocurrió

### Opción C: snapshot completo con /diag/context

```bash
curl http://localhost:8000/diag/context | jq
```

Devuelve JSON con: environment, endpoints registrados, opciones de configuración, paths resueltos, si el SPA existe en disco, etc.

---

## Formato del llm-audit.md generado

```markdown
# LLM Audit Log — AulaIA.Api
Generated: 2026-05-06T10:30:00Z

---

## [STARTUP] AulaIA.Api — 2026-05-06T10:30:00Z
- Framework: .NET 10.0.0
- ConnectionString activa: DefaultConnection
- Auth0 configurado: true

## [DECISION] Asistencia — 2026-05-06T10:30:01Z
Decision: Permitir toma de lista
Rationale: Grupo activo con estudiantes inscritos

## [ERROR] Database — 2026-05-06T10:31:15Z
❌ Fallo al guardar asistencia
Exception: `DbUpdateException` — ...
```

---

## Convenciones de categorías

| Categoría | Cuándo usarla |
|-----------|---------------|
| `STARTUP` | Inicialización de servicios, módulos, conexiones |
| `DECISION` | Branching logic, feature flags, condicionales relevantes |
| `ERROR` | Excepciones capturadas, fallos de validación |
| `FLOW` | Pasos de un proceso multi-etapa |
| `INTEGRATION` | Llamadas a APIs externas, colas, webhooks, PowerSync |

---

## Seguridad

- `/diag/*` solo se mapea dentro de `if (app.Environment.IsDevelopment())`
- `LlmAudit.Enabled` está en `false` por defecto en `appsettings.json`
- El archivo `logs/llm-audit.md` debe estar en `.gitignore`
- No loguear datos sensibles (passwords, tokens Auth0, PII de estudiantes) en el audit log

---

## Integración con aulaia-web (Next.js)

aulaia-web ya tiene un proxy `/api/* → http://localhost:8000` configurado en `next.config.ts`. El `LlmAuditService` de Next.js hace `POST /api/diag/audit-event` y el proxy lo redirige al backend.

```
Browser (next dev :3000)
  └─ LlmAuditService.send()
       POST /api/diag/audit-event   ← relativa, Next.js intercepta
         └─ next.config.ts rewrites → http://localhost:8000/diag/audit-event
              └─ ILlmAuditService → logs/llm-audit.md
```

> Implementación detallada: [`reference/aulaia-web.md`](reference/aulaia-web.md)

---

## Integración con aulaia-app (React Native/Expo)

La app móvil accede directamente al endpoint sin proxy. En `__DEV__` mode puede apuntar al backend local.

```
Expo app (__DEV__)
  └─ LlmAuditService.send()
       POST http://{LOCAL_IP}:8000/diag/audit-event
         └─ ILlmAuditService → logs/llm-audit.md
```

> Implementación detallada: [`reference/aulaia-app.md`](reference/aulaia-app.md)

---

## Relación con AulaIA.Tests

`AulaIA.Tests` (xUnit) y `mep-dotnet-ai-runtime-audit` son complementarios. Tienen roles distintos:

| Herramienta | Rol | Puede un LLM correrlo autónomamente |
|-------------|-----|-------------------------------------|
| `AulaIA.Tests` — `SanityTests` | Verifica contratos HTTP básicos (200, 401, 404) | ✅ Sí — rápidos, deterministas |
| `AulaIA.Tests` — validaciones de body (400, schema) | Verifica que el servidor rechaza input inválido | ✅ Sí — sin dependencias externas |
| `AulaIA.Tests` — `FlujoCompleto_*` (Curriculum, Planeamiento) | Verifica el ciclo completo incluyendo Hangfire + GPT-5.5 | ⚠️ Solo si el job tiene audit instrumentation; si falla sin logs es ciego |
| `mep-dotnet-ai-runtime-audit` | Explica *por qué* falló un job en background | ✅ Sí — leer `llm-audit.md` o `GET /diag/audit` |

### Protocolo combinado para tests con jobs en background

Cuando un `FlujoCompleto_*` falla por timeout del polling:

1. **No relanzar el test** directamente — el log ya tiene la evidencia.
2. Leer `src/AulaIA.Api/logs/llm-audit.md` para buscar errores del job:
   ```
   Lee src/AulaIA.Api/logs/llm-audit.md — busca eventos [ERROR] o [EVENT] del job 
   que corrió entre las {hora_inicio} y {hora_fin} del test fallido
   ```
3. Si **no hay ningún evento del job** en el log → el job nunca fue instrumentado con `ILlmAuditService`. Agregar `LogEvent`/`LogError` al job antes de reintentar.
4. Si **hay un `[ERROR]`** → leer el mensaje + exception y corregir la causa raíz.

### Jobs que deben estar instrumentados (obligatorio)

Todo `BackgroundJob` que haga I/O externo debe tener al menos:

```csharp
// Al inicio del job
audit.LogEvent("NombreJob", "Iniciando", $"blobUrl={blobUrl}");

// Al completar
audit.LogEvent("NombreJob", "Completado", $"✅ {units.Count} unidades guardadas");

// En el catch
audit.LogError("NombreJob", "Falló la ejecución", ex);
```

Jobs críticos actuales:
- `ExtractCurriculumJob` — descarga PDF → GPT-5.5 → `curriculum_units` (**pendiente instrumentar**)
- `GenerarPlaneamientoJob` — curriculum validado → GPT-5.5 → `lesson_plans` (**pendiente instrumentar**)

> Referencia de integración con tests: [`reference/aulaia-tests.md`](reference/aulaia-tests.md)
