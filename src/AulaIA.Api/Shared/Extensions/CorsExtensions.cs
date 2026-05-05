namespace AulaIA.Api.Shared.Extensions;

public static class CorsExtensions
{
    internal const string DevPolicy      = "development";
    internal const string FrontendPolicy = "frontend";

    extension(WebApplicationBuilder builder)
    {
        public void AddAulaIACors()
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(FrontendPolicy, policy =>
                    policy.WithOrigins("https://mep.ezekl.com", "https://api.mep.ezekl.com")
                          .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                          .WithHeaders("Content-Type", "Authorization")
                          .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));

                options.AddPolicy(DevPolicy, policy =>
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
        }
    }
}
