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
        notas.MapDelete("/actividades/{actividadId:guid}", DeleteActividadAsync).WithName("DeleteActividad");
        notas.MapGet("/actividades/{actividadId:guid}/calificaciones", GetCalificacionesAsync).WithName("GetCalificaciones");
        notas.MapPost("/actividades/{actividadId:guid}/calificaciones", SaveCalificacionesAsync).WithName("SaveCalificaciones");
        notas.MapGet("/notas/resumen", GetResumenNotasAsync).WithName("GetResumenNotas");

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
                    GroupId = grupoId,
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

    private static async Task<IResult> DeleteActividadAsync(Guid grupoId, Guid actividadId, AulaIADbContext db, CancellationToken ct)
    {
        var actividad = await db.EvaluationActivities
            .FirstOrDefaultAsync(a => a.Id == actividadId && a.GroupId == grupoId, ct);
        if (actividad is null) return TypedResults.NotFound();
        db.EvaluationActivities.Remove(actividad);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Resumen ponderado por alumno para el grupo.
    /// La ponderación por defecto del MEP es configurable; aquí se usa la estándar de secundaria:
    ///   Trabajo Cotidiano 20% / Pruebas 45% / Trabajo Extraclase 20% / Otros 15%
    /// El promedio final se calcula: sum(nota_normalizada * porcentaje_actividad) / sum(porcentaje_actividad)
    /// donde nota_normalizada = (score / max_score) * 100.
    /// </summary>
    private static async Task<IResult> GetResumenNotasAsync(Guid grupoId, AulaIADbContext db, CancellationToken ct)
    {
        var actividades = await db.EvaluationActivities
            .AsNoTracking()
            .Where(a => a.GroupId == grupoId)
            .Include(a => a.Grades)
            .ToListAsync(ct);

        var estudiantes = await db.Students
            .AsNoTracking()
            .Where(s => s.GroupId == grupoId)
            .Select(s => new { s.Id, s.FullName, s.StudentCode })
            .ToListAsync(ct);

        var resumen = estudiantes.Select(est =>
        {
            var notasPonderadas = actividades
                .Where(a => a.Percentage > 0)
                .Select(a =>
                {
                    var grade = a.Grades.FirstOrDefault(g => g.StudentId == est.Id);
                    if (grade is null) return (PesoAplicado: 0m, Puntaje: 0m, TienNota: false);
                    var normalizada = a.MaxScore > 0 ? (grade.Score / a.MaxScore) * 100m : 0m;
                    return (PesoAplicado: a.Percentage, Puntaje: normalizada * a.Percentage, TienNota: true);
                })
                .ToList();

            var pesoTotal = notasPonderadas.Where(n => n.TienNota).Sum(n => n.PesoAplicado);
            var promedio = pesoTotal > 0
                ? Math.Round(notasPonderadas.Where(n => n.TienNota).Sum(n => n.Puntaje) / pesoTotal, 1)
                : (decimal?)null;

            var notasPorActividad = actividades.Select(a =>
            {
                var grade = a.Grades.FirstOrDefault(g => g.StudentId == est.Id);
                return new NotaActividadItem(a.Id, a.Name, a.Type, a.MaxScore, a.Percentage,
                    grade?.Score, grade?.Comments);
            }).ToList();

            return new ResumenEstudianteResponse(
                est.Id, est.FullName, est.StudentCode,
                promedio,
                notasPorActividad);
        }).ToList();

        return TypedResults.Ok(new ResumenGrupoResponse(grupoId, actividades.Count, resumen));
    }
}

public sealed record ActividadResponse(Guid Id, string Name, string Type, decimal MaxScore, decimal Percentage, DateOnly? DueDate);
public sealed record CreateActividadRequest(string Name, string Type, decimal MaxScore, decimal Percentage, DateOnly? DueDate);
public sealed record CalificacionResponse(Guid Id, Guid StudentId, Guid ActivityId, decimal Score, string? Comments);
public sealed record SaveCalificacionRequest(Guid StudentId, decimal Score, string? Comments);
public sealed record NotaActividadItem(Guid ActividadId, string Nombre, string Tipo, decimal MaxScore, decimal Porcentaje, decimal? Nota, string? Comentario);
public sealed record ResumenEstudianteResponse(Guid StudentId, string FullName, string StudentCode, decimal? Promedio, List<NotaActividadItem> Notas);
public sealed record ResumenGrupoResponse(Guid GroupId, int TotalActividades, List<ResumenEstudianteResponse> Estudiantes);
