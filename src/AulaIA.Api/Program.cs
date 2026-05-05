using AulaIA.Api.Features.Asistencia;
using AulaIA.Api.Features.Estudiantes;
using AulaIA.Api.Features.Grupos;
using AulaIA.Api.Features.Notas;
using AulaIA.Api.Features.Planeamiento;
using AulaIA.Api.Features.PowerSync;
using AulaIA.Api.Features.Reportes;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Options ───────────────────────────────────────────────────────────────
builder.Services
    .AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<AuthOptions>()
    .BindConfiguration(AuthOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<StorageOptions>()
    .BindConfiguration(StorageOptions.Section)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AulaIADbContext>(options =>
{
    var connStr = builder.Configuration[$"{DatabaseOptions.Section}:{nameof(DatabaseOptions.ConnectionString)}"]
                  ?? builder.Configuration.GetConnectionString("DefaultConnection")
                  ?? throw new InvalidOperationException("Database connection string not configured.");

    options.UseNpgsql(connStr, npgsql =>
    {
        npgsql.EnableRetryOnFailure(3);
        npgsql.CommandTimeout(30);
    });
});

// ── Auth ──────────────────────────────────────────────────────────────────
var authority = builder.Configuration[$"{AuthOptions.Section}:{nameof(AuthOptions.Authority)}"];
var audience  = builder.Configuration[$"{AuthOptions.Section}:{nameof(AuthOptions.Audience)}"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience  = audience;
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
    });

builder.Services.AddAuthorization();

// ── OpenAPI ────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── Modules ───────────────────────────────────────────────────────────────
builder.Services
    .AddGruposModule()
    .AddEstudiantesModule()
    .AddAsistenciaModule()
    .AddNotasModule()
    .AddPlaneamientoModule()
    .AddReportesModule()
    .AddPowerSyncModule();

// ── CORS (dev only) ────────────────────────────────────────────────────────
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
}

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors();
}

app.UseAuthentication();
app.UseAuthorization();

// ── Endpoints ─────────────────────────────────────────────────────────────
app.MapGet("/health", () => TypedResults.Ok(new { status = "healthy", version = "1.0.0" }))
   .WithName("Health")
   .AllowAnonymous();

app.MapGruposEndpoints()
   .MapEstudiantesEndpoints()
   .MapAsistenciaEndpoints()
   .MapNotasEndpoints()
   .MapPlaneamientoEndpoints()
   .MapReportesEndpoints()
   .MapPowerSyncEndpoints();

app.Run();
