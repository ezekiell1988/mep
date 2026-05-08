using AulaIA.Api.Shared.Options;
using Azure.AI.OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

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

            services
                .AddOptions<PowerSyncOptions>()
                .BindConfiguration(PowerSyncOptions.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddOptions<AiOptions>()
                .BindConfiguration(AiOptions.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddOptions<SinpeOptions>()
                .BindConfiguration(SinpeOptions.Section)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // AzureOpenAIClient como singleton: reutiliza pool de conexiones HTTP y
            // evita el NetworkTimeout de 100s por defecto que falla en Azure con PDFs grandes.
            services.AddSingleton(sp =>
            {
                var ai = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiOptions>>().Value;
                var clientOptions = new AzureOpenAIClientOptions
                {
                    NetworkTimeout = TimeSpan.FromMinutes(10)
                };
                return new AzureOpenAIClient(
                    new Uri(ai.Endpoint),
                    new ApiKeyCredential(ai.ApiKey ?? ""),
                    clientOptions);
            });
        }
    }
}
