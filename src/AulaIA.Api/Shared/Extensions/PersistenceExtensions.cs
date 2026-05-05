using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Shared.Extensions;

public static class PersistenceExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public void AddAulaIAPersistence()
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            builder.Services.AddDbContext<AulaIADbContext>(options =>
            {
                var connStr = builder.Configuration[
                    $"{DatabaseOptions.Section}:{nameof(DatabaseOptions.ConnectionString)}"]
                    ?? throw new InvalidOperationException("Database connection string not configured.");

                options.UseNpgsql(connStr, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(3);
                    npgsql.CommandTimeout(30);
                });
            });
        }
    }
}
