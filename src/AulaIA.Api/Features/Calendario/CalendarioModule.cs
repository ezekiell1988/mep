using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AulaIA.Api.Features.Calendario;

public static class CalendarioModule
{
    public static IServiceCollection AddCalendarioModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapCalendarioEndpoints(this IEndpointRouteBuilder app)
    {
        var byGroup = app.MapGroup("/api/grupos/{grupoId:guid}/calendario")
                         .WithTags("Calendario")
                         .RequireAuthorization();

        byGroup.MapGet("/", GetEventosAsync).WithName("GetCalendario");
        byGroup.MapGet("/lecciones", GetLeccionesDisponiblesAsync).WithName("GetLeccionesDisponibles");
        byGroup.MapPost("/", CrearEventoAsync).WithName("CrearEventoCalendario");
        byGroup.MapDelete("/{id:guid}", EliminarEventoAsync).WithName("EliminarEventoCalendario");

        return app;
    }

    // ── GET /api/grupos/{grupoId}/calendario?year=2026&month=4 ────────────
    private static async Task<Ok<IReadOnlyList<CalendarEventResponse>>> GetEventosAsync(
        Guid grupoId,
        int? year,
        int? month,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var schoolYear = year ?? DateTime.UtcNow.Year;

        var query = db.CalendarEvents
            .Where(e => e.SchoolYear == schoolYear &&
                        (e.GroupId == null || e.GroupId == grupoId));

        if (month.HasValue)
            query = query.Where(e => e.Date.Month == month.Value);

        var events = await query
            .OrderBy(e => e.Date)
            .Select(e => new CalendarEventResponse(
                e.Id,
                e.GroupId,
                e.Date,
                e.EndDate,
                e.Title,
                e.Type.ToString(),
                e.GroupId == null,
                e.GroupId == grupoId))
            .ToListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<CalendarEventResponse>>(events);
    }

    // ── GET /api/grupos/{grupoId}/calendario/lecciones?from=&to=&leccionesPorSemana= ─
    private static async Task<Results<Ok<LeccionesDisponiblesResponse>, BadRequest<string>>> GetLeccionesDisponiblesAsync(
        Guid grupoId,
        DateOnly from,
        DateOnly to,
        int leccionesPorSemana,
        AulaIADbContext db,
        CancellationToken ct)
    {
        if (to < from)
            return TypedResults.BadRequest("La fecha 'to' debe ser igual o posterior a 'from'.");
        if (leccionesPorSemana < 1 || leccionesPorSemana > 40)
            return TypedResults.BadRequest("leccionesPorSemana debe estar entre 1 y 40.");

        var schoolYear = from.Year;

        // Eventos no lectivos en el rango (nacionales + del grupo)
        var eventos = await db.CalendarEvents
            .Where(e => e.SchoolYear == schoolYear &&
                        (e.GroupId == null || e.GroupId == grupoId) &&
                        e.Date <= to &&
                        (e.EndDate == null ? e.Date >= from : e.EndDate >= from))
            .ToListAsync(ct);

        int diasHabiles = CountWeekdays(from, to);

        // Días hábiles cubiertos por eventos no lectivos
        var diasNoLectivosSet = new HashSet<DateOnly>();
        foreach (var evt in eventos)
        {
            var evtEnd = evt.EndDate ?? evt.Date;
            for (var d = evt.Date; d <= evtEnd; d = d.AddDays(1))
                if (d >= from && d <= to && IsWeekday(d))
                    diasNoLectivosSet.Add(d);
        }

        int diasNoLectivos = diasNoLectivosSet.Count;
        int diasEfectivos = diasHabiles - diasNoLectivos;
        int leccionesDisponibles = (int)Math.Floor(diasEfectivos * leccionesPorSemana / 5.0);

        return TypedResults.Ok(new LeccionesDisponiblesResponse(
            from, to, leccionesPorSemana,
            diasHabiles, diasNoLectivos, diasEfectivos, leccionesDisponibles));
    }

    // ── POST /api/grupos/{grupoId}/calendario ──────────────────────────────
    private static async Task<Results<Created<CalendarEventResponse>, BadRequest<string>, NotFound>> CrearEventoAsync(
        Guid grupoId,
        CrearEventoRequest req,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);
        if (user is null) return TypedResults.NotFound();

        var group = await db.Groups
            .FirstOrDefaultAsync(g => g.Id == grupoId && g.TeacherSub == user.Auth0Sub, ct);
        if (group is null)
            return TypedResults.BadRequest("Grupo no encontrado o no pertenece al docente.");

        if (req.EndDate.HasValue && req.EndDate.Value < req.Date)
            return TypedResults.BadRequest("La fecha de fin no puede ser anterior a la fecha de inicio.");

        if (!Enum.TryParse<CalendarEventType>(req.Type, ignoreCase: true, out var eventType))
            return TypedResults.BadRequest($"Tipo de evento inválido: '{req.Type}'. Valores válidos: {string.Join(", ", Enum.GetNames<CalendarEventType>())}");

        var evt = new CalendarEvent
        {
            GroupId = grupoId,
            Date = req.Date,
            EndDate = req.EndDate,
            Title = req.Title,
            Type = eventType,
            SchoolYear = req.Date.Year,
            CreatedByAuth0Sub = user.Auth0Sub
        };

        db.CalendarEvents.Add(evt);
        await db.SaveChangesAsync(ct);

        return TypedResults.Created(
            $"/api/grupos/{grupoId}/calendario/{evt.Id}",
            new CalendarEventResponse(
                evt.Id, evt.GroupId, evt.Date, evt.EndDate,
                evt.Title, evt.Type.ToString(), false, true));
    }

    // ── DELETE /api/grupos/{grupoId}/calendario/{id} ───────────────────────
    private static async Task<Results<NoContent, NotFound, ForbidHttpResult>> EliminarEventoAsync(
        Guid grupoId,
        Guid id,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);
        if (user is null) return TypedResults.NotFound();

        var evt = await db.CalendarEvents
            .FirstOrDefaultAsync(e => e.Id == id && e.GroupId == grupoId, ct);
        if (evt is null) return TypedResults.NotFound();

        var group = await db.Groups
            .FirstOrDefaultAsync(g => g.Id == grupoId && g.TeacherSub == user.Auth0Sub, ct);
        if (group is null) return TypedResults.Forbid();

        db.CalendarEvents.Remove(evt);
        await db.SaveChangesAsync(ct);

        return TypedResults.NoContent();
    }

    // ── Utilidades ─────────────────────────────────────────────────────────
    private static int CountWeekdays(DateOnly from, DateOnly to)
    {
        int count = 0;
        for (var d = from; d <= to; d = d.AddDays(1))
            if (IsWeekday(d)) count++;
        return count;
    }

    private static bool IsWeekday(DateOnly d) =>
        d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;
}

// ── DTOs ───────────────────────────────────────────────────────────────────
public record CalendarEventResponse(
    Guid Id,
    Guid? GroupId,
    DateOnly Date,
    DateOnly? EndDate,
    string Title,
    string Type,
    bool IsNational,
    bool IsEditable);

public record CrearEventoRequest(
    [Required] DateOnly Date,
    DateOnly? EndDate,
    [Required, MaxLength(200)] string Title,
    [Required] string Type);

public record LeccionesDisponiblesResponse(
    DateOnly From,
    DateOnly To,
    int LeccionesPorSemana,
    int DiasHabiles,
    int DiasNoLectivos,
    int DiasEfectivos,
    int LeccionesDisponibles);
