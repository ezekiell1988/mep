using Hangfire.Dashboard;

namespace AulaIA.Api.Shared.Extensions;

public sealed class HangfireAdminAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.HasClaim("https://aulaia.ezekl.com/roles", "admin");
    }
}
