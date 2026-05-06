using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using AulaIA.Api.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;
using System.Security.Claims;
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

            // ── Auth0 test — requiere token válido ──────────────────────────
            group.MapGet("/auth-test", (HttpContext ctx, ILlmAuditService audit) =>
            {
                var principal = ctx.User;
                var sub = principal.FindFirstValue("sub")
                       ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? "(no sub)";
                var claims = principal.Claims
                    .Select(c => new { type = c.Type, value = c.Value })
                    .ToList();

                audit.LogEvent(
                    "Auth0",
                    "auth-test ejecutado",
                    $"✅ sub={sub} | total_claims={claims.Count} | roles=[{string.Join(",", principal.FindAll(ClaimTypes.Role).Select(c => c.Value))}]",
                    new { sub, claimCount = claims.Count });

                return Results.Ok(new { sub, claims });
            }).RequireAuthorization();

            // ── User lookup — busca el usuario en BD por email o sub ────────
            group.MapGet("/user-lookup", async (string? email, string? sub, AulaIADbContext db, ILlmAuditService audit) =>
            {
                var query = db.Users.AsNoTracking();
                if (!string.IsNullOrEmpty(email))
                    query = query.Where(u => u.Email == email);
                else if (!string.IsNullOrEmpty(sub))
                    query = query.Where(u => u.Auth0Sub == sub);
                else
                    return Results.BadRequest("Requiere ?email= o ?sub=");

                var users = await query
                    .Select(u => new { u.Id, u.Auth0Sub, u.Email, u.FullName, Role = u.Role.ToString() })
                    .ToListAsync();

                audit.LogEvent("UserLookup", $"Búsqueda por {(email is not null ? $"email={email}" : $"sub={sub}")}", $"{users.Count} resultado(s)");

                return Results.Ok(users);
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
                $"Diag endpoints: GET /api/diag/audit, GET /api/diag/context, GET /api/diag/auth-test, DELETE /api/diag/audit, POST /api/diag/audit-event"
            ]);
            return app;
        }
    }
}

internal sealed record AuditEventDto(
    string? Type, string? Category, string? Area, string? Intent,
    string? Result, string? Decision, string? Rationale,
    string? Message, string? Stack, object? Context);
