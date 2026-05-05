namespace AulaIA.Api.Features.Planeamiento;

public static class PlaneamientoModule
{
    public static IServiceCollection AddPlaneamientoModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapPlaneamientoEndpoints(this IEndpointRouteBuilder app)
    {
        var planeamiento = app.MapGroup("/api/planeamiento")
                              .WithTags("Planeamiento")
                              .RequireAuthorization();

        // Los endpoints de IA se implementarán en F1 (Fase 1)
        planeamiento.MapGet("/health", () => TypedResults.Ok(new { status = "Planeamiento module ready" }))
                    .WithName("PlaneamientoHealth")
                    .AllowAnonymous();

        return app;
    }
}
