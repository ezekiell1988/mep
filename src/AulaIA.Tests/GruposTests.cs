using System.Text.Json.Serialization;
using AulaIA.Tests.Infrastructure;

namespace AulaIA.Tests;

/// <summary>
/// Tests para el módulo Grupos.
///
/// Flujos cubiertos:
///   1. GET /api/grupos → 200, devuelve campos de ponderación
///   2. PUT /api/grupos/{id}/ponderacion con suma válida (100) → 204 No Content
///   3. PUT /api/grupos/{id}/ponderacion con suma inválida (≠ 100) → 400 Bad Request
///   4. PUT /api/grupos/{id}/ponderacion id inexistente → 404 Not Found
///   5. Flujo: actualizar ponderación → verificar persistencia con GET
///
/// Prerequisito: debe existir al menos un grupo del docente.
/// GroupId: env var AULAIA_TEST_GroupId o appsettings.test.json
/// </summary>
public class GruposTests : IntegrationTestBase
{
    private static readonly Guid? GroupId = Guid.TryParse(
        Environment.GetEnvironmentVariable("AULAIA_TEST_GroupId"), out var g) ? g : null;

    // ── Tests básicos ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "GET /api/grupos sin token → 401")]
    public async Task ListarGrupos_SinToken_Returns401()
    {
        await using var anon = ApiClient.CreateAnonymous();
        var response = await anon.GetAsync("/api/grupos");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/grupos con token → 200 y campos ponderación presentes")]
    public async Task ListarGrupos_Returns200_ConCamposPonderacion()
    {
        var response = await Api.GetAsync("/api/grupos");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var grupos = await response.Content.ReadFromJsonAsync<List<GrupoResponse>>();
        grupos.Should().NotBeNull();

        if (grupos!.Count > 0)
        {
            var g = grupos[0];
            // Los 4 campos de ponderación deben existir y sumar 100
            var suma = g.PctCotidiano + g.PctPruebas + g.PctExtraclase + g.PctOtros;
            suma.Should().BeApproximately(100m, 0.01m,
                "los 4 pesos deben sumar 100 (defaults MEP: 20/45/20/15)");
        }
    }

    // ── Ponderación: validación suma ──────────────────────────────────────────

    [Fact(DisplayName = "PUT /api/grupos/{id}/ponderacion con suma ≠ 100 → 400 Bad Request")]
    public async Task UpdatePonderacion_SumaInvalida_Returns400()
    {
        if (GroupId is null) return;

        var req = new UpdatePonderacionRequest(
            PctCotidiano: 25m,
            PctPruebas: 25m,
            PctExtraclase: 25m,
            PctOtros: 25m   // suma = 100, necesitamos que falle → usamos 26m
        );
        // Usamos 26 para que sume 101
        var reqInvalido = new UpdatePonderacionRequest(25m, 26m, 25m, 25m);

        var response = await Api.PutAsJsonAsync(
            $"/api/grupos/{GroupId}/ponderacion", reqInvalido);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PUT /api/grupos/{id}/ponderacion id inexistente → 404 Not Found")]
    public async Task UpdatePonderacion_IdInexistente_Returns404()
    {
        var req = new UpdatePonderacionRequest(20m, 45m, 20m, 15m);
        var response = await Api.PutAsJsonAsync(
            $"/api/grupos/{Guid.NewGuid()}/ponderacion", req);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT /api/grupos/{id}/ponderacion sin token → 401")]
    public async Task UpdatePonderacion_SinToken_Returns401()
    {
        if (GroupId is null) return;
        await using var anon = ApiClient.CreateAnonymous();
        var req = new UpdatePonderacionRequest(20m, 45m, 20m, 15m);
        var response = await anon.PutAsJsonAsync(
            $"/api/grupos/{GroupId}/ponderacion", req);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Flujo completo ────────────────────────────────────────────────────────

    [Fact(DisplayName = "Flujo: actualizar ponderación → persiste → restaurar defaults")]
    public async Task FlujoCompleto_ActualizarPonderacion_Persiste()
    {
        if (GroupId is null) return;

        // 1. Guardar ponderación actual (para restaurar al final)
        var getResp = await Api.GetAsync($"/api/grupos/{GroupId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var original = await getResp.Content.ReadFromJsonAsync<GrupoResponse>();
        original.Should().NotBeNull();

        // 2. Actualizar a ponderación no-MEP (pero válida: suma = 100)
        var nuevaPond = new UpdatePonderacionRequest(
            PctCotidiano: 25m,
            PctPruebas: 40m,
            PctExtraclase: 25m,
            PctOtros: 10m);   // suma = 100

        var putResp = await Api.PutAsJsonAsync(
            $"/api/grupos/{GroupId}/ponderacion", nuevaPond);
        await AssertStatusAsync(putResp, HttpStatusCode.NoContent);

        // 3. Verificar que el GET refleja los nuevos valores
        var getResp2 = await Api.GetAsync($"/api/grupos/{GroupId}");
        getResp2.StatusCode.Should().Be(HttpStatusCode.OK);
        var actualizado = await getResp2.Content.ReadFromJsonAsync<GrupoResponse>();
        actualizado!.PctCotidiano.Should().Be(25m);
        actualizado.PctPruebas.Should().Be(40m);
        actualizado.PctExtraclase.Should().Be(25m);
        actualizado.PctOtros.Should().Be(10m);

        // 4. Restaurar defaults MEP
        var defaults = new UpdatePonderacionRequest(
            original!.PctCotidiano,
            original.PctPruebas,
            original.PctExtraclase,
            original.PctOtros);
        var restoreResp = await Api.PutAsJsonAsync(
            $"/api/grupos/{GroupId}/ponderacion", defaults);
        restoreResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

// ── DTOs locales ──────────────────────────────────────────────────────────────

file sealed record GrupoResponse(
    [property: JsonPropertyName("id")]             Guid Id,
    [property: JsonPropertyName("name")]           string Name,
    [property: JsonPropertyName("level")]          string Level,
    [property: JsonPropertyName("subject")]        string Subject,
    [property: JsonPropertyName("schoolYear")]     int SchoolYear,
    [property: JsonPropertyName("teacherId")]      Guid TeacherId,
    [property: JsonPropertyName("pctCotidiano")]   decimal PctCotidiano,
    [property: JsonPropertyName("pctPruebas")]     decimal PctPruebas,
    [property: JsonPropertyName("pctExtraclase")]  decimal PctExtraclase,
    [property: JsonPropertyName("pctOtros")]       decimal PctOtros);

file sealed record UpdatePonderacionRequest(
    [property: JsonPropertyName("pctCotidiano")]   decimal PctCotidiano,
    [property: JsonPropertyName("pctPruebas")]     decimal PctPruebas,
    [property: JsonPropertyName("pctExtraclase")]  decimal PctExtraclase,
    [property: JsonPropertyName("pctOtros")]       decimal PctOtros);
