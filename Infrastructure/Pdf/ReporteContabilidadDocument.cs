using ApiConcilacionFr.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace ApiConcilacionFr.Infrastructure.Pdf;

public sealed class ReporteContabilidadDocument : IDocument
{
    private static readonly CultureInfo CulturaMoneda = CultureInfo.GetCultureInfo("es-MX");
    private readonly int _mes;
    private readonly string _etapa;
    private readonly IReadOnlyList<ReporteContabilidad> _reportes;

    public ReporteContabilidadDocument(
        int mes,
        string etapa,
        IReadOnlyList<ReporteContabilidad> reportes)
    {
        _mes = mes;
        _etapa = etapa;
        _reportes = reportes;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(24);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(style => style.FontSize(8).FontFamily(Fonts.Arial));
            page.Header().Column(column =>
            {
                column.Item().Text("Reporte de saldos contables")
                    .FontSize(18)
                    .SemiBold()
                    .FontColor(Colors.Blue.Darken3);
                column.Item().PaddingTop(4).Text($"Mes: MES{_mes}   |   Etapa: {_etapa}")
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken2);
            });
            page.Content().PaddingTop(12).Column(ComposeStages);
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }

    private void ComposeStages(ColumnDescriptor column)
    {
        foreach (var etapa in _reportes.GroupBy(reporte => reporte.ETAPA).OrderBy(grupo => grupo.Key))
        {
            column.Item().PaddingTop(10)
                .Background(Colors.Blue.Lighten4)
                .Padding(6)
                .Text($"ETAPA {etapa.Key}")
                .FontSize(12)
                .Bold()
                .FontColor(Colors.Blue.Darken3);

            foreach (var grupo in etapa
                         .GroupBy(reporte => string.IsNullOrWhiteSpace(reporte.GRUPO)
                             ? "SIN GRUPO"
                             : reporte.GRUPO)
                         .OrderBy(grupo => grupo.Key))
            {
                column.Item().PaddingTop(6).Text($"Grupo: {grupo.Key}")
                    .FontSize(10)
                    .SemiBold();

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2.2f);
                        columns.RelativeColumn(3.2f);
                        columns.RelativeColumn(0.9f);
                        columns.RelativeColumn(1.3f);
                        columns.RelativeColumn(1.3f);
                        columns.RelativeColumn(1.3f);
                    });

                    table.Header(header =>
                    {
                        IContainer HeaderStyle(IContainer cell) =>
                            cell.Background(Colors.Blue.Darken3)
                                .PaddingVertical(5)
                                .PaddingHorizontal(3);

                        header.Cell().Element(HeaderStyle).Text("CUENTA").FontColor(Colors.White).SemiBold();
                        header.Cell().Element(HeaderStyle).Text("NOMBRE").FontColor(Colors.White).SemiBold();
                        header.Cell().Element(HeaderStyle).AlignRight()
                            .Text("S. CLAVE").FontColor(Colors.White).SemiBold();
                        header.Cell().Element(HeaderStyle).AlignRight()
                            .Text("SALDO CONTABLE").FontColor(Colors.White).SemiBold();
                        header.Cell().Element(HeaderStyle).AlignRight()
                            .Text("SALDO PROSIGO").FontColor(Colors.White).SemiBold();
                        header.Cell().Element(HeaderStyle).AlignRight()
                            .Text("DIFERENCIA").FontColor(Colors.White).SemiBold();
                    });

                    var index = 0;
                    foreach (var reporte in grupo.OrderBy(reporte => reporte.CUENTA))
                    {
                        var background = index++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                        IContainer CellStyle(IContainer cell) =>
                            cell.Background(background).PaddingVertical(4).PaddingHorizontal(3);

                        table.Cell().Element(CellStyle).Text(reporte.CUENTA ?? "-");
                        table.Cell().Element(CellStyle).Text(reporte.C_DESCRIPCION ?? "-");
                        table.Cell().Element(CellStyle).AlignRight()
                            .Text(reporte.S_CLAVE > 0 ? reporte.S_CLAVE.ToString(CultureInfo.InvariantCulture) : "-");
                        table.Cell().Element(CellStyle).AlignRight()
                            .Text(reporte.SALDO.ToString("N2", CulturaMoneda));
                        table.Cell().Element(CellStyle).AlignRight()
                            .Text(reporte.SALDO_INSOLUTO.ToString("N2", CulturaMoneda));
                        table.Cell().Element(CellStyle).AlignRight()
                            .Text(reporte.DIFERENCIA.ToString("N2", CulturaMoneda));
                    }
                });
            }
        }
    }
}
