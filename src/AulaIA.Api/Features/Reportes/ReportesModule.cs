namespace AulaIA.Api.Features.Reportes;

public static class ReportesModule
{
    public static IServiceCollection AddReportesModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapReportesEndpoints(this IEndpointRouteBuilder app)
    {
        var reportes = app.MapGroup("/api/reportes")
                          .WithTags("Reportes")
                          .RequireAuthorization();

        // Los endpoints de generación de reportes se implementarán en F2 (Fase 2)
        reportes.MapGet("/health", () => TypedResults.Ok(new { status = "Reportes module ready" }))
                .WithName("ReportesHealth")
                .AllowAnonymous();

        return app;
    }
}
