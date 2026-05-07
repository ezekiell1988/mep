using AulaIA.Api.Features.Adecuaciones;
using AulaIA.Api.Features.Asistencia;
using AulaIA.Api.Features.Dashboard;
using AulaIA.Api.Features.Calendario;
using AulaIA.Api.Features.Curriculum;
using AulaIA.Api.Features.Estudiantes;
using AulaIA.Api.Features.Grupos;
using AulaIA.Api.Features.Notas;
using AulaIA.Api.Features.Planeamiento;
using AulaIA.Api.Features.PowerSync;
using AulaIA.Api.Features.Reportes;
using AulaIA.Api.Features.Suscripciones;
using AulaIA.Api.Features.Suscripciones.Jobs;
using AulaIA.Api.Shared.Extensions;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAulaIAOptions();
builder.AddAulaIAPersistence();
builder.AddAulaIAAuth();
builder.AddAulaIACors();
builder.AddAulaIAHangfire();

builder.Services.AddOpenApi();
builder.AddLlmAuditServices();

builder.Services
    .AddGruposModule()
    .AddEstudiantesModule()
    .AddAsistenciaModule()
    .AddNotasModule()
    .AddPlaneamientoModule()
    .AddCurriculumModule()
    .AddReportesModule()
    .AddCalendarioModule()
    .AddAdecuacionesModule()
    .AddPowerSyncModule()
    .AddSuscripcionesModule();

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(CorsExtensions.DevPolicy);
    app.MapLlmDiagEndpoints();
}
else
{
    app.UseCors(CorsExtensions.FrontendPolicy);
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAulaIAHangfire();

// ── Endpoints ─────────────────────────────────────────────────────────────
app.MapGet("/health", () => TypedResults.Ok(new { status = "healthy", version = "1.0.0" }))
   .WithName("Health")
   .AllowAnonymous();

app.MapGruposEndpoints()
   .MapEstudiantesEndpoints()
   .MapAsistenciaEndpoints()
   .MapNotasEndpoints()
   .MapPlaneamientoEndpoints()
   .MapCurriculumEndpoints()
   .MapReportesEndpoints()
   .MapCalendarioEndpoints()
   .MapAdecuacionesEndpoints()
   .MapDashboardEndpoints()
   .MapPowerSyncEndpoints()
   .MapSuscripcionesEndpoints()
   .MapPaymentsEndpoints()
   .MapReferralsEndpoints();

// ── Hangfire recurring jobs ───────────────────────────────────────────────
RecurringJob.AddOrUpdate<UpdateExchangeRateJob>(
    "update-exchange-rate",
    j => j.ExecuteAsync(CancellationToken.None),
    "0 12 * * *");  // 12 PM UTC = 6 AM Costa Rica

RecurringJob.AddOrUpdate<CheckExpiredSubscriptionsJob>(
    "check-expired-subscriptions",
    j => j.ExecuteAsync(CancellationToken.None),
    "0 8 * * *");   // 8 AM UTC = 2 AM Costa Rica

app.LogStartupFacts();
app.Run();
