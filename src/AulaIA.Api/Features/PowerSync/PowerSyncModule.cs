using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AulaIA.Api.Features.PowerSync;

public static class PowerSyncModule
{
    private static readonly JwtSecurityTokenHandler _handler = new();

    public static IServiceCollection AddPowerSyncModule(this IServiceCollection services) => services;

    public static IEndpointRouteBuilder MapPowerSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var ps = app.MapGroup("/api/powersync")
                    .WithTags("PowerSync");

        // PowerSync Cloud llama a este endpoint para autenticar la sincronización offline.
        // Retorna un JWT firmado con el sub del usuario autenticado.
        ps.MapGet("/token", GetTokenAsync)
          .WithName("PowerSyncToken")
          .RequireAuthorization();

        return app;
    }

    // GET /api/powersync/token
    // Genera un JWT firmado que el cliente PowerSync usa para sincronizar contra PowerSync Cloud.
    // La instancia de PowerSync Cloud debe configurarse con:
    //   JWKS/Secret = mismo SigningKey, Issuer = api.mep.ezekl.com
    private static IResult GetTokenAsync(
        HttpContext context,
        ICurrentUserService currentUser,
        IOptions<PowerSyncOptions> options)
    {
        var sub = context.User.FindFirst("sub")?.Value
               ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(sub))
            return TypedResults.Unauthorized();

        var opt = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        creds.Key.KeyId = opt.KeyId; // kid en el header del JWT — debe coincidir con el KID configurado en PowerSync Cloud

        var now    = DateTime.UtcNow;
        var expiry = now.AddHours(1);

        var token = new JwtSecurityToken(
            issuer:   "https://api.mep.ezekl.com",
            audience: opt.InstanceUrl,
            claims:   [new Claim(JwtRegisteredClaimNames.Sub, sub)],
            notBefore: now,
            expires:   expiry,
            signingCredentials: creds);

        var tokenString = _handler.WriteToken(token);

        return TypedResults.Ok(new PowerSyncTokenResponse(tokenString, expiry));
    }
}

public sealed record PowerSyncTokenResponse(string Token, DateTime ExpiresAt);
