using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Features.Dashboard;

public static class DirectorModule
{
    extension(IEndpointRouteBuilder app)
    {
        public IEndpointRouteBuilder MapDirectorEndpoints()
        {
            var group = app.MapGroup("/api/director")
                           .WithTags("Director")
                           .RequireAuthorization("director");

            // GET /api/director/resumen
            group.MapGet("/resumen", GetResumenAsync).WithName("GetDirectorResumen");

            // GET /api/director/docentes
            group.MapGet("/docentes", GetDocentesAsync).WithName("GetDirectorDocentes");

            return app;
        }
    }

    // GET /api/director/resumen
    private static async Task<Ok<ResumenInstitucionalResponse>> GetResumenAsync(
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var director = await currentUser.ResolveAsync(ct);

        var institution = await db.Institutions
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == director.InstitutionId, ct);

        var docentes = await db.Users
            .AsNoTracking()
            .Where(u => u.InstitutionId == director.InstitutionId)
            .ToListAsync(ct);

        var docenteIds = docentes.Select(d => d.Id).ToList();
        var docenteSubs = docentes.Select(d => d.Auth0Sub).ToList();

        var grupos = await db.Groups
            .AsNoTracking()
            .Where(g => g.InstitutionId == director.InstitutionId && g.IsActive)
            .Select(g => new { g.Id, g.TeacherId })
            .ToListAsync(ct);

        var grupoIds = grupos.Select(g => g.Id).ToList();

        var totalEstudiantes = await db.Students
            .CountAsync(s => grupoIds.Contains(s.GroupId) && s.IsActive, ct);

        // Suscripción institucional activa (del director o de cualquier usuario de la institución con plan Institutional)
        var subInstitucional = await db.Subscriptions
            .AsNoTracking()
            .Where(s => docenteIds.Contains(s.UserId)
                     && s.Plan == SubscriptionPlan.Institutional
                     && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.CurrentPeriodEnd)
            .FirstOrDefaultAsync(ct);

        int? diasRestantes = subInstitucional is null
            ? null
            : (int)Math.Max(0, (subInstitucional.CurrentPeriodEnd - DateTime.UtcNow).TotalDays);

        return TypedResults.Ok(new ResumenInstitucionalResponse(
            InstitutionId:      director.InstitutionId,
            InstitutionName:    institution?.Name ?? "—",
            TotalDocentes:      docentes.Count,
            TotalGrupos:        grupos.Count,
            TotalEstudiantes:   totalEstudiantes,
            PlanInstitucional:  subInstitucional?.Plan.ToString(),
            DiasRestantes:      diasRestantes));
    }

    // GET /api/director/docentes
    private static async Task<Ok<IReadOnlyList<DocenteInstitucionalResponse>>> GetDocentesAsync(
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var director = await currentUser.ResolveAsync(ct);

        var docentes = await db.Users
            .AsNoTracking()
            .Where(u => u.InstitutionId == director.InstitutionId)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

        var docenteIds = docentes.Select(d => d.Id).ToList();

        // Grupos activos por docente
        var grupos = await db.Groups
            .AsNoTracking()
            .Where(g => docenteIds.Contains(g.TeacherId) && g.IsActive)
            .Select(g => new { g.Id, g.TeacherId, g.Name, g.Subject, g.Level })
            .ToListAsync(ct);

        var grupoIds = grupos.Select(g => g.Id).ToList();

        // Conteo de estudiantes por grupo
        var estudiantesPorGrupo = await db.Students
            .Where(s => grupoIds.Contains(s.GroupId) && s.IsActive)
            .GroupBy(s => s.GroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var estudiantesMap = estudiantesPorGrupo.ToDictionary(e => e.GroupId, e => e.Count);

        // Suscripciones de cada docente
        var subs = await db.Subscriptions
            .AsNoTracking()
            .Where(s => docenteIds.Contains(s.UserId))
            .ToListAsync(ct);

        var subsMap = subs.ToDictionary(s => s.UserId);

        var result = docentes.Select(d =>
        {
            var misGrupos = grupos
                .Where(g => g.TeacherId == d.Id)
                .Select(g => new GrupoResumenResponse(
                    g.Id,
                    g.Name,
                    g.Subject,
                    g.Level,
                    estudiantesMap.GetValueOrDefault(g.Id, 0)))
                .ToList();

            subsMap.TryGetValue(d.Id, out var sub);

            return new DocenteInstitucionalResponse(
                DocenteId: d.Id,
                FullName: d.FullName,
                Email: d.Email,
                TotalGrupos: misGrupos.Count,
                TotalEstudiantes: misGrupos.Sum(g => g.TotalEstudiantes),
                Plan: sub?.Plan.ToString() ?? "Sin suscripción",
                PlanActivo: sub?.Status == SubscriptionStatus.Active,
                Grupos: misGrupos);
        }).ToList();

        return TypedResults.Ok<IReadOnlyList<DocenteInstitucionalResponse>>(result);
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record ResumenInstitucionalResponse(
    Guid InstitutionId,
    string InstitutionName,
    int TotalDocentes,
    int TotalGrupos,
    int TotalEstudiantes,
    string? PlanInstitucional,
    int? DiasRestantes);

public record DocenteInstitucionalResponse(
    Guid DocenteId,
    string FullName,
    string Email,
    int TotalGrupos,
    int TotalEstudiantes,
    string Plan,
    bool PlanActivo,
    IReadOnlyList<GrupoResumenResponse> Grupos);

public record GrupoResumenResponse(
    Guid Id,
    string Name,
    string Subject,
    string Level,
    int TotalEstudiantes);
