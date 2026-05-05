using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Features.Estudiantes;

public static class EstudiantesModule
{
    public static IServiceCollection AddEstudiantesModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapEstudiantesEndpoints(this IEndpointRouteBuilder app)
    {
        var estudiantes = app.MapGroup("/api/grupos/{grupoId:guid}/estudiantes")
                             .WithTags("Estudiantes")
                             .RequireAuthorization();

        estudiantes.MapGet("/", GetByGroupAsync).WithName("GetEstudiantes");
        estudiantes.MapGet("/{id:guid}", GetByIdAsync).WithName("GetEstudianteById");
        estudiantes.MapPost("/", CreateAsync).WithName("CreateEstudiante");
        estudiantes.MapPut("/{id:guid}", UpdateAsync).WithName("UpdateEstudiante");

        return app;
    }

    private static async Task<IResult> GetByGroupAsync(Guid grupoId, AulaIADbContext db, CancellationToken ct)
    {
        var estudiantes = await db.Students
            .AsNoTracking()
            .Where(s => s.GroupId == grupoId && s.IsActive)
            .Select(s => new EstudianteResponse(s.Id, s.FullName, s.StudentCode, s.GroupId))
            .ToListAsync(ct);

        return TypedResults.Ok(estudiantes);
    }

    private static async Task<IResult> GetByIdAsync(Guid grupoId, Guid id, AulaIADbContext db, CancellationToken ct)
    {
        var estudiante = await db.Students
            .AsNoTracking()
            .Where(s => s.Id == id && s.GroupId == grupoId && s.IsActive)
            .Select(s => new EstudianteResponse(s.Id, s.FullName, s.StudentCode, s.GroupId))
            .FirstOrDefaultAsync(ct);

        return estudiante is null ? TypedResults.NotFound() : TypedResults.Ok(estudiante);
    }

    private static async Task<IResult> CreateAsync(Guid grupoId, CreateEstudianteRequest request, AulaIADbContext db, CancellationToken ct)
    {
        var estudiante = new Student
        {
            FullName = request.FullName,
            StudentCode = request.StudentCode,
            GroupId = grupoId
        };

        db.Students.Add(estudiante);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created(
            $"/api/grupos/{grupoId}/estudiantes/{estudiante.Id}",
            new EstudianteResponse(estudiante.Id, estudiante.FullName, estudiante.StudentCode, estudiante.GroupId));
    }

    private static async Task<IResult> UpdateAsync(Guid grupoId, Guid id, UpdateEstudianteRequest request, AulaIADbContext db, CancellationToken ct)
    {
        var estudiante = await db.Students.FirstOrDefaultAsync(s => s.Id == id && s.GroupId == grupoId, ct);
        if (estudiante is null) return TypedResults.NotFound();

        estudiante.FullName = request.FullName;
        estudiante.StudentCode = request.StudentCode;

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}

public sealed record EstudianteResponse(Guid Id, string FullName, string StudentCode, Guid GroupId);
public sealed record CreateEstudianteRequest(string FullName, string StudentCode);
public sealed record UpdateEstudianteRequest(string FullName, string StudentCode);
