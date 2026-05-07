using AulaIA.Api.Features.Suscripciones.Jobs;
using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Features.Suscripciones;

public static class ReferralsModule
{
    public static IEndpointRouteBuilder MapReferralsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/referidos")
                       .WithTags("Referidos")
                       .RequireAuthorization();

        // ── GET /api/referidos/mi-codigo — obtener o crear código personal ─
        group.MapGet("/mi-codigo", GetOrCreateCodeAsync).WithName("GetOrCreateReferralCode");

        // ── GET /api/referidos/panel — lista de usuarios referidos ─────────
        group.MapGet("/panel", GetPanelAsync).WithName("GetReferralPanel");

        // ── GET /api/referidos/comisiones — historial de comisiones ────────
        group.MapGet("/comisiones", GetComisionesAsync).WithName("GetComisiones");

        // ── Admin: POST /api/admin/referidos/cierre-mensual ────────────────
        var adminGroup = app.MapGroup("/api/admin/referidos")
                            .WithTags("Admin - Referidos")
                            .RequireAuthorization("admin");

        adminGroup.MapPost("/cierre-mensual", EjecutarCierreMensualAsync)
                  .WithName("EjecutarCierreMensual");

        adminGroup.MapGet("/comisiones", GetAdminComisionesAsync)
                  .WithName("GetAdminComisiones");

        adminGroup.MapPost("/comisiones/{id:guid}/pagar", MarcarComisionPagadaAsync)
                  .WithName("MarcarComisionPagada");

        return app;
    }

    // ── GET /api/referidos/mi-codigo ──────────────────────────────────────
    private static async Task<Ok<ReferralCodeResponse>> GetOrCreateCodeAsync(
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        var code = await db.ReferralCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == user.Id, ct);

        if (code is null)
        {
            code = new ReferralCode
            {
                UserId = user.Id,
                Code = GenerateReferralCode(user.FullName),
                IsActive = true
            };
            db.ReferralCodes.Add(code);
            await db.SaveChangesAsync(ct);
        }

        return TypedResults.Ok(new ReferralCodeResponse(
            code.Id, code.Code, code.IsActive, code.CreatedAt));
    }

    // ── GET /api/referidos/panel ──────────────────────────────────────────
    private static async Task<Ok<ReferralPanelResponse>> GetPanelAsync(
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        var code = await db.ReferralCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == user.Id, ct);

        if (code is null)
            return TypedResults.Ok(new ReferralPanelResponse(null, 0, 0m, []));

        var referidos = await db.Users
            .Where(u => u.ReferredByCode == code.Code)
            .Include(u => u.Subscription)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new ReferidoResumen(
                u.FullName,
                u.Email,
                u.Subscription != null ? u.Subscription.Plan.ToString() : "Sin plan",
                u.Subscription != null ? u.Subscription.Status.ToString() : "Sin suscripción",
                u.CreatedAt))
            .ToListAsync(ct);

        var totalComisiones = await db.Commissions
            .Where(c => c.ReferralCodeId == code.Id)
            .SumAsync(c => c.CommissionAmountCrc, ct);

        return TypedResults.Ok(new ReferralPanelResponse(
            code.Code,
            referidos.Count,
            totalComisiones,
            referidos));
    }

    // ── GET /api/referidos/comisiones ─────────────────────────────────────
    private static async Task<Ok<IReadOnlyList<ComisionResponse>>> GetComisionesAsync(
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        var code = await db.ReferralCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == user.Id, ct);

        if (code is null)
            return TypedResults.Ok<IReadOnlyList<ComisionResponse>>([]);

        var items = await db.Commissions
            .Where(c => c.ReferralCodeId == code.Id)
            .Include(c => c.ReferredUser)
            .OrderByDescending(c => c.Month)
            .Select(c => new ComisionResponse(
                c.Id,
                c.ReferredUser!.FullName,
                c.Month,
                c.GrossRevenueCrc,
                c.InfraCostCrc,
                c.BaseAmountCrc,
                c.CommissionAmountCrc,
                c.Status.ToString(),
                c.CreatedAt))
            .ToListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<ComisionResponse>>(items);
    }

    // ── POST /api/admin/referidos/cierre-mensual ──────────────────────────
    private static async Task<Results<Accepted<string>, BadRequest<string>>> EjecutarCierreMensualAsync(
        CierreMensualRequest req,
        AulaIADbContext db,
        CancellationToken ct)
    {
        if (req.InfraCostCrc <= 0)
            return TypedResults.BadRequest("El costo de infraestructura debe ser mayor a 0.");

        if (req.Month < 202501 || req.Month > 209912)
            return TypedResults.BadRequest("Período inválido. Use formato YYYYMM.");

        // Encolar job en background (puede tardar para muchos usuarios)
        BackgroundJob.Enqueue<CalculateCommissionsJob>(
            j => j.ExecuteAsync(req.Month, req.InfraCostCrc, CancellationToken.None));

        return TypedResults.Accepted<string>("/api/admin/referidos/comisiones", $"Job de cierre mensual para {req.Month} encolado.");
    }

    // ── GET /api/admin/referidos/comisiones ───────────────────────────────
    private static async Task<Ok<IReadOnlyList<AdminComisionResponse>>> GetAdminComisionesAsync(
        AulaIADbContext db,
        CancellationToken ct)
    {
        var items = await db.Commissions
            .Include(c => c.ReferralCode).ThenInclude(rc => rc!.User)
            .Include(c => c.ReferredUser)
            .OrderByDescending(c => c.Month)
            .Select(c => new AdminComisionResponse(
                c.Id,
                c.ReferralCode!.Code,
                c.ReferralCode.User!.FullName,
                c.ReferredUser!.FullName,
                c.Month,
                c.CommissionAmountCrc,
                c.Status.ToString()))
            .ToListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<AdminComisionResponse>>(items);
    }

    // ── POST /api/admin/referidos/comisiones/{id}/pagar ───────────────────
    private static async Task<Results<Ok, NotFound>> MarcarComisionPagadaAsync(
        Guid id,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var commission = await db.Commissions.FindAsync([id], ct);
        if (commission is null) return TypedResults.NotFound();

        commission.Status = CommissionStatus.Paid;
        await db.SaveChangesAsync(ct);
        return TypedResults.Ok();
    }

    private static string GenerateReferralCode(string fullName)
    {
        var base64 = fullName.ToUpperInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "USER";
        base64 = new string(base64.Where(char.IsLetterOrDigit).Take(10).ToArray());
        return $"{base64}{DateTime.UtcNow.Year}";
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record ReferralCodeResponse(Guid Id, string Code, bool IsActive, DateTime CreatedAt);

public record ReferralPanelResponse(
    string? Code,
    int TotalReferidos,
    decimal TotalComisionesCrc,
    IReadOnlyList<ReferidoResumen> Referidos);

public record ReferidoResumen(
    string Nombre,
    string Email,
    string Plan,
    string Estado,
    DateTime Registrado);

public record ComisionResponse(
    Guid Id,
    string ReferidoNombre,
    int Month,
    decimal GrossRevenueCrc,
    decimal InfraCostCrc,
    decimal BaseAmountCrc,
    decimal CommissionAmountCrc,
    string Status,
    DateTime CreatedAt);

public record CierreMensualRequest(int Month, decimal InfraCostCrc);

public record AdminComisionResponse(
    Guid Id,
    string CodigoReferido,
    string ReferidorNombre,
    string ReferidoNombre,
    int Month,
    decimal CommissionAmountCrc,
    string Status);
