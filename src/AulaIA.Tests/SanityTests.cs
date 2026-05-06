using AulaIA.Tests.Infrastructure;

namespace AulaIA.Tests;

/// <summary>
/// Sanity checks: verifica que la API responda y que la autenticación funcione.
/// Correr primero para descartar problemas de conectividad antes de los tests de negocio.
/// </summary>
public class SanityTests : IntegrationTestBase
{
    [Fact(DisplayName = "GET /health → 200 OK sin autenticación")]
    public async Task Health_ReturnsOk()
    {
        await using var anon = ApiClient.CreateAnonymous();
        var response = await anon.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /api/grupos sin token → 401 Unauthorized")]
    public async Task Grupos_SinToken_Returns401()
    {
        await using var anon = ApiClient.CreateAnonymous();
        var response = await anon.GetAsync("/api/grupos");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/grupos con token válido → 200 OK")]
    public async Task Grupos_ConToken_Returns200()
    {
        var response = await Api.GetAsync("/api/grupos");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /api/curriculum con token → 200 OK o 403 Forbidden")]
    public async Task Curriculum_ConToken_Returns200Or403()
    {
        var response = await Api.GetAsync("/api/curriculum?asignatura=Artes+Plasticas");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }
}
