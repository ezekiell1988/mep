using AulaIA.Api.Shared.Options;
using Hangfire;
using Hangfire.PostgreSql;

namespace AulaIA.Api.Shared.Extensions;

public static class HangfireExtensions
{
    public const string DashboardPath = "/hangfire";

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
        public void UseAulaIAHangfire()
        {
            // Dashboard solo accesible con rol admin
            app.UseHangfireDashboard(DashboardPath, new DashboardOptions
            {
                Authorization = [new HangfireAdminAuthFilter()],
                DarkModeEnabled = true
            });
        }
    }
}
