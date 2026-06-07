using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;

namespace AulaIA.Api.Features.Planeamiento.Services;

public sealed partial class PlaneamientoPdfService(AulaIADbContext db)
{
    public async Task<(byte[] Bytes, string FileName)> GenerateAsync(
        Guid planId,
        string teacherSub,
        CancellationToken ct)
    {
        var plan = await db.LessonPlans
            .AsNoTracking()
            .Include(p => p.Group)
                .ThenInclude(g => g!.Institution)
            .FirstOrDefaultAsync(p => p.Id == planId && p.TeacherSub == teacherSub, ct)
            ?? throw new KeyNotFoundException($"Planeamiento {planId} no encontrado.");

        if (plan.Status != LessonPlanStatus.Ready || string.IsNullOrWhiteSpace(plan.ContenidoGenerado))
            throw new InvalidOperationException("El planeamiento todavía no está listo para descargar.");

        QuestPDF.Settings.License = LicenseType.Community;

        var lines = NormalizeMarkdown(plan.ContenidoGenerado);
        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Text("AulaIA — Planeamiento Didáctico")
                        .Bold().FontSize(14).FontColor(Colors.Blue.Darken3);
                    col.Item().PaddingTop(2).Text(
                        $"{plan.Asignatura} · {plan.Nivel}° año · Trimestre {plan.Trimestre} · {plan.AnioLectivo}")
                        .FontSize(10).FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(2).Text(
                        $"Grupo: {plan.Group?.Name ?? "—"} · Institución: {plan.Group?.Institution?.Name ?? "—"} · " +
                        $"Período: {plan.FechaInicio:dd/MM/yyyy} al {plan.FechaFin:dd/MM/yyyy}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Blue.Darken3);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(5);
                    foreach (var line in lines)
                    {
                        AddMarkdownLine(col, line);
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();

        return (bytes, $"planeamiento-{plan.Asignatura.ToLowerInvariant().Replace(" ", "-")}-{plan.Id:N}.pdf");
    }

    private static IReadOnlyList<string> NormalizeMarkdown(string markdown) =>
        markdown.Replace("\r\n", "\n")
            .Split('\n')
            .Select(line => line.TrimEnd())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

    private static void AddMarkdownLine(ColumnDescriptor col, string rawLine)
    {
        var line = CleanMarkdown(rawLine);
        if (line.StartsWith("### ", StringComparison.Ordinal))
        {
            col.Item().PaddingTop(6).Text(line[4..]).Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
            return;
        }

        if (line.StartsWith("## ", StringComparison.Ordinal))
        {
            col.Item().PaddingTop(9).Text(line[3..]).Bold().FontSize(13).FontColor(Colors.Blue.Darken3);
            return;
        }

        if (line.StartsWith("# ", StringComparison.Ordinal))
        {
            col.Item().PaddingTop(10).Text(line[2..]).Bold().FontSize(16).FontColor(Colors.Blue.Darken4);
            return;
        }

        if (line.StartsWith("|", StringComparison.Ordinal))
        {
            col.Item().Text(line).FontSize(8).FontFamily("Courier New");
            return;
        }

        if (line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal))
        {
            col.Item().PaddingLeft(10).Text($"• {line[2..]}").FontSize(9);
            return;
        }

        col.Item().Text(line).FontSize(9);
    }

    private static string CleanMarkdown(string value)
    {
        var cleaned = BoldOrItalicRegex().Replace(value, match =>
            match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value);
        return cleaned.Replace("`", "");
    }

    [GeneratedRegex(@"\*\*([^*]+)\*\*|\*([^*]+)\*")]
    private static partial Regex BoldOrItalicRegex();
}
