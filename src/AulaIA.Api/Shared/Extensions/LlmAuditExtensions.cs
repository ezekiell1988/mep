using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Services;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AulaIA.Api.Shared.Extensions;

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

            var group = app.MapGroup("/api/diag").AllowAnonymous();

            group.MapGet("/audit", (ILlmAuditService audit) =>
                Results.Text(
                    File.Exists(audit.LogPath) ? File.ReadAllText(audit.LogPath) : "# (empty)",
                    "text/markdown"));

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
                var dto = await req.ReadFromJsonAsync<AuditEventDto>();
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
                        audit.LogError(dto.Category ?? "Web", dto.Message ?? "",
                            dto.Stack is not null ? new Exception(dto.Stack) : null);
                        break;
                    default:
                        audit.LogEvent("[Client] raw", dto.Type ?? "unknown", JsonSerializer.Serialize(dto));
                        break;
                }

                return Results.NoContent();
            });

            return app;
        }
    }
}

internal sealed record AuditEventDto(
    string? Type, string? Category, string? Area, string? Intent,
    string? Result, string? Decision, string? Rationale,
    string? Message, string? Stack, object? Context);
