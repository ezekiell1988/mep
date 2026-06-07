using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using AulaIA.Api.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
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

            // Solo endpoints de ESCRITURA — no GET de datos.
            // El LLM lee directamente desde el archivo MD o la BD (psql / PS1).
            var group = app.MapGroup("/api/diag").AllowAnonymous();

            // ── Limpiar audit log (archivo + BD si PersistToDb) ─────────────
            group.MapDelete("/audit", (ILlmAuditService audit) =>
            {
                audit.Clear();
                return Results.NoContent();
            });

            // ── Agregar evento desde frontend (Next.js / React Native) ──────
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

            // ── Patch auth0Sub — solo dev, une BD con el sub real del token ─
            group.MapPatch("/user-fix-sub", async (string email, string newSub, AulaIADbContext db, ILlmAuditService audit) =>
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user is null) return Results.NotFound($"No existe usuario con email={email}");

                var oldSub = user.Auth0Sub;
                user.Auth0Sub = newSub;
                await db.SaveChangesAsync();

                audit.LogEvent("UserFixSub", $"Auth0Sub actualizado para {email}",
                    $"old={oldSub} → new={newSub}");

                return Results.Ok(new { email, oldSub, newSub });
            });

            return app;
        }

        public WebApplication LogStartupFacts()
        {
            var audit = app.Services.GetRequiredService<ILlmAuditService>();
            audit.LogStartup("AulaIA.Api", [
                $"Framework: {RuntimeInformation.FrameworkDescription}",
                $"Environment: {app.Environment.EnvironmentName}",
                $"Auth0 Authority: {app.Configuration["Auth:Authority"]}",
                $"Auth0 Audience: {app.Configuration["Auth:Audience"]}",
                $"Módulos registrados: Grupos, Estudiantes, Asistencia, Notas, Planeamiento, Curriculum, Reportes, PowerSync",
                $"Diag endpoints (solo escritura): DELETE /api/diag/audit, POST /api/diag/audit-event, PATCH /api/diag/user-fix-sub",
                $"LLM lee datos desde: archivo MD ({app.Configuration["LlmAudit:LogPath"] ?? "logs/llm-audit.md"}) o tabla llm_audit_entries (si PersistToDb=true)"
            ]);
            return app;
        }
    }
}

internal sealed record AuditEventDto(
    string? Type, string? Category, string? Area, string? Intent,
    string? Result, string? Decision, string? Rationale,
    string? Message, string? Stack, object? Context);
