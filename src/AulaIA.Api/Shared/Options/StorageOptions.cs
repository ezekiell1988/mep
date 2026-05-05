namespace AulaIA.Api.Shared.Options;

public sealed class StorageOptions
{
    public const string Section = "Storage";

    [System.ComponentModel.DataAnnotations.Required]
    public required string ConnectionString { get; init; }

    public string ContainerPlaneamientos { get; init; } = "planeamientos";
    public string ContainerReportes { get; init; } = "reportes";
    public string ContainerAdjuntos { get; init; } = "adjuntos";
}
