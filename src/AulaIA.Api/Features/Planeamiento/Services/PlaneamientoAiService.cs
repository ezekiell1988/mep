using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace AulaIA.Api.Features.Planeamiento.Services;

public sealed class PlaneamientoAiService(
    IOptions<AiOptions> aiOpts,
    AulaIADbContext db,
    ILogger<PlaneamientoAiService> logger)
{
    public async Task<string> GenerarAsync(
        LessonPlan plan,
        CancellationToken ct)
    {
        var units = await db.CurriculumUnits
            .Where(u => u.Asignatura == plan.Asignatura
                     && u.Nivel == plan.Nivel
                     && u.Trimestre == plan.Trimestre
                     && u.ValidatedAt != null)
            .OrderBy(u => u.UnidadNumero)
            .ToListAsync(ct);

        if (units.Count == 0)
        {
            logger.LogWarning("No hay unidades de currículo validadas para {Asignatura} {Nivel}° T{Trimestre}",
                plan.Asignatura, plan.Nivel, plan.Trimestre);
            throw new InvalidOperationException(
                $"No hay programa de {plan.Asignatura} {plan.Nivel}° validado para el Trimestre {plan.Trimestre}.");
        }

        var curriculumContext = BuildCurriculumContext(units);
        var totalLecciones = plan.LeccionesPorSemana *
            (int)Math.Ceiling((plan.FechaFin.DayNumber - plan.FechaInicio.DayNumber) / 7.0);

        var opts = aiOpts.Value;
        var azureClient = new AzureOpenAIClient(
            new Uri(opts.Endpoint),
            new Azure.AzureKeyCredential(opts.ApiKey ?? ""));
        var chatClient = azureClient.GetChatClient(opts.DeploymentName);

        var systemPrompt = $"""
            Eres un asistente experto en planificación didáctica del sistema educativo de Costa Rica (MEP).
            Generates planeamientos en el formato oficial del MEP, usando EXACTAMENTE los aprendizajes esperados,
            indicadores y contenidos del programa oficial que se te proporciona. NUNCA inventes contenido curricular.

            Reglas de formato:
            - Usa Markdown estructurado con encabezados ##, tablas y listas.
            - Sección "Datos Generales" con: docente, institución, asignatura, nivel, sección, trimestre, año lectivo, período.
            - Sección "Aprendizajes Esperados" — copia textual del programa.
            - Sección "Indicadores de Evaluación" — copia textual del programa.
            - Sección "Contenidos" — tabla con tres columnas: Conceptual | Procedimental | Actitudinal.
            - Sección "Estrategias de Mediación" — tabla semana por semana con: Semana | Fecha | Lección # | Actividad de Inicio | Desarrollo | Cierre | Recursos.
            - Sección "Evaluación" — instrumentos y criterios.
            - Sección "Tareas Sugeridas" — lista de tareas listas para asignar.
            - Idioma: español formal de Costa Rica. Terminología exacta del MEP.
            """;

        var userMessage = $"""
            Genera el planeamiento con los siguientes parámetros:
            - Asignatura: {plan.Asignatura}
            - Nivel: {plan.Nivel}° año
            - Trimestre: {plan.Trimestre}
            - Año lectivo: {plan.AnioLectivo}
            - Período: {plan.FechaInicio:dd/MM/yyyy} al {plan.FechaFin:dd/MM/yyyy}
            - Lecciones por semana: {plan.LeccionesPorSemana}
            - Total de lecciones estimadas: {totalLecciones}

            PROGRAMA OFICIAL DEL MEP (usar EXACTAMENTE este contenido):
            {curriculumContext}
            """;

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage)
        };

        var result = await chatClient.CompleteChatAsync(messages, cancellationToken: ct);
        return result.Value.Content[0].Text;
    }

    private static string BuildCurriculumContext(List<CurriculumUnit> units)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var unit in units)
        {
            sb.AppendLine($"## Unidad {unit.UnidadNumero}: {unit.UnidadNombre}");
            sb.AppendLine("### Aprendizajes Esperados");
            foreach (var a in unit.AprendizajesEsperados) sb.AppendLine($"- {a}");
            sb.AppendLine("### Indicadores de Evaluación");
            foreach (var i in unit.IndicadoresEvaluacion) sb.AppendLine($"- {i}");
            sb.AppendLine("### Contenidos");
            sb.AppendLine("**Conceptual:** " + string.Join("; ", unit.ContenidoConceptual));
            sb.AppendLine("**Procedimental:** " + string.Join("; ", unit.ContenidoProcedimental));
            sb.AppendLine("**Actitudinal:** " + string.Join("; ", unit.ContenidoActitudinal));
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
