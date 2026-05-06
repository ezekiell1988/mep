using System.Text.Json.Serialization;
using AulaIA.Tests.Infrastructure;

namespace AulaIA.Tests;

/// <summary>
/// Tests para el módulo Planeamiento.
///
/// Flujo:
///   1. POST /api/planeamiento  → recibe planId con Status=Pending
///   2. Hangfire procesa el job (consulta curriculum validado + llama GPT-5.5)
///   3. GET  /api/planeamiento/{id} → Status va Pending → Generating → Ready (o Failed)
///   4. GET  /api/planeamiento  → aparece en el listado del docente
///
/// Prerequisito: debe existir al menos un grupo activo del docente autenticado.
/// GroupId: env var AULAIA_TEST_GroupId o appsettings.test.json
/// </summary>
public class PlaneamientoTests : IntegrationTestBase
{
    // ── Config ────────────────────────────────────────────────────────────────
    private static readonly Guid? GroupId = Guid.TryParse(
        Environment.GetEnvironmentVariable("AULAIA_TEST_GroupId"), out var g) ? g : null;

    private static readonly TimeSpan JobTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact(DisplayName = "GET /api/planeamiento → 200 OK con lista (puede estar vacía)")]
    public async Task ListarPlaneamientos_Returns200()
    {
        var response = await Api.GetAsync("/api/planeamiento");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "POST /api/planeamiento sin body → 400 Bad Request")]
    public async Task CrearPlaneamiento_SinBody_Returns400()
    {
        var response = await Api.PostAsync("/api/planeamiento",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "GET /api/planeamiento/{id} inexistente → 404 Not Found")]
    public async Task GetPlaneamiento_IdInexistente_Returns404()
    {
        var response = await Api.GetAsync($"/api/planeamiento/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test de flujo completo. Se skipea si AULAIA_TEST_GroupId no está configurado.
    /// Requiere además al menos una CurriculumUnit validada de "Artes Plasticas" en la BD.
    /// </summary>
    [Fact(DisplayName = "Flujo completo: crear planeamiento → poll hasta Ready")]
    public async Task FlujoCompleto_CrearPollReady()
    {
        if (GroupId is null) return; // skip gracioso sin GroupId

        // 1. Crear planeamiento
        var request = new CrearPlaneamientoRequest(
            GroupId: GroupId.Value,
            Asignatura: "Artes Plasticas",
            Nivel: 7,
            Trimestre: 1,
            AnioLectivo: 2026,
            FechaInicio: new DateOnly(2026, 2, 10),
            FechaFin: new DateOnly(2026, 4, 30),
            LeccionesPorSemana: 4);

        var createResp = await Api.PostAsJsonAsync("/api/planeamiento", request);
        createResp.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var created = await ReadJsonAsync<PlaneamientoResponse>(createResp);
        created.Id.Should().NotBe(Guid.Empty);
        created.Status.Should().Be("Pending");

        // 2. Polling hasta Ready o Failed
        PlaneamientoResponse? final = null;
        var deadline = DateTimeOffset.UtcNow + JobTimeout;

        while (DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(PollInterval);

            var pollResp = await Api.GetAsync($"/api/planeamiento/{created.Id}");
            pollResp.StatusCode.Should().Be(HttpStatusCode.OK);

            final = await ReadJsonAsync<PlaneamientoResponse>(pollResp);
            if (final.Status is "Ready" or "Failed") break;
        }

        final.Should().NotBeNull();
        final!.Status.Should().Be("Ready",
            because: $"después de {JobTimeout.TotalMinutes} min el planeamiento debe estar listo. " +
                     "Si es 'Failed' revisar Hangfire; si sigue en 'Generating' aumentar JobTimeout.");

        final.Contenido.Should().NotBeNullOrWhiteSpace(
            because: "un planeamiento Ready debe tener contenido Markdown generado");

        // 3. Verificar que aparece en el listado
        var listResp = await Api.GetAsync("/api/planeamiento");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await listResp.Content.ReadFromJsonAsync<List<PlaneamientoListItem>>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        list.Should().Contain(p => p.Id == created.Id,
            because: "el planeamiento recién creado debe aparecer en el listado del docente");
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    private record CrearPlaneamientoRequest(
        [property: JsonPropertyName("groupId")] Guid GroupId,
        [property: JsonPropertyName("asignatura")] string Asignatura,
        [property: JsonPropertyName("nivel")] int Nivel,
        [property: JsonPropertyName("trimestre")] int Trimestre,
        [property: JsonPropertyName("anioLectivo")] int AnioLectivo,
        [property: JsonPropertyName("fechaInicio")] DateOnly FechaInicio,
        [property: JsonPropertyName("fechaFin")] DateOnly FechaFin,
        [property: JsonPropertyName("leccionesPorSemana")] int LeccionesPorSemana);

    private record PlaneamientoResponse(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("contenido")] string? Contenido);

    private record PlaneamientoListItem(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("asignatura")] string Asignatura,
        [property: JsonPropertyName("nivel")] int Nivel,
        [property: JsonPropertyName("trimestre")] int Trimestre,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt);
}

