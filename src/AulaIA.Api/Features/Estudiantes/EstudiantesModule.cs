using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AulaIA.Api.Features.Estudiantes;

public static class EstudiantesModule
{
    public static IServiceCollection AddEstudiantesModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapEstudiantesEndpoints(this IEndpointRouteBuilder app)
    {
        var estudiantes = app.MapGroup("/api/grupos/{grupoId:guid}/estudiantes")
                             .WithTags("Estudiantes")
                             .RequireAuthorization("teacher");

        estudiantes.MapGet("/",            GetByGroupAsync).WithName("GetEstudiantes");
        estudiantes.MapGet("/{id:guid}",   GetByIdAsync).WithName("GetEstudianteById");
        estudiantes.MapPost("/",           CreateAsync).WithName("CreateEstudiante");
        estudiantes.MapPut("/{id:guid}",   UpdateAsync).WithName("UpdateEstudiante");
        estudiantes.MapDelete("/{id:guid}", DeleteAsync).WithName("DeleteEstudiante");

        return app;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Verifica que el grupo existe y pertenece al docente autenticado.</summary>
    private static async Task<Group?> ResolveGroupAsync(
        Guid grupoId, ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);
        return await db.Groups
            .FirstOrDefaultAsync(g => g.Id == grupoId && g.TeacherId == user.Id && g.IsActive, ct);
    }

    // ── Endpoints ────────────────────────────────────────────────────────────

    private static async Task<IResult> GetByGroupAsync(
        Guid grupoId, ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        if (await ResolveGroupAsync(grupoId, currentUser, db, ct) is null)
            return TypedResults.NotFound();

        var lista = await db.Students
            .AsNoTracking()
            .Where(s => s.GroupId == grupoId && s.IsActive)
            .OrderBy(s => s.FullName)
            .Select(s => new EstudianteResponse(s.Id, s.FullName, s.StudentCode, s.GroupId, s.QrCode))
            .ToListAsync(ct);

        return TypedResults.Ok(lista);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid grupoId, Guid id, ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        if (await ResolveGroupAsync(grupoId, currentUser, db, ct) is null)
            return TypedResults.NotFound();

        var estudiante = await db.Students
            .AsNoTracking()
            .Where(s => s.Id == id && s.GroupId == grupoId && s.IsActive)
            .Select(s => new EstudianteResponse(s.Id, s.FullName, s.StudentCode, s.GroupId, s.QrCode))
            .FirstOrDefaultAsync(ct);

        return estudiante is null ? TypedResults.NotFound() : TypedResults.Ok(estudiante);
    }

    private static async Task<IResult> CreateAsync(
        Guid grupoId, CreateEstudianteRequest request,
        ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        if (await ResolveGroupAsync(grupoId, currentUser, db, ct) is null)
            return TypedResults.NotFound();

        var estudiante = new Student
        {
            FullName    = request.FullName,
            StudentCode = request.StudentCode,
            GroupId     = grupoId,
            QrCode      = Guid.NewGuid().ToString("N")
        };

        db.Students.Add(estudiante);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created(
            $"/api/grupos/{grupoId}/estudiantes/{estudiante.Id}",
            new EstudianteResponse(estudiante.Id, estudiante.FullName, estudiante.StudentCode, estudiante.GroupId, estudiante.QrCode));
    }

    private static async Task<IResult> UpdateAsync(
        Guid grupoId, Guid id, UpdateEstudianteRequest request,
        ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        if (await ResolveGroupAsync(grupoId, currentUser, db, ct) is null)
            return TypedResults.NotFound();

        var estudiante = await db.Students
            .FirstOrDefaultAsync(s => s.Id == id && s.GroupId == grupoId && s.IsActive, ct);
        if (estudiante is null) return TypedResults.NotFound();

        estudiante.FullName    = request.FullName;
        estudiante.StudentCode = request.StudentCode;

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid grupoId, Guid id, ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        if (await ResolveGroupAsync(grupoId, currentUser, db, ct) is null)
            return TypedResults.NotFound();

        var estudiante = await db.Students
            .FirstOrDefaultAsync(s => s.Id == id && s.GroupId == grupoId && s.IsActive, ct);
        if (estudiante is null) return TypedResults.NotFound();

        estudiante.IsActive = false;
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}

public sealed record EstudianteResponse(Guid Id, string FullName, string StudentCode, Guid GroupId, string QrCode);

public sealed record CreateEstudianteRequest(
    [property: Required, StringLength(200, MinimumLength = 2)] string FullName,
    [property: Required, StringLength(20)]                     string StudentCode);

public sealed record UpdateEstudianteRequest(
    [property: Required, StringLength(200, MinimumLength = 2)] string FullName,
    [property: Required, StringLength(20)]                     string StudentCode);
