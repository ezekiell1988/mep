using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Features.Grupos;

public static class GruposModule
{
    public static IServiceCollection AddGruposModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapGruposEndpoints(this IEndpointRouteBuilder app)
    {
        var grupos = app.MapGroup("/api/grupos")
                        .WithTags("Grupos")
                        .RequireAuthorization();

        grupos.MapGet("/", GetAllAsync).WithName("GetGrupos");
        grupos.MapGet("/{id:guid}", GetByIdAsync).WithName("GetGrupoById");
        grupos.MapPost("/", CreateAsync).WithName("CreateGrupo");
        grupos.MapPut("/{id:guid}", UpdateAsync).WithName("UpdateGrupo");
        grupos.MapDelete("/{id:guid}", DeleteAsync).WithName("DeleteGrupo");

        return app;
    }

    private static async Task<IResult> GetAllAsync(AulaIADbContext db, CancellationToken ct)
    {
        var grupos = await db.Groups
            .AsNoTracking()
            .Where(g => g.IsActive)
            .Select(g => new GrupoResponse(g.Id, g.Name, g.Level, g.Subject, g.SchoolYear, g.TeacherId))
            .ToListAsync(ct);

        return TypedResults.Ok(grupos);
    }

    private static async Task<IResult> GetByIdAsync(Guid id, AulaIADbContext db, CancellationToken ct)
    {
        var grupo = await db.Groups
            .AsNoTracking()
            .Where(g => g.Id == id && g.IsActive)
            .Select(g => new GrupoResponse(g.Id, g.Name, g.Level, g.Subject, g.SchoolYear, g.TeacherId))
            .FirstOrDefaultAsync(ct);

        return grupo is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(grupo);
    }

    private static async Task<IResult> CreateAsync(CreateGrupoRequest request, AulaIADbContext db, CancellationToken ct)
    {
        var grupo = new Group
        {
            Name = request.Name,
            Level = request.Level,
            Subject = request.Subject,
            SchoolYear = request.SchoolYear,
            TeacherId = request.TeacherId,
            InstitutionId = request.InstitutionId
        };

        db.Groups.Add(grupo);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created($"/api/grupos/{grupo.Id}",
            new GrupoResponse(grupo.Id, grupo.Name, grupo.Level, grupo.Subject, grupo.SchoolYear, grupo.TeacherId));
    }

    private static async Task<IResult> UpdateAsync(Guid id, UpdateGrupoRequest request, AulaIADbContext db, CancellationToken ct)
    {
        var grupo = await db.Groups.FindAsync([id], ct);
        if (grupo is null) return TypedResults.NotFound();

        grupo.Name = request.Name;
        grupo.Level = request.Level;
        grupo.Subject = request.Subject;

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteAsync(Guid id, AulaIADbContext db, CancellationToken ct)
    {
        var grupo = await db.Groups.FindAsync([id], ct);
        if (grupo is null) return TypedResults.NotFound();

        grupo.IsActive = false;
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}

public sealed record GrupoResponse(Guid Id, string Name, string Level, string Subject, int SchoolYear, Guid TeacherId);

public sealed record CreateGrupoRequest(
    string Name,
    string Level,
    string Subject,
    int SchoolYear,
    Guid TeacherId,
    Guid InstitutionId);

public sealed record UpdateGrupoRequest(string Name, string Level, string Subject);
