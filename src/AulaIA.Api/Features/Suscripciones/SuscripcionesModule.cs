using AulaIA.Api.Features.Suscripciones.Jobs;
using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using Azure.Storage.Blobs;
using Hangfire;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AulaIA.Api.Features.Suscripciones;

public static class SuscripcionesModule
{
    public static IServiceCollection AddSuscripcionesModule(this IServiceCollection services)
    {
        services.AddScoped<UpdateExchangeRateJob>();
        services.AddScoped<CheckExpiredSubscriptionsJob>();
        services.AddScoped<CalculateCommissionsJob>();
        return services;
    }

    public static IEndpointRouteBuilder MapSuscripcionesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suscripcion")
                       .WithTags("Suscripciones")
                       .RequireAuthorization();

        // ── GET /api/suscripcion — estado actual del usuario ──────────────
        group.MapGet("", GetEstadoAsync).WithName("GetSuscripcionEstado");

        // ── POST /api/suscripcion/trial — activar trial 30 días ───────────
        group.MapPost("/trial", ActivarTrialAsync).WithName("ActivarTrial");

        // ── POST /api/suscripcion/pago — solicitar pago SINPE ─────────────
        group.MapPost("/pago", SolicitarPagoAsync).WithName("SolicitarPago");

        // ── POST /api/suscripcion/pago/{id}/comprobante — subir imagen ────
        group.MapPost("/pago/{id:guid}/comprobante", SubirComprobanteAsync)
             .WithName("SubirComprobante")
             .DisableAntiforgery();

        // ── GET /api/suscripcion/info — precios + SINPE (público) ─────────
        app.MapGet("/api/suscripcion/info", GetInfoPublicaAsync)
           .WithTags("Suscripciones")
           .WithName("GetInfoSuscripcion")
           .AllowAnonymous();

        return app;
    }

    // ── GET /api/suscripcion ───────────────────────────────────────────────
    private static async Task<Ok<SuscripcionEstadoResponse>> GetEstadoAsync(
        ICurrentUserService currentUser,
        AulaIADbContext db,
        IOptions<SinpeOptions> sinpeOpts,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        var sub = await db.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == user.Id, ct);

        if (sub is null)
            return TypedResults.Ok(new SuscripcionEstadoResponse(
                HasSubscription: false,
                Plan: null,
                Status: null,
                IsTrial: false,
                CurrentPeriodEnd: null,
                DaysRemaining: null,
                TrialDays: sinpeOpts.Value.TrialDays));

        var daysRemaining = sub.Status == SubscriptionStatus.Active
            ? (int)Math.Max(0, (sub.CurrentPeriodEnd - DateTime.UtcNow).TotalDays)
            : 0;

        return TypedResults.Ok(new SuscripcionEstadoResponse(
            HasSubscription: true,
            Plan: sub.Plan.ToString(),
            Status: sub.Status.ToString(),
            IsTrial: sub.IsTrial,
            CurrentPeriodEnd: sub.CurrentPeriodEnd,
            DaysRemaining: daysRemaining,
            TrialDays: sinpeOpts.Value.TrialDays));
    }

    // ── POST /api/suscripcion/trial ────────────────────────────────────────
    private static async Task<Results<Ok<SuscripcionEstadoResponse>, Conflict<string>>> ActivarTrialAsync(
        ICurrentUserService currentUser,
        AulaIADbContext db,
        IOptions<SinpeOptions> sinpeOpts,
        CancellationToken ct)
    {
        var user = await currentUser.ResolveAsync(ct);

        var exists = await db.Subscriptions.AnyAsync(s => s.UserId == user.Id, ct);
        if (exists)
            return TypedResults.Conflict("Ya tienes una suscripción activa o trial iniciado.");

        var trialDays = sinpeOpts.Value.TrialDays;
        var now = DateTime.UtcNow;
        db.Subscriptions.Add(new Subscription
        {
            UserId = user.Id,
            Plan = SubscriptionPlan.Trial,
            Status = SubscriptionStatus.Active,
            IsTrial = true,
            CurrentPeriodStart = now,
            CurrentPeriodEnd = now.AddDays(trialDays),
            UpdatedAt = now
        });
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(new SuscripcionEstadoResponse(
            HasSubscription: true,
            Plan: SubscriptionPlan.Trial.ToString(),
            Status: SubscriptionStatus.Active.ToString(),
            IsTrial: true,
            CurrentPeriodEnd: now.AddDays(trialDays),
            DaysRemaining: trialDays,
            TrialDays: trialDays));
    }

    // ── POST /api/suscripcion/pago ─────────────────────────────────────────
    private static async Task<Results<Ok<PaymentRequestResponse>, BadRequest<string>>> SolicitarPagoAsync(
        SolicitarPagoRequest req,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        IOptions<SinpeOptions> sinpeOpts,
        CancellationToken ct)
    {
        if (!Enum.TryParse<SubscriptionPlan>(req.Plan, true, out var plan) || plan == SubscriptionPlan.Trial)
            return TypedResults.BadRequest("Plan inválido. Use: Basic, Professional o Institutional.");

        var user = await currentUser.ResolveAsync(ct);
        var opts = sinpeOpts.Value;

        var priceUsd = plan switch
        {
            SubscriptionPlan.Basic => opts.PriceBasicUsd,
            SubscriptionPlan.Professional => opts.PriceProfessionalUsd,
            SubscriptionPlan.Institutional => opts.PriceInstitutionalUsd,
            _ => 0m
        };

        // Tipo de cambio más reciente
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tc = await db.ExchangeRates
            .Where(r => r.Date <= today)
            .OrderByDescending(r => r.Date)
            .Select(r => r.UsdToCrc)
            .FirstOrDefaultAsync(ct);

        if (tc <= 0) tc = 540m; // fallback: TC aproximado si aún no hay datos del BCCR

        var amountCrc = Math.Round(priceUsd * tc, 0);

        // Generar reference_code único: AUI-YYYYMMDD-XXXX
        var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
        var suffix = GenerateAlphanumericCode(4);
        var referenceCode = $"AUI-{dateStr}-{suffix}";

        var now = DateTime.UtcNow;
        var payment = new PaymentRequest
        {
            UserId = user.Id,
            Plan = plan,
            AmountUsd = priceUsd,
            AmountCrc = amountCrc,
            ExchangeRateUsed = tc,
            ReferenceCode = referenceCode,
            Status = PaymentRequestStatus.Pending,
            UpdatedAt = now
        };

        db.PaymentRequests.Add(payment);
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok(new PaymentRequestResponse(
            Id: payment.Id,
            Plan: plan.ToString(),
            AmountUsd: priceUsd,
            AmountCrc: amountCrc,
            ExchangeRateUsed: tc,
            ReferenceCode: referenceCode,
            Status: PaymentRequestStatus.Pending.ToString(),
            SinpePhone: opts.PhoneNumber,
            SinpeAccountName: opts.AccountName,
            CreatedAt: payment.CreatedAt));
    }

    // ── POST /api/suscripcion/pago/{id}/comprobante ────────────────────────
    private static async Task<Results<Ok, NotFound, BadRequest<string>>> SubirComprobanteAsync(
        Guid id,
        IFormFile file,
        ICurrentUserService currentUser,
        AulaIADbContext db,
        IOptions<StorageOptions> storageOpts,
        CancellationToken ct)
    {
        if (file.Length > 10 * 1024 * 1024) // max 10 MB
            return TypedResults.BadRequest("El archivo no puede superar 10 MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".pdf" or ".webp"))
            return TypedResults.BadRequest("Formato no soportado. Use JPG, PNG, PDF o WebP.");

        var user = await currentUser.ResolveAsync(ct);
        var payment = await db.PaymentRequests
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.Id, ct);

        if (payment is null) return TypedResults.NotFound();
        if (payment.Status != PaymentRequestStatus.Pending)
            return TypedResults.BadRequest("Solo se puede subir comprobante para pagos pendientes.");

        var blobName = $"{user.Id}/{payment.Id}{ext}";
        var blobClient = new BlobClient(
            storageOpts.Value.ConnectionString, "pagos", blobName);

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: ct);

        payment.VoucherBlobPath = blobName;
        payment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return TypedResults.Ok();
    }

    // ── GET /api/suscripcion/info (público) ────────────────────────────────
    private static async Task<Ok<SuscripcionInfoResponse>> GetInfoPublicaAsync(
        IOptions<SinpeOptions> opts,
        AulaIADbContext db,
        CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var tc = await db.ExchangeRates
            .Where(r => r.Date <= today)
            .OrderByDescending(r => r.Date)
            .Select(r => r.UsdToCrc)
            .FirstOrDefaultAsync(ct);

        if (tc <= 0) tc = 540m;
        var o = opts.Value;

        return TypedResults.Ok(new SuscripcionInfoResponse(
            SinpePhone: o.PhoneNumber,
            SinpeAccountName: o.AccountName,
            ExchangeRate: tc,
            TrialDays: o.TrialDays,
            Plans:
            [
                new PlanInfo("Basic", "Básico", o.PriceBasicUsd, Math.Round(o.PriceBasicUsd * tc, 0), "Planeamiento + asistencia + notas (máx. 5 grupos)"),
                new PlanInfo("Professional", "Profesional", o.PriceProfessionalUsd, Math.Round(o.PriceProfessionalUsd * tc, 0), "Todo + adecuaciones + reportes exportables + grupos ilimitados"),
                new PlanInfo("Institutional", "Institucional", o.PriceInstitutionalUsd, Math.Round(o.PriceInstitutionalUsd * tc, 0), "Todos los docentes + panel de director + reportes institucionales")
            ]));
    }

    private static string GenerateAlphanumericCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record SuscripcionEstadoResponse(
    bool HasSubscription,
    string? Plan,
    string? Status,
    bool IsTrial,
    DateTime? CurrentPeriodEnd,
    int? DaysRemaining,
    int TrialDays);

public record SolicitarPagoRequest(string Plan);

public record PaymentRequestResponse(
    Guid Id,
    string Plan,
    decimal AmountUsd,
    decimal AmountCrc,
    decimal ExchangeRateUsed,
    string ReferenceCode,
    string Status,
    string SinpePhone,
    string SinpeAccountName,
    DateTime CreatedAt);

public record SuscripcionInfoResponse(
    string SinpePhone,
    string SinpeAccountName,
    decimal ExchangeRate,
    int TrialDays,
    IReadOnlyList<PlanInfo> Plans);

public record PlanInfo(
    string Id,
    string Nombre,
    decimal PrecioUsd,
    decimal PrecioCrc,
    string Descripcion);
