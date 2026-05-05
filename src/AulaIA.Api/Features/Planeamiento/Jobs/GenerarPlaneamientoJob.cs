using AulaIA.Api.Features.Planeamiento.Services;
using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Hangfire;

namespace AulaIA.Api.Features.Planeamiento.Jobs;

public sealed class GenerarPlaneamientoJob(
    AulaIADbContext db,
    PlaneamientoAiService aiService,
    ILogger<GenerarPlaneamientoJob> logger)
{
    [Queue("planeamiento")]
    [AutomaticRetry(Attempts = 1)]
    public async Task ExecuteAsync(Guid planId, CancellationToken ct)
    {
        var plan = await db.LessonPlans.FindAsync([planId], ct);
        if (plan is null)
        {
            logger.LogWarning("LessonPlan {PlanId} no encontrado", planId);
            return;
        }

        plan.Status = LessonPlanStatus.Generating;
        await db.SaveChangesAsync(ct);

        try
        {
            plan.ContenidoGenerado = await aiService.GenerarAsync(plan, ct);
            plan.Status = LessonPlanStatus.Ready;
            plan.GeneratedAt = DateTimeOffset.UtcNow;
            logger.LogInformation("Planeamiento {PlanId} generado correctamente", planId);
        }
        catch (Exception ex)
        {
            plan.Status = LessonPlanStatus.Failed;
            plan.ErrorMessage = ex.Message;
            logger.LogError(ex, "Error generando planeamiento {PlanId}", planId);
        }

        await db.SaveChangesAsync(ct);
    }
}
