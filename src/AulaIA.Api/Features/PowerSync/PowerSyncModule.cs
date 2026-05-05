namespace AulaIA.Api.Features.PowerSync;

public static class PowerSyncModule
{
    public static IServiceCollection AddPowerSyncModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapPowerSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var ps = app.MapGroup("/api/powersync")
                    .WithTags("PowerSync");

        // Endpoint de token JWT para PowerSync Cloud (F0-08)
        // PowerSync llama a este endpoint para autenticar sincronización offline
        ps.MapGet("/token", GetTokenAsync)
          .WithName("PowerSyncToken")
          .RequireAuthorization();

        return app;
    }

    private static IResult GetTokenAsync(HttpContext context)
    {
        // TODO F1: generar JWT firmado con el sub del usuario autenticado
        // PowerSync necesita: { token: "<jwt>", expiresIn: <seconds> }
        var sub = context.User.FindFirst("sub")?.Value ?? "anonymous";
        return TypedResults.Ok(new { token = "TODO_IMPLEMENT_JWT", sub, expiresIn = 3600 });
    }
}
