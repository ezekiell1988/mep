using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AulaIA.Api.Features.Planeamiento.Services;

public sealed partial class PlaneamientoPdfService(AulaIADbContext db)
{
    private const string PrimaryColor = "#1D4ED8";
    private const string PrimarySoft = "#DBEAFE";
    private const string PrimaryMuted = "#EFF6FF";
    private const string InkColor = "#0F172A";
    private const string MutedColor = "#475569";
    private const string BorderColor = "#CBD5E1";

    public async Task<(byte[] Bytes, string FileName)> GenerateAsync(
        Guid planId,
        string teacherSub,
        CancellationToken ct)
    {
        var plan = await db.LessonPlans
            .AsNoTracking()
            .Include(p => p.Group)
                .ThenInclude(g => g!.Institution)
            .Include(p => p.Group)
                .ThenInclude(g => g!.Teacher)
            .FirstOrDefaultAsync(p => p.Id == planId && p.TeacherSub == teacherSub, ct)
            ?? throw new KeyNotFoundException($"Planeamiento {planId} no encontrado.");

        if (plan.Status != LessonPlanStatus.Ready || string.IsNullOrWhiteSpace(plan.ContenidoGenerado))
            throw new InvalidOperationException("El planeamiento todavia no esta listo para descargar.");

        QuestPDF.Settings.License = LicenseType.Community;

        var blocks = ParseBlocks(plan.ContenidoGenerado);
        var context = BuildRenderContext(plan, blocks);
        var contentBlocks = RemoveDuplicateCoverBlocks(blocks);

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1.6f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontFamily("Arial").FontColor(InkColor));

                page.Header().Element(c => RenderHeader(c, context));

                page.Content().PaddingTop(10).Column(col =>
                {
                    RenderHero(col, context);
                    RenderSummary(col, context);

                    foreach (var block in contentBlocks)
                    {
                        RenderBlock(col, block, context);
                    }
                });

                page.Footer().Element(c => RenderFooter(c, context));
            });
        }).GeneratePdf();

        return (bytes, BuildFileName(plan));
    }

    private static PdfRenderContext BuildRenderContext(LessonPlan plan, IReadOnlyList<MarkdownBlock> blocks)
    {
        var dataTable = blocks
            .OfType<TableBlock>()
            .FirstOrDefault(IsDatosGeneralesTable);

        var unitTitle = TryGetDataValue(dataTable, "Unidad") ?? plan.Asignatura;
        var totalLecciones = TryGetDataValue(dataTable, "Total de lecciones estimadas")
            ?? EstimateTotalLessons(plan).ToString(CultureInfo.InvariantCulture);

        return new PdfRenderContext(
            Title: "Planeamiento Didactico",
            Subject: plan.Asignatura,
            UnitTitle: unitTitle,
            TeacherName: plan.Group?.Teacher?.FullName ?? "Docente por completar",
            InstitutionName: plan.Group?.Institution?.Name ?? "Institucion por completar",
            GroupName: plan.Group?.Name ?? "Grupo por completar",
            LevelLabel: $"{plan.Nivel}° ano",
            PeriodLabel: $"{plan.FechaInicio:dd/MM/yyyy} al {plan.FechaFin:dd/MM/yyyy}",
            TrimesterLabel: $"Trimestre {plan.Trimestre}",
            SchoolYearLabel: plan.AnioLectivo.ToString(CultureInfo.InvariantCulture),
            LessonsPerWeekLabel: $"{plan.LeccionesPorSemana} por semana",
            TotalLessonsLabel: totalLecciones,
            GeneratedAtLabel: DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture));
    }

    private static IReadOnlyList<MarkdownBlock> RemoveDuplicateCoverBlocks(IReadOnlyList<MarkdownBlock> blocks)
    {
        var start = 0;
        if (blocks.Count > 0 &&
            blocks[0] is HeadingBlock firstHeading &&
            firstHeading.Level <= 2 &&
            firstHeading.Text.Contains("Planeamiento", StringComparison.OrdinalIgnoreCase))
        {
            start = 1;
        }

        return blocks.Skip(start).ToArray();
    }

    private static void RenderHeader(IContainer container, PdfRenderContext context)
    {
        container.Column(col =>
        {
            col.Item().Background(PrimaryColor).Height(10);
            col.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Column(inner =>
                {
                    inner.Item().Text("AulaIA").Bold().FontSize(11).FontColor(PrimaryColor);
                    inner.Item().Text($"{context.Subject}  |  {context.GroupName}  |  {context.TrimesterLabel}")
                        .FontSize(8.5f).FontColor(MutedColor);
                });
                row.ConstantItem(180).AlignRight().Column(inner =>
                {
                    inner.Item().Text(context.InstitutionName).FontSize(8.5f).FontColor(MutedColor);
                    inner.Item().Text($"Periodo: {context.PeriodLabel}").FontSize(8.5f).FontColor(MutedColor);
                });
            });
            col.Item().PaddingTop(6).LineHorizontal(1).LineColor(BorderColor);
        });
    }

    private static void RenderHero(ColumnDescriptor col, PdfRenderContext context)
    {
        col.Item().Background(PrimaryMuted).Border(1).BorderColor(PrimarySoft).Padding(16).Column(hero =>
        {
            hero.Item().Text(context.Title).Bold().FontSize(18).FontColor(PrimaryColor);
            hero.Item().PaddingTop(4).Text(context.Subject).Bold().FontSize(13).FontColor(InkColor);
            hero.Item().PaddingTop(2).Text(context.UnitTitle).FontSize(10).FontColor(MutedColor);

            hero.Item().PaddingTop(10).Row(row =>
            {
                RenderTag(row, context.LevelLabel);
                RenderTag(row, context.TrimesterLabel);
                RenderTag(row, $"Ano lectivo {context.SchoolYearLabel}");
                RenderTag(row, context.LessonsPerWeekLabel);
            });
        });
    }

    private static void RenderTag(RowDescriptor row, string value)
    {
        row.AutoItem().PaddingRight(6).Element(c =>
            c.Background(Colors.White)
             .Border(1)
             .BorderColor(PrimarySoft)
             .PaddingVertical(4)
             .PaddingHorizontal(8))
            .Text(value)
            .FontSize(8.5f)
            .FontColor(PrimaryColor);
    }

    private static void RenderSummary(ColumnDescriptor col, PdfRenderContext context)
    {
        col.Item().PaddingTop(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            RenderSummaryCell(table, "Docente", context.TeacherName);
            RenderSummaryCell(table, "Institucion", context.InstitutionName);
            RenderSummaryCell(table, "Grupo", context.GroupName);
            RenderSummaryCell(table, "Periodo", context.PeriodLabel);
            RenderSummaryCell(table, "Lecciones/semana", context.LessonsPerWeekLabel);
            RenderSummaryCell(table, "Lecciones estimadas", context.TotalLessonsLabel);
        });
    }

    private static void RenderSummaryCell(TableDescriptor table, string label, string value)
    {
        table.Cell().Padding(4).Element(c =>
            c.Border(1)
             .BorderColor(BorderColor)
             .Padding(8))
            .Column(col =>
            {
                col.Item().Text(label).FontSize(7.5f).Bold().FontColor(MutedColor);
                col.Item().PaddingTop(2).Text(value).FontSize(9.5f).FontColor(InkColor);
            });
    }

    private static void RenderBlock(ColumnDescriptor col, MarkdownBlock block, PdfRenderContext context)
    {
        switch (block)
        {
            case HeadingBlock heading:
                RenderHeading(col, heading);
                break;
            case ParagraphBlock paragraph:
                col.Item().PaddingTop(5).Text(paragraph.Text).FontSize(9.5f).LineHeight(1.35f);
                break;
            case BulletListBlock bullets:
                foreach (var item in bullets.Items)
                {
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        row.ConstantItem(14).PaddingTop(2).Text("•").Bold().FontColor(PrimaryColor);
                        row.RelativeItem().Text(item).FontSize(9.3f).LineHeight(1.3f);
                    });
                }
                break;
            case QuoteBlock quote:
                col.Item().PaddingTop(8).Element(c =>
                    c.BorderLeft(3)
                     .BorderColor(PrimaryColor)
                     .Background(PrimaryMuted)
                     .Padding(10))
                    .Text(quote.Text)
                    .Italic()
                    .FontSize(9)
                    .FontColor(MutedColor);
                break;
            case HorizontalRuleBlock:
                col.Item().PaddingVertical(8).LineHorizontal(1).LineColor(BorderColor);
                break;
            case TableBlock table:
                RenderTable(col, table, context);
                break;
        }
    }

    private static void RenderHeading(ColumnDescriptor col, HeadingBlock heading)
    {
        if (heading.Level <= 2)
        {
            col.Item().PaddingTop(14).Element(c =>
                c.Background(PrimarySoft)
                 .BorderLeft(4)
                 .BorderColor(PrimaryColor)
                 .PaddingVertical(8)
                 .PaddingHorizontal(10))
                .Text(heading.Text)
                .Bold()
                .FontSize(11.5f)
                .FontColor(InkColor);
            return;
        }

        col.Item().PaddingTop(10).Text(heading.Text).Bold().FontSize(10).FontColor(PrimaryColor);
    }

    private static void RenderTable(ColumnDescriptor col, TableBlock table, PdfRenderContext context)
    {
        if (IsDatosGeneralesTable(table))
        {
            RenderDatosGeneralesTable(col, table, context);
            return;
        }

        var columnCount = Math.Max(table.Headers.Count, table.Rows.Count == 0 ? 0 : table.Rows.Max(r => r.Count));
        if (columnCount == 0)
            return;

        col.Item().PaddingTop(8).Table(t =>
        {
            t.ColumnsDefinition(columns =>
            {
                for (var i = 0; i < columnCount; i++)
                    columns.RelativeColumn();
            });

            foreach (var header in table.Headers)
            {
                t.Cell().Element(c => TableHeaderStyle(c)).Text(header).Bold().FontSize(8.3f).FontColor(Colors.White);
            }

            var shade = false;
            foreach (var row in table.Rows)
            {
                var rowBackground = shade ? "#F8FAFC" : "#FFFFFF";
                shade = !shade;

                for (var i = 0; i < columnCount; i++)
                {
                    var value = i < row.Count ? row[i] : string.Empty;
                    t.Cell().Element(c => TableCellStyle(c, rowBackground))
                        .Text(value)
                        .FontSize(GetTableFontSize(columnCount))
                        .LineHeight(1.2f);
                }
            }
        });
    }

    private static void RenderDatosGeneralesTable(ColumnDescriptor col, TableBlock table, PdfRenderContext context)
    {
        col.Item().PaddingTop(8).Table(t =>
        {
            t.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.2f);
                columns.RelativeColumn(2.8f);
            });

            foreach (var row in table.Rows)
            {
                if (row.Count < 2)
                    continue;

                var label = row[0];
                var value = ResolveDatosGeneralesValue(label, row[1], context);

                t.Cell().Element(c => TableHeaderStyle(c, alignLeft: true))
                    .Text(label)
                    .Bold()
                    .FontSize(8.5f)
                    .FontColor(Colors.White);

                t.Cell().Element(c => TableCellStyle(c, Colors.White))
                    .Text(value)
                    .FontSize(9.2f);
            }
        });
    }

    private static IContainer TableHeaderStyle(IContainer container, bool alignLeft = false) =>
        (alignLeft ? container.AlignLeft() : container.AlignCenter())
            .Background(PrimaryColor)
            .Border(1)
            .BorderColor(BorderColor)
            .Padding(6)
            .AlignMiddle();

    private static IContainer TableCellStyle(IContainer container, string background) =>
        container.Background(background)
            .Border(1)
            .BorderColor(BorderColor)
            .Padding(6)
            .AlignMiddle();

    private static float GetTableFontSize(int columnCount) =>
        columnCount switch
        {
            <= 2 => 9.2f,
            3 => 8.8f,
            4 => 8.4f,
            5 => 8.1f,
            _ => 7.5f
        };

    private static void RenderFooter(IContainer container, PdfRenderContext context)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(6).LineHorizontal(1).LineColor(BorderColor);
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text($"Generado por AulaIA el {context.GeneratedAtLabel}")
                    .FontSize(8)
                    .FontColor(MutedColor);
                row.AutoItem().Text(text =>
                {
                    text.Span("Pagina ").FontSize(8).FontColor(MutedColor);
                    text.CurrentPageNumber().FontSize(8).FontColor(MutedColor);
                    text.Span(" de ").FontSize(8).FontColor(MutedColor);
                    text.TotalPages().FontSize(8).FontColor(MutedColor);
                });
            });
        });
    }

    private static bool IsDatosGeneralesTable(TableBlock table) =>
        table.Headers.Count >= 2 &&
        string.Equals(table.Headers[0], "Elemento", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(table.Headers[1], "Informacion", StringComparison.OrdinalIgnoreCase);

    private static string ResolveDatosGeneralesValue(string label, string currentValue, PdfRenderContext context)
    {
        var normalized = NormalizeLabel(label);
        var cleaned = CleanupPlaceholder(currentValue);

        return normalized switch
        {
            "docente" => PreferContextValue(cleaned, context.TeacherName),
            "institucion" => PreferContextValue(cleaned, context.InstitutionName),
            "asignatura" => PreferContextValue(cleaned, context.Subject),
            "nivel" => PreferContextValue(cleaned, context.LevelLabel),
            "seccion" => PreferContextValue(cleaned, context.GroupName),
            "trimestre" => PreferContextValue(cleaned, context.TrimesterLabel.Replace("Trimestre ", "", StringComparison.Ordinal)),
            "anolectivo" => PreferContextValue(cleaned, context.SchoolYearLabel),
            "periodo" => PreferContextValue(cleaned, context.PeriodLabel),
            "leccionesporsemana" => PreferContextValue(cleaned, context.LessonsPerWeekLabel.Replace(" por semana", "", StringComparison.Ordinal)),
            "totaldeleccionesestimadas" => PreferContextValue(cleaned, context.TotalLessonsLabel),
            "unidad" => PreferContextValue(cleaned, context.UnitTitle),
            _ => string.IsNullOrWhiteSpace(cleaned) ? "—" : cleaned
        };
    }

    private static string PreferContextValue(string currentValue, string fallback) =>
        string.IsNullOrWhiteSpace(currentValue) || currentValue == "—" ? fallback : currentValue;

    private static string CleanupPlaceholder(string value)
    {
        var cleaned = value.Replace("_", "").Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "—" : cleaned;
    }

    private static string NormalizeLabel(string label)
    {
        var normalized = label.Normalize(NormalizationForm.FormD);
        var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(chars.ToArray())
            .Replace(" ", "", StringComparison.Ordinal)
            .Replace(":", "", StringComparison.Ordinal)
            .ToLowerInvariant();
    }

    private static string? TryGetDataValue(TableBlock? table, string label)
    {
        if (table is null)
            return null;

        foreach (var row in table.Rows)
        {
            if (row.Count < 2)
                continue;

            if (string.Equals(row[0], label, StringComparison.OrdinalIgnoreCase))
                return CleanupPlaceholder(row[1]);
        }

        return null;
    }

    private static int EstimateTotalLessons(LessonPlan plan)
    {
        var totalDays = plan.FechaFin.DayNumber - plan.FechaInicio.DayNumber + 1;
        var estimatedWeeks = Math.Max(1, (int)Math.Ceiling(totalDays / 7d));
        return estimatedWeeks * plan.LeccionesPorSemana;
    }

    private static string BuildFileName(LessonPlan plan)
    {
        var slug = NonAlphaNumericRegex()
            .Replace(plan.Asignatura.Normalize(NormalizationForm.FormD), string.Empty)
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant()
            .Replace(" ", "-", StringComparison.Ordinal);

        return $"planeamiento-{slug}-{plan.Id:N}.pdf";
    }

    private static IReadOnlyList<MarkdownBlock> ParseBlocks(string markdown)
    {
        var lines = NormalizeMarkdown(markdown);
        var blocks = new List<MarkdownBlock>();

        for (var i = 0; i < lines.Count;)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                i++;
                continue;
            }

            if (TryParseHeading(line, out var heading))
            {
                blocks.Add(heading);
                i++;
                continue;
            }

            if (IsHorizontalRule(line))
            {
                blocks.Add(new HorizontalRuleBlock());
                i++;
                continue;
            }

            if (LooksLikeTableStart(lines, i))
            {
                blocks.Add(ParseTable(lines, ref i));
                continue;
            }

            if (IsBullet(line))
            {
                blocks.Add(ParseBulletList(lines, ref i));
                continue;
            }

            if (line.StartsWith(">", StringComparison.Ordinal))
            {
                blocks.Add(ParseQuote(lines, ref i));
                continue;
            }

            blocks.Add(ParseParagraph(lines, ref i));
        }

        return blocks;
    }

    private static IReadOnlyList<string> NormalizeMarkdown(string markdown) =>
        markdown.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select(line => line.TrimEnd())
            .ToArray();

    private static bool TryParseHeading(string line, out HeadingBlock heading)
    {
        var match = HeadingRegex().Match(line);
        if (match.Success)
        {
            heading = new HeadingBlock(match.Groups[1].Value.Length, CleanMarkdown(match.Groups[2].Value));
            return true;
        }

        heading = null!;
        return false;
    }

    private static bool IsHorizontalRule(string line)
    {
        var trimmed = line.Trim();
        return trimmed is "---" or "***";
    }

    private static bool IsBullet(string line) =>
        line.StartsWith("- ", StringComparison.Ordinal) || line.StartsWith("* ", StringComparison.Ordinal);

    private static bool LooksLikeTableStart(IReadOnlyList<string> lines, int index) =>
        index + 1 < lines.Count &&
        lines[index].TrimStart().StartsWith("|", StringComparison.Ordinal) &&
        TableSeparatorRegex().IsMatch(lines[index + 1].Trim());

    private static TableBlock ParseTable(IReadOnlyList<string> lines, ref int index)
    {
        var headers = SplitMarkdownRow(lines[index]);
        index += 2; // skip separator line

        var rows = new List<IReadOnlyList<string>>();
        while (index < lines.Count && lines[index].TrimStart().StartsWith("|", StringComparison.Ordinal))
        {
            rows.Add(SplitMarkdownRow(lines[index]));
            index++;
        }

        return new TableBlock(headers, rows);
    }

    private static BulletListBlock ParseBulletList(IReadOnlyList<string> lines, ref int index)
    {
        var items = new List<string>();
        while (index < lines.Count && IsBullet(lines[index]))
        {
            items.Add(CleanMarkdown(lines[index][2..]));
            index++;
        }

        return new BulletListBlock(items);
    }

    private static QuoteBlock ParseQuote(IReadOnlyList<string> lines, ref int index)
    {
        var parts = new List<string>();
        while (index < lines.Count && lines[index].StartsWith(">", StringComparison.Ordinal))
        {
            parts.Add(CleanMarkdown(lines[index].TrimStart('>', ' ')));
            index++;
        }

        return new QuoteBlock(string.Join(" ", parts));
    }

    private static ParagraphBlock ParseParagraph(IReadOnlyList<string> lines, ref int index)
    {
        var parts = new List<string>();
        while (index < lines.Count)
        {
            var line = lines[index];
            if (string.IsNullOrWhiteSpace(line) ||
                TryParseHeading(line, out _) ||
                IsHorizontalRule(line) ||
                LooksLikeTableStart(lines, index) ||
                IsBullet(line) ||
                line.StartsWith(">", StringComparison.Ordinal))
            {
                break;
            }

            parts.Add(CleanMarkdown(line));
            index++;
        }

        return new ParagraphBlock(string.Join(" ", parts));
    }

    private static IReadOnlyList<string> SplitMarkdownRow(string line) =>
        line.Trim()
            .Trim('|')
            .Split('|')
            .Select(part => CleanMarkdown(part.Trim()))
            .ToArray();

    private static string CleanMarkdown(string value)
    {
        var withoutFormatting = BoldOrItalicRegex().Replace(value, match =>
            match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value);

        return withoutFormatting
            .Replace("`", string.Empty, StringComparison.Ordinal)
            .Replace("\\|", "|", StringComparison.Ordinal)
            .Trim();
    }

    [GeneratedRegex(@"^(#{1,6})\s+(.+)$")]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"^\|?\s*:?-{3,}:?\s*(\|\s*:?-{3,}:?\s*)+\|?$")]
    private static partial Regex TableSeparatorRegex();

    [GeneratedRegex(@"\*\*([^*]+)\*\*|\*([^*]+)\*")]
    private static partial Regex BoldOrItalicRegex();

    [GeneratedRegex(@"[\p{Mn}\p{P}\p{S}]+")]
    private static partial Regex NonAlphaNumericRegex();

    private sealed record PdfRenderContext(
        string Title,
        string Subject,
        string UnitTitle,
        string TeacherName,
        string InstitutionName,
        string GroupName,
        string LevelLabel,
        string PeriodLabel,
        string TrimesterLabel,
        string SchoolYearLabel,
        string LessonsPerWeekLabel,
        string TotalLessonsLabel,
        string GeneratedAtLabel);

    private abstract record MarkdownBlock;
    private sealed record HeadingBlock(int Level, string Text) : MarkdownBlock;
    private sealed record ParagraphBlock(string Text) : MarkdownBlock;
    private sealed record BulletListBlock(IReadOnlyList<string> Items) : MarkdownBlock;
    private sealed record QuoteBlock(string Text) : MarkdownBlock;
    private sealed record HorizontalRuleBlock() : MarkdownBlock;
    private sealed record TableBlock(IReadOnlyList<string> Headers, IReadOnlyList<IReadOnlyList<string>> Rows) : MarkdownBlock;
}
