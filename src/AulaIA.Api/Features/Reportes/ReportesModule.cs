namespace AulaIA.Api.Features.Reportes;

public static class ReportesModule
{
    public static IServiceCollection AddReportesModule(this IServiceCollection services)
    {
        services.AddScoped<ActaNotasService>();
        services.AddScoped<ReporteAsistenciaService>();
        services.AddScoped<InformeDirectorService>();
        return services;
    }

    public static IEndpointRouteBuilder MapReportesEndpoints(this IEndpointRouteBuilder app)
    {
        var reportes = app.MapGroup("/api/grupos/{grupoId:guid}/reportes")
                          .WithTags("Reportes")
                          .RequireAuthorization();

        reportes.MapGet("/notas/xlsx",       DownloadNotasXlsxAsync).WithName("DescargarActaXlsx");
        reportes.MapGet("/notas/pdf",        DownloadNotasPdfAsync).WithName("DescargarActaPdf");
        reportes.MapGet("/asistencia/xlsx",  DownloadAsistenciaXlsxAsync).WithName("DescargarAsistenciaXlsx");
        reportes.MapGet("/asistencia/pdf",   DownloadAsistenciaPdfAsync).WithName("DescargarAsistenciaPdf");
        reportes.MapGet("/informe-director", DownloadInformeDirectorAsync).WithName("DescargarInformeDirector");

        return app;
    }

    private static async Task<IResult> DownloadNotasXlsxAsync(
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

    private static async Task<IResult> DownloadNotasPdfAsync(
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

    private static async Task<IResult> DownloadAsistenciaXlsxAsync(
        Guid grupoId, string? from, string? to,
        ReporteAsistenciaService svc, CancellationToken ct)
    {
        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
            return TypedResults.BadRequest(new { error = "Parámetros 'from' y 'to' requeridos (YYYY-MM-DD)." });
        try
        {
            var bytes = await svc.GenerateXlsxAsync(grupoId, fromDate, toDate, ct);
            return Results.File(bytes,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: $"asistencia-{grupoId:N}-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx");
        }
        catch (KeyNotFoundException e)
        {
            return TypedResults.NotFound(new { error = e.Message });
        }
    }

    private static async Task<IResult> DownloadAsistenciaPdfAsync(
        Guid grupoId, string? from, string? to,
        ReporteAsistenciaService svc, CancellationToken ct)
    {
        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
            return TypedResults.BadRequest(new { error = "Parámetros 'from' y 'to' requeridos (YYYY-MM-DD)." });
        try
        {
            var bytes = await svc.GeneratePdfAsync(grupoId, fromDate, toDate, ct);
            return Results.File(bytes,
                contentType: "application/pdf",
                fileDownloadName: $"asistencia-{grupoId:N}-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.pdf");
        }
        catch (KeyNotFoundException e)
        {
            return TypedResults.NotFound(new { error = e.Message });
        }
    }

    private static async Task<IResult> DownloadInformeDirectorAsync(
        Guid grupoId, string? from, string? to,
        InformeDirectorService svc, CancellationToken ct)
    {
        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
            return TypedResults.BadRequest(new { error = "Parámetros 'from' y 'to' requeridos (YYYY-MM-DD)." });
        try
        {
            var bytes = await svc.GeneratePdfAsync(grupoId, fromDate, toDate, ct);
            return Results.File(bytes,
                contentType: "application/pdf",
                fileDownloadName: $"informe-director-{grupoId:N}-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.pdf");
        }
        catch (KeyNotFoundException e)
        {
            return TypedResults.NotFound(new { error = e.Message });
        }
    }
}
