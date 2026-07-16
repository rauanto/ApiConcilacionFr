using ApiConcilacionFr.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiConcilacionFr.Infrastructure.Pdf;

public class AmortizacionSimpleDocument : IDocument
{
    private readonly int _pqClave;
    private readonly DatosSocio? _socio;
    private readonly List<Amortizacion> _amortizaciones;

    public AmortizacionSimpleDocument(
        int pqClave,
        DatosSocio? socio,
        List<Amortizacion> amortizaciones)
    {
        _pqClave = pqClave;
        _socio = socio;
        _amortizaciones = amortizaciones;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(20);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial)); // Fuente base ligeramente más pequeña y limpia

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    private void ComposeHeader(IContainer container)
    {
        PdfHeaderHelper.ComposeSocioHeader(container, _pqClave, _socio, "Reporte de Amortizaciones (Resumen)");
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            if (!_amortizaciones.Any())
            {
                column.Item().Text("No se encontraron amortizaciones.")
                      .FontSize(12)
                      .FontColor(Colors.Grey.Medium)
                      .Italic();
                return;
            }

            // 1. Agregar Tarjetas de Indicadores (KPIs)
            column.Item().Element(ComposeSummaryIndicators);
            
            // Espaciado entre indicadores y la tabla
            column.Item().PaddingBottom(15); 

            // 2. Tabla Compacta
            column.Item().Element(ComposeCompactTable);
        });
    }

    private enum EstadoAmortizacion { Pagada, Vencida, Vigente, Otro }

    private EstadoAmortizacion ObtenerCategoriaEstado(string? estado)
    {
        var e = (estado ?? "").Trim().ToLowerInvariant();
        
        // Crítico: Atrapar "no pagada" primero, de lo contrario e.Contains("pagad") da un falso positivo.
        if (e.Contains("no pagad")) return EstadoAmortizacion.Vigente;

        if (e.Contains("pagad") || e.Contains("liquidado")) return EstadoAmortizacion.Pagada;
        if (e.Contains("vencid")) return EstadoAmortizacion.Vencida;
        if (e.Contains("vigente") || e.Contains("corriente") || e.Contains("activo") || e.Contains("normal") || e.Contains("por vencer") || e.Contains("pendiente")) 
            return EstadoAmortizacion.Vigente;
        
        return EstadoAmortizacion.Otro;
    }

    private void ComposeSummaryIndicators(IContainer container)
    {
        var pagadas = _amortizaciones.Count(a => ObtenerCategoriaEstado(a.Estado) == EstadoAmortizacion.Pagada);
        var vencidas = _amortizaciones.Count(a => ObtenerCategoriaEstado(a.Estado) == EstadoAmortizacion.Vencida);
        var vigentes = _amortizaciones.Count(a => ObtenerCategoriaEstado(a.Estado) == EstadoAmortizacion.Vigente);

        container.Row(row =>
        {
            row.RelativeItem().Element(c => ComposeIndicatorCard(c, "PAGADAS", pagadas.ToString(), Colors.Green.Medium));
            row.Spacing(10);
            row.RelativeItem().Element(c => ComposeIndicatorCard(c, "VENCIDAS", vencidas.ToString(), Colors.Red.Medium));
            row.Spacing(10);
            row.RelativeItem().Element(c => ComposeIndicatorCard(c, "VIGENTES", vigentes.ToString(), Colors.Orange.Medium));
            row.Spacing(10);
            row.RelativeItem().Element(c => ComposeIndicatorCard(c, "TOTAL", _amortizaciones.Count.ToString(), Colors.Blue.Medium));
        });
    }

    private void ComposeIndicatorCard(IContainer container, string title, string value, string color)
    {
        // Estilo moderno: Borde lateral izquierdo coloreado para indicar la métrica
        container
            .Background(Colors.Grey.Lighten4)
            .BorderLeft(3).BorderColor(color)
            .Padding(8)
            .Column(column =>
            {
                column.Item().Text(title).FontSize(7).FontColor(Colors.Grey.Darken2).SemiBold();
                column.Item().Text(value).FontSize(12).FontColor(Colors.Black).Bold();
            });
    }

    private void ComposeCompactTable(IContainer container)
    {
        container.Table(table =>
        {
            // Definición de columnas optimizada
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(25); // NO. (Más estrecho)
                columns.RelativeColumn();   // VENCIMIENTO
                columns.RelativeColumn();   // CAPITAL
                columns.RelativeColumn();   // INTERÉS
                columns.RelativeColumn();   // IVA
                columns.RelativeColumn();   // TOTAL
                columns.RelativeColumn();   // SALDO INSOLUTO
                columns.RelativeColumn();   // TOTAL
                columns.RelativeColumn();   // SALDO INSOLUTO

                columns.ConstantColumn(55); // ESTADO (Ajustado a la píldora)
            });

            // Encabezados con alineación financiera correcta
            table.Header(header =>
            {
                // Función local para estilizar rápidamente celdas del header
                IContainer HeaderStyle(IContainer c) =>
                    c.BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingVertical(4).PaddingHorizontal(2);

                header.Cell().Element(HeaderStyle).AlignCenter().Text("NO.").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
                header.Cell().Element(HeaderStyle).AlignLeft().Text("VENCIMIENTO").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
                header.Cell().Element(HeaderStyle).AlignRight().Text("CAPITAL").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
                header.Cell().Element(HeaderStyle).AlignRight().Text("INTERÉS").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
                header.Cell().Element(HeaderStyle).AlignRight().Text("IVA").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
                header.Cell().Element(HeaderStyle).AlignRight().Text("TOTAL").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
                header.Cell().Element(HeaderStyle).AlignRight().Text("SALDO INSOLUTO").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
                header.Cell().Element(HeaderStyle).AlignRight().Text("ABONO").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
                header.Cell().Element(HeaderStyle).AlignRight().Text("SAL. PEND.").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
                header.Cell().Element(HeaderStyle).AlignCenter().Text("ESTADO").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
            });

            // Filas con Zebra Striping para hacerlas compactas sin que se pierda la lectura
            var rowIndex = 0;
            foreach (var amortizacion in _amortizaciones)
            {
                var isEven = rowIndex % 2 == 0;
                var backgroundColor = isEven ? Colors.White : Colors.Grey.Lighten4;

                IContainer CellStyle(IContainer c) =>
                    c.Background(backgroundColor).PaddingVertical(3).PaddingHorizontal(2);

                table.Cell().Element(CellStyle).AlignCenter().Text(amortizacion.A_NUMERO.ToString()).FontSize(8);
                table.Cell().Element(CellStyle).AlignLeft().Text(amortizacion.FECHA_VENCIMIENTO ?? "-").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text(amortizacion.CAPITAL ?? "-").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text(amortizacion.INTERES ?? "-").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text(amortizacion.IVA ?? "-").FontSize(8);
                table.Cell().Element(CellStyle).AlignRight().Text(amortizacion.TOTAL ?? "-").FontSize(8).SemiBold(); // Remarcamos el total a pagar
                table.Cell().Element(CellStyle).AlignRight().Text(amortizacion.SALDO_INSOLUTO ?? "-").FontSize(8).FontColor(Colors.Grey.Darken2);
                table.Cell().Element(CellStyle).AlignRight().Text(amortizacion.ABONO ?? "-").FontSize(8).SemiBold(); // Remarcamos el total a pagar
                table.Cell().Element(CellStyle).AlignRight().Text(amortizacion.SALDO_PENDIENTE ?? "-").FontSize(8).FontColor(Colors.Grey.Darken2);
                // Estado Pill centrado
                table.Cell().Element(CellStyle).AlignCenter().Element(c => ComposeEstadoPill(c, amortizacion.Estado));

                rowIndex++;
            }
        });
    }

    private void ComposeEstadoPill(IContainer container, string? estado)
    {
        var categoria = ObtenerCategoriaEstado(estado);
        
        // Estilo Moderno UI: Fondos muy claros con textos oscuros saturados
        var bgColor = Colors.Grey.Lighten3;
        var txtColor = Colors.Grey.Darken3;

        if (categoria == EstadoAmortizacion.Pagada)
        {
            bgColor = Colors.Green.Lighten4;
            txtColor = Colors.Green.Darken3;
        }
        else if (categoria == EstadoAmortizacion.Vencida)
        {
            bgColor = Colors.Red.Lighten4;
            txtColor = Colors.Red.Darken3;
        }
        else if (categoria == EstadoAmortizacion.Vigente)
        {
            // Usamos Amber que es un amarillo/naranja más vibrante para "Vigente"
            bgColor = Colors.Amber.Lighten4;
            txtColor = Colors.Amber.Darken4;
        }

        container
            .Background(bgColor)
            .PaddingVertical(1)
            .PaddingHorizontal(4)
            .AlignCenter()
            .Text((estado ?? "-").ToUpper()) // Mayúsculas para los badges se ven más ordenadas
            .FontSize(7)
            .FontColor(txtColor)
            .Bold(); // Texto en bold para máxima legibilidad en un fondo claro
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Página ").FontSize(8).FontColor(Colors.Grey.Medium);
            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
            x.Span(" de ").FontSize(8).FontColor(Colors.Grey.Medium);
            x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }
}