using AulaIA.Api.Features.Planeamiento.Jobs;
using AulaIA.Api.Features.Planeamiento.Services;
using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AulaIA.Api.Features.Planeamiento;

public static class PlaneamientoModule
{
    public static IServiceCollection AddPlaneamientoModule(this IServiceCollection services)
    {
        services.AddScoped<PlaneamientoAiService>();
        services.AddScoped<GenerarPlaneamientoJob>();
        return services;
    }

    public static IEndpointRouteBuilder MapPlaneamientoEndpoints(this IEndpointRouteBuilder app)
    {
        var planeamiento = app.MapGroup("/api/planeamiento")
                              .WithTags("Planeamiento")
                              .RequireAuthorization();

        // POST /api/planeamiento — crea y encola la generación
        planeamiento.MapPost("/", async Task<Results<Accepted<PlaneamientoResponse>, BadRequest<string>, NotFound>> (
            CrearPlaneamientoRequest req,
            ICurrentUserService currentUser,
            AulaIADbContext db,
            IBackgroundJobClient jobs,
            CancellationToken ct) =>
        {
            var user = await currentUser.ResolveAsync(ct);
            if (user is null) return TypedResults.NotFound();

            var group = await db.Groups
                .FirstOrDefaultAsync(g => g.Id == req.GroupId && g.TeacherSub == user.Auth0Sub, ct);
            if (group is null) return TypedResults.BadRequest("Grupo no encontrado o no pertenece al docente.");

            var plan = new LessonPlan
            {
                GroupId = req.GroupId,
                TeacherSub = user.Auth0Sub,
                Asignatura = req.Asignatura,
                Nivel = req.Nivel,
                Trimestre = req.Trimestre,
                AnioLectivo = req.AnioLectivo,
                FechaInicio = req.FechaInicio,
                FechaFin = req.FechaFin,
                LeccionesPorSemana = req.LeccionesPorSemana,
                Status = LessonPlanStatus.Pending
            };

            db.LessonPlans.Add(plan);
            await db.SaveChangesAsync(ct);

            jobs.Enqueue<GenerarPlaneamientoJob>(
                "planeamiento",
                j => j.ExecuteAsync(plan.Id, CancellationToken.None));

            return TypedResults.Accepted($"/api/planeamiento/{plan.Id}",
                new PlaneamientoResponse(plan.Id, plan.Status.ToString(), null));
        })
        .WithName("CrearPlaneamiento");

        // GET /api/planeamiento/{id} — obtiene estado y contenido
        planeamiento.MapGet("/{id:guid}", async Task<Results<Ok<PlaneamientoResponse>, NotFound>> (
            Guid id,
            ICurrentUserService currentUser,
            AulaIADbContext db,
            CancellationToken ct) =>
        {
            var user = await currentUser.ResolveAsync(ct);
            if (user is null) return TypedResults.NotFound();

            var plan = await db.LessonPlans
                .FirstOrDefaultAsync(p => p.Id == id && p.TeacherSub == user.Auth0Sub, ct);

            return plan is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(new PlaneamientoResponse(plan.Id, plan.Status.ToString(), plan.ContenidoGenerado));
        })
        .WithName("GetPlaneamiento");

        // GET /api/planeamiento — lista planeamientos del docente
        planeamiento.MapGet("/", async Task<Ok<List<PlaneamientoListItem>>> (
            [FromQuery] Guid? groupId,
            ICurrentUserService currentUser,
            AulaIADbContext db,
            CancellationToken ct) =>
        {
            var user = await currentUser.ResolveAsync(ct);
            if (user is null) return TypedResults.Ok(new List<PlaneamientoListItem>());

            var query = db.LessonPlans.Where(p => p.TeacherSub == user.Auth0Sub);
            if (groupId.HasValue) query = query.Where(p => p.GroupId == groupId.Value);

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PlaneamientoListItem(p.Id, p.Asignatura, p.Nivel, p.Trimestre, p.Status.ToString(), p.CreatedAt))
                .ToListAsync(ct);

            return TypedResults.Ok(items);
        })
        .WithName("ListPlaneamientos");

        // GET /api/planeamiento/curriculum-check — verifica si hay unidades validadas
        // para una combinación asignatura/nivel/trimestre dada (disponible para docentes)
        planeamiento.MapGet("/curriculum-check", async Task<Ok<CurriculumCheckResponse>> (
            [FromQuery] string asignatura,
            [FromQuery] int nivel,
            [FromQuery] int trimestre,
            AulaIADbContext db,
            CancellationToken ct) =>
        {
            var count = await db.CurriculumUnits
                .CountAsync(u => u.Asignatura == asignatura
                              && u.Nivel == nivel
                              && u.Trimestre == trimestre
                              && u.ValidatedAt != null, ct);

            return TypedResults.Ok(new CurriculumCheckResponse(count > 0, count));
        })
        .WithName("CheckCurriculumDisponible");

        return app;
    }

    public record CrearPlaneamientoRequest(
        [Required] Guid GroupId,
        [Required, MaxLength(100)] string Asignatura,
        [Range(1, 12)] int Nivel,
        [Range(1, 3)] int Trimestre,
        [Range(2020, 2050)] int AnioLectivo,
        DateOnly FechaInicio,
        DateOnly FechaFin,
        [Range(1, 10)] int LeccionesPorSemana);

    public record PlaneamientoResponse(Guid Id, string Status, string? Contenido);
    public record PlaneamientoListItem(Guid Id, string Asignatura, int Nivel, int Trimestre, string Status, DateTimeOffset CreatedAt);
    public record CurriculumCheckResponse(bool Disponible, int Unidades);
}

