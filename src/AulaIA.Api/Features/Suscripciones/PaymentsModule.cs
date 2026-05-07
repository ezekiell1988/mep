using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Features.Suscripciones;

public static class PaymentsModule
{
    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/pagos")
                       .WithTags("Admin - Pagos")
                       .RequireAuthorization("admin");

        // ── GET /api/admin/pagos/pendientes ───────────────────────────────
        group.MapGet("/pendientes", GetPendientesAsync).WithName("GetPagosPendientes");

        // ── GET /api/admin/pagos/historial ────────────────────────────────
        group.MapGet("/historial", GetHistorialAsync).WithName("GetPagosHistorial");

        // ── POST /api/admin/pagos/{id}/aprobar ────────────────────────────
        group.MapPost("/{id:guid}/aprobar", AprobarPagoAsync).WithName("AprobarPago");

        // ── POST /api/admin/pagos/{id}/rechazar ───────────────────────────
        group.MapPost("/{id:guid}/rechazar", RechazarPagoAsync).WithName("RechazarPago");

        // ── GET /api/admin/suscripciones ──────────────────────────────────
        var subGroup = app.MapGroup("/api/admin/suscripciones")
                          .WithTags("Admin - Suscripciones")
                          .RequireAuthorization("admin");

        subGroup.MapGet("", GetSuscripcionesAsync).WithName("GetSuscripciones");

        return app;
    }

    // ── GET /api/admin/pagos/pendientes ───────────────────────────────────
    private static async Task<Ok<IReadOnlyList<AdminPagoResponse>>> GetPendientesAsync(
        AulaIADbContext db,
        CancellationToken ct)
    {
        var items = await db.PaymentRequests
            .Include(p => p.User)
            .Where(p => p.Status == PaymentRequestStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .Select(p => ToAdminResponse(p))
            .ToListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<AdminPagoResponse>>(items);
    }

    // ── GET /api/admin/pagos/historial ────────────────────────────────────
    private static async Task<Ok<IReadOnlyList<AdminPagoResponse>>> GetHistorialAsync(
        AulaIADbContext db,
        CancellationToken ct)
    {
        var items = await db.PaymentRequests
            .Include(p => p.User)
            .Where(p => p.Status != PaymentRequestStatus.Pending)
            .OrderByDescending(p => p.ReviewedAt)
            .Take(200)
            .Select(p => ToAdminResponse(p))
            .ToListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<AdminPagoResponse>>(items);
    }

    // ── POST /api/admin/pagos/{id}/aprobar ────────────────────────────────
    private static async Task<Results<Ok, NotFound, Conflict<string>>> AprobarPagoAsync(
        Guid id,
        AprobarPagoRequest req,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var admin = await currentUser.ResolveAsync(ct);

        var payment = await db.PaymentRequests
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (payment is null) return TypedResults.NotFound();
        if (payment.Status != PaymentRequestStatus.Pending)
            return TypedResults.Conflict("Este pago ya fue procesado.");

        var now = DateTime.UtcNow;
        payment.Status = PaymentRequestStatus.Approved;
        payment.AdminNote = req.Nota;
        payment.ReviewedByAuth0Sub = admin.Auth0Sub;
        payment.ReviewedAt = now;
        payment.UpdatedAt = now;

        // Activar o renovar suscripción
        var sub = await db.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == payment.UserId, ct);

        if (sub is null)
        {
            sub = new Subscription
            {
                UserId = payment.UserId,
                UpdatedAt = now
            };
            db.Subscriptions.Add(sub);
        }

        var periodStart = sub.Status == SubscriptionStatus.Active && sub.CurrentPeriodEnd > now
            ? sub.CurrentPeriodEnd  // renovación: extiende desde el fin actual
            : now;

        sub.Plan = payment.Plan;
        sub.Status = SubscriptionStatus.Active;
        sub.IsTrial = false;
        sub.CurrentPeriodStart = periodStart;
        sub.CurrentPeriodEnd = periodStart.AddMonths(1);
        sub.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        return TypedResults.Ok();
    }

    // ── POST /api/admin/pagos/{id}/rechazar ───────────────────────────────
    private static async Task<Results<Ok, NotFound, Conflict<string>>> RechazarPagoAsync(
        Guid id,
        RechazarPagoRequest req,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var admin = await currentUser.ResolveAsync(ct);

        var payment = await db.PaymentRequests
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (payment is null) return TypedResults.NotFound();
        if (payment.Status != PaymentRequestStatus.Pending)
            return TypedResults.Conflict("Este pago ya fue procesado.");

        var now = DateTime.UtcNow;
        payment.Status = PaymentRequestStatus.Rejected;
        payment.AdminNote = req.Nota;
        payment.ReviewedByAuth0Sub = admin.Auth0Sub;
        payment.ReviewedAt = now;
        payment.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        return TypedResults.Ok();
    }

    // ── GET /api/admin/suscripciones ──────────────────────────────────────
    private static async Task<Ok<IReadOnlyList<AdminSuscripcionResponse>>> GetSuscripcionesAsync(
        AulaIADbContext db,
        CancellationToken ct)
    {
        var items = await db.Subscriptions
            .Include(s => s.User)
            .OrderByDescending(s => s.CurrentPeriodEnd)
            .Select(s => new AdminSuscripcionResponse(
                s.Id,
                s.User!.FullName,
                s.User.Email,
                s.Plan.ToString(),
                s.Status.ToString(),
                s.IsTrial,
                s.CurrentPeriodStart,
                s.CurrentPeriodEnd,
                (int)Math.Max(0, (s.CurrentPeriodEnd - DateTime.UtcNow).TotalDays)))
            .ToListAsync(ct);

        return TypedResults.Ok<IReadOnlyList<AdminSuscripcionResponse>>(items);
    }

    private static AdminPagoResponse ToAdminResponse(PaymentRequest p) =>
        new(p.Id,
            p.User!.FullName,
            p.User.Email,
            p.Plan.ToString(),
            p.AmountUsd,
            p.AmountCrc,
            p.ReferenceCode,
            p.Status.ToString(),
            p.VoucherBlobPath != null,
            p.AdminNote,
            p.CreatedAt,
            p.ReviewedAt);
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record AdminPagoResponse(
    Guid Id,
    string UserName,
    string UserEmail,
    string Plan,
    decimal AmountUsd,
    decimal AmountCrc,
    string ReferenceCode,
    string Status,
    bool HasVoucher,
    string? AdminNote,
    DateTime CreatedAt,
    DateTime? ReviewedAt);

public record AprobarPagoRequest(string? Nota);
public record RechazarPagoRequest(string Nota);

public record AdminSuscripcionResponse(
    Guid Id,
    string UserName,
    string UserEmail,
    string Plan,
    string Status,
    bool IsTrial,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int DaysRemaining);
