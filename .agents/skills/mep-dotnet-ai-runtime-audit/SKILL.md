---
name: mep-dotnet-ai-runtime-audit
description: >
  Patrón reutilizable para instrumentar cualquier app .NET + frontend con un sistema
  liviano de audit log legible por LLMs. El LLM lee datos DIRECTAMENTE desde el
  archivo llm-audit.md (dev local) o desde la tabla llm_audit_entries en PostgreSQL
  (contenedor remoto, activar con LlmAudit__PersistToDb=true).
  Los endpoints /diag/* son solo de ESCRITURA — nunca GET. El LLM usa los PS1 de
  examples/ para leer. Aplica a cualquier proyecto ASP.NET Core + Next.js / React Native.
  Triggers: debug con IA, instrumentar runtime, llm-audit, PersistToDb, audit log,
  runtime debug, diag endpoints, revisar logs en contenedor, background job debug.
---

# llm-audit-runtime — Instrumentación .NET en runtime para LLMs

Sistema liviano de observabilidad semántica para que un LLM analice el comportamiento
de una app .NET en ejecución. Escribe un archivo Markdown estructurado (y opcionalmente
una tabla en BD) que el LLM lee directamente — sin endpoints GET ni polling.

## Arquitectura

```
Tu API (.NET, cualquier puerto)
  ├── ILlmAuditService
  │     ├── escribe ──► <log-path>/llm-audit.md     (siempre, modo local)
  │     └── escribe ──► tabla llm_audit_entries      (si PersistToDb=true)
  │
  └── /diag/*  (solo Development — solo ESCRITURA)
        ├── DELETE /diag/audit       → limpia archivo + BD
        └── POST   /diag/audit-event → recibe eventos desde frontend

El LLM lee DIRECTAMENTE (nunca por endpoint GET):
  • Dev local  → archivo llm-audit.md      (examples/read-audit.ps1)
  • Contenedor → llm_audit_entries en BD   (examples/read-db-audit.ps1)
```

> Los endpoints `/diag/*` **solo existen en `Development`** (`IsDevelopment()`).  
> `LlmAudit:Enabled = false` en producción. `LlmAudit:PersistToDb = false` por defecto.

---

## Quick start — 3 pasos

### 1. Crear los 3 archivos C#

Ver código completo en [`reference/dotnet-implementation.md`](./reference/dotnet-implementation.md):
- `LlmAuditOptions.cs` — Options pattern
- `LlmAuditService.cs` — Interface + Singleton que escribe MD y/o BD
- `LlmAuditExtensions.cs` — `AddLlmAuditServices()` + `MapLlmDiagEndpoints()`

Si usas `PersistToDb`, también necesitas la entidad EF:
- `LlmAuditEntry.cs` — entidad + `DbSet<LlmAuditEntry>` en tu DbContext + migración

### 2. Registrar en Program.cs

```csharp
// builder section:
builder.AddLlmAuditServices();

// app section (dentro de IsDevelopment):
if (app.Environment.IsDevelopment())
    app.MapLlmDiagEndpoints();
```

### 3. Configurar `audit.config.ps1` en la raíz del repo

Copiar [`examples/audit.config.ps1.example`](./examples/audit.config.ps1.example) como `audit.config.ps1` y ajustar rutas/puerto. Los scripts PS1 lo cargan automáticamente.

---

## Configuración (appsettings)

```json
// appsettings.json — producción (desactivado)
"LlmAudit": {
  "Enabled": false,
  "LogPath": "logs/llm-audit.md",
  "MaxFileSizeKb": 2048,
  "PersistToDb": false,
  "MaxDbEntries": 5000
}
```

```json
// appsettings.Development.json — dev (activado)
"LlmAudit": {
  "Enabled": true,
  "LogPath": "logs/llm-audit.md"
}
```

Variable de entorno para contenedores remotos:
```
LlmAudit__Enabled=true
LlmAudit__PersistToDb=true
```

> ⚠️ **No dejar `PersistToDb=true` en producción permanentemente** — llenarás la BD con entradas de debug.

---

## ILlmAuditService — interfaz

```csharp
public interface ILlmAuditService
{
    string LogPath { get; }
    void LogStartup(string component, IEnumerable<string> facts);
    void LogEvent(string category, string intent, string result, object? context = null);
    void LogDecision(string area, string decision, string rationale);
    void LogError(string category, string message, Exception? ex = null);
    void Clear();
}
```

Inyectar como Singleton en constructor primary:

```csharp
public class MyService(ILlmAuditService audit)
{
    public void DoWork()
    {
        audit.LogEvent("MyService", "Procesando item", "✅ completado", new { id = 42 });
    }
}
```

---

## Convenciones de categorías

| Categoría | Cuándo usarla |
|-----------|---------------|
| `STARTUP` | Inicialización de servicios, módulos, conexiones |
| `DECISION` | Branching logic, feature flags, condicionales relevantes |
| `ERROR` | Excepciones capturadas, fallos de validación |
| `FLOW` | Pasos de un proceso multi-etapa |
| `INTEGRATION` | Llamadas a APIs externas, colas, webhooks |

---

## Scripts PowerShell

Los scripts leen su configuración desde `audit.config.ps1` en el directorio donde se ejecutan,
o aceptan parámetros explícitos. **No tienen rutas ni puertos hardcodeados.**

| Script | Propósito | Cuándo |
|--------|-----------|--------|
| [examples/read-audit.ps1](./examples/read-audit.ps1) | Lee `llm-audit.md` con colores; `-Filter`, `-Tail` | **Dev local** |
| [examples/read-db-audit.ps1](./examples/read-db-audit.ps1) | Consulta `llm_audit_entries` en BD; `-Filter`, `-Component`, `-Since`, `-Limit` | **Contenedor remoto** |
| [examples/watch-audit.ps1](./examples/watch-audit.ps1) | Tail en tiempo real del archivo MD | Dev, jobs en ejecución |
| [examples/diag.ps1](./examples/diag.ps1) | Solo escritura: `clear`, `event` | Limpiar, eventos manuales |
| [examples/truncate-db-audit.ps1](./examples/truncate-db-audit.ps1) | Limpia tabla BD; `-KeepLast`, `-OlderThan`, truncate total | Evitar basura en BD |
| [examples/debug-job.ps1](./examples/debug-job.ps1) | Protocolo: limpia audit → corre test → lee evidencia | Jobs fallidos |
| [examples/audit.config.ps1.example](./examples/audit.config.ps1.example) | **Plantilla de config por proyecto** | Copiar una vez a raíz del repo |

```powershell
# Desde la raíz del repo (donde está audit.config.ps1):

# Leer log local
pwsh .agents/skills/mep-dotnet-ai-runtime-audit/examples/read-audit.ps1
pwsh .agents/skills/mep-dotnet-ai-runtime-audit/examples/read-audit.ps1 -Filter ERROR

# Leer desde BD (contenedor remoto)
pwsh .agents/skills/mep-dotnet-ai-runtime-audit/examples/read-db-audit.ps1
pwsh .agents/skills/mep-dotnet-ai-runtime-audit/examples/read-db-audit.ps1 -Filter ERROR -Since "2026-06-07 10:00"

# Limpiar log
pwsh .agents/skills/mep-dotnet-ai-runtime-audit/examples/diag.ps1 clear

# Diagnosticar job fallido
pwsh .agents/skills/mep-dotnet-ai-runtime-audit/examples/debug-job.ps1 -TestName "MiTest_FlujoCompleto"
```

---

## Flujo LLM

### Dev local — leer el archivo directamente
```
Lee el archivo <log-path>/llm-audit.md y dime qué ocurrió durante el startup
y si hay errores en los últimos jobs ejecutados.
```

### Contenedor remoto — leer desde BD
1. Deploy con `LlmAudit__PersistToDb=true`
2. Reproducir el escenario
3. Ejecutar `read-db-audit.ps1` y pasar el resultado al LLM
4. Limpiar con `truncate-db-audit.ps1 -KeepLast 0` cuando termines

---

## Seguridad

- Los endpoints `/diag/*` solo se registran dentro de `if (app.Environment.IsDevelopment())`
- `LlmAudit:Enabled = false` por defecto en `appsettings.json`
- `logs/llm-audit.md` debe estar en `.gitignore`
- No loguear passwords, tokens, ni PII en el audit log

---

## Referencias de implementación

| Archivo | Contenido |
|---------|-----------|
| [`reference/dotnet-implementation.md`](./reference/dotnet-implementation.md) | Código C# completo: Options, Service, Extensions, Entity |
| [`reference/nextjs-implementation.md`](./reference/nextjs-implementation.md) | `llm-audit.ts` para Next.js + config de rewrites |
| [`reference/react-native-implementation.md`](./reference/react-native-implementation.md) | `llm-audit.ts` para Expo + detección de IP local |
| [`reference/testing-integration.md`](./reference/testing-integration.md) | Protocolo para tests con jobs en background |

