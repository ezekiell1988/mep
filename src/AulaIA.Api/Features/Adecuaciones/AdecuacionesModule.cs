using AulaIA.Api.Features.Adecuaciones.Jobs;
using AulaIA.Api.Features.Adecuaciones.Services;
using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AulaIA.Api.Features.Adecuaciones;

public static class AdecuacionesModule
{
    public static IServiceCollection AddAdecuacionesModule(this IServiceCollection services)
    {
        services.AddScoped<AdecuacionAiService>();
        services.AddScoped<GenerarAdecuacionJob>();
        services.AddScoped<InformeAdecuacionService>();
        return services;
    }

    public static IEndpointRouteBuilder MapAdecuacionesEndpoints(this IEndpointRouteBuilder app)
    {
        var byGroup = app.MapGroup("/api/grupos/{grupoId:guid}")
                         .WithTags("Adecuaciones")
                         .RequireAuthorization();

        // ── Listar todas las adecuaciones del grupo ────────────────────────
        byGroup.MapGet("/adecuaciones", ListAdecuacionesAsync)
               .WithName("ListAdecuaciones");

        // ── CRUD por estudiante ────────────────────────────────────────────
        byGroup.MapGet("/estudiantes/{studentId:guid}/adecuacion", GetAdecuacionAsync)
               .WithName("GetAdecuacion");

        byGroup.MapPut("/estudiantes/{studentId:guid}/adecuacion", UpsertAdecuacionAsync)
               .WithName("UpsertAdecuacion");

        byGroup.MapDelete("/estudiantes/{studentId:guid}/adecuacion", DeleteAdecuacionAsync)
               .WithName("DeleteAdecuacion");

        // ── Generación IA ──────────────────────────────────────────────────
        byGroup.MapPost("/estudiantes/{studentId:guid}/adecuacion/generar", GenerarPropuestaAsync)
               .WithName("GenerarPropuestaAdecuacion");

        // ── Informe PDF para expediente CAE ───────────────────────────────
        byGroup.MapGet("/estudiantes/{studentId:guid}/adecuacion/informe", DescargarInformeAsync)
               .WithName("DescargarInformeAdecuacion");

        return app;
    }

    // ── GET /api/grupos/{grupoId}/adecuaciones ─────────────────────────────
    private static async Task<Ok<IReadOnlyList<AdecuacionResumen>>> ListAdecuacionesAsync(
        Guid grupoId,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);
        if (user is null) return TypedResults.Ok<IReadOnlyList<AdecuacionResumen>>([]);

        var ownGroup = await db.Groups
            .AnyAsync(g => g.Id == grupoId && g.TeacherSub == user.Auth0Sub, ct);
        if (!ownGroup) return TypedResults.Ok<IReadOnlyList<AdecuacionResumen>>([]);

        var items = await db.Accommodations
            .Include(a => a.Student)
            .Where(a => a.GroupId == grupoId)
            .OrderBy(a => a.Student!.FullName)
            .Select(a => new AdecuacionResumen(
                a.Id,
                a.StudentId,
                a.Student!.FullName,
                a.Student.StudentCode,
                a.Type.ToString(),
                a.Diagnostico,
                a.Status.ToString(),
                a.GeneratedAt))
            .ToListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<AdecuacionResumen>>(items);
    }

    // ── GET /api/grupos/{grupoId}/estudiantes/{studentId}/adecuacion ───────
    private static async Task<Results<Ok<AdecuacionResponse>, NotFound>> GetAdecuacionAsync(
        Guid grupoId,
        Guid studentId,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);
        if (user is null) return TypedResults.NotFound();

        var acc = await db.Accommodations
            .Include(a => a.Student)
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.GroupId == grupoId, ct);

        if (acc is null) return TypedResults.NotFound();

        var ownGroup = await db.Groups
            .AnyAsync(g => g.Id == grupoId && g.TeacherSub == user.Auth0Sub, ct);
        if (!ownGroup) return TypedResults.NotFound();

        return TypedResults.Ok(ToResponse(acc));
    }

    // ── PUT /api/grupos/{grupoId}/estudiantes/{studentId}/adecuacion ───────
    private static async Task<Results<Ok<AdecuacionResponse>, BadRequest<string>, NotFound>> UpsertAdecuacionAsync(
        Guid grupoId,
        Guid studentId,
        UpsertAdecuacionRequest req,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);
        if (user is null) return TypedResults.NotFound();

        var group = await db.Groups
            .FirstOrDefaultAsync(g => g.Id == grupoId && g.TeacherSub == user.Auth0Sub, ct);
        if (group is null) return TypedResults.BadRequest("Grupo no encontrado o no pertenece al docente.");

        var student = await db.Students
            .FirstOrDefaultAsync(s => s.Id == studentId && s.GroupId == grupoId, ct);
        if (student is null) return TypedResults.BadRequest("Estudiante no encontrado en este grupo.");

        if (!Enum.TryParse<AccommodationType>(req.Type, ignoreCase: true, out var tipo))
            return TypedResults.BadRequest($"Tipo inválido: '{req.Type}'. Válidos: AS, ANS, AA.");

        var acc = await db.Accommodations
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.GroupId == grupoId, ct);

        if (acc is null)
        {
            acc = new Accommodation
            {
                StudentId = studentId,
                GroupId = grupoId,
                Diagnostico = req.Diagnostico,
                CreatedByAuth0Sub = user.Auth0Sub
            };
            db.Accommodations.Add(acc);
        }

        acc.Type = tipo;
        acc.Diagnostico = req.Diagnostico;
        acc.CondicionEspecial = req.CondicionEspecial;
        acc.EstrategiasMediacion = req.EstrategiasMediacion;
        acc.EstrategiasEvaluacion = req.EstrategiasEvaluacion;
        acc.Observaciones = req.Observaciones;
        acc.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return TypedResults.Ok(ToResponse(acc));
    }

    // ── DELETE /api/grupos/{grupoId}/estudiantes/{studentId}/adecuacion ────
    private static async Task<Results<NoContent, NotFound, ForbidHttpResult>> DeleteAdecuacionAsync(
        Guid grupoId,
        Guid studentId,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);
        if (user is null) return TypedResults.NotFound();

        var acc = await db.Accommodations
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.GroupId == grupoId, ct);
        if (acc is null) return TypedResults.NotFound();

        var ownGroup = await db.Groups
            .AnyAsync(g => g.Id == grupoId && g.TeacherSub == user.Auth0Sub, ct);
        if (!ownGroup) return TypedResults.Forbid();

        db.Accommodations.Remove(acc);
        await db.SaveChangesAsync(ct);
        return TypedResults.NoContent();
    }

    // ── POST /api/grupos/{grupoId}/estudiantes/{studentId}/adecuacion/generar
    private static async Task<Results<Accepted<AdecuacionResponse>, BadRequest<string>, NotFound>> GenerarPropuestaAsync(
        Guid grupoId,
        Guid studentId,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        IBackgroundJobClient jobs,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);
        if (user is null) return TypedResults.NotFound();

        var ownGroup = await db.Groups
            .AnyAsync(g => g.Id == grupoId && g.TeacherSub == user.Auth0Sub, ct);
        if (!ownGroup) return TypedResults.BadRequest("Grupo no encontrado o no pertenece al docente.");

        var acc = await db.Accommodations
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.GroupId == grupoId, ct);
        if (acc is null)
            return TypedResults.BadRequest("Debe crear la adecuación antes de generar la propuesta.");

        if (acc.Status == AccommodationStatus.Generating || acc.Status == AccommodationStatus.Pending)
            return TypedResults.BadRequest("La propuesta ya está siendo generada.");

        acc.Status = AccommodationStatus.Pending;
        acc.ErrorMessage = null;
        acc.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        jobs.Enqueue<GenerarAdecuacionJob>(j => j.ExecuteAsync(acc.Id, CancellationToken.None));

        return TypedResults.Accepted(
            $"/api/grupos/{grupoId}/estudiantes/{studentId}/adecuacion",
            ToResponse(acc));
    }

    // ── GET /api/grupos/{grupoId}/estudiantes/{studentId}/adecuacion/informe
    private static async Task<Results<FileContentHttpResult, NotFound, BadRequest<string>>> DescargarInformeAsync(
        Guid grupoId,
        Guid studentId,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        InformeAdecuacionService pdfService,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);
        if (user is null) return TypedResults.NotFound();

        var ownGroup = await db.Groups
            .AnyAsync(g => g.Id == grupoId && g.TeacherSub == user.Auth0Sub, ct);
        if (!ownGroup) return TypedResults.NotFound();

        var acc = await db.Accommodations
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.GroupId == grupoId, ct);
        if (acc is null) return TypedResults.NotFound();

        if (acc.Status != AccommodationStatus.Ready && acc.Status != AccommodationStatus.Draft)
            return TypedResults.BadRequest("La propuesta aún no ha sido generada.");

        var pdf = await pdfService.GeneratePdfAsync(acc.Id, ct);
        var student = await db.Students.FindAsync([studentId], ct);
        var fileName = $"adecuacion_{student?.StudentCode ?? studentId.ToString("N")}.pdf";

        return TypedResults.File(pdf, "application/pdf", fileName);
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private static AdecuacionResponse ToResponse(Accommodation acc) =>
        new(acc.Id, acc.StudentId, acc.GroupId,
            acc.Type.ToString(), acc.Diagnostico,
            acc.CondicionEspecial, acc.EstrategiasMediacion,
            acc.EstrategiasEvaluacion, acc.Observaciones,
            acc.PropuestaGenerada, acc.Status.ToString(),
            acc.GeneratedAt, acc.ErrorMessage,
            acc.CreatedAt, acc.UpdatedAt);
}

// ── DTOs ───────────────────────────────────────────────────────────────────

public record AdecuacionResumen(
    Guid Id,
    Guid StudentId,
    string StudentName,
    string StudentCode,
    string Type,
    string Diagnostico,
    string Status,
    DateTimeOffset? GeneratedAt);

public record AdecuacionResponse(
    Guid Id,
    Guid StudentId,
    Guid GroupId,
    string Type,
    string Diagnostico,
    string? CondicionEspecial,
    string? EstrategiasMediacion,
    string? EstrategiasEvaluacion,
    string? Observaciones,
    string? PropuestaGenerada,
    string Status,
    DateTimeOffset? GeneratedAt,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record UpsertAdecuacionRequest(
    [Required] string Type,
    [Required, MaxLength(500)] string Diagnostico,
    [MaxLength(300)] string? CondicionEspecial,
    string? EstrategiasMediacion,
    string? EstrategiasEvaluacion,
    [MaxLength(1000)] string? Observaciones);
