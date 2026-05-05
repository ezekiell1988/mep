namespace AulaIA.Api.Shared.Options;

public sealed class DatabaseOptions
{
    public const string Section = "Database";

    [System.ComponentModel.DataAnnotations.Required]
    public required string ConnectionString { get; init; }
}
