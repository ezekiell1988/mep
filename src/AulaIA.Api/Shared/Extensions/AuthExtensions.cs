using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

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
                    o.TokenValidationParameters.RoleClaimType = "https://aulaia.ezekl.com/roles";

                    o.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = ctx =>
                        {
                            var audit = ctx.HttpContext.RequestServices.GetService<ILlmAuditService>();
                            if (audit is null) return Task.CompletedTask;

                            var principal = ctx.Principal!;
                            var sub = principal.FindFirstValue("sub")
                                   ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                                   ?? "(no sub)";
                            var roles = principal.FindAll(ClaimTypes.Role)
                                                 .Select(c => c.Value).ToList();
                            var nsRoles = principal.FindAll("https://aulaia.ezekl.com/roles")
                                                   .Select(c => c.Value).ToList();
                            var issuer = ctx.SecurityToken?.Issuer ?? "(no issuer)";

                            audit.LogEvent(
                                "Auth0",
                                "Token validado",
                                $"✅ sub={sub} | roles=[{string.Join(",", roles)}] | ns_roles=[{string.Join(",", nsRoles)}] | issuer={issuer}");

                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = ctx =>
                        {
                            var audit = ctx.HttpContext.RequestServices.GetService<ILlmAuditService>();
                            audit?.LogError(
                                "Auth0",
                                $"Validación fallida en {ctx.Request.Path}: {ctx.Exception.GetType().Name}: {ctx.Exception.Message}",
                                ctx.Exception);
                            return Task.CompletedTask;
                        },

                        OnChallenge = ctx =>
                        {
                            var audit = ctx.HttpContext.RequestServices.GetService<ILlmAuditService>();
                            audit?.LogEvent(
                                "Auth0",
                                $"Challenge 401 → {ctx.Request.Path}",
                                $"error={ctx.Error ?? "(none)"} desc={ctx.ErrorDescription ?? "(none)"}");
                            return Task.CompletedTask;
                        },

                        OnForbidden = ctx =>
                        {
                            var audit = ctx.HttpContext.RequestServices.GetService<ILlmAuditService>();
                            var sub = ctx.HttpContext.User.FindFirstValue("sub")
                                   ?? ctx.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                                   ?? "(no sub)";
                            audit?.LogEvent(
                                "Auth0",
                                $"Forbidden 403 → {ctx.Request.Path}",
                                $"sub={sub}");
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("admin",    p => p.RequireRole("admin"))
                .AddPolicy("teacher",  p => p.RequireRole("teacher", "admin"))
                .AddPolicy("director", p => p.RequireRole("director", "admin"));
        }
    }
}
