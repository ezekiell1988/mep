using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Features.Notas;

public static class NotasModule
{
    public static IServiceCollection AddNotasModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapNotasEndpoints(this IEndpointRouteBuilder app)
    {
        var notas = app.MapGroup("/api/grupos/{grupoId:guid}")
                       .WithTags("Notas")
                       .RequireAuthorization();

        notas.MapGet("/actividades", GetActividadesAsync).WithName("GetActividades");
        notas.MapPost("/actividades", CreateActividadAsync).WithName("CreateActividad");
        notas.MapGet("/actividades/{actividadId:guid}/calificaciones", GetCalificacionesAsync).WithName("GetCalificaciones");
        notas.MapPost("/actividades/{actividadId:guid}/calificaciones", SaveCalificacionesAsync).WithName("SaveCalificaciones");

        return app;
    }

    private static async Task<IResult> GetActividadesAsync(Guid grupoId, AulaIADbContext db, CancellationToken ct)
    {
        var actividades = await db.EvaluationActivities
            .AsNoTracking()
            .Where(a => a.GroupId == grupoId)
            .Select(a => new ActividadResponse(a.Id, a.Name, a.Type, a.MaxScore, a.Percentage, a.DueDate))
            .ToListAsync(ct);

        return TypedResults.Ok(actividades);
    }

    private static async Task<IResult> CreateActividadAsync(Guid grupoId, CreateActividadRequest request, AulaIADbContext db, CancellationToken ct)
    {
        var actividad = new EvaluationActivity
        {
            GroupId = grupoId,
            Name = request.Name,
            Type = request.Type,
            MaxScore = request.MaxScore,
            Percentage = request.Percentage,
            DueDate = request.DueDate
        };

        db.EvaluationActivities.Add(actividad);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created(
            $"/api/grupos/{grupoId}/actividades/{actividad.Id}",
            new ActividadResponse(actividad.Id, actividad.Name, actividad.Type, actividad.MaxScore, actividad.Percentage, actividad.DueDate));
    }

    private static async Task<IResult> GetCalificacionesAsync(Guid grupoId, Guid actividadId, AulaIADbContext db, CancellationToken ct)
    {
        var calificaciones = await db.Grades
            .AsNoTracking()
            .Where(g => g.ActivityId == actividadId)
            .Select(g => new CalificacionResponse(g.Id, g.StudentId, g.ActivityId, g.Score, g.Comments))
            .ToListAsync(ct);

        return TypedResults.Ok(calificaciones);
    }

    private static async Task<IResult> SaveCalificacionesAsync(Guid grupoId, Guid actividadId, List<SaveCalificacionRequest> request, AulaIADbContext db, CancellationToken ct)
    {
        var existentes = await db.Grades
            .Where(g => g.ActivityId == actividadId)
            .ToListAsync(ct);

        foreach (var item in request)
        {
            var existente = existentes.FirstOrDefault(g => g.StudentId == item.StudentId);
            if (existente is not null)
            {
                existente.Score = item.Score;
                existente.Comments = item.Comments;
                existente.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                db.Grades.Add(new Grade
                {
                    ActivityId = actividadId,
                    StudentId = item.StudentId,
                    Score = item.Score,
                    Comments = item.Comments
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}

public sealed record ActividadResponse(Guid Id, string Name, string Type, decimal MaxScore, decimal Percentage, DateOnly? DueDate);
public sealed record CreateActividadRequest(string Name, string Type, decimal MaxScore, decimal Percentage, DateOnly? DueDate);
public sealed record CalificacionResponse(Guid Id, Guid StudentId, Guid ActivityId, decimal Score, string? Comments);
public sealed record SaveCalificacionRequest(Guid StudentId, decimal Score, string? Comments);
