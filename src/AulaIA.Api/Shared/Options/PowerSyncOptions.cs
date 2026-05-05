using System.ComponentModel.DataAnnotations;

namespace AulaIA.Api.Shared.Options;

public sealed class PowerSyncOptions
{
    public const string Section = "PowerSync";

    /// <summary>URL de la instancia PowerSync Cloud, ej. https://abc123.powersync.journeyapps.com</summary>
    [Required]
    public required string InstanceUrl { get; init; }

    /// <summary>Secreto HS256 (mín. 32 caracteres) para firmar el JWT que PowerSync valida.</summary>
    [Required, MinLength(32)]
    public required string SigningKey { get; init; }

    /// <summary>Key ID registrado en PowerSync Cloud → Client Auth → HS256. Ej: "aulaia-v1".</summary>
    [Required]
    public required string KeyId { get; init; }
}
