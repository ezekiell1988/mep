using FluentAssertions;

namespace AulaIA.Tests.Infrastructure;

/// <summary>
/// Clase base para todos los tests de integración.
/// Obtiene el token en InitializeAsync (una vez por clase) y expone
/// un ApiClient listo para usar en cada test.
///
/// Patrón del skill dotnet-10-csharp-14/testing.md:
///   - IAsyncLifetime para setup/teardown async
///   - ApiClient compartido dentro de la clase, no recreado por test
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected ApiClient Api { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Api = await ApiClient.CreateAsync();
        await SetupAsync();
    }

    public async Task DisposeAsync()
    {
        await TeardownAsync();
        await Api.DisposeAsync();
    }

    /// <summary>Override para setup adicional por clase de test.</summary>
    protected virtual Task SetupAsync() => Task.CompletedTask;

    /// <summary>Override para teardown adicional por clase de test.</summary>
    protected virtual Task TeardownAsync() => Task.CompletedTask;

    // ── Helpers de aserción HTTP ──────────────────────────────────────────

    protected static async Task AssertStatusAsync(
        HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(expected,
                because: $"la respuesta fue: {body}");
        }
    }

    protected static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<T>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        result.Should().NotBeNull();
        return result!;
    }
}
