using AulaIA.Api.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Infraestructura ────────────────────────────────────────────────────────
builder.Services.AddAulaIAOptions();
builder.AddAulaIAPersistence();
builder.AddAulaIAAuth();
builder.AddAulaIACors();
builder.AddAulaIAHangfire();
builder.Services.AddOpenApi();
builder.AddLlmAuditServices();

// ── Módulos de feature ─────────────────────────────────────────────────────
builder.Services.AddAulaIAModules();

var app = builder.Build();

// ── Middleware (orden obligatorio) ─────────────────────────────────────────
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

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAulaIAHangfire();

// ── Endpoints ──────────────────────────────────────────────────────────────
app.MapGet("/health", () => TypedResults.Ok(new { status = "healthy", version = "1.0.0" }))
   .WithName("Health")
   .AllowAnonymous();

app.MapAulaIAEndpoints();

// SPA fallback: rutas no-API → index.html (Next.js client-side router)
app.MapFallbackToFile("index.html").ExcludeFromDescription();

// ── Startup ────────────────────────────────────────────────────────────────
await app.RunMigrationsAsync();
app.AddAulaIARecurringJobs();
app.LogStartupFacts();
app.Run();

