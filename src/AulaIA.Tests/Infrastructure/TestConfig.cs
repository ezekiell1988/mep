using Microsoft.Extensions.Configuration;

namespace AulaIA.Tests.Infrastructure;

/// <summary>
/// Lee la configuración de appsettings.test.json (gitignoreado) o variables de entorno.
/// Prioridad: ENV VARS > appsettings.test.json > appsettings.test.template.json
/// </summary>
public static class TestConfig
{
    private static readonly IConfiguration _config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.test.json", optional: true)
        .AddEnvironmentVariables("AULAIA_TEST_")
        .Build();

    /// <summary>URL base de la API. Default: http://localhost:5070</summary>
    public static string ApiBaseUrl =>
        _config["Api:BaseUrl"] ?? "http://localhost:5070";

    public static string Auth0Domain =>
        _config["Auth0:Domain"] ?? throw new InvalidOperationException("Auth0:Domain no configurado.");

    public static string Auth0ClientId =>
        _config["Auth0:ClientId"] ?? "";

    public static string Auth0ClientSecret =>
        _config["Auth0:ClientSecret"] ?? "";

    public static string Auth0Audience =>
        _config["Auth0:Audience"] ?? "https://api.aulaia.mep.go.cr";

    /// <summary>
    /// Token Bearer ya generado. Si está presente, se usa directamente
    /// sin llamar a Auth0. Útil para correr tests rápidos pegando un
    /// token copiado del portal Auth0 o del navegador.
    /// </summary>
    public static string? DirectBearerToken =>
        string.IsNullOrWhiteSpace(_config["BearerToken"]) ? null : _config["BearerToken"];
}
