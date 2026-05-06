using AulaIA.Api.Features.Asistencia;
using AulaIA.Api.Features.Curriculum;
using AulaIA.Api.Features.Estudiantes;
using AulaIA.Api.Features.Grupos;
using AulaIA.Api.Features.Notas;
using AulaIA.Api.Features.Planeamiento;
using AulaIA.Api.Features.PowerSync;
using AulaIA.Api.Features.Reportes;
using AulaIA.Api.Shared.Extensions;
using AulaIA.Api.Shared.Services;
using System.Runtime.InteropServices;

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
   .MapPowerSyncEndpoints();

// ── LLM Audit — startup facts ─────────────────────────────────────────────
var audit = app.Services.GetRequiredService<ILlmAuditService>();
audit.LogStartup("AulaIA.Api", [
    $"Framework: {RuntimeInformation.FrameworkDescription}",
    $"Environment: {app.Environment.EnvironmentName}",
    $"Auth0 Domain: {app.Configuration["Auth:Authority"]}",
    $"Módulos registrados: Grupos, Estudiantes, Asistencia, Notas, Planeamiento, Curriculum, Reportes, PowerSync",
    $"Diag endpoints: GET /api/diag/audit, GET /api/diag/context, DELETE /api/diag/audit, POST /api/diag/audit-event"
]);

app.Run();
