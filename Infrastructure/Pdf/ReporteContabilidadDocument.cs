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
    private readonly bool _indice;

    public ReporteContabilidadDocument(
        int mes,
        string etapa,
        IReadOnlyList<ReporteContabilidad> reportes,
        bool indice)
    {
        _mes = mes;
        _etapa = etapa;
        _reportes = reportes;
        _indice = indice;
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
            page.Content().PaddingTop(12).Column(ComposeGroups);
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        });
    }

    private void ComposeGroups(ColumnDescriptor column)
    {
        var grupos = _reportes
            .GroupBy(reporte => reporte.S_GRUPO)
            .OrderBy(grupo => NombreGrupo(grupo.First()), StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(grupo => grupo.Key)
            .ToList();

        if (_indice)
            ComposeIndex(column, grupos);

        var primerGrupo = true;
        foreach (var grupo in grupos)
        {
            if (_indice || !primerGrupo)
                column.Item().PageBreak();

            var sectionName = SectionName(grupo.Key);
            column.Item().Section(sectionName).Column(groupColumn =>
            {
                groupColumn.Item().PaddingTop(10)
                    .Background(Colors.Blue.Lighten4)
                    .Padding(6)
                    .Text($"Grupo: {NombreGrupo(grupo.First())} (S_GRUPO {grupo.Key})")
                    .FontSize(10)
                    .SemiBold();

                foreach (var etapa in grupo.GroupBy(reporte => reporte.ETAPA).OrderBy(etapa => etapa.Key))
                {
                    groupColumn.Item().PaddingTop(8)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(4)
                        .Text($"ETAPA {etapa.Key}")
                        .FontSize(9)
                        .Bold();

                    ComposeTable(groupColumn, etapa);
                }
            });

            primerGrupo = false;
        }
    }

    private void ComposeIndex(
        ColumnDescriptor column,
        IReadOnlyList<IGrouping<int, ReporteContabilidad>> grupos)
    {
        column.Item().Text("Índice de grupos")
            .FontSize(16)
            .SemiBold()
            .FontColor(Colors.Blue.Darken3);

        foreach (var grupo in grupos)
        {
            var sectionName = SectionName(grupo.Key);
            column.Item().PaddingVertical(4).SectionLink(sectionName).Row(row =>
            {
                row.RelativeItem()
                    .Text($"Grupo: {NombreGrupo(grupo.First())} (S_GRUPO {grupo.Key})")
                    .FontSize(10);
                row.ConstantItem(40).AlignRight().Text(text =>
                {
                    text.BeginPageNumberOfSection(sectionName);
                });
            });
        }
    }

    private static void ComposeTable(
        ColumnDescriptor column,
        IEnumerable<ReporteContabilidad> reportes)
    {
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
                    .Text("SALDO CAPITAL CONTABLE").FontColor(Colors.White).SemiBold();
                header.Cell().Element(HeaderStyle).AlignRight()
                    .Text("SALDO CAPITAL PROSIGO").FontColor(Colors.White).SemiBold();
                header.Cell().Element(HeaderStyle).AlignRight()
                    .Text("DIFERENCIA").FontColor(Colors.White).SemiBold();
                header.Cell().Element(HeaderStyle).AlignRight()
                    .Text("PRESTAMOS").FontColor(Colors.White).SemiBold();
            });

            var index = 0;
            foreach (var reporte in reportes.OrderBy(reporte => reporte.CUENTA))
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
                table.Cell().Element(CellStyle).AlignRight()
                    .Text(reporte.PRESTAMOS ?? "-");
            }
        });
    }

    private static string NombreGrupo(ReporteContabilidad reporte) =>
        string.IsNullOrWhiteSpace(reporte.GRUPO) ? "SIN GRUPO" : reporte.GRUPO;

    private static string SectionName(int sGrupo) =>
        $"grupo-{sGrupo.ToString(CultureInfo.InvariantCulture)}";
}
