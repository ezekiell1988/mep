using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AulaIA.Api.Features.PowerSync;

public static class PowerSyncModule
{
    private static readonly JwtSecurityTokenHandler _handler = new();

    public static IServiceCollection AddPowerSyncModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapPowerSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var ps = app.MapGroup("/api/powersync")
                    .WithTags("PowerSync");

        ps.MapGet("/token", GetTokenAsync)
          .WithName("PowerSyncToken")
          .RequireAuthorization();

        // PowerSync Cloud llama a este endpoint para subir mutations desde el cliente offline.
        ps.MapPut("/crud", HandleCrudAsync)
          .WithName("PowerSyncCrud")
          .RequireAuthorization();

        return app;
    }

    // GET /api/powersync/token
    private static async Task<IResult> GetTokenAsync(
        HttpContext context,
        ICurrentUserService currentUser,
        IOptions<PowerSyncOptions> options,
        CancellationToken ct)
    {
        var sub = context.User.FindFirst("sub")?.Value
               ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(sub))
            return TypedResults.Unauthorized();

        // Resolvemos el User para obtener su Id (UUID) — lo incluimos como claim
        // `teacher_id` en el JWT de PowerSync para usarlo directamente en las Sync Rules
        // via token_parameters.teacher_id, evitando subqueries no soportados.
        User user;
        try { user = await currentUser.ResolveAsync(ct); }
        catch (UnauthorizedAccessException) { return TypedResults.Unauthorized(); }

        var opt = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        creds.Key.KeyId = opt.KeyId;

        var now    = DateTime.UtcNow;
        var expiry = now.AddHours(1);

        var token = new JwtSecurityToken(
            issuer:   "https://api.mep.ezekl.com",
            audience: opt.InstanceUrl,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, sub),
                new Claim("teacher_id", user.Id.ToString()),
            ],
            notBefore: now,
            expires:   expiry,
            signingCredentials: creds);

        return TypedResults.Ok(new PowerSyncTokenResponse(_handler.WriteToken(token), expiry));
    }

    // PUT /api/powersync/crud
    // PowerSync Cloud llama a este endpoint con las mutaciones que el cliente generó offline.
    // Solo se aceptan mutaciones sobre `attendance_records` (escrituras de asistencia desde el móvil).
    // Los grupos y estudiantes se crean/editan solo desde la web → bajan al móvil, nunca suben.
    private static async Task<IResult> HandleCrudAsync(
        HttpContext context,
        ICurrentUserService currentUserService,
        AulaIADbContext db,
        CancellationToken ct)
    {
        User user;
        try { user = await currentUserService.ResolveAsync(ct); }
        catch (UnauthorizedAccessException) { return TypedResults.Unauthorized(); }

        CrudBatch? batch;
        try
        {
            batch = await context.Request.ReadFromJsonAsync<CrudBatch>(ct);
        }
        catch
        {
            return TypedResults.BadRequest("Payload inválido.");
        }

        if (batch?.Batch is null or { Count: 0 })
            return TypedResults.Ok();

        foreach (var op in batch.Batch)
        {
            if (op.Table != "attendance_records") continue; // solo se aceptan registros de asistencia

            if (!Guid.TryParse(op.Id, out var recordId)) continue;

            if (op.Op is "PUT" or "PATCH")
            {
                if (op.Data is null) continue;

                if (!Guid.TryParse(op.Data.GetValueOrDefault("group_id"), out var groupId)) continue;
                if (!Guid.TryParse(op.Data.GetValueOrDefault("student_id"), out var studentId)) continue;
                if (!DateOnly.TryParse(op.Data.GetValueOrDefault("date"), out var date)) continue;
                if (!Enum.TryParse<AttendanceStatus>(op.Data.GetValueOrDefault("status"), true, out var status)) continue;

                // Verificar que el grupo pertenece al docente autenticado
                var groupBelongsToTeacher = await db.Groups
                    .AnyAsync(g => g.Id == groupId && g.TeacherId == user.Id, ct);
                if (!groupBelongsToTeacher) continue;

                var existing = await db.AttendanceRecords.FindAsync([recordId], ct);
                if (existing is null)
                {
                    db.AttendanceRecords.Add(new AttendanceRecord
                    {
                        Id        = recordId,
                        GroupId   = groupId,
                        StudentId = studentId,
                        Date      = date,
                        Status    = status,
                        Notes     = op.Data.GetValueOrDefault("notes"),
                    });
                }
                else
                {
                    existing.Status = status;
                    existing.Notes  = op.Data.GetValueOrDefault("notes");
                }
            }
            else if (op.Op == "DELETE")
            {
                var existing = await db.AttendanceRecords.FindAsync([recordId], ct);
                if (existing is not null)
                {
                    var groupBelongsToTeacher = await db.Groups
                        .AnyAsync(g => g.Id == existing.GroupId && g.TeacherId == user.Id, ct);
                    if (groupBelongsToTeacher)
                        db.AttendanceRecords.Remove(existing);
                }
            }
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.Ok();
    }
}

// ── DTOs del protocolo PowerSync CRUD ────────────────────────────────────────

public sealed record PowerSyncTokenResponse(string Token, DateTime ExpiresAt);

public sealed class CrudBatch
{
    [JsonPropertyName("batch")]
    public List<CrudEntry>? Batch { get; set; }
}

public sealed class CrudEntry
{
    [JsonPropertyName("op")]
    public string Op { get; set; } = "";          // "PUT" | "PATCH" | "DELETE"

    [JsonPropertyName("type")]
    public string Table { get; set; } = "";        // "attendance_records"

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";           // UUID del registro

    [JsonPropertyName("data")]
    public Dictionary<string, string?>? Data { get; set; }
}

