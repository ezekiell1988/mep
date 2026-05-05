using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Features.Asistencia;

public static class AsistenciaModule
{
    public static IServiceCollection AddAsistenciaModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapAsistenciaEndpoints(this IEndpointRouteBuilder app)
    {
        var asistencia = app.MapGroup("/api/grupos/{grupoId:guid}/asistencia")
                            .WithTags("Asistencia")
                            .RequireAuthorization();

        asistencia.MapGet("/{fecha}", GetByDateAsync).WithName("GetAsistenciaPorFecha");
        asistencia.MapPost("/", SaveAsync).WithName("SaveAsistencia");

        return app;
    }

    private static async Task<IResult> GetByDateAsync(Guid grupoId, string fecha, AulaIADbContext db, CancellationToken ct)
    {
        if (!DateOnly.TryParse(fecha, out var date))
            return TypedResults.BadRequest(new { error = "Formato de fecha inválido. Use yyyy-MM-dd." });

        var registros = await db.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.GroupId == grupoId && r.Date == date)
            .Select(r => new AsistenciaResponse(r.Id, r.StudentId, r.Date, r.Status.ToString(), r.Notes))
            .ToListAsync(ct);

        return TypedResults.Ok(registros);
    }

    private static async Task<IResult> SaveAsync(Guid grupoId, List<SaveAsistenciaRequest> request, AulaIADbContext db, CancellationToken ct)
    {
        if (request.Count == 0) return TypedResults.BadRequest(new { error = "La lista de asistencia no puede estar vacía." });

        var date = request.First().Date;
        var existentes = await db.AttendanceRecords
            .Where(r => r.GroupId == grupoId && r.Date == date)
            .ToListAsync(ct);

        foreach (var item in request)
        {
            var existente = existentes.FirstOrDefault(r => r.StudentId == item.StudentId);
            if (existente is not null)
            {
                existente.Status = Enum.Parse<AttendanceStatus>(item.Status);
                existente.Notes = item.Notes;
            }
            else
            {
                db.AttendanceRecords.Add(new AttendanceRecord
                {
                    GroupId = grupoId,
                    StudentId = item.StudentId,
                    Date = item.Date,
                    Status = Enum.Parse<AttendanceStatus>(item.Status),
                    Notes = item.Notes
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }
}

public sealed record AsistenciaResponse(Guid Id, Guid StudentId, DateOnly Date, string Status, string? Notes);
public sealed record SaveAsistenciaRequest(Guid StudentId, DateOnly Date, string Status, string? Notes);
