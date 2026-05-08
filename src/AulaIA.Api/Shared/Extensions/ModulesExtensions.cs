using AulaIA.Api.Features.Adecuaciones;
using AulaIA.Api.Features.Asistencia;
using AulaIA.Api.Features.Calendario;
using AulaIA.Api.Features.Curriculum;
using AulaIA.Api.Features.Curriculum.Jobs;
using AulaIA.Api.Features.Dashboard;
using AulaIA.Api.Features.Estudiantes;
using AulaIA.Api.Features.Grupos;
using AulaIA.Api.Features.Notas;
using AulaIA.Api.Features.Planeamiento;
using AulaIA.Api.Features.PowerSync;
using AulaIA.Api.Features.Reportes;
using AulaIA.Api.Features.Suscripciones;
using AulaIA.Api.Features.Suscripciones.Jobs;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace AulaIA.Api.Shared.Extensions;

public static class ModulesExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Registra todos los módulos de feature en el contenedor DI.</summary>
        public void AddAulaIAModules() =>
            services
                .AddGruposModule()
                .AddEstudiantesModule()
                .AddAsistenciaModule()
                .AddNotasModule()
                .AddPlaneamientoModule()
                .AddCurriculumModule()
                .AddReportesModule()
                .AddCalendarioModule()
                .AddAdecuacionesModule()
                .AddPowerSyncModule()
                .AddSuscripcionesModule();
    }

    extension(WebApplication app)
    {
        /// <summary>Mapea todos los endpoints de los módulos de feature.</summary>
        public void MapAulaIAEndpoints() =>
            app.MapGruposEndpoints()
               .MapEstudiantesEndpoints()
               .MapAsistenciaEndpoints()
               .MapNotasEndpoints()
               .MapPlaneamientoEndpoints()
               .MapCurriculumEndpoints()
               .MapReportesEndpoints()
               .MapCalendarioEndpoints()
               .MapAdecuacionesEndpoints()
               .MapDashboardEndpoints()
               .MapPowerSyncEndpoints()
               .MapSuscripcionesEndpoints()
               .MapPaymentsEndpoints()
               .MapReferralsEndpoints();

        /// <summary>Registra todos los recurring jobs de Hangfire.</summary>
        public void AddAulaIARecurringJobs()
        {
            RecurringJob.AddOrUpdate<UpdateExchangeRateJob>(
                "update-exchange-rate",
                j => j.ExecuteAsync(CancellationToken.None),
                "0 12 * * *");   // 12 PM UTC = 6 AM Costa Rica

            RecurringJob.AddOrUpdate<CheckExpiredSubscriptionsJob>(
                "check-expired-subscriptions",
                j => j.ExecuteAsync(CancellationToken.None),
                "0 8 * * *");    // 8 AM UTC = 2 AM Costa Rica

            // "0 0 30 2 *" → 30 de febrero: nunca ejecuta automáticamente.
            // Disparar manualmente desde /hangfire.
            RecurringJob.AddOrUpdate<SyncCurriculumJob>(
                "sync-curriculum-mep",
                j => j.ExecuteAsync(CancellationToken.None),
                "0 0 30 2 *");   // manual only
        }

        /// <summary>Aplica las migraciones EF Core pendientes al arrancar.</summary>
        public async Task RunMigrationsAsync()
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider
                          .GetRequiredService<AulaIA.Api.Shared.Persistence.AulaIADbContext>();
            await db.Database.MigrateAsync();
        }
    }
}
