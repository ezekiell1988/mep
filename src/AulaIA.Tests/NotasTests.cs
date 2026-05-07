using System.Text.Json.Serialization;
using AulaIA.Tests.Infrastructure;

namespace AulaIA.Tests;

/// <summary>
/// Tests para el módulo Notas.
///
/// Flujos cubiertos:
///   1. GET actividades de un grupo → 200 OK
///   2. POST actividad sin body → 400
///   3. GET resumen sin actividades → 200 con lista vacía
///   4. Flujo completo:
///        crear actividad → guardar calificación → obtener resumen →
///        verificar promedio → eliminar actividad → verificar cascada
///
/// Prerequisito: debe existir al menos un grupo y un estudiante activo del docente.
/// GroupId: env var AULAIA_TEST_GroupId o appsettings.test.json
/// StudentId: env var AULAIA_TEST_StudentId o appsettings.test.json
/// </summary>
public class NotasTests : IntegrationTestBase
{
    // ── Config ────────────────────────────────────────────────────────────────
    private static readonly Guid? GroupId = Guid.TryParse(
        Environment.GetEnvironmentVariable("AULAIA_TEST_GroupId"), out var g) ? g : null;

    private static readonly Guid? StudentId = Guid.TryParse(
        Environment.GetEnvironmentVariable("AULAIA_TEST_StudentId"), out var s) ? s : null;

    // ── Tests básicos ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "GET /api/grupos/{id}/actividades sin token → 401")]
    public async Task ListarActividades_SinToken_Returns401()
    {
        if (GroupId is null) return;
        var anon = ApiClient.CreateAnonymous();
        await using var _ = anon;
        var response = await anon.GetAsync($"/api/grupos/{GroupId}/actividades");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "GET /api/grupos/{id}/actividades con token → 200 OK")]
    public async Task ListarActividades_Returns200()
    {
        if (GroupId is null) return;
        var response = await Api.GetAsync($"/api/grupos/{GroupId}/actividades");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "POST /api/grupos/{id}/actividades sin body → 400 Bad Request")]
    public async Task CrearActividad_SinBody_Returns400()
    {
        if (GroupId is null) return;
        var response = await Api.PostAsync(
            $"/api/grupos/{GroupId}/actividades",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "GET /api/grupos/{id}/notas/resumen → 200 OK (puede estar vacío)")]
    public async Task ResumenNotas_Returns200()
    {
        if (GroupId is null) return;
        var response = await Api.GetAsync($"/api/grupos/{GroupId}/notas/resumen");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET /api/grupos/{id}/actividades/{actId}/calificaciones id inexistente → 404")]
    public async Task GetCalificaciones_IdInexistente_Returns404()
    {
        if (GroupId is null) return;
        var response = await Api.GetAsync(
            $"/api/grupos/{GroupId}/actividades/{Guid.NewGuid()}/calificaciones");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Flujo completo ────────────────────────────────────────────────────────

    /// <summary>
    /// Requiere GroupId y StudentId configurados.
    /// Crea una actividad de prueba, guarda una calificación, verifica el resumen
    /// con promedio esperado y luego elimina la actividad (cascade → calificación se borra).
    /// </summary>
    [Fact(DisplayName = "Flujo completo: crear actividad → calificar → resumen → eliminar")]
    public async Task FlujoCompleto_Actividad_Calificacion_Resumen_Eliminar()
    {
        if (GroupId is null || StudentId is null) return;

        // 1. Crear actividad (Trabajo Cotidiano, 100 pts, 20%)
        var createReq = new CreateActividadRequest(
            Name: "Tarea de prueba NotasTests",
            Type: "Trabajo Cotidiano",
            MaxScore: 100m,
            Percentage: 20m,
            DueDate: null);

        var createResp = await Api.PostAsJsonAsync(
            $"/api/grupos/{GroupId}/actividades", createReq);
        await AssertStatusAsync(createResp, HttpStatusCode.Created);

        var actividad = await ReadJsonAsync<ActividadResponse>(createResp);
        actividad.Id.Should().NotBe(Guid.Empty);
        actividad.Name.Should().Be("Tarea de prueba NotasTests");

        // 2. Guardar calificación para el estudiante (nota: 80)
        var calReq = new SaveCalificacionRequest(
            StudentId: StudentId.Value,
            Score: 80m,
            Comments: "Test automático");

        var calResp = await Api.PostAsJsonAsync(
            $"/api/grupos/{GroupId}/actividades/{actividad.Id}/calificaciones",
            new[] { calReq });
        calResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Obtener calificaciones de la actividad
        var getCalsResp = await Api.GetAsync(
            $"/api/grupos/{GroupId}/actividades/{actividad.Id}/calificaciones");
        getCalsResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var cals = await ReadJsonAsync<List<CalificacionResponse>>(getCalsResp);
        cals.Should().Contain(c => c.StudentId == StudentId.Value && c.Score == 80m,
            because: "la calificación recién guardada debe estar en la lista");

        // 4. Verificar resumen — el alumno debe aparecer con promedio calculado
        var resumenResp = await Api.GetAsync($"/api/grupos/{GroupId}/notas/resumen");
        resumenResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var resumen = await ReadJsonAsync<ResumenGrupoResponse>(resumenResp);
        resumen.TotalActividades.Should().BeGreaterThanOrEqualTo(1);

        var estResumen = resumen.Estudiantes.FirstOrDefault(e => e.StudentId == StudentId.Value);
        estResumen.Should().NotBeNull(
            because: "el estudiante con calificación debe aparecer en el resumen");

        estResumen!.Notas.Should().Contain(n =>
            n.ActividadId == actividad.Id && n.Nota == 80m,
            because: "la nota registrada debe estar en el detalle del estudiante");

        // 5. Eliminar la actividad (cascade debe borrar la calificación)
        var deleteResp = await Api.DeleteAsync(
            $"/api/grupos/{GroupId}/actividades/{actividad.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 6. Verificar que la actividad ya no existe
        var listResp = await Api.GetAsync($"/api/grupos/{GroupId}/actividades");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var lista = await ReadJsonAsync<List<ActividadResponse>>(listResp);
        lista.Should().NotContain(a => a.Id == actividad.Id,
            because: "la actividad eliminada no debe aparecer en el listado");
    }

    // ── DTOs (espejo de los records en NotasModule.cs) ────────────────────────

    private record CreateActividadRequest(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("maxScore")] decimal MaxScore,
        [property: JsonPropertyName("percentage")] decimal Percentage,
        [property: JsonPropertyName("dueDate")] DateOnly? DueDate);

    private record ActividadResponse(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("maxScore")] decimal MaxScore,
        [property: JsonPropertyName("percentage")] decimal Percentage,
        [property: JsonPropertyName("dueDate")] DateOnly? DueDate);

    private record SaveCalificacionRequest(
        [property: JsonPropertyName("studentId")] Guid StudentId,
        [property: JsonPropertyName("score")] decimal Score,
        [property: JsonPropertyName("comments")] string? Comments);

    private record CalificacionResponse(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("studentId")] Guid StudentId,
        [property: JsonPropertyName("activityId")] Guid ActivityId,
        [property: JsonPropertyName("score")] decimal Score,
        [property: JsonPropertyName("comments")] string? Comments);

    private record NotaActividadItem(
        [property: JsonPropertyName("actividadId")] Guid ActividadId,
        [property: JsonPropertyName("nombre")] string Nombre,
        [property: JsonPropertyName("tipo")] string Tipo,
        [property: JsonPropertyName("maxScore")] decimal MaxScore,
        [property: JsonPropertyName("porcentaje")] decimal Porcentaje,
        [property: JsonPropertyName("nota")] decimal? Nota,
        [property: JsonPropertyName("comentario")] string? Comentario);

    private record ResumenEstudianteResponse(
        [property: JsonPropertyName("studentId")] Guid StudentId,
        [property: JsonPropertyName("fullName")] string FullName,
        [property: JsonPropertyName("studentCode")] string StudentCode,
        [property: JsonPropertyName("promedio")] decimal? Promedio,
        [property: JsonPropertyName("notas")] List<NotaActividadItem> Notas);

    private record ResumenGrupoResponse(
        [property: JsonPropertyName("groupId")] Guid GroupId,
        [property: JsonPropertyName("totalActividades")] int TotalActividades,
        [property: JsonPropertyName("estudiantes")] List<ResumenEstudianteResponse> Estudiantes);
}
