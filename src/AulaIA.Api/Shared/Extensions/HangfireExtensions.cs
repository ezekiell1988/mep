using AulaIA.Api.Shared.Options;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AulaIA.Api.Shared.Extensions;

public static class HangfireExtensions
{
    public const string DashboardPath = "/hangfire";
    private const string LoginPath   = "/hangfire-login";
    private const string SessionPath = "/hangfire-session";

    extension(WebApplicationBuilder builder)
    {
        public void AddAulaIAHangfire()
        {
            var connStr = builder.Configuration[
                $"{DatabaseOptions.Section}:{nameof(DatabaseOptions.ConnectionString)}"]
                ?? throw new InvalidOperationException("Database connection string not configured.");

            builder.Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseConsole()
                .UseFilter(new AutomaticRetryAttribute { Attempts = 0 })  // sin reintentos por defecto
                .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connStr)));

            builder.Services.AddHangfireServer(options =>
            {
                options.WorkerCount = 2;
                options.Queues = ["default", "curriculum", "planeamiento"];
            });
        }
    }

    extension(WebApplication app)
    {
        // Registrar ANTES de UseAuthentication() en Program.cs.
        // Inyecta el JWT de la cookie como Authorization header para que
        // JwtBearer lo valide en su primera pasada y llene ctx.User correctamente.
        public void UseAulaIAHangfireCookieInjection()
        {
            if (app.Environment.IsDevelopment()) return;

            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Path.StartsWithSegments(DashboardPath)
                    && !ctx.Request.Headers.ContainsKey("Authorization")
                    && ctx.Request.Cookies.TryGetValue("hangfire_token", out var token)
                    && !string.IsNullOrEmpty(token))
                {
                    ctx.Request.Headers.Authorization = $"Bearer {token}";
                }
                await next();
            });
        }

        public void UseAulaIAHangfire()
        {
            // ── Middleware: protege /hangfire con sesión válida ───────────────────
            // UseAuthentication() ya corrió (y leyó el header que inyectó
            // UseAulaIAHangfireCookieInjection), así que ctx.User está listo.
            if (!app.Environment.IsDevelopment())
            {
                app.Use(async (ctx, next) =>
                {
                    if (ctx.Request.Path.StartsWithSegments(DashboardPath))
                    {
                        if (ctx.User.Identity?.IsAuthenticated != true)
                        {
                            ctx.Response.Redirect(LoginPath);
                            return;
                        }
                        if (!ctx.User.HasClaim("https://aulaia.ezekl.com/roles", "admin"))
                        {
                            ctx.Response.StatusCode = 403;
                            await ctx.Response.WriteAsync("Acceso denegado: se requiere rol admin.");
                            return;
                        }
                    }
                    await next();
                });
            }

            // ── Endpoint: página de login con Auth0 SPA SDK ──────────────────────
            app.MapGet(LoginPath, () =>
                Results.Content(LoginHtml, "text/html"))
                .AllowAnonymous();

            // ── Endpoint: intercambio de JWT → cookie con el token ──────────────────────
            app.MapPost(SessionPath, async (HttpContext ctx) =>
            {
                var authResult = await ctx.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                if (!authResult.Succeeded)
                    return Results.Unauthorized();

                var principal = authResult.Principal!;
                if (!principal.HasClaim("https://aulaia.ezekl.com/roles", "admin"))
                    return Results.StatusCode(403);

                // Extraer el token crudo del header para guardarlo en cookie
                var token = ctx.Request.Headers.Authorization.ToString()
                    .Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();

                ctx.Response.Cookies.Append("hangfire_token", token, new CookieOptions
                {
                    HttpOnly  = true,
                    Secure    = true,
                    SameSite  = SameSiteMode.Strict,
                    Expires   = DateTimeOffset.UtcNow.AddHours(8)
                });
                return Results.Ok();
            }).AllowAnonymous();

            // ── Dashboard Hangfire ────────────────────────────────────────────────
            IDashboardAuthorizationFilter[] filters = app.Environment.IsDevelopment()
                ? [new LocalRequestsOnlyAuthorizationFilter()]
                : [new HangfireAdminAuthFilter()];

            app.UseHangfireDashboard(DashboardPath, new DashboardOptions
            {
                Authorization   = filters,
                DarkModeEnabled = true
            });
        }
    }

    // ── HTML inline para /hangfire-login ─────────────────────────────────────
    // Sin dependencias CDN: lee el token directamente de localStorage donde
    // @auth0/auth0-react (cacheLocation:'localstorage') lo guarda.
    // Clave: @@auth0spajs@@::<clientId>::<audience>::<scope>
    // Si no hay sesión activa, redirige al SPA (/) con returnTo en la URL
    // para que el SPA haga loginWithRedirect y vuelva aquí al terminar.
    private const string LoginHtml = """
        <!DOCTYPE html>
        <html lang="es">
        <head>
          <meta charset="UTF-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>AulaIA — Acceso Hangfire</title>
          <style>
            body { font-family: system-ui,sans-serif; display:flex; align-items:center;
                   justify-content:center; min-height:100vh; margin:0;
                   background:#1a1a1a; color:#e0e0e0; }
            .card { background:#2d2d2d; padding:2rem; border-radius:8px;
                    text-align:center; max-width:400px; width:90%; }
            h2 { margin-top:0; }
            .btn { background:#635dff; color:#fff; border:none; padding:.75rem 2rem;
                   border-radius:6px; cursor:pointer; font-size:1rem;
                   text-decoration:none; display:inline-block; margin-top:1rem; }
            .error { color:#ff6b6b; margin-top:1rem; }
            #msg { color:#aaa; }
          </style>
        </head>
        <body>
          <div class="card">
            <h2>&#128295; Hangfire Dashboard</h2>
            <p id="msg">Verificando sesi&#243;n&#8230;</p>
            <div id="actions"></div>
          </div>
          <script>
            (async () => {
              const msg     = document.getElementById('msg');
              const actions = document.getElementById('actions');

              // ── Leer token del cache de @auth0/auth0-react (localStorage) ──
              // Busca por prefijo en vez de clave exacta porque el scope puede
              // estar en distinto orden o incluir scopes adicionales del servidor.
              const CLIENT_ID = 'onUw2TnTZSwUX6K43KtSgt7kQ9lBlJl4';
              const PREFIX    = '@@auth0spajs@@::' + CLIENT_ID + '::';

              let token = null;
              try {
                const now = Math.floor(Date.now() / 1000);
                for (let i = 0; i < localStorage.length; i++) {
                  const key = localStorage.key(i);
                  if (!key || !key.startsWith(PREFIX)) continue;
                  try {
                    const entry     = JSON.parse(localStorage.getItem(key) || '{}');
                    const body      = entry.body ?? entry;
                    const expiresAt = entry.expiresAt ?? 0;
                    if (body.access_token && (expiresAt === 0 || expiresAt > now)) {
                      token = body.access_token;
                      break;
                    }
                  } catch (_) { /* JSON inválido, continuar */ }
                }
              } catch (_) { /* localStorage bloqueado */ }

              if (!token) {
                // Sin sesión o token expirado → mostrar botón que va al SPA
                // El SPA (/login-redirect) hace loginWithRedirect y regresa aquí.
                msg.textContent = 'Inicia sesi&#243;n en AulaIA para acceder al dashboard.';
                const btn = document.createElement('button');
                btn.className   = 'btn';
                btn.textContent = 'Iniciar sesi\u00f3n';
                btn.onclick = () => {
                  // Redirigir al SPA con el destino codificado en la URL.
                  // La página raíz del SPA detecta ?hangfire_return=1 y hace
                  // loginWithRedirect con appState.returnTo='/hangfire-login'.
                  window.location.href = '/?hangfire_return=1';
                };
                actions.appendChild(btn);
                return;
              }

              // ── Token encontrado: intercambiar por cookie de sesión ────────
              msg.textContent = 'Verificando rol admin&#8230;';
              try {
                const resp = await fetch('/hangfire-session', {
                  method: 'POST',
                  headers: { 'Authorization': 'Bearer ' + token }
                });

                if (resp.ok) {
                  window.location.replace('/hangfire');
                } else if (resp.status === 403) {
                  msg.textContent = '';
                  actions.innerHTML = '<p class="error">Acceso denegado: se requiere rol <strong>admin</strong>.</p>';
                } else if (resp.status === 401) {
                  // Token expirado en el server → volver a login
                  localStorage.removeItem(CACHE_KEY);
                  msg.textContent = 'Sesi&#243;n expirada.';
                  const btn2 = document.createElement('button');
                  btn2.className   = 'btn';
                  btn2.textContent = 'Volver a iniciar sesi\u00f3n';
                  btn2.onclick = () => { window.location.href = '/?hangfire_return=1'; };
                  actions.appendChild(btn2);
                } else {
                  msg.textContent = '';
                  actions.innerHTML = '<p class="error">Error al verificar sesi&#243;n (' + resp.status + ').</p>';
                }
              } catch (e) {
                msg.textContent = '';
                actions.innerHTML = '<p class="error">Error de red: ' + e.message + '</p>';
              }
            })();
          </script>
        </body>
        </html>
        """;
}
