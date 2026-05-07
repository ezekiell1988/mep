using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AulaIA.Api.Features.Reportes;

/// <summary>
/// Genera el informe consolidado de grupo para la dirección del centro educativo.
/// Incluye: datos del grupo, tabla de notas y promedio por alumno, resumen de asistencia
/// del período solicitado, adecuaciones curriculares activas y estadísticas generales.
/// </summary>
public sealed class InformeDirectorService(AulaIADbContext db)
{
    private sealed record InformeData(
        Group Grupo,
        Institution? Institucion,
        List<Student> Estudiantes,
        List<EvaluationActivity> Actividades,
        List<Grade> Calificaciones,
        List<AttendanceRecord> Asistencias,
        List<Accommodation> Adecuaciones,
        DateOnly From,
        DateOnly To);

    private async Task<InformeData> LoadAsync(Guid groupId, DateOnly from, DateOnly to, CancellationToken ct)
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

        var asistencias = await db.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.GroupId == groupId && r.Date >= from && r.Date <= to)
            .ToListAsync(ct);

        var studentIds = estudiantes.Select(s => s.Id).ToList();
        var adecuaciones = studentIds.Count > 0
            ? await db.Accommodations.AsNoTracking()
                .Where(a => a.GroupId == groupId)
                .ToListAsync(ct)
            : [];

        return new InformeData(grupo, grupo.Institution, estudiantes, actividades,
            calificaciones, asistencias, adecuaciones, from, to);
    }

    private static decimal? Promedio(Guid studentId, List<EvaluationActivity> acts, List<Grade> grades)
    {
        var ponderadas = acts
            .Where(a => a.Percentage > 0)
            .Select(a =>
            {
                var g = grades.FirstOrDefault(x => x.ActivityId == a.Id && x.StudentId == studentId);
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

    private static int Umbral(Group grupo) =>
        int.TryParse(grupo.Level.Replace("°", "").Trim(), out var n) && n >= 10 ? 70 : 65;

    private static (int P, int A, int T, int J) CountsFor(
        Guid studentId, List<AttendanceRecord> registros)
    {
        int p = 0, a = 0, t = 0, j = 0;
        foreach (var r in registros.Where(r => r.StudentId == studentId))
        {
            switch (r.Status)
            {
                case AttendanceStatus.Present:   p++; break;
                case AttendanceStatus.Absent:    a++; break;
                case AttendanceStatus.Late:      t++; break;
                case AttendanceStatus.Justified: j++; break;
            }
        }
        return (p, a, t, j);
    }

    public async Task<byte[]> GeneratePdfAsync(Guid groupId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var d = await LoadAsync(groupId, from, to, ct);
        QuestPDF.Settings.License = LicenseType.Community;

        int umbral = Umbral(d.Grupo);
        int aprobados = 0, reprobados = 0, sinNotas = 0;

        // Pre-calcular datos por alumno para las estadísticas del encabezado
        var porAlumno = d.Estudiantes.Select(est =>
        {
            var prom = Promedio(est.Id, d.Actividades, d.Calificaciones);
            var (p, a, t, j) = CountsFor(est.Id, d.Asistencias);
            int total = p + a + t + j;
            double pctAsist = total > 0 ? Math.Round((double)(p + t + j) / total * 100, 1) : 0;
            var adec = d.Adecuaciones.FirstOrDefault(x => x.StudentId == est.Id);
            return (est, prom, p, a, t, j, total, pctAsist, adec);
        }).ToList();

        foreach (var (_, prom, _, _, _, _, _, _, _) in porAlumno)
        {
            if (prom is null) sinNotas++;
            else if (prom >= umbral) aprobados++;
            else reprobados++;
        }

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(9));

                // ══════════════════ HEADER ══════════════════
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item().Text("MINISTERIO DE EDUCACIÓN PÚBLICA")
                                .FontSize(10).Bold().FontColor(Colors.Blue.Darken4);
                            inner.Item().Text("INFORME DOCENTE PARA DIRECCIÓN")
                                .FontSize(13).Bold();
                            inner.Item().PaddingTop(2).Text(
                                $"{d.Grupo.Subject}  —  {d.Grupo.Level}  —  {d.Grupo.Name}")
                                .FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                        row.ConstantItem(180).Column(inner =>
                        {
                            inner.Item().Text($"Institución: {d.Institucion?.Name ?? "—"}").FontSize(8);
                            inner.Item().Text($"Año lectivo: {d.Grupo.SchoolYear}").FontSize(8);
                            inner.Item().Text($"Período: {from:dd/MM/yyyy} — {to:dd/MM/yyyy}").FontSize(8);
                            inner.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });

                    // Estadísticas rápidas en cápsulas
                    col.Item().PaddingTop(8).Row(row =>
                    {
                        void Stat(RowDescriptor r, string label, string value, string bg, string fg)
                        {
                            r.AutoItem().Padding(3).Background(bg)
                                .Padding(6).Column(c =>
                                {
                                    c.Item().Text(value).FontSize(16).Bold().FontColor(fg);
                                    c.Item().Text(label).FontSize(7).FontColor(fg);
                                });
                        }

                        Stat(row, "Estudiantes", d.Estudiantes.Count.ToString(), Colors.Blue.Lighten4, Colors.Blue.Darken4);
                        row.AutoItem().Width(6);
                        Stat(row, "Aprobados", aprobados.ToString(), Colors.Green.Lighten4, Colors.Green.Darken3);
                        row.AutoItem().Width(6);
                        Stat(row, "Reprobados", reprobados.ToString(), Colors.Red.Lighten4, Colors.Red.Darken3);
                        row.AutoItem().Width(6);
                        Stat(row, "Sin notas", sinNotas.ToString(), Colors.Grey.Lighten3, Colors.Grey.Darken2);
                        row.AutoItem().Width(6);
                        var pctApro = d.Estudiantes.Count > 0
                            ? $"{Math.Round((double)aprobados / d.Estudiantes.Count * 100, 1)}%"
                            : "—";
                        Stat(row, "% Aprobación", pctApro, Colors.Teal.Lighten4, Colors.Teal.Darken3);
                        row.AutoItem().Width(6);
                        Stat(row, "Adecuaciones", d.Adecuaciones.Count.ToString(), Colors.Orange.Lighten4, Colors.Orange.Darken3);
                    });

                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Blue.Darken4);
                });

                // ══════════════════ CONTENIDO ══════════════════
                page.Content().PaddingTop(10).Column(content =>
                {
                    // ── Sección 1: Tabla consolidada por alumno ──
                    content.Item().Text("1. Rendimiento y Asistencia por Estudiante")
                        .FontSize(10).Bold().FontColor(Colors.Blue.Darken4);
                    content.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3f);  // Nombre
                            cols.ConstantColumn(55);  // Expediente
                            cols.ConstantColumn(45);  // Promedio
                            cols.ConstantColumn(45);  // Estado
                            cols.ConstantColumn(22);  // P
                            cols.ConstantColumn(22);  // A
                            cols.ConstantColumn(22);  // T
                            cols.ConstantColumn(22);  // J
                            cols.ConstantColumn(40);  // % Asist
                            cols.ConstantColumn(55);  // Adecuación
                        });

                        static IContainer TH(IContainer c) =>
                            c.Background(Colors.Blue.Darken4).Padding(4).AlignCenter().AlignMiddle();

                        table.Header(h =>
                        {
                            h.Cell().Element(TH).Text("Nombre").FontColor(Colors.White).Bold().FontSize(8);
                            h.Cell().Element(TH).Text("Expediente").FontColor(Colors.White).Bold().FontSize(8);
                            h.Cell().Element(TH).Text("Promedio").FontColor(Colors.White).Bold().FontSize(8);
                            h.Cell().Element(TH).Text("Estado").FontColor(Colors.White).Bold().FontSize(8);
                            h.Cell().Element(TH).Text("P").FontColor(Colors.White).Bold().FontSize(8);
                            h.Cell().Element(TH).Text("A").FontColor(Colors.White).Bold().FontSize(8);
                            h.Cell().Element(TH).Text("T").FontColor(Colors.White).Bold().FontSize(8);
                            h.Cell().Element(TH).Text("J").FontColor(Colors.White).Bold().FontSize(8);
                            h.Cell().Element(TH).Text("% Asist.").FontColor(Colors.White).Bold().FontSize(8);
                            h.Cell().Element(TH).Text("Adecuación").FontColor(Colors.White).Bold().FontSize(8);
                        });

                        bool shade = false;
                        foreach (var (est, prom, p, a, t, j, total, pctAsist, adec) in porAlumno)
                        {
                            var rowBg = shade ? Colors.Grey.Lighten4 : Colors.White;
                            shade = !shade;

                            IContainer TD(IContainer c) =>
                                c.Background(rowBg).Padding(4).AlignMiddle();

                            bool aprobado = prom.HasValue && prom.Value >= umbral;
                            var promBg = prom is null ? rowBg
                                : aprobado ? Colors.Green.Lighten4 : Colors.Red.Lighten4;
                            var pctBg = total == 0 ? rowBg
                                : pctAsist >= 85 ? Colors.Green.Lighten4
                                : pctAsist >= 70 ? Colors.Yellow.Lighten3
                                : Colors.Red.Lighten4;

                            table.Cell().Element(TD).Text(est.FullName).FontSize(8);
                            table.Cell().Element(TD).AlignCenter().Text(est.StudentCode).FontSize(8);

                            // Promedio con color
                            table.Cell().Background(promBg).Padding(4).AlignCenter().AlignMiddle()
                                .Text(prom.HasValue ? prom.Value.ToString("0.0") : "—")
                                .Bold().FontSize(8)
                                .FontColor(prom is null ? Colors.Grey.Medium
                                    : aprobado ? Colors.Green.Darken3 : Colors.Red.Darken3);

                            table.Cell().Background(promBg).Padding(4).AlignCenter().AlignMiddle()
                                .Text(prom is null ? "Sin notas" : aprobado ? "Aprobado" : "Reprobado")
                                .FontSize(7.5f)
                                .FontColor(prom is null ? Colors.Grey.Medium
                                    : aprobado ? Colors.Green.Darken3 : Colors.Red.Darken3);

                            table.Cell().Element(TD).AlignCenter().Text(p.ToString()).FontSize(8).FontColor(Colors.Green.Darken2);
                            table.Cell().Element(TD).AlignCenter().Text(a.ToString()).FontSize(8).FontColor(Colors.Red.Darken2);
                            table.Cell().Element(TD).AlignCenter().Text(t.ToString()).FontSize(8).FontColor(Colors.Orange.Darken2);
                            table.Cell().Element(TD).AlignCenter().Text(j.ToString()).FontSize(8).FontColor(Colors.Blue.Darken2);

                            table.Cell().Background(pctBg).Padding(4).AlignCenter().AlignMiddle()
                                .Text(total > 0 ? $"{pctAsist}%" : "—").Bold().FontSize(8);

                            var adecText = adec is null ? "—" : adec.Type switch
                            {
                                AccommodationType.AS  => "AS",
                                AccommodationType.ANS => "ANS",
                                AccommodationType.AA  => "Acc. Acceso",
                                _ => adec.Type.ToString(),
                            };
                            var adecBg = adec is null ? rowBg : Colors.Orange.Lighten4;
                            table.Cell().Background(adecBg).Padding(4).AlignCenter().AlignMiddle()
                                .Text(adecText).FontSize(7.5f)
                                .FontColor(adec is null ? Colors.Grey.Lighten2 : Colors.Orange.Darken3);
                        }
                    });

                    content.Item().PaddingTop(16);

                    // ── Sección 2: Adecuaciones curriculares activas ──
                    if (d.Adecuaciones.Count > 0)
                    {
                        content.Item().Text("2. Adecuaciones Curriculares Activas")
                            .FontSize(10).Bold().FontColor(Colors.Blue.Darken4);
                        content.Item().PaddingTop(4).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2.5f); // Nombre
                                cols.ConstantColumn(40);   // Tipo
                                cols.RelativeColumn(3f);   // Diagnóstico
                                cols.RelativeColumn(4f);   // Estrategias mediación
                            });

                            static IContainer TH2(IContainer c) =>
                                c.Background(Colors.Orange.Darken2).Padding(4).AlignCenter().AlignMiddle();

                            table.Header(h =>
                            {
                                h.Cell().Element(TH2).Text("Estudiante").FontColor(Colors.White).Bold().FontSize(8);
                                h.Cell().Element(TH2).Text("Tipo").FontColor(Colors.White).Bold().FontSize(8);
                                h.Cell().Element(TH2).Text("Diagnóstico").FontColor(Colors.White).Bold().FontSize(8);
                                h.Cell().Element(TH2).Text("Estrategias de mediación").FontColor(Colors.White).Bold().FontSize(8);
                            });

                            bool shade2 = false;
                            foreach (var adec in d.Adecuaciones)
                            {
                                var est = d.Estudiantes.FirstOrDefault(e => e.Id == adec.StudentId);
                                var bg = shade2 ? Colors.Orange.Lighten5 : Colors.White;
                                shade2 = !shade2;

                                IContainer TD2(IContainer c) => c.Background(bg).Padding(4).AlignMiddle();

                                table.Cell().Element(TD2).Text(est?.FullName ?? "—").FontSize(8);
                                table.Cell().Element(TD2).AlignCenter().Text(adec.Type.ToString()).Bold().FontSize(8)
                                    .FontColor(Colors.Orange.Darken3);
                                table.Cell().Element(TD2).Text(adec.Diagnostico).FontSize(7.5f);
                                table.Cell().Element(TD2).Text(
                                    string.IsNullOrWhiteSpace(adec.EstrategiasMediacion)
                                        ? "—"
                                        : adec.EstrategiasMediacion.Length > 120
                                            ? adec.EstrategiasMediacion[..120] + "…"
                                            : adec.EstrategiasMediacion)
                                    .FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                            }
                        });

                        content.Item().PaddingTop(6).Text(
                            "Nota legal: Las adecuaciones significativas (AS) requieren registro formal en el SIMED del MEP (Ley 7600).")
                            .FontSize(7).FontColor(Colors.Orange.Darken3).Italic();

                        content.Item().PaddingTop(14);
                    }

                    // ── Sección 3: Firmas ──
                    content.Item().PaddingTop(20).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.ConstantColumn(40);
                            cols.RelativeColumn();
                            cols.ConstantColumn(40);
                            cols.RelativeColumn();
                        });

                        // Líneas de firma
                        table.Cell().PaddingTop(30).LineHorizontal(0.8f);
                        table.Cell();
                        table.Cell().PaddingTop(30).LineHorizontal(0.8f);
                        table.Cell();
                        table.Cell().PaddingTop(30).LineHorizontal(0.8f);

                        // Labels
                        table.Cell().PaddingTop(4).AlignCenter()
                            .Text("Firma del Docente").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                        table.Cell();
                        table.Cell().PaddingTop(4).AlignCenter()
                            .Text("Firma del Director / Directora").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                        table.Cell();
                        table.Cell().PaddingTop(4).AlignCenter()
                            .Text("Sello Institucional").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                    });
                });

                // ══════════════════ FOOTER ══════════════════
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("AulaIA  ·  Documento generado automáticamente  ·  Página ").FontColor(Colors.Grey.Medium).FontSize(7);
                    x.CurrentPageNumber().FontColor(Colors.Grey.Medium).FontSize(7);
                    x.Span(" de ").FontColor(Colors.Grey.Medium).FontSize(7);
                    x.TotalPages().FontColor(Colors.Grey.Medium).FontSize(7);
                });
            });
        });

        return doc.GeneratePdf();
    }
}
