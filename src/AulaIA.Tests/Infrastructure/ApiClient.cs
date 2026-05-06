using System.Net.Http.Headers;

namespace AulaIA.Tests.Infrastructure;

/// <summary>
/// HttpClient preconfigurado apuntando a la API bajo prueba.
/// Uso:
///   await using var api = await ApiClient.CreateAsync();
///   var response = await api.GetAsync("/health");
/// </summary>
public sealed class ApiClient : IAsyncDisposable
{
    private readonly HttpClient _http;

    private ApiClient(HttpClient http) => _http = http;

    /// <summary>Crea un cliente autenticado con token admin.</summary>
    public static async Task<ApiClient> CreateAsync()
    {
        var http = new HttpClient { BaseAddress = new Uri(TestConfig.ApiBaseUrl) };
        var token = await Auth0TokenHelper.GetTokenAsync(http);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return new ApiClient(http);
    }

    /// <summary>Crea un cliente sin autenticación (para testear endpoints anónimos).</summary>
    public static ApiClient CreateAnonymous() =>
        new(new HttpClient { BaseAddress = new Uri(TestConfig.ApiBaseUrl) });

    public Task<HttpResponseMessage> GetAsync(string url) => _http.GetAsync(url);

    public Task<HttpResponseMessage> PostAsync(string url, HttpContent content) =>
        _http.PostAsync(url, content);

    public Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T body) =>
        _http.PostAsJsonAsync(url, body);

    public ValueTask DisposeAsync()
    {
        _http.Dispose();
        return ValueTask.CompletedTask;
    }
}
