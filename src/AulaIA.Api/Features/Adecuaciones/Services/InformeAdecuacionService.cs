using AulaIA.Api.Shared.Domain;
using AulaIA.Api.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AulaIA.Api.Features.Adecuaciones.Services;

/// <summary>
/// Genera el informe de adecuación curricular en PDF para el expediente del CAE.
/// Incluye: datos del estudiante, tipo de adecuación, diagnóstico, estrategias y propuesta pedagógica.
/// </summary>
public sealed class InformeAdecuacionService(AulaIADbContext db)
{
    public async Task<byte[]> GeneratePdfAsync(Guid accommodationId, CancellationToken ct)
    {
        var acc = await db.Accommodations
            .Include(a => a.Student)
            .Include(a => a.Group)
                .ThenInclude(g => g!.Institution)
            .FirstOrDefaultAsync(a => a.Id == accommodationId, ct)
            ?? throw new KeyNotFoundException($"Adecuación {accommodationId} no encontrada.");

        var student  = acc.Student!;
        var group    = acc.Group!;
        var inst     = group.Institution?.Name ?? "—";
        var tipoNombre = acc.Type switch
        {
            AccommodationType.AS  => "Adecuación Significativa (AS)",
            AccommodationType.ANS => "Adecuación No Significativa (ANS)",
            AccommodationType.AA  => "Apoyo Académico (AA)",
            _                     => acc.Type.ToString()
        };

        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("MINISTERIO DE EDUCACIÓN PÚBLICA")
                        .FontSize(13).Bold();
                    col.Item().AlignCenter().Text("INFORME DE ADECUACIÓN CURRICULAR")
                        .FontSize(12).Bold();
                    col.Item().AlignCenter().Text("Expediente para el Comité de Apoyo Educativo (CAE)")
                        .FontSize(9).Italic();
                    col.Item().PaddingTop(4).LineHorizontal(1);
                });

                page.Content().PaddingTop(12).Column(col =>
                {
                    // ── Datos generales ────────────────────────────────────
                    col.Item().Text("1. DATOS GENERALES").Bold().FontSize(11);
                    col.Item().PaddingTop(4).Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(3); });
                        void Cell(string label, string value, int span = 1)
                        {
                            t.Cell().Element(e => e.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4))
                                    .Text(label).Bold().FontSize(9);
                            t.Cell().ColumnSpan((uint)span).Element(e => e.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4))
                                    .Text(value).FontSize(9);
                        }
                        Cell("Institución:", inst, 3);
                        Cell("Estudiante:", student.FullName);
                        Cell("Expediente:", student.StudentCode);
                        Cell("Asignatura:", group.Subject);
                        Cell("Nivel:", group.Level);
                        Cell("Tipo Adecuación:", tipoNombre, 3);
                        Cell("Diagnóstico:", acc.Diagnostico, 3);
                        if (!string.IsNullOrWhiteSpace(acc.CondicionEspecial))
                            Cell("Condición especial:", acc.CondicionEspecial, 3);
                        Cell("Fecha del informe:", DateTime.Now.ToString("dd/MM/yyyy"), 3);
                    });

                    col.Item().PaddingTop(12);

                    // ── Estrategias de mediación ───────────────────────────
                    if (!string.IsNullOrWhiteSpace(acc.EstrategiasMediacion))
                    {
                        col.Item().Text("2. ESTRATEGIAS DE MEDIACIÓN").Bold().FontSize(11);
                        col.Item().PaddingTop(4).Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(6).Text(acc.EstrategiasMediacion).FontSize(9);
                        col.Item().PaddingTop(8);
                    }

                    // ── Estrategias de evaluación ──────────────────────────
                    if (!string.IsNullOrWhiteSpace(acc.EstrategiasEvaluacion))
                    {
                        col.Item().Text("3. ESTRATEGIAS DE EVALUACIÓN").Bold().FontSize(11);
                        col.Item().PaddingTop(4).Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(6).Text(acc.EstrategiasEvaluacion).FontSize(9);
                        col.Item().PaddingTop(8);
                    }

                    // ── Propuesta pedagógica IA ────────────────────────────
                    if (!string.IsNullOrWhiteSpace(acc.PropuestaGenerada))
                    {
                        col.Item().Text("4. PROPUESTA PEDAGÓGICA INDIVIDUALIZADA").Bold().FontSize(11);
                        col.Item().PaddingTop(4).Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(8).Text(acc.PropuestaGenerada).FontSize(9);
                        col.Item().PaddingTop(8);
                    }

                    // ── Observaciones ──────────────────────────────────────
                    if (!string.IsNullOrWhiteSpace(acc.Observaciones))
                    {
                        col.Item().Text("5. OBSERVACIONES DEL DOCENTE").Bold().FontSize(11);
                        col.Item().PaddingTop(4).Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(6).Text(acc.Observaciones).FontSize(9);
                        col.Item().PaddingTop(12);
                    }

                    // ── Nota legal AS ──────────────────────────────────────
                    if (acc.Type == AccommodationType.AS)
                    {
                        col.Item().Border(1).BorderColor(Colors.Orange.Medium)
                            .Background(Colors.Orange.Lighten5).Padding(6)
                            .Text("NOTA: Esta Adecuación Significativa requiere registro formal en el SIMED del MEP. " +
                                  "AulaIA genera este documento de soporte; el registro en SIMED es responsabilidad del docente.")
                            .FontSize(8).Italic();
                        col.Item().PaddingTop(12);
                    }

                    // ── Firmas ─────────────────────────────────────────────
                    col.Item().PaddingTop(20).Row(row =>
                    {
                        void FirmaBox(string label) =>
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                                c.Item().AlignCenter().Text(label).FontSize(8).Italic();
                            });

                        FirmaBox("Docente responsable");
                        row.ConstantItem(20);
                        FirmaBox("Director(a) del centro");
                        row.ConstantItem(20);
                        FirmaBox("Orientador(a) / Psicólogo(a)");
                    });
                });

                page.Footer().AlignCenter()
                    .Text(t =>
                    {
                        t.Span("Generado por AulaIA · ").FontSize(8).Italic();
                        t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).Italic();
                        t.Span(" · Página ").FontSize(8).Italic();
                        t.CurrentPageNumber().FontSize(8);
                        t.Span(" de ").FontSize(8);
                        t.TotalPages().FontSize(8);
                    });
            });
        });

        return doc.GeneratePdf();
    }
}
