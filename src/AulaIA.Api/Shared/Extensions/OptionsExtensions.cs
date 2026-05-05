using AulaIA.Api.Shared.Options;

namespace AulaIA.Api.Shared.Extensions;

public static class OptionsExtensions
{
    extension(IServiceCollection services)
    {
        public void AddAulaIAOptions()
        {
            services
                .AddOptions<DatabaseOptions>()
                .BindConfiguration(DatabaseOptions.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddOptions<AuthOptions>()
                .BindConfiguration(AuthOptions.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddOptions<StorageOptions>()
                .BindConfiguration(StorageOptions.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }
    }
}
