using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Options;
using AulaIA.Api.Shared.Persistence;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace AulaIA.Api.Features.Adecuaciones.Services;

/// <summary>
/// Genera la propuesta pedagógica de adecuación curricular usando GPT.
/// El output está anclado al tipo de adecuación (AS/ANS/AA) y al programa MEP del grupo.
/// </summary>
public sealed class AdecuacionAiService(
    IOptions<AiOptions> aiOpts,
    AzureOpenAIClient aiClient,
    AulaIADbContext db,
    ILogger<AdecuacionAiService> logger)
{
    public async Task<string> GenerarPropuestaAsync(Accommodation acc, CancellationToken ct)
    {
        var student = await db.Students
            .FirstOrDefaultAsync(s => s.Id == acc.StudentId, ct);
        var group = await db.Groups
            .FirstOrDefaultAsync(g => g.Id == acc.GroupId, ct);

        if (student is null || group is null)
            throw new InvalidOperationException("Estudiante o grupo no encontrado.");

        // Currículo validado del grupo (si existe)
        var units = await db.CurriculumUnits
            .Where(u => u.Asignatura == group.Subject
                     && u.ValidatedAt != null)
            .OrderBy(u => u.Nivel).ThenBy(u => u.Trimestre).ThenBy(u => u.UnidadNumero)
            .Take(10)
            .ToListAsync(ct);

        var chatClient = aiClient.GetChatClient(aiOpts.Value.DeploymentName);

        var tipoDescripcion = acc.Type switch
        {
            AccommodationType.AS  => "Adecuación Significativa (AS) — modifica el currículo oficial. Requiere registro en SIMED. El estudiante trabaja con objetivos y contenidos adaptados.",
            AccommodationType.ANS => "Adecuación No Significativa (ANS) — no modifica los objetivos ni contenidos del currículo. Solo ajusta la forma de presentar y evaluar.",
            AccommodationType.AA  => "Apoyo Académico (AA) — refuerzo pedagógico sin modificación curricular formal. No requiere registro en SIMED.",
            _                     => acc.Type.ToString()
        };

        var systemPrompt = $"""
            Eres un especialista en adecuaciones curriculares del sistema educativo de Costa Rica (MEP).
            Tu tarea es generar una propuesta pedagógica individual para un estudiante con adecuación curricular,
            siguiendo los lineamientos de la Ley 7600 y las directrices del Departamento de Educación Especial del MEP.

            Reglas:
            - Usa Markdown estructurado con encabezados ##.
            - La propuesta debe ser concreta, aplicable en el aula, y en español formal costarricense.
            - Para AS: propón objetivos modificados, contenidos adaptados y criterios de evaluación diferenciados.
            - Para ANS: propón ajustes en la forma de presentar el contenido, metodología y evaluación, SIN cambiar objetivos.
            - Para AA: propón actividades de refuerzo y apoyo adicional, estrategias de motivación.
            - Incluye siempre una tabla de "Estrategias de Mediación" y una de "Estrategias de Evaluación".
            - Incluye una sección "Seguimiento y Compromisos" con indicadores observables mensualmente.
            - NO inventes diagnósticos ni condiciones médicas. Trabaja solo con lo que te proporciona el docente.
            """;

        var curriculumContext = units.Count > 0
            ? "PROGRAMA OFICIAL (referencia):\n" + string.Join("\n", units.Select(u =>
                $"- Unidad {u.UnidadNumero}: {u.UnidadNombre} | AE: {string.Join("; ", u.AprendizajesEsperados.Take(2))}"))
            : "";

        var userMessage = $"""
            Genera la propuesta pedagógica de adecuación curricular con los siguientes datos:

            **Estudiante:** {student.FullName} (expediente: {student.StudentCode})
            **Asignatura:** {group.Subject}
            **Nivel:** {group.Level}
            **Tipo de adecuación:** {acc.Type} — {tipoDescripcion}
            **Diagnóstico:** {acc.Diagnostico}
            {(acc.CondicionEspecial is not null ? $"**Condición especial:** {acc.CondicionEspecial}" : "")}
            {(acc.EstrategiasMediacion is not null ? $"**Estrategias propuestas por el docente:** {acc.EstrategiasMediacion}" : "")}
            {(acc.Observaciones is not null ? $"**Observaciones del docente:** {acc.Observaciones}" : "")}
            {curriculumContext}
            """;

        logger.LogInformation("Generando propuesta adecuación {AccId} para estudiante {StudentId}",
            acc.Id, acc.StudentId);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage)
        };

        var result = await chatClient.CompleteChatAsync(messages, cancellationToken: ct);
        return result.Value.Content[0].Text;
    }
}
