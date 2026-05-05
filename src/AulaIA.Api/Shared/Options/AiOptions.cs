using System.ComponentModel.DataAnnotations;

namespace AulaIA.Api.Shared.Options;

public sealed class AiOptions
{
    public const string Section = "AI";

    [Required]
    public required string Endpoint { get; init; }

    // Modelos disponibles en AI Foundry (demo-itqs-resource)
    // Texto / razonamiento — planeamientos y extracción de currículo
    [Required]
    public required string DeploymentChat { get; init; }      // gpt-5.5

    // Generación de imágenes — futuro: ilustraciones para materiales didácticos
    public string? DeploymentImage { get; init; }             // gpt-image-2

    // Video / animación — futuro: videos explicativos cortos
    public string? DeploymentVideo { get; init; }             // sora-2

    // Audio / voz en tiempo real — futuro: lectura de planeamientos, dictado
    public string? DeploymentRealtime { get; init; }          // gpt-realtime

    // Managed Identity se usa en producción; ApiKey solo para desarrollo local
    public string? ApiKey { get; init; }

    // Alias para mantener compatibilidad con código existente
    public string DeploymentName => DeploymentChat;
}
