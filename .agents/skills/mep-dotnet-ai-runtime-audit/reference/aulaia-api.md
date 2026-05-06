# Referencia: mep-dotnet-ai-runtime-audit en AulaIA.Api

Implementación verificada del skill en **AulaIA.Api** (ASP.NET Core WebApplication, .NET 10, patrón estándar).

## Archivos

| Archivo | Namespace | Descripción |
|---|---|---|
| `AulaIA.Api/Options/LlmAuditOptions.cs` | `AulaIA.Api.Options` | Options `sealed` con `init` properties |
| `AulaIA.Api/Services/LlmAuditService.cs` | `AulaIA.Api.Services` | `ILlmAuditService` + `LlmAuditService` (Singleton) |
| `AulaIA.Api/Extensions/LlmAuditExtensions.cs` | `AulaIA.Api.Extensions` | `AddLlmAuditServices()` + `MapLlmDiagEndpoints()` con C# 14 extension blocks |

## Configuración

**appsettings.json** (producción — desactivado):
```json
"LlmAudit": {
  "Enabled": false,
  "LogPath": "logs/llm-audit.md"
}
```

**appsettings.Development.json** (dev — activado):
```json
"LlmAudit": {
  "Enabled": true,
  "LogPath": "logs/llm-audit.md"
}
```

> **Ruta física**: `LogPath` es relativa al directorio de trabajo del proceso. Con `dotnet run` desde `src/AulaIA.Api/`, el archivo queda en **`src/AulaIA.Api/logs/llm-audit.md`**.  
> Para confirmar en runtime: `GET /api/diag/context` devuelve `llmAudit.logPath` como ruta absoluta.

## Registro DI — Program.cs

```csharp
// En el bloque de builder (antes de builder.Build()):
builder.AddLlmAuditServices();

// En el bloque de app, dentro del guard IsDevelopment():
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(CorsExtensions.DevPolicy);
    app.MapLlmDiagEndpoints();   // ← endpoints diag
}
```

**LogStartup post-Build** — después de `builder.Build()`, antes de `app.Run()`:

```csharp
var audit = app.Services.GetRequiredService<ILlmAuditService>();
audit.LogStartup("AulaIA.Api", [
    $"Framework: {RuntimeInformation.FrameworkDescription}",
    $"Environment: {app.Environment.EnvironmentName}",
    $"Auth0 Domain: {app.Configuration["Auth0:Domain"]}",
    $"ConnectionString activa: DefaultConnection",
    $"Módulos registrados: Grupos, Estudiantes, Asistencia, Notas, Planeamiento, Curriculum, Reportes, PowerSync",
    $"Diag endpoints: GET /diag/audit, GET /diag/context, DELETE /diag/audit, POST /diag/audit-event"
]);
```

## Endpoints diag

Puerto local: **http://localhost:8000** (perfil `http` en `launchSettings.json`)

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/diag/audit` | Retorna `logs/llm-audit.md` como `text/markdown` |
| DELETE | `/diag/audit` | Limpia el log |
| GET | `/diag/context` | Snapshot JSON: environment, llmAudit config, endpoints, paths |
| POST | `/diag/audit-event` | Recibe eventos desde aulaia-web (Next.js) o aulaia-app |
| OPTIONS | `/diag/audit-event` | Preflight CORS para clientes externos |

Todos solo se registran si `app.Environment.IsDevelopment()`.

## LlmAuditExtensions.cs — patrón C# 14

```csharp
public static class LlmAuditExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddLlmAuditServices()
        {
            builder.Services.AddOptions<LlmAuditOptions>()
                .BindConfiguration(LlmAuditOptions.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddSingleton<ILlmAuditService, LlmAuditService>();
            return builder;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication MapLlmDiagEndpoints()
        {
            if (!app.Environment.IsDevelopment()) return app;

            var group = app.MapGroup("/diag").AllowAnonymous();

            group.MapGet("/audit", (ILlmAuditService audit) =>
                Results.Text(File.Exists(audit.LogPath)
                    ? File.ReadAllText(audit.LogPath)
                    : "# (empty)", "text/markdown"));

            group.MapDelete("/audit", (ILlmAuditService audit) =>
            {
                audit.Clear();
                return Results.NoContent();
            });

            group.MapGet("/context", (IOptions<LlmAuditOptions> opts, IWebHostEnvironment env) =>
                Results.Ok(new
                {
                    environment = env.EnvironmentName,
                    timestamp = DateTimeOffset.UtcNow,
                    llmAudit = new
                    {
                        opts.Value.Enabled,
                        logPath = Path.GetFullPath(opts.Value.LogPath),
                        logFileExists = File.Exists(opts.Value.LogPath),
                        logFileSizeKb = File.Exists(opts.Value.LogPath)
                            ? Math.Round(new FileInfo(opts.Value.LogPath).Length / 1024.0, 1)
                            : 0
                    }
                }));

            group.MapPost("/audit-event", async (HttpRequest req, ILlmAuditService audit) =>
            {
                var dto = await req.ReadFromJsonAsync<AngularAuditEventDto>();
                if (dto is null) return Results.BadRequest();
                switch (dto.Type)
                {
                    case "event":
                        audit.LogEvent(dto.Category ?? "Web", dto.Intent ?? "", dto.Result ?? "");
                        break;
                    case "decision":
                        audit.LogDecision(dto.Area ?? dto.Category ?? "Web", dto.Decision ?? "", dto.Rationale ?? "");
                        break;
                    case "error":
                        audit.LogError(dto.Category ?? "Web", dto.Message ?? "", dto.Stack is not null ? new Exception(dto.Stack) : null);
                        break;
                    default:
                        audit.LogEvent("[Client] raw", dto.Type ?? "unknown", System.Text.Json.JsonSerializer.Serialize(dto));
                        break;
                }
                return Results.NoContent();
            });

            return app;
        }
    }
}

internal sealed record AngularAuditEventDto(
    string? Type, string? Category, string? Area, string? Intent,
    string? Result, string? Decision, string? Rationale,
    string? Message, string? Stack, object? Context);
```

## GET /diag/context — campos del snapshot

```json
{
  "environment": "Development",
  "timestamp": "2026-05-06T10:30:00Z",
  "llmAudit": {
    "enabled": true,
    "logPath": "/abs/path/src/AulaIA.Api/logs/llm-audit.md",
    "logFileExists": true,
    "logFileSizeKb": 3.8
  }
}
```

## Uso desde el LLM

```bash
# Leer el audit log
curl http://localhost:8000/diag/audit

# Snapshot del estado
curl http://localhost:8000/diag/context | jq

# Limpiar para nueva sesión
curl -X DELETE http://localhost:8000/diag/audit
```

O leer el archivo directamente (sin servidor activo):

```
Lee el archivo src/AulaIA.Api/logs/llm-audit.md
```
