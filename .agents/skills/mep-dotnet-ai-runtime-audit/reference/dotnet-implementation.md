# Implementación C# — llm-audit-runtime

Código completo para implementar el sistema en cualquier proyecto **ASP.NET Core (.NET 8+)**.
Adaptar namespaces y nombres de proyecto según corresponda.

---

## LlmAuditOptions.cs

```csharp
namespace MyApp.Shared.Options;   // ← adaptar namespace

public sealed class LlmAuditOptions
{
    public const string Section = "LlmAudit";
    public bool Enabled { get; init; } = false;
    public string LogPath { get; init; } = "logs/llm-audit.md";
    public int MaxFileSizeKb { get; init; } = 2048;
    /// Activar con env var LlmAudit__PersistToDb=true para contenedores remotos.
    public bool PersistToDb { get; init; } = false;
    public int MaxDbEntries { get; init; } = 5000;
}
```

---

## LlmAuditEntry.cs (solo si usas PersistToDb)

```csharp
namespace MyApp.Shared.Domain;

public sealed class LlmAuditEntry
{
    public long Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public string Category { get; init; } = "";   // STARTUP|EVENT|DECISION|ERROR|FLOW|INTEGRATION
    public string Component { get; init; } = "";
    public string Intent { get; init; } = "";
    public string Result { get; init; } = "";
    public string? ContextJson { get; init; }
    public bool IsError { get; init; }
}
```

Agregar al DbContext y crear migración:
```csharp
public DbSet<LlmAuditEntry> LlmAuditEntries { get; set; }
```
```bash
dotnet ef migrations add AddLlmAuditEntries && dotnet ef database update
```

---

## LlmAuditService.cs

```csharp
using MyApp.Shared.Domain;
using MyApp.Shared.Options;
using MyApp.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace MyApp.Shared.Services;

public interface ILlmAuditService
{
    string LogPath { get; }
    void LogStartup(string component, IEnumerable<string> facts);
    void LogEvent(string category, string intent, string result, object? context = null);
    void LogDecision(string area, string decision, string rationale);
    void LogError(string category, string message, Exception? ex = null);
    void Clear();
}

internal sealed class LlmAuditService(
    IOptions<LlmAuditOptions> options,
    IServiceScopeFactory scopeFactory) : ILlmAuditService
{
    private readonly LlmAuditOptions _opts = options.Value;
    private readonly Lock _lock = new();

    public string LogPath => _opts.LogPath;

    private void WriteFile(string block)
    {
        if (!_opts.Enabled) return;
        lock (_lock)
        {
            var dir = Path.GetDirectoryName(_opts.LogPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            if (!File.Exists(_opts.LogPath))
                File.WriteAllText(_opts.LogPath,
                    $"# LLM Audit Log\nGenerated: {DateTimeOffset.UtcNow:O}\n\n---\n", Encoding.UTF8);

            if (_opts.MaxFileSizeKb > 0 && new FileInfo(_opts.LogPath).Length > _opts.MaxFileSizeKb * 1024L)
                File.WriteAllText(_opts.LogPath,
                    $"# LLM Audit Log (truncado)\nGenerated: {DateTimeOffset.UtcNow:O}\n\n---\n", Encoding.UTF8);

            File.AppendAllText(_opts.LogPath, block, Encoding.UTF8);
        }
    }

    private void WriteDb(LlmAuditEntry entry)
    {
        if (!_opts.Enabled || !_opts.PersistToDb) return;
        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<MyAppDbContext>(); // ← tu DbContext
                db.LlmAuditEntries.Add(entry);
                await db.SaveChangesAsync();

                if (_opts.MaxDbEntries > 0)
                {
                    var count = await db.LlmAuditEntries.CountAsync();
                    if (count > _opts.MaxDbEntries)
                    {
                        var excess = count - _opts.MaxDbEntries;
                        var oldest = await db.LlmAuditEntries
                            .OrderBy(e => e.Id).Take(excess).Select(e => e.Id).ToListAsync();
                        await db.LlmAuditEntries.Where(e => oldest.Contains(e.Id)).ExecuteDeleteAsync();
                    }
                }
            }
            catch { /* No romper la app por fallos de audit */ }
        });
    }

    public void LogStartup(string component, IEnumerable<string> facts)
    {
        var factList = facts.ToList();
        var sb = new StringBuilder();
        sb.AppendLine($"\n## [STARTUP] {component} — {DateTimeOffset.UtcNow:O}");
        foreach (var f in factList) sb.AppendLine($"- {f}");
        WriteFile(sb.ToString());
        WriteDb(new LlmAuditEntry { Category = "STARTUP", Component = component,
            Intent = "Inicialización", Result = string.Join(" | ", factList), CreatedAt = DateTimeOffset.UtcNow });
    }

    public void LogEvent(string category, string intent, string result, object? context = null)
    {
        var ctxJson = context is not null ? JsonSerializer.Serialize(context) : null;
        WriteFile($"\n## [EVENT] {category} — {DateTimeOffset.UtcNow:O}\nIntent: {intent}\nResult: {result}"
                + (ctxJson is not null ? $"\nContext: `{ctxJson}`" : "") + "\n");
        WriteDb(new LlmAuditEntry { Category = "EVENT", Component = category,
            Intent = intent, Result = result, ContextJson = ctxJson, CreatedAt = DateTimeOffset.UtcNow });
    }

    public void LogDecision(string area, string decision, string rationale)
    {
        WriteFile($"\n## [DECISION] {area} — {DateTimeOffset.UtcNow:O}\nDecision: {decision}\nRationale: {rationale}\n");
        WriteDb(new LlmAuditEntry { Category = "DECISION", Component = area,
            Intent = decision, Result = rationale, CreatedAt = DateTimeOffset.UtcNow });
    }

    public void LogError(string category, string message, Exception? ex = null)
    {
        var exInfo = ex is not null ? $"\nException: `{ex.GetType().Name}` — {ex.Message}" : string.Empty;
        WriteFile($"\n## [ERROR] {category} — {DateTimeOffset.UtcNow:O}\n❌ {message}{exInfo}\n");
        WriteDb(new LlmAuditEntry { Category = "ERROR", Component = category,
            Intent = message, Result = ex?.ToString() ?? message, IsError = true, CreatedAt = DateTimeOffset.UtcNow });
    }

    public void Clear()
    {
        lock (_lock)
        {
            var dir = Path.GetDirectoryName(_opts.LogPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(_opts.LogPath,
                $"# LLM Audit Log\nGenerated: {DateTimeOffset.UtcNow:O}\n\n---\n", Encoding.UTF8);
        }
    }
}
```

---

## LlmAuditExtensions.cs (C# 14 extension blocks)

```csharp
using MyApp.Shared.Options;
using MyApp.Shared.Services;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace MyApp.Shared.Extensions;

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

            // Solo ESCRITURA — el LLM lee directamente desde archivo o BD.
            var group = app.MapGroup("/api/diag").AllowAnonymous();

            group.MapDelete("/audit", (ILlmAuditService audit) =>
            {
                audit.Clear();
                return Results.NoContent();
            });

            group.MapPost("/audit-event", async (HttpRequest req, ILlmAuditService audit) =>
            {
                var dto = await req.ReadFromJsonAsync<AuditEventDto>();
                if (dto is null) return Results.BadRequest();
                switch (dto.Type)
                {
                    case "event":    audit.LogEvent(dto.Category ?? "Client", dto.Intent ?? "", dto.Result ?? ""); break;
                    case "decision": audit.LogDecision(dto.Area ?? "Client", dto.Decision ?? "", dto.Rationale ?? ""); break;
                    case "error":    audit.LogError(dto.Category ?? "Client", dto.Message ?? "",
                                        dto.Stack is not null ? new Exception(dto.Stack) : null); break;
                    default:         audit.LogEvent("[Client] raw", dto.Type ?? "unknown", JsonSerializer.Serialize(dto)); break;
                }
                return Results.NoContent();
            });

            return app;
        }

        public WebApplication LogStartupFacts(params string[] extraFacts)
        {
            var audit = app.Services.GetRequiredService<ILlmAuditService>();
            var facts = new List<string>
            {
                $"Framework: {RuntimeInformation.FrameworkDescription}",
                $"Environment: {app.Environment.EnvironmentName}",
                $"Diag (solo escritura): DELETE /api/diag/audit, POST /api/diag/audit-event"
            };
            facts.AddRange(extraFacts);
            audit.LogStartup("App", facts);
            return app;
        }
    }
}

internal sealed record AuditEventDto(
    string? Type, string? Category, string? Area, string? Intent,
    string? Result, string? Decision, string? Rationale,
    string? Message, string? Stack, object? Context);
```

---

## Patrón para background jobs

```csharp
public class MyBackgroundJob(ILlmAuditService audit, ...)
{
    public async Task ExecuteAsync(string param, CancellationToken ct)
    {
        audit.LogEvent("MyBackgroundJob", "Iniciando", $"param={param}");
        try
        {
            // ... trabajo ...
            audit.LogEvent("MyBackgroundJob", "Completado", $"✅ {count} items");
        }
        catch (Exception ex)
        {
            audit.LogError("MyBackgroundJob", "Falló la ejecución", ex);
            throw;
        }
    }
}
```

> **Regla**: todo job con I/O externo (HTTP, BD, blob) debe tener LogEvent al inicio,
> al completar, y LogError en el catch. Sin esto el LLM no puede diagnosticar fallos remotos.
