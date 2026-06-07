using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace AulaIA.Api.Shared.Services;

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

    // ── Escritura en archivo markdown ─────────────────────────────────────
    private void WriteFile(string block)
    {
        if (!_opts.Enabled) return;
        lock (_lock)
        {
            var dir = Path.GetDirectoryName(_opts.LogPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            if (!File.Exists(_opts.LogPath))
                File.WriteAllText(_opts.LogPath,
                    $"# LLM Audit Log — AulaIA.Api\nGenerated: {DateTimeOffset.UtcNow:O}\n\n---\n",
                    Encoding.UTF8);

            if (_opts.MaxFileSizeKb > 0)
            {
                var info = new FileInfo(_opts.LogPath);
                if (info.Length > _opts.MaxFileSizeKb * 1024L)
                    File.WriteAllText(_opts.LogPath,
                        $"# LLM Audit Log — AulaIA.Api (truncado)\nGenerated: {DateTimeOffset.UtcNow:O}\n\n---\n",
                        Encoding.UTF8);
            }

            File.AppendAllText(_opts.LogPath, block, Encoding.UTF8);
        }
    }

    // ── Escritura en PostgreSQL (fire-and-forget) ──────────────────────────
    private void WriteDb(LlmAuditEntry entry)
    {
        if (!_opts.Enabled || !_opts.PersistToDb) return;

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AulaIADbContext>();

                db.LlmAuditEntries.Add(entry);
                await db.SaveChangesAsync();

                // Trim si supera MaxDbEntries
                if (_opts.MaxDbEntries > 0)
                {
                    var count = await db.LlmAuditEntries.CountAsync();
                    if (count > _opts.MaxDbEntries)
                    {
                        var excess = count - _opts.MaxDbEntries;
                        var oldest = await db.LlmAuditEntries
                            .OrderBy(e => e.Id)
                            .Take(excess)
                            .Select(e => e.Id)
                            .ToListAsync();
                        await db.LlmAuditEntries
                            .Where(e => oldest.Contains(e.Id))
                            .ExecuteDeleteAsync();
                    }
                }
            }
            catch
            {
                // Ignorar fallos — el audit no debe romper la app
            }
        });
    }

    // ── API pública ────────────────────────────────────────────────────────
    public void LogStartup(string component, IEnumerable<string> facts)
    {
        var factList = facts.ToList();
        var sb = new StringBuilder();
        sb.AppendLine($"\n## [STARTUP] {component} — {DateTimeOffset.UtcNow:O}");
        foreach (var fact in factList)
            sb.AppendLine($"- {fact}");
        WriteFile(sb.ToString());
        WriteDb(new LlmAuditEntry
        {
            Category  = "STARTUP",
            Component = component,
            Intent    = "Inicialización",
            Result    = string.Join(" | ", factList),
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    public void LogEvent(string category, string intent, string result, object? context = null)
    {
        var ctxJson = context is not null ? JsonSerializer.Serialize(context) : null;
        var ctxStr  = ctxJson is not null ? $"\nContext: `{ctxJson}`" : string.Empty;
        WriteFile($"\n## [EVENT] {category} — {DateTimeOffset.UtcNow:O}\nIntent: {intent}\nResult: {result}{ctxStr}\n");
        WriteDb(new LlmAuditEntry
        {
            Category    = "EVENT",
            Component   = category,
            Intent      = intent,
            Result      = result,
            ContextJson = ctxJson,
            CreatedAt   = DateTimeOffset.UtcNow
        });
    }

    public void LogDecision(string area, string decision, string rationale)
    {
        WriteFile($"\n## [DECISION] {area} — {DateTimeOffset.UtcNow:O}\nDecision: {decision}\nRationale: {rationale}\n");
        WriteDb(new LlmAuditEntry
        {
            Category  = "DECISION",
            Component = area,
            Intent    = decision,
            Result    = rationale,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    public void LogError(string category, string message, Exception? ex = null)
    {
        var exInfo = ex is not null
            ? $"\nException: `{ex.GetType().Name}` — {ex.Message}{BuildInnerChain(ex.InnerException)}"
            : string.Empty;
        WriteFile($"\n## [ERROR] {category} — {DateTimeOffset.UtcNow:O}\n❌ {message}{exInfo}\n");
        WriteDb(new LlmAuditEntry
        {
            Category  = "ERROR",
            Component = category,
            Intent    = message,
            Result    = ex?.ToString() ?? message,
            IsError   = true,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private static string BuildInnerChain(Exception? inner)
    {
        if (inner is null) return string.Empty;
        var sb = new StringBuilder();
        while (inner is not null)
        {
            sb.Append($"\n  → [{inner.GetType().Name}] {inner.Message}");
            inner = inner.InnerException;
        }
        return sb.ToString();
    }

    public void Clear()
    {
        lock (_lock)
        {
            var dir = Path.GetDirectoryName(_opts.LogPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(_opts.LogPath,
                $"# LLM Audit Log — AulaIA.Api\nGenerated: {DateTimeOffset.UtcNow:O}\n\n---\n",
                Encoding.UTF8);
        }
    }
}
