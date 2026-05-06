using AulaIA.Api.Shared.Options;
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

internal sealed class LlmAuditService(IOptions<LlmAuditOptions> options) : ILlmAuditService
{
    private readonly LlmAuditOptions _opts = options.Value;
    private readonly Lock _lock = new();

    public string LogPath => _opts.LogPath;

    private void Write(string block)
    {
        if (!_opts.Enabled) return;
        lock (_lock)
        {
            var dir = Path.GetDirectoryName(_opts.LogPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            // Escribir header si el archivo no existe
            if (!File.Exists(_opts.LogPath))
                File.WriteAllText(_opts.LogPath,
                    $"# LLM Audit Log — AulaIA.Api\nGenerated: {DateTimeOffset.UtcNow:O}\n\n---\n",
                    Encoding.UTF8);

            // Truncar si excede el tamaño máximo
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

    public void LogStartup(string component, IEnumerable<string> facts)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"\n## [STARTUP] {component} — {DateTimeOffset.UtcNow:O}");
        foreach (var fact in facts)
            sb.AppendLine($"- {fact}");
        Write(sb.ToString());
    }

    public void LogEvent(string category, string intent, string result, object? context = null)
    {
        var ctx = context is not null
            ? $"\nContext: `{JsonSerializer.Serialize(context)}`"
            : string.Empty;
        Write($"\n## [EVENT] {category} — {DateTimeOffset.UtcNow:O}\nIntent: {intent}\nResult: {result}{ctx}\n");
    }

    public void LogDecision(string area, string decision, string rationale)
        => Write($"\n## [DECISION] {area} — {DateTimeOffset.UtcNow:O}\nDecision: {decision}\nRationale: {rationale}\n");

    public void LogError(string category, string message, Exception? ex = null)
    {
        var exInfo = ex is not null
            ? $"\nException: `{ex.GetType().Name}` — {ex.Message}"
            : string.Empty;
        Write($"\n## [ERROR] {category} — {DateTimeOffset.UtcNow:O}\n❌ {message}{exInfo}\n");
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
