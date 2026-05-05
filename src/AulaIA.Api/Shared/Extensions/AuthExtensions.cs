using AulaIA.Api.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AulaIA.Api.Shared.Extensions;

public static class AuthExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public void AddAulaIAAuth()
        {
            var opts = builder.Configuration
                .GetSection(AuthOptions.Section)
                .Get<AuthOptions>()
                ?? throw new InvalidOperationException("Auth configuration missing.");

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.Authority = opts.Authority;
                    o.Audience  = opts.Audience;
                    o.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    o.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(5);
                });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("admin",    p => p.RequireRole("admin"))
                .AddPolicy("teacher",  p => p.RequireRole("teacher", "admin"))
                .AddPolicy("director", p => p.RequireRole("director", "admin"));
        }
    }
}
