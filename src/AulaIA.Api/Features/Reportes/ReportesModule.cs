namespace AulaIA.Api.Features.Reportes;

public static class ReportesModule
{
    public static IServiceCollection AddReportesModule(this IServiceCollection services)
    {
        services.AddScoped<ActaNotasService>();
        return services;
    }

    public static IEndpointRouteBuilder MapReportesEndpoints(this IEndpointRouteBuilder app)
    {
        var reportes = app.MapGroup("/api/grupos/{grupoId:guid}/reportes")
                          .WithTags("Reportes")
                          .RequireAuthorization();

        reportes.MapGet("/notas/xlsx", DownloadXlsxAsync).WithName("DescargarActaXlsx");
        reportes.MapGet("/notas/pdf", DownloadPdfAsync).WithName("DescargarActaPdf");

        return app;
    }

    private static async Task<IResult> DownloadXlsxAsync(
        Guid grupoId, ActaNotasService svc, CancellationToken ct)
    {
        try
        {
            var bytes = await svc.GenerateXlsxAsync(grupoId, ct);
            return Results.File(bytes,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: $"acta-notas-{grupoId:N}.xlsx");
        }
        catch (KeyNotFoundException e)
        {
            return TypedResults.NotFound(new { error = e.Message });
        }
    }

    private static async Task<IResult> DownloadPdfAsync(
        Guid grupoId, ActaNotasService svc, CancellationToken ct)
    {
        try
        {
            var bytes = await svc.GeneratePdfAsync(grupoId, ct);
            return Results.File(bytes,
                contentType: "application/pdf",
                fileDownloadName: $"acta-notas-{grupoId:N}.pdf");
        }
        catch (KeyNotFoundException e)
        {
            return TypedResults.NotFound(new { error = e.Message });
        }
    }
}
