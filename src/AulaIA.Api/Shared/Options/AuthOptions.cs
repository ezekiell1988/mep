namespace AulaIA.Api.Shared.Options;

public sealed class AuthOptions
{
    public const string Section = "Auth";

    [System.ComponentModel.DataAnnotations.Required]
    public required string Authority { get; init; }

    [System.ComponentModel.DataAnnotations.Required]
    public required string Audience { get; init; }
}
