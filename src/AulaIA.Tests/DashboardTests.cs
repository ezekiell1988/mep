using System.Text.Json.Serialization;
using AulaIA.Tests.Infrastructure;

namespace AulaIA.Tests;

/// <summary>
/// Tests para el módulo Dashboard (GET /api/docente/resumen).
///
/// Flujos cubiertos:
///   1. Sin token → 401
///   2. Con token → 200 OK con todos los campos requeridos
///   3. Verificar coherencia: totalGrupos >= 0, totalEstudiantes >= 0
///   4. Verificar que proximosEventos no es nulo
/// </summary>
public class DashboardTests : IntegrationTestBase
{
    [Fact(DisplayName = "GET /api/docente/resumen sin token → 401 Unauthorized")]
    public async Task GetResumen_SinToken_Returns401()
    {
        await using var anon = ApiClient.CreateAnonymous();
        var response = await anon.GetAsync("/api/docente/resumen");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/docente/resumen con token → 200 OK con campos completos")]
    public async Task GetResumen_Returns200_ConCamposCompletos()
    {
        var response = await Api.GetAsync("/api/docente/resumen");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resumen = await ReadJsonAsync<DocenteResumenResponse>(response);

        resumen.TotalGrupos.Should().BeGreaterThanOrEqualTo(0);
        resumen.TotalEstudiantes.Should().BeGreaterThanOrEqualTo(0);
        resumen.EstudiantesEnRiesgo.Should().BeGreaterThanOrEqualTo(0);
        resumen.EstudiantesEnRiesgo.Should().BeLessThanOrEqualTo(resumen.TotalEstudiantes,
            because: "los estudiantes en riesgo no pueden superar el total de estudiantes");
        resumen.PlaneamientosPendientes.Should().BeGreaterThanOrEqualTo(0);
        resumen.PlaneamientosListos.Should().BeGreaterThanOrEqualTo(0);
        resumen.AdecuacionesActivas.Should().BeGreaterThanOrEqualTo(0);
        resumen.ProximosEventos.Should().NotBeNull(
            because: "la lista de próximos eventos debe ser un array (puede estar vacío)");
    }

    [Fact(DisplayName = "GET /api/docente/resumen → proximosEventos no exceden 14 días")]
    public async Task GetResumen_EventosDentroDeVentana()
    {
        var response = await Api.GetAsync("/api/docente/resumen");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resumen = await ReadJsonAsync<DocenteResumenResponse>(response);

        var hoy      = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite   = hoy.AddDays(14);

        foreach (var ev in resumen.ProximosEventos)
        {
            var fecha = DateOnly.Parse(ev.Fecha);
            (fecha >= hoy).Should().BeTrue(
                because: $"{ev.Fecha} debe ser hoy ({hoy}) o futuro");
            (fecha <= limite).Should().BeTrue(
                because: $"{ev.Fecha} debe estar dentro de los próximos 14 días ({limite})");
        }
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    private record DocenteResumenResponse(
        [property: JsonPropertyName("totalGrupos")]           int TotalGrupos,
        [property: JsonPropertyName("totalEstudiantes")]      int TotalEstudiantes,
        [property: JsonPropertyName("estudiantesEnRiesgo")]   int EstudiantesEnRiesgo,
        [property: JsonPropertyName("planeamientosPendientes")] int PlaneamientosPendientes,
        [property: JsonPropertyName("planeamientosListos")]   int PlaneamientosListos,
        [property: JsonPropertyName("adecuacionesActivas")]   int AdecuacionesActivas,
        [property: JsonPropertyName("proximosEventos")]       List<ProximoEvento> ProximosEventos);

    private record ProximoEvento(
        [property: JsonPropertyName("fecha")]  string Fecha,
        [property: JsonPropertyName("titulo")] string Titulo,
        [property: JsonPropertyName("tipo")]   string Tipo);
}
