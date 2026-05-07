using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AulaIA.Api.Features.Reportes;

/// <summary>
/// Genera el acta de notas del período en XLSX (compatible SEA) y PDF.
/// Ponderación MEP estándar: Trabajo Cotidiano 20% / Pruebas 45% / Extraclase 20% / Otros 15%.
/// Umbral de aprobación: 65 (III Ciclo o menor) / 70 (Diversificado 10°–12°).
/// </summary>
public sealed class ActaNotasService(AulaIADbContext db)
{
    // ─────────────────────────── datos comunes ───────────────────────────────

    private sealed record ActaData(
        Group Grupo,
        Institution? Institucion,
        List<Student> Estudiantes,
        List<EvaluationActivity> Actividades,
        List<Grade> Calificaciones);

    private async Task<ActaData> LoadAsync(Guid groupId, CancellationToken ct)
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

        var actividades = await db.EvaluationActivities
            .AsNoTracking()
            .Where(a => a.GroupId == groupId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(ct);

        var actIds = actividades.Select(a => a.Id).ToList();
        var calificaciones = actIds.Count > 0
            ? await db.Grades.AsNoTracking().Where(g => actIds.Contains(g.ActivityId)).ToListAsync(ct)
            : [];

        return new ActaData(grupo, grupo.Institution, estudiantes, actividades, calificaciones);
    }

    private static decimal? Promedio(Student est, List<EvaluationActivity> acts, List<Grade> grades)
    {
        var ponderadas = acts
            .Where(a => a.Percentage > 0)
            .Select(a =>
            {
                var g = grades.FirstOrDefault(x => x.ActivityId == a.Id && x.StudentId == est.Id);
                if (g is null) return ((decimal Peso, decimal Puntaje)?)null;
                var norm = a.MaxScore > 0 ? g.Score / a.MaxScore * 100m : 0m;
                return (Peso: a.Percentage, Puntaje: norm * a.Percentage);
            })
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        if (ponderadas.Count == 0) return null;
        var pesoTotal = ponderadas.Sum(p => p.Peso);
        return pesoTotal > 0 ? Math.Round(ponderadas.Sum(p => p.Puntaje) / pesoTotal, 1) : null;
    }

    private static int Umbral(int nivel) => nivel >= 10 ? 70 : 65;

    // ────────────────────────────── XLSX ─────────────────────────────────────

    public async Task<byte[]> GenerateXlsxAsync(Guid groupId, CancellationToken ct)
    {
        var data = await LoadAsync(groupId, ct);
        var u = Umbral(int.TryParse(data.Grupo.Level, out var n) ? n : 0);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Acta de Notas");

        // ── Encabezado ──
        ws.Cell("A1").Value = "MINISTERIO DE EDUCACIÓN PÚBLICA — ACTA DE NOTAS";
        ws.Range("A1:Z1").Merge().Style.Font.Bold = true;
        ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Cell("A2").Value = $"Institución: {data.Institucion?.Name ?? "—"}";
        ws.Cell("A3").Value = $"Grupo: {data.Grupo.Name}   |   Asignatura: {data.Grupo.Subject}   |   Nivel: {data.Grupo.Level}   |   Año: {data.Grupo.SchoolYear}";
        ws.Cell("A4").Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}   |   Umbral aprobatorio: {u}";

        // ── Cabecera de columnas (fila 6) ──
        int headerRow = 6;
        ws.Cell(headerRow, 1).Value = "Expediente";
        ws.Cell(headerRow, 2).Value = "Nombre completo";

        int col = 3;
        foreach (var act in data.Actividades)
        {
            ws.Cell(headerRow, col).Value = $"{act.Name}\n({act.Type})\n{act.Percentage}%";
            ws.Column(col).Width = 14;
            col++;
        }
        ws.Cell(headerRow, col).Value = "Promedio";
        ws.Cell(headerRow, col + 1).Value = "Estado";

        // Estilo cabecera
        var headerRange = ws.Range(headerRow, 1, headerRow, col + 1);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Alignment.WrapText = true;
        ws.Row(headerRow).Height = 48;

        // ── Filas de estudiantes ──
        int row = headerRow + 1;
        int aprobados = 0, reprobados = 0, sinNotas = 0;

        foreach (var est in data.Estudiantes)
        {
            ws.Cell(row, 1).Value = est.StudentCode;
            ws.Cell(row, 2).Value = est.FullName;
            ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            int c = 3;
            foreach (var act in data.Actividades)
            {
                var grade = data.Calificaciones.FirstOrDefault(g => g.ActivityId == act.Id && g.StudentId == est.Id);
                if (grade is not null)
                    ws.Cell(row, c).Value = (double)grade.Score;
                else
                    ws.Cell(row, c).Value = "—";
                ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                c++;
            }

            var prom = Promedio(est, data.Actividades, data.Calificaciones);
            if (prom.HasValue)
            {
                ws.Cell(row, c).Value = (double)prom.Value;
                ws.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, c).Style.Font.Bold = true;
                if (prom.Value >= u)
                {
                    ws.Cell(row, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#dcfce7");
                    ws.Cell(row, c + 1).Value = "Aprobado";
                    ws.Cell(row, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#dcfce7");
                    aprobados++;
                }
                else
                {
                    ws.Cell(row, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#fee2e2");
                    ws.Cell(row, c + 1).Value = "Reprobado";
                    ws.Cell(row, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#fee2e2");
                    ws.Cell(row, c + 1).Style.Font.FontColor = XLColor.FromHtml("#991b1b");
                    reprobados++;
                }
            }
            else
            {
                ws.Cell(row, c).Value = "—";
                ws.Cell(row, c + 1).Value = "Sin notas";
                sinNotas++;
            }

            // Zebra
            if (row % 2 == 0)
                ws.Range(row, 1, row, c + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#f9fafb");

            row++;
        }

        // ── Resumen ──
        row++;
        ws.Cell(row, 1).Value = "RESUMEN";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row + 1, 1).Value = $"Total estudiantes: {data.Estudiantes.Count}";
        ws.Cell(row + 2, 1).Value = $"Aprobados: {aprobados}";
        ws.Cell(row + 3, 1).Value = $"Reprobados: {reprobados}";
        ws.Cell(row + 4, 1).Value = $"Sin notas: {sinNotas}";
        if (data.Estudiantes.Count > 0)
        {
            var pct = Math.Round((double)aprobados / data.Estudiantes.Count * 100, 1);
            ws.Cell(row + 5, 1).Value = $"% Aprobación: {pct}%";
        }

        // ── Formato final ──
        ws.Column(1).Width = 14;
        ws.Column(2).Width = 36;
        ws.Column(col).Width = 12;
        ws.Column(col + 1).Width = 14;
        ws.SheetView.FreezeRows(headerRow);

        // Bordes tabla
        var tableRange = ws.Range(headerRow, 1, row - 2, col + 1);
        tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
        tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    // ────────────────────────────── PDF ──────────────────────────────────────

    public async Task<byte[]> GeneratePdfAsync(Guid groupId, CancellationToken ct)
    {
        var data = await LoadAsync(groupId, ct);
        var u = Umbral(int.TryParse(data.Grupo.Level, out var n) ? n : 0);

        // Pre-calcular promedios
        var promedios = data.Estudiantes
            .Select(e => (est: e, prom: Promedio(e, data.Actividades, data.Calificaciones)))
            .ToList();

        int aprobados = promedios.Count(x => x.prom.HasValue && x.prom.Value >= u);
        int reprobados = promedios.Count(x => x.prom.HasValue && x.prom.Value < u);

        QuestPDF.Settings.License = LicenseType.Community;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("MINISTERIO DE EDUCACIÓN PÚBLICA — ACTA DE NOTAS")
                        .Bold().FontSize(13).AlignCenter();
                    col.Item().PaddingTop(4).Text(
                        $"Institución: {data.Institucion?.Name ?? "—"}   |   " +
                        $"Grupo: {data.Grupo.Name}   |   Asignatura: {data.Grupo.Subject}   |   " +
                        $"Nivel: {data.Grupo.Level}   |   Año: {data.Grupo.SchoolYear}");
                    col.Item().Text(
                        $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}   |   " +
                        $"Umbral aprobatorio: {u}   |   " +
                        $"Aprobados: {aprobados}/{data.Estudiantes.Count}   |   " +
                        $"Reprobados: {reprobados}/{data.Estudiantes.Count}")
                        .FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Blue.Darken3);
                });

                page.Content().PaddingTop(8).Table(table =>
                {
                    // Definir columnas
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(60);   // Expediente
                        cols.RelativeColumn(3);     // Nombre
                        foreach (var _ in data.Actividades)
                            cols.RelativeColumn(1); // Una columna por actividad
                        cols.ConstantColumn(45);    // Promedio
                        cols.ConstantColumn(50);    // Estado
                    });

                    // Cabecera
                    static IContainer HeaderCell(IContainer c) => c
                        .Background(Colors.Blue.Darken3).Padding(4).AlignCenter();

                    table.Header(h =>
                    {
                        h.Cell().Element(HeaderCell).Text("Expediente").FontColor(Colors.White).Bold();
                        h.Cell().Element(HeaderCell).Text("Nombre completo").FontColor(Colors.White).Bold();
                        foreach (var act in data.Actividades)
                            h.Cell().Element(HeaderCell)
                                .Text($"{act.Name}\n{act.Percentage}%").FontColor(Colors.White).Bold();
                        h.Cell().Element(HeaderCell).Text("Promedio").FontColor(Colors.White).Bold();
                        h.Cell().Element(HeaderCell).Text("Estado").FontColor(Colors.White).Bold();
                    });

                    // Filas
                    bool alt = false;
                    foreach (var (est, prom) in promedios)
                    {
                        string bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                        alt = !alt;

                        IContainer DataCell(IContainer c) => c
                            .Background(bg).Padding(3).AlignCenter();

                        table.Cell().Element(DataCell).Text(est.StudentCode);
                        table.Cell().Background(bg).Padding(3).AlignLeft().Text(est.FullName);

                        foreach (var act in data.Actividades)
                        {
                            var grade = data.Calificaciones
                                .FirstOrDefault(g => g.ActivityId == act.Id && g.StudentId == est.Id);
                            table.Cell().Element(DataCell)
                                .Text(grade?.Score.ToString("0.#") ?? "—");
                        }

                        if (prom.HasValue)
                        {
                            bool aprobado = prom.Value >= u;
                            string promBg = aprobado ? Colors.Green.Lighten3 : Colors.Red.Lighten3;
                            table.Cell().Background(promBg).Padding(3).AlignCenter()
                                .Text(prom.Value.ToString("0.#")).Bold();
                            table.Cell().Background(promBg).Padding(3).AlignCenter()
                                .Text(aprobado ? "Aprobado" : "Reprobado")
                                .FontColor(aprobado ? Colors.Green.Darken3 : Colors.Red.Darken3).Bold();
                        }
                        else
                        {
                            table.Cell().Background(bg).Padding(3).AlignCenter().Text("—");
                            table.Cell().Background(bg).Padding(3).AlignCenter()
                                .Text("Sin notas").FontColor(Colors.Grey.Darken2);
                        }
                    }
                });

                page.Footer().AlignRight()
                    .Text(t =>
                    {
                        t.Span("Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.Span(" / ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        t.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
            });
        });

        return pdf.GeneratePdf();
    }
}
