using System.Text.Json.Serialization;
using AulaIA.Tests.Infrastructure;

namespace AulaIA.Tests;

/// <summary>
/// Tests para el módulo Adecuaciones (Ley 7600).
///
/// Flujos cubiertos:
///   1. GET lista sin token → 401
///   2. GET lista con token → 200 (puede estar vacía)
///   3. GET adecuación de estudiante inexistente → 404
///   4. Flujo CRUD: upsert → get → generar propuesta (poll) → informe PDF → delete
///   5. PUT upsert con body inválido (sin diagnóstico) → 400
///
/// Prerequisito:
///   GroupId y StudentId configurados en appsettings.test.json o env vars.
///   Al menos una CurriculumUnit validada para el grupo (para el test de generación IA).
/// </summary>
public class AdecuacionesTests : IntegrationTestBase
{
    private static readonly Guid? GroupId = Guid.TryParse(
        Environment.GetEnvironmentVariable("AULAIA_TEST_GroupId"), out var g) ? g : null;

    private static readonly Guid? StudentId = Guid.TryParse(
        Environment.GetEnvironmentVariable("AULAIA_TEST_StudentId"), out var s) ? s : null;

    private static readonly TimeSpan JobTimeout   = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    // ── Tests básicos ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "GET /api/grupos/{id}/adecuaciones sin token → 401")]
    public async Task ListarAdecuaciones_SinToken_Returns401()
    {
        if (GroupId is null) return;
        await using var anon = ApiClient.CreateAnonymous();
        var response = await anon.GetAsync($"/api/grupos/{GroupId}/adecuaciones");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/grupos/{id}/adecuaciones con token → 200 OK")]
    public async Task ListarAdecuaciones_Returns200()
    {
        if (GroupId is null) return;
        var response = await Api.GetAsync($"/api/grupos/{GroupId}/adecuaciones");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET adecuación de estudiante inexistente → 404")]
    public async Task GetAdecuacion_EstudianteInexistente_Returns404()
    {
        if (GroupId is null) return;
        var response = await Api.GetAsync(
            $"/api/grupos/{GroupId}/estudiantes/{Guid.NewGuid()}/adecuacion");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT adecuación sin diagnóstico → 400 Bad Request")]
    public async Task UpsertAdecuacion_SinDiagnostico_Returns400()
    {
        if (GroupId is null || StudentId is null) return;
        var response = await Api.PutAsJsonAsync(
            $"/api/grupos/{GroupId}/estudiantes/{StudentId}/adecuacion",
            new { type = "ANS" }); // falta diagnostico
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Flujo completo ────────────────────────────────────────────────────────

    [Fact(DisplayName = "Flujo: upsert adecuación → get → verificar campos → delete")]
    public async Task FlujoBasico_UpsertGetDelete()
    {
        if (GroupId is null || StudentId is null) return;

        var req = new UpsertAdecuacionRequest(
            Type: "ANS",
            Diagnostico: "TDAH leve (test automatizado — eliminar)",
            CondicionEspecial: null,
            EstrategiasMediacion: "Instrucciones claras y breves",
            EstrategiasEvaluacion: "Tiempo extra en evaluaciones escritas",
            Observaciones: null);

        // 1. Upsert
        var upsertResp = await Api.PutAsJsonAsync(
            $"/api/grupos/{GroupId}/estudiantes/{StudentId}/adecuacion", req);
        await AssertStatusAsync(upsertResp, HttpStatusCode.OK);

        var created = await ReadJsonAsync<AdecuacionResponse>(upsertResp);
        created.StudentId.Should().Be(StudentId.Value);
        created.Type.Should().Be("ANS");
        created.Diagnostico.Should().Be(req.Diagnostico);

        // 2. GET por estudiante
        var getResp = await Api.GetAsync(
            $"/api/grupos/{GroupId}/estudiantes/{StudentId}/adecuacion");
        await AssertStatusAsync(getResp, HttpStatusCode.OK);

        var fetched = await ReadJsonAsync<AdecuacionResponse>(getResp);
        fetched.Id.Should().Be(created.Id);

        // 3. Aparece en la lista del grupo
        var listResp = await Api.GetAsync($"/api/grupos/{GroupId}/adecuaciones");
        await AssertStatusAsync(listResp, HttpStatusCode.OK);

        var list = await listResp.Content.ReadFromJsonAsync<List<AdecuacionResumen>>(
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        list.Should().Contain(a => a.StudentId == StudentId.Value,
            because: "la adecuación recién creada debe aparecer en la lista del grupo");

        // 4. Delete
        var deleteResp = await Api.DeleteAsync(
            $"/api/grupos/{GroupId}/estudiantes/{StudentId}/adecuacion");
        await AssertStatusAsync(deleteResp, HttpStatusCode.NoContent);

        // 5. Verificar eliminación
        var getAfterDelete = await Api.GetAsync(
            $"/api/grupos/{GroupId}/estudiantes/{StudentId}/adecuacion");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Flujo IA: upsert → generar propuesta (poll) → status Ready → informe PDF → delete")]
    public async Task FlujoIA_GenerarPropuesta_Poll_InformePDF()
    {
        if (GroupId is null || StudentId is null) return;

        var req = new UpsertAdecuacionRequest(
            Type: "AS",
            Diagnostico: "Dificultades de aprendizaje (test IA — eliminar)",
            CondicionEspecial: "Discalculia diagnosticada",
            EstrategiasMediacion: "Apoyo visual y manipulativo",
            EstrategiasEvaluacion: "Evaluación oral alternativa",
            Observaciones: "Generado por test automático");

        // 1. Upsert
        var upsertResp = await Api.PutAsJsonAsync(
            $"/api/grupos/{GroupId}/estudiantes/{StudentId}/adecuacion", req);
        await AssertStatusAsync(upsertResp, HttpStatusCode.OK);
        var adecuacion = await ReadJsonAsync<AdecuacionResponse>(upsertResp);

        // 2. Disparar generación IA
        var genResp = await Api.PostAsJsonAsync<object>(
            $"/api/grupos/{GroupId}/estudiantes/{StudentId}/adecuacion/generar", new { });
        await AssertStatusAsync(genResp, HttpStatusCode.Accepted);

        // 3. Poll hasta Ready o Failed
        AdecuacionResponse? final = null;
        var deadline = DateTimeOffset.UtcNow + JobTimeout;

        while (DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(PollInterval);
            var pollResp = await Api.GetAsync(
                $"/api/grupos/{GroupId}/estudiantes/{StudentId}/adecuacion");
            pollResp.StatusCode.Should().Be(HttpStatusCode.OK);
            final = await ReadJsonAsync<AdecuacionResponse>(pollResp);
            if (final.Status is "Ready" or "Failed") break;
        }

        final.Should().NotBeNull();
        final!.Status.Should().Be("Ready",
            because: $"después de {JobTimeout.TotalMinutes} min la propuesta debe estar lista. " +
                     "Si es 'Failed' revisar Hangfire y logs en /api/diag/audit.");

        final.PropuestaGenerada.Should().NotBeNullOrWhiteSpace(
            because: "una adecuación Ready debe tener propuesta pedagógica generada");

        // 4. Descargar informe PDF
        var pdfResp = await Api.GetAsync(
            $"/api/grupos/{GroupId}/estudiantes/{StudentId}/adecuacion/informe");
        await AssertStatusAsync(pdfResp, HttpStatusCode.OK);
        pdfResp.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        var pdfBytes = await pdfResp.Content.ReadAsByteArrayAsync();
        pdfBytes.Length.Should().BeGreaterThan(100,
            because: "el PDF debe tener contenido real");

        // 5. Cleanup
        await Api.DeleteAsync($"/api/grupos/{GroupId}/estudiantes/{StudentId}/adecuacion");
    }

    // ── DTOs ─────────────────────────────────────────────────────────────────

    private record UpsertAdecuacionRequest(
        [property: JsonPropertyName("type")]                 string Type,
        [property: JsonPropertyName("diagnostico")]          string Diagnostico,
        [property: JsonPropertyName("condicionEspecial")]    string? CondicionEspecial,
        [property: JsonPropertyName("estrategiasMediacion")] string? EstrategiasMediacion,
        [property: JsonPropertyName("estrategiasEvaluacion")] string? EstrategiasEvaluacion,
        [property: JsonPropertyName("observaciones")]        string? Observaciones);

    private record AdecuacionResponse(
        [property: JsonPropertyName("id")]               Guid Id,
        [property: JsonPropertyName("studentId")]        Guid StudentId,
        [property: JsonPropertyName("groupId")]          Guid GroupId,
        [property: JsonPropertyName("type")]             string Type,
        [property: JsonPropertyName("diagnostico")]      string Diagnostico,
        [property: JsonPropertyName("propuestaGenerada")] string? PropuestaGenerada,
        [property: JsonPropertyName("status")]           string Status,
        [property: JsonPropertyName("errorMessage")]     string? ErrorMessage);

    private record AdecuacionResumen(
        [property: JsonPropertyName("id")]          Guid Id,
        [property: JsonPropertyName("studentId")]   Guid StudentId,
        [property: JsonPropertyName("studentName")] string StudentName,
        [property: JsonPropertyName("type")]        string Type,
        [property: JsonPropertyName("status")]      string Status);
}
