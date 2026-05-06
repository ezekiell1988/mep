using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AulaIA.Tests.Infrastructure;

/// <summary>
/// Obtiene un token Bearer para los tests via Auth0 client_credentials (M2M).
/// Si en TestConfig hay un BearerToken directo, lo devuelve sin llamar a Auth0.
///
/// Para crear la M2M app en Auth0:
///   Dashboard → Applications → APIs → aulaia-api → Machine to Machine Applications
///   → Authorize la app de test → asignar permiso "admin" (o el scope que se requiera)
/// </summary>
public static class Auth0TokenHelper
{
    private static string? _cachedToken;
    private static DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;

    public static async Task<string> GetTokenAsync(HttpClient? httpClient = null)
    {
        // Opción rápida: token pegado directamente en la config
        if (TestConfig.DirectBearerToken is { } direct)
            return direct;

        // Cache: reusar mientras no expire (con 60s de margen)
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _tokenExpiry)
            return _cachedToken;

        if (string.IsNullOrEmpty(TestConfig.Auth0ClientId) ||
            string.IsNullOrEmpty(TestConfig.Auth0ClientSecret))
        {
            throw new InvalidOperationException(
                "Configura Auth0:ClientId y Auth0:ClientSecret en appsettings.test.json, " +
                "o pega un token directamente en BearerToken para correr los tests sin M2M.");
        }

        using var client = httpClient ?? new HttpClient();

        var response = await client.PostAsJsonAsync(
            $"https://{TestConfig.Auth0Domain}/oauth/token",
            new
            {
                grant_type = "client_credentials",
                client_id = TestConfig.Auth0ClientId,
                client_secret = TestConfig.Auth0ClientSecret,
                audience = TestConfig.Auth0Audience
            });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TokenResponse>()
            ?? throw new InvalidOperationException("Auth0 devolvió respuesta vacía.");

        _cachedToken = result.AccessToken;
        _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(result.ExpiresIn - 60);

        return _cachedToken;
    }

    private record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("token_type")] string TokenType);
}
