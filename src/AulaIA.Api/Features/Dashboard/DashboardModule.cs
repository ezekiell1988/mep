using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Features.Dashboard;

public static class DashboardModule
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/docente/resumen", GetResumenAsync)
           .WithTags("Dashboard")
           .WithName("GetDocenteResumen")
           .RequireAuthorization();

        return app;
    }

    // GET /api/docente/resumen
    private static async Task<Ok<DocenteResumenResponse>> GetResumenAsync(
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        // IDs de grupos activos del docente
        var grupoIds = await db.Groups
            .Where(g => g.TeacherSub == user.Auth0Sub && g.IsActive)
            .Select(g => g.Id)
            .ToListAsync(ct);

        var totalGrupos = grupoIds.Count;

        var totalEstudiantes = await db.Students
            .CountAsync(s => grupoIds.Contains(s.GroupId) && s.IsActive, ct);

        // Estudiantes en riesgo: promedio simple de notas < 65
        // (umbral MEP III Ciclo y primaria)
        var estudiantesEnRiesgo = await db.Students
            .Where(s => grupoIds.Contains(s.GroupId) && s.IsActive)
            .Where(s => s.Grades.Any() &&
                        s.Grades.Average(g => g.Score) < 65)
            .CountAsync(ct);

        var planeamientosPendientes = await db.LessonPlans
            .CountAsync(p => grupoIds.Contains(p.GroupId)
                          && (p.Status == LessonPlanStatus.Pending
                           || p.Status == LessonPlanStatus.Generating), ct);

        var planeamientosListos = await db.LessonPlans
            .CountAsync(p => grupoIds.Contains(p.GroupId)
                          && p.Status == LessonPlanStatus.Ready, ct);

        var adecuacionesActivas = await db.Accommodations
            .CountAsync(a => grupoIds.Contains(a.GroupId), ct);

        // Próximos eventos de calendario (hoy + 14 días)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var twoWeeks = today.AddDays(14);
        var proximosEventos = await db.CalendarEvents
            .Where(e => (e.GroupId == null || grupoIds.Contains(e.GroupId.Value))
                     && e.Date >= today
                     && e.Date <= twoWeeks)
            .OrderBy(e => e.Date)
            .Take(10)
            .Select(e => new ProximoEvento(e.Date.ToString("yyyy-MM-dd"), e.Title, e.Type.ToString()))
            .ToListAsync(ct);

        return TypedResults.Ok(new DocenteResumenResponse(
            TotalGrupos: totalGrupos,
            TotalEstudiantes: totalEstudiantes,
            EstudiantesEnRiesgo: estudiantesEnRiesgo,
            PlaneamientosPendientes: planeamientosPendientes,
            PlaneamientosListos: planeamientosListos,
            AdecuacionesActivas: adecuacionesActivas,
            ProximosEventos: proximosEventos));
    }
}

public record DocenteResumenResponse(
    int TotalGrupos,
    int TotalEstudiantes,
    int EstudiantesEnRiesgo,
    int PlaneamientosPendientes,
    int PlaneamientosListos,
    int AdecuacionesActivas,
    IReadOnlyList<ProximoEvento> ProximosEventos);

public record ProximoEvento(string Fecha, string Titulo, string Tipo);
