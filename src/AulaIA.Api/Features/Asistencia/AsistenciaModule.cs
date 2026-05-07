using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AulaIA.Api.Features.Asistencia;

public static class AsistenciaModule
{
    public static IServiceCollection AddAsistenciaModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapAsistenciaEndpoints(this IEndpointRouteBuilder app)
    {
        var byGroup = app.MapGroup("/api/grupos/{grupoId:guid}/asistencia")
                         .WithTags("Asistencia")
                         .RequireAuthorization("teacher");

        byGroup.MapGet("/",          GetByDateAsync).WithName("GetAsistencia");
        byGroup.MapGet("/historial", GetHistorialAsync).WithName("GetHistorialAsistencia");
        byGroup.MapPost("/",         UpsertAsync).WithName("UpsertAsistencia");

        var qr = app.MapGroup("/api/asistencia")
                    .WithTags("Asistencia")
                    .RequireAuthorization("teacher");

        qr.MapPost("/qr", ScanQrAsync).WithName("ScanQrAsistencia");

        return app;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static async Task<Group?> ResolveGroupAsync(
        Guid grupoId, ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);
        return await db.Groups
            .FirstOrDefaultAsync(g => g.Id == grupoId && g.TeacherId == user.Id && g.IsActive, ct);
    }

    // ── Endpoints ────────────────────────────────────────────────────────────

    // GET /api/grupos/{grupoId}/asistencia?date=2026-05-05
    // Devuelve todos los alumnos del grupo con su estado de asistencia para esa fecha.
    // Si no hay registro para un alumno, Status = null (lista no tomada aún).
    private static async Task<IResult> GetByDateAsync(
        Guid grupoId, DateOnly? date,
        ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        if (await ResolveGroupAsync(grupoId, currentUser, db, ct) is null)
            return TypedResults.NotFound();

        var fecha = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var resultado = await db.Students
            .AsNoTracking()
            .Where(s => s.GroupId == grupoId && s.IsActive)
            .OrderBy(s => s.FullName)
            .Select(s => new AsistenciaDiaResponse(
                s.Id,
                s.FullName,
                s.StudentCode,
                s.QrCode,
                db.AttendanceRecords
                    .Where(r => r.StudentId == s.Id && r.Date == fecha)
                    .Select(r => (AttendanceStatus?)r.Status)
                    .FirstOrDefault(),
                db.AttendanceRecords
                    .Where(r => r.StudentId == s.Id && r.Date == fecha)
                    .Select(r => r.Notes)
                    .FirstOrDefault()))
            .ToListAsync(ct);

        return TypedResults.Ok(new AsistenciaGrupoResponse(grupoId, fecha, resultado));
    }

    // POST /api/grupos/{grupoId}/asistencia
    // Upsert de toda la lista de asistencia para una fecha.
    // Permite tomar lista manual o sincronizar desde offline (PowerSync).
    private static async Task<IResult> UpsertAsync(
        Guid grupoId, UpsertAsistenciaRequest request,
        ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        if (await ResolveGroupAsync(grupoId, currentUser, db, ct) is null)
            return TypedResults.NotFound();

        var studentIds = request.Records.Select(r => r.StudentId).ToHashSet();

        var validIds = await db.Students
            .Where(s => studentIds.Contains(s.Id) && s.GroupId == grupoId && s.IsActive)
            .Select(s => s.Id)
            .ToHashSetAsync(ct);

        if (validIds.Count != studentIds.Count)
            return TypedResults.BadRequest("Uno o más estudiantes no pertenecen a este grupo.");

        var existing = await db.AttendanceRecords
            .Where(r => r.GroupId == grupoId && r.Date == request.Date)
            .ToDictionaryAsync(r => r.StudentId, ct);

        foreach (var rec in request.Records)
        {
            if (existing.TryGetValue(rec.StudentId, out var current))
            {
                current.Status = rec.Status;
                current.Notes  = rec.Notes;
            }
            else
            {
                db.AttendanceRecords.Add(new AttendanceRecord
                {
                    GroupId   = grupoId,
                    StudentId = rec.StudentId,
                    Date      = request.Date,
                    Status    = rec.Status,
                    Notes     = rec.Notes
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    // POST /api/asistencia/qr
    // El docente escanea el QR de un alumno en el aula.
    // Retorna el nombre del estudiante para confirmación visual inmediata.
    private static async Task<IResult> ScanQrAsync(
        ScanQrRequest request,
        ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        if (await ResolveGroupAsync(request.GrupoId, currentUser, db, ct) is null)
            return TypedResults.NotFound();

        var student = await db.Students
            .FirstOrDefaultAsync(s => s.QrCode == request.QrCode && s.GroupId == request.GrupoId && s.IsActive, ct);

        if (student is null)
            return TypedResults.NotFound(new { message = "QR no corresponde a un estudiante de este grupo." });

        var fecha  = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var status = request.Status ?? AttendanceStatus.Present;

        var record = await db.AttendanceRecords
            .FirstOrDefaultAsync(r => r.StudentId == student.Id && r.Date == fecha, ct);

        if (record is null)
        {
            db.AttendanceRecords.Add(new AttendanceRecord
            {
                GroupId   = request.GrupoId,
                StudentId = student.Id,
                Date      = fecha,
                Status    = status
            });
        }
        else
        {
            record.Status = status;
        }

        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(new ScanQrResponse(student.Id, student.FullName, student.StudentCode, fecha, status));
    }

    // GET /api/grupos/{grupoId}/asistencia/historial?from=2026-04-01&to=2026-04-30
    // Devuelve la matriz de asistencia: estudiantes × fechas registradas en el rango.
    // El frontend puede pivotar la respuesta para mostrar la tabla.
    private static async Task<IResult> GetHistorialAsync(
        Guid grupoId, DateOnly? from, DateOnly? to,
        ICurrentUserService currentUser, AulaIADbContext db, CancellationToken ct)
    {
        if (await ResolveGroupAsync(grupoId, currentUser, db, ct) is null)
            return TypedResults.NotFound();

        var inicio = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var fin    = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);

        if (fin < inicio)
            return TypedResults.BadRequest("La fecha 'to' debe ser mayor o igual a 'from'.");

        var estudiantes = await db.Students
            .AsNoTracking()
            .Where(s => s.GroupId == grupoId && s.IsActive)
            .OrderBy(s => s.FullName)
            .Select(s => new { s.Id, s.FullName, s.StudentCode })
            .ToListAsync(ct);

        var registros = await db.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.GroupId == grupoId && r.Date >= inicio && r.Date <= fin)
            .Select(r => new { r.StudentId, r.Date, r.Status })
            .ToListAsync(ct);

        var fechas = registros
            .Select(r => r.Date.ToString("yyyy-MM-dd"))
            .Distinct()
            .OrderBy(f => f)
            .ToList();

        var lookup = registros
            .GroupBy(r => r.StudentId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(r => r.Date.ToString("yyyy-MM-dd"), r => r.Status.ToString()));

        var filas = estudiantes.Select(s => new HistorialEstudianteRow(
            s.Id,
            s.FullName,
            s.StudentCode,
            fechas.ToDictionary(
                f => f,
                f => lookup.TryGetValue(s.Id, out var byDate) && byDate.TryGetValue(f, out var st) ? st : null)
        )).ToList();

        return TypedResults.Ok(new HistorialAsistenciaResponse(grupoId, inicio, fin, fechas, filas));
    }
}

// ── DTOs ────────────────────────────────────────────────────────────────────

public sealed record AsistenciaGrupoResponse(
    Guid GrupoId,
    DateOnly Date,
    IReadOnlyList<AsistenciaDiaResponse> Students);

public sealed record AsistenciaDiaResponse(
    Guid StudentId,
    string FullName,
    string StudentCode,
    string QrCode,
    AttendanceStatus? Status,
    string? Notes);

public sealed record AsistenciaRecordRequest(
    [property: Required] Guid StudentId,
    AttendanceStatus Status,
    [property: StringLength(500)] string? Notes);

public sealed record UpsertAsistenciaRequest(
    DateOnly Date,
    [property: Required, MinLength(1)] IReadOnlyList<AsistenciaRecordRequest> Records);

public sealed record ScanQrRequest(
    [property: Required] Guid GrupoId,
    [property: Required, StringLength(32, MinimumLength = 32)] string QrCode,
    DateOnly? Date,
    AttendanceStatus? Status);

public sealed record ScanQrResponse(
    Guid StudentId,
    string FullName,
    string StudentCode,
    DateOnly Date,
    AttendanceStatus Status);

public sealed record HistorialAsistenciaResponse(
    Guid GrupoId,
    DateOnly From,
    DateOnly To,
    IReadOnlyList<string> Fechas,
    IReadOnlyList<HistorialEstudianteRow> Filas);

public sealed record HistorialEstudianteRow(
    Guid StudentId,
    string FullName,
    string StudentCode,
    IReadOnlyDictionary<string, string?> Asistencia); // key = "yyyy-MM-dd", value = status string | null
