using AulaIA.Api.Features.Asistencia;
using AulaIA.Api.Features.Calendario;
using AulaIA.Api.Features.Curriculum;
using AulaIA.Api.Features.Estudiantes;
using AulaIA.Api.Features.Grupos;
using AulaIA.Api.Features.Notas;
using AulaIA.Api.Features.Planeamiento;
using AulaIA.Api.Features.PowerSync;
using AulaIA.Api.Features.Reportes;
using AulaIA.Api.Shared.Extensions;

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
    .AddPowerSyncModule();

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
   .MapPowerSyncEndpoints();

app.LogStartupFacts();
app.Run();
