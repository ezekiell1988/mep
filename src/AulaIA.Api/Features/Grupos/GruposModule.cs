using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AulaIA.Api.Features.Grupos;

public static class GruposModule
{
    public static IServiceCollection AddGruposModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapGruposEndpoints(this IEndpointRouteBuilder app)
    {
        var grupos = app.MapGroup("/api/grupos")
                        .WithTags("Grupos")
                        .RequireAuthorization("teacher");

        grupos.MapGet("/", GetAllAsync).WithName("GetGrupos");
        grupos.MapGet("/{id:guid}", GetByIdAsync).WithName("GetGrupoById");
        grupos.MapPost("/", CreateAsync).WithName("CreateGrupo");
        grupos.MapPut("/{id:guid}", UpdateAsync).WithName("UpdateGrupo");
        grupos.MapDelete("/{id:guid}", DeleteAsync).WithName("DeleteGrupo");

        return app;
    }

    private static async Task<IResult> GetAllAsync(
        ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        var grupos = await db.Groups
            .AsNoTracking()
            .Where(g => g.TeacherId == user.Id && g.IsActive)
            .Select(g => new GrupoResponse(g.Id, g.Name, g.Level, g.Subject, g.SchoolYear, g.TeacherId))
            .ToListAsync(ct);

        return TypedResults.Ok(grupos);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id, ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        var grupo = await db.Groups
            .AsNoTracking()
            .Where(g => g.Id == id && g.TeacherId == user.Id && g.IsActive)
            .Select(g => new GrupoResponse(g.Id, g.Name, g.Level, g.Subject, g.SchoolYear, g.TeacherId))
            .FirstOrDefaultAsync(ct);

        return grupo is null ? TypedResults.NotFound() : TypedResults.Ok(grupo);
    }

    private static async Task<IResult> CreateAsync(
        CreateGrupoRequest request, ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        var grupo = new Group
        {
            Name          = request.Name,
            Level         = request.Level,
            Subject       = request.Subject,
            SchoolYear    = request.SchoolYear,
            TeacherId     = user.Id,
            TeacherSub    = currentUser.Auth0Sub,
            InstitutionId = user.InstitutionId
        };

        db.Groups.Add(grupo);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created($"/api/grupos/{grupo.Id}",
            new GrupoResponse(grupo.Id, grupo.Name, grupo.Level, grupo.Subject, grupo.SchoolYear, grupo.TeacherId));
    }

    private static async Task<IResult> UpdateAsync(
        Guid id, UpdateGrupoRequest request, ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        var grupo = await db.Groups.FirstOrDefaultAsync(g => g.Id == id && g.TeacherId == user.Id, ct);
        if (grupo is null) return TypedResults.NotFound();

        grupo.Name    = request.Name;
        grupo.Level   = request.Level;
        grupo.Subject = request.Subject;

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        Guid id, ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        var grupo = await db.Groups.FirstOrDefaultAsync(g => g.Id == id && g.TeacherId == user.Id, ct);
        if (grupo is null) return TypedResults.NotFound();

        grupo.IsActive = false;
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}

public sealed record GrupoResponse(Guid Id, string Name, string Level, string Subject, int SchoolYear, Guid TeacherId);

public sealed record CreateGrupoRequest(
    [property: Required, StringLength(100, MinimumLength = 2)] string Name,
    [property: Required, StringLength(50)]                     string Level,
    [property: Required, StringLength(100)]                    string Subject,
    [property: Range(2020, 2100)]                              int SchoolYear);

public sealed record UpdateGrupoRequest(
    [property: Required, StringLength(100, MinimumLength = 2)] string Name,
    [property: Required, StringLength(50)]                     string Level,
    [property: Required, StringLength(100)]                    string Subject);
