using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AulaIA.Api.Features.Reportes;

/// <summary>
/// Genera el reporte de asistencia por período en PDF y XLSX.
/// Columnas: fechas del rango con registro. Filas: alumnos activos.
/// Resumen por alumno: totales P/A/T/J y % asistencia.
/// </summary>
public sealed class ReporteAsistenciaService(AulaIADbContext db)
{
    private sealed record ReporteData(
        Group Grupo,
        Institution? Institucion,
        List<Student> Estudiantes,
        List<DateOnly> Fechas,
        Dictionary<(Guid StudentId, DateOnly Date), AttendanceStatus> Registros);

    private async Task<ReporteData> LoadAsync(Guid groupId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var grupo = await db.Groups
            .Include(g => g.Institution)
            .FirstOrDefaultAsync(g => g.Id == groupId, ct)
            ?? throw new KeyNotFoundException($"Grupo {groupId} no encontrado.");

        var estudiantes = await db.Students
            .AsNoTracking()
            .Where(s => s.GroupId == groupId && s.IsActive)
            .OrderBy(s => s.FullName)
            .ToListAsync(ct);

        var registros = await db.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.GroupId == groupId && r.Date >= from && r.Date <= to)
            .ToListAsync(ct);

        var fechas = registros
            .Select(r => r.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var dict = registros.ToDictionary(r => (r.StudentId, r.Date), r => r.Status);

        return new ReporteData(grupo, grupo.Institution, estudiantes, fechas, dict);
    }

    private static (int P, int A, int T, int J) CountsFor(
        Guid studentId, List<DateOnly> fechas, Dictionary<(Guid, DateOnly), AttendanceStatus> dict)
    {
        int p = 0, a = 0, t = 0, j = 0;
        foreach (var f in fechas)
        {
            if (!dict.TryGetValue((studentId, f), out var st)) continue;
            switch (st)
            {
                case AttendanceStatus.Present:   p++; break;
                case AttendanceStatus.Absent:    a++; break;
                case AttendanceStatus.Late:      t++; break;
                case AttendanceStatus.Justified: j++; break;
            }
        }
        return (p, a, t, j);
    }

    private static string Label(AttendanceStatus st) => st switch
    {
        AttendanceStatus.Present   => "P",
        AttendanceStatus.Absent    => "A",
        AttendanceStatus.Late      => "T",
        AttendanceStatus.Justified => "J",
        _ => "—",
    };

    // ────────────────────────────── XLSX ─────────────────────────────────────

    public async Task<byte[]> GenerateXlsxAsync(Guid groupId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var data = await LoadAsync(groupId, from, to, ct);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Asistencia");

        // ── Encabezado institucional ──
        ws.Cell("A1").Value = "MINISTERIO DE EDUCACIÓN PÚBLICA — REGISTRO DE ASISTENCIA";
        ws.Range("A1:Z1").Merge().Style.Font.Bold = true;
        ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Cell("A2").Value = $"Institución: {data.Institucion?.Name ?? "—"}";
        ws.Cell("A3").Value = $"Grupo: {data.Grupo.Name}   |   Asignatura: {data.Grupo.Subject}   |   Nivel: {data.Grupo.Level}   |   Año: {data.Grupo.SchoolYear}";
        ws.Cell("A4").Value = $"Período: {from:dd/MM/yyyy} — {to:dd/MM/yyyy}   |   Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

        // ── Cabecera columnas (fila 6) ──
        int headerRow = 6;
        ws.Cell(headerRow, 1).Value = "Expediente";
        ws.Cell(headerRow, 2).Value = "Nombre completo";

        int col = 3;
        foreach (var f in data.Fechas)
        {
            ws.Cell(headerRow, col).Value = f.ToString("dd/MM");
            ws.Column(col).Width = 7;
            col++;
        }

        ws.Cell(headerRow, col).Value = "P";
        ws.Cell(headerRow, col + 1).Value = "A";
        ws.Cell(headerRow, col + 2).Value = "T";
        ws.Cell(headerRow, col + 3).Value = "J";
        ws.Cell(headerRow, col + 4).Value = "% Asist.";

        var headerRange = ws.Range(headerRow, 1, headerRow, col + 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Row(headerRow).Height = 36;

        // ── Filas de estudiantes ──
        int row = headerRow + 1;
        foreach (var est in data.Estudiantes)
        {
            ws.Cell(row, 1).Value = est.StudentCode;
            ws.Cell(row, 2).Value = est.FullName;
            ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            int c = 3;
            foreach (var f in data.Fechas)
            {
                if (data.Registros.TryGetValue((est.Id, f), out var st))
                {
                    ws.Cell(row, c).Value = Label(st);
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(row, c).Style.Fill.BackgroundColor = st switch
                    {
                        AttendanceStatus.Present   => XLColor.FromHtml("#dcfce7"),
                        AttendanceStatus.Absent    => XLColor.FromHtml("#fee2e2"),
                        AttendanceStatus.Late      => XLColor.FromHtml("#fef9c3"),
                        AttendanceStatus.Justified => XLColor.FromHtml("#dbeafe"),
                        _ => XLColor.NoColor,
                    };
                }
                else
                {
                    ws.Cell(row, c).Value = "—";
                    ws.Cell(row, c).Style.Font.FontColor = XLColor.LightGray;
                    ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
                c++;
            }

            var (p, a, t, j) = CountsFor(est.Id, data.Fechas, data.Registros);
            int total = p + a + t + j;
            double pctAsist = total > 0 ? Math.Round((double)(p + t + j) / total * 100, 1) : 0;

            ws.Cell(row, c).Value = p;
            ws.Cell(row, c + 1).Value = a;
            ws.Cell(row, c + 2).Value = t;
            ws.Cell(row, c + 3).Value = j;
            ws.Cell(row, c + 4).Value = total > 0 ? $"{pctAsist}%" : "—";

            // Color % asistencia
            if (total > 0)
            {
                ws.Cell(row, c + 4).Style.Fill.BackgroundColor = pctAsist >= 85
                    ? XLColor.FromHtml("#dcfce7")
                    : pctAsist >= 70
                        ? XLColor.FromHtml("#fef9c3")
                        : XLColor.FromHtml("#fee2e2");
            }

            for (int ci = c; ci <= c + 4; ci++)
                ws.Cell(row, ci).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Zebra
            if (row % 2 == 0)
            {
                var zebraRange = ws.Range(row, 1, row, c + 4);
                foreach (var cell in zebraRange.Cells())
                    if (cell.Style.Fill.BackgroundColor == XLColor.NoColor)
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#f9fafb");
            }

            row++;
        }

        // ── Resumen totales por columna (fila final) ──
        if (data.Fechas.Count > 0 && data.Estudiantes.Count > 0)
        {
            row++;
            ws.Cell(row, 2).Value = "TOTALES POR FECHA";
            ws.Cell(row, 2).Style.Font.Bold = true;

            int c = 3;
            foreach (var f in data.Fechas)
            {
                int presentes = data.Registros.Count(kv => kv.Key.Date == f && kv.Value == AttendanceStatus.Present);
                ws.Cell(row, c).Value = presentes;
                ws.Cell(row, c).Style.Font.Bold = true;
                ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c++;
            }
        }

        // ── Formato final ──
        ws.Column(1).Width = 14;
        ws.Column(2).Width = 36;
        ws.SheetView.FreezeRows(headerRow);
        ws.SheetView.FreezeColumns(2);

        var tableRange = ws.Range(headerRow, 1, row, col + 4);
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#e5e7eb");

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ─────────────────────────────── PDF ─────────────────────────────────────

    public async Task<byte[]> GeneratePdfAsync(Guid groupId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var data = await LoadAsync(groupId, from, to, ct);

        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(8));

                // ── Header ──
                page.Header().Column(col =>
                {
                    col.Item().Text("MINISTERIO DE EDUCACIÓN PÚBLICA — REGISTRO DE ASISTENCIA")
                        .FontSize(11).Bold().AlignCenter();
                    col.Item().PaddingTop(4).Text(
                        $"{data.Grupo.Name}  ·  {data.Grupo.Subject}  ·  {data.Grupo.Level}  ·  {data.Grupo.SchoolYear}  |  " +
                        $"Institución: {data.Institucion?.Name ?? "—"}")
                        .FontSize(8).AlignCenter();
                    col.Item().Text(
                        $"Período: {from:dd/MM/yyyy} — {to:dd/MM/yyyy}   |   Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(7).FontColor(Colors.Grey.Darken1).AlignCenter();
                    col.Item().PaddingTop(4).LineHorizontal(0.5f);
                });

                // ── Cuerpo ──
                page.Content().PaddingTop(6).Table(table =>
                {
                    // Columnas: Nombre + fechas + P + A + T + J + %Asist
                    int totalCols = 2 + data.Fechas.Count + 5;
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3f);   // Nombre
                        cols.ConstantColumn(60);   // Expediente
                        foreach (var _ in data.Fechas)
                            cols.ConstantColumn(28);
                        cols.ConstantColumn(20); // P
                        cols.ConstantColumn(20); // A
                        cols.ConstantColumn(20); // T
                        cols.ConstantColumn(20); // J
                        cols.ConstantColumn(40); // %
                    });

                    // ── Cabecera ──
                    static IContainer HeaderCell(IContainer c) =>
                        c.Background(Colors.Blue.Darken4)
                         .Padding(3)
                         .AlignCenter()
                         .AlignMiddle();

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Nombre").FontColor(Colors.White).Bold();
                        header.Cell().Element(HeaderCell).Text("Exp.").FontColor(Colors.White).Bold();
                        foreach (var f in data.Fechas)
                            header.Cell().Element(HeaderCell).Text(f.ToString("dd/MM")).FontColor(Colors.White).FontSize(6.5f);
                        foreach (var lbl in new[] { "P", "A", "T", "J", "% As." })
                            header.Cell().Element(HeaderCell).Text(lbl).FontColor(Colors.White).Bold();
                    });

                    // ── Filas ──
                    bool shade = false;
                    foreach (var est in data.Estudiantes)
                    {
                        var bg = shade ? Colors.Grey.Lighten4 : Colors.White;
                        shade = !shade;

                        IContainer Cell(IContainer c) =>
                            c.Background(bg).Padding(3).AlignMiddle();

                        table.Cell().Element(Cell).Text(est.FullName).FontSize(7.5f);
                        table.Cell().Element(Cell).AlignCenter().Text(est.StudentCode).FontSize(7);

                        foreach (var f in data.Fechas)
                        {
                            if (data.Registros.TryGetValue((est.Id, f), out var st))
                            {
                                var cellBg = st switch
                                {
                                    AttendanceStatus.Present   => Colors.Green.Lighten4,
                                    AttendanceStatus.Absent    => Colors.Red.Lighten4,
                                    AttendanceStatus.Late      => Colors.Yellow.Lighten3,
                                    AttendanceStatus.Justified => Colors.Blue.Lighten4,
                                    _ => bg,
                                };
                                table.Cell().Background(cellBg).Padding(3).AlignCenter().AlignMiddle()
                                    .Text(Label(st)).Bold().FontSize(7);
                            }
                            else
                            {
                                table.Cell().Element(Cell).AlignCenter()
                                    .Text("—").FontColor(Colors.Grey.Lighten2);
                            }
                        }

                        var (p, a, t, j) = CountsFor(est.Id, data.Fechas, data.Registros);
                        int total = p + a + t + j;
                        double pct = total > 0 ? Math.Round((double)(p + t + j) / total * 100, 1) : 0;

                        table.Cell().Element(Cell).AlignCenter().Text(p.ToString()).FontColor(Colors.Green.Darken2);
                        table.Cell().Element(Cell).AlignCenter().Text(a.ToString()).FontColor(Colors.Red.Darken2);
                        table.Cell().Element(Cell).AlignCenter().Text(t.ToString()).FontColor(Colors.Orange.Darken2);
                        table.Cell().Element(Cell).AlignCenter().Text(j.ToString()).FontColor(Colors.Blue.Darken2);

                        var pctBg = total == 0 ? bg : pct >= 85 ? Colors.Green.Lighten4 : pct >= 70 ? Colors.Yellow.Lighten3 : Colors.Red.Lighten4;
                        table.Cell().Background(pctBg).Padding(3).AlignCenter().AlignMiddle()
                            .Text(total > 0 ? $"{pct}%" : "—").Bold().FontSize(7);
                    }
                });

                // ── Footer ──
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("AulaIA · Página ").FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontColor(Colors.Grey.Medium);
                    x.Span(" de ").FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontColor(Colors.Grey.Medium);
                });
            });
        });

        return doc.GeneratePdf();
    }
}
