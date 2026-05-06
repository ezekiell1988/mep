namespace AulaIA.Api.Shared.Options;

public sealed class LlmAuditOptions
{
    public const string Section = "LlmAudit";

    public bool Enabled { get; init; } = false;
    public string LogPath { get; init; } = "logs/llm-audit.md";
    public int MaxFileSizeKb { get; init; } = 2048;
}
