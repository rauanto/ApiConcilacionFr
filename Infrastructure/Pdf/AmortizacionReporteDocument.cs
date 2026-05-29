using System.Net.Http;
using System.IO;
using ApiConcilacionFr.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiConcilacionFr.Infrastructure.Pdf;

public class AmortizacionReporteDocument : IDocument
{
    private readonly int _pqClave;
    private readonly DatosSocio? _socio;
    private readonly List<Amortizacion> _amortizaciones;
    private readonly Dictionary<int, List<Bitacora>> _bitacorasPorAmortizacion;
    private readonly Dictionary<int, List<BitacoraArchivo>> _evidenciasPorBitacora;
    private readonly HttpClient _httpClient;
    private readonly string _contentRootPath;
    private readonly bool _incluirBitacoras;

    public AmortizacionReporteDocument(
        int pqClave,
        DatosSocio? socio,
        List<Amortizacion> amortizaciones,
        Dictionary<int, List<Bitacora>> bitacorasPorAmortizacion,
        Dictionary<int, List<BitacoraArchivo>> evidenciasPorBitacora,
        HttpClient httpClient,
        string contentRootPath,
        bool incluirBitacoras)
    {
        _pqClave = pqClave;
        _socio = socio;
        _amortizaciones = amortizaciones;
        _bitacorasPorAmortizacion = bitacorasPorAmortizacion;
        _evidenciasPorBitacora = evidenciasPorBitacora;
        _httpClient = httpClient;
        _contentRootPath = contentRootPath;
        _incluirBitacoras = incluirBitacoras;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"Reporte de Amortizaciones - Trámite/Crédito: {_pqClave}")
                    .FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);

                if (_socio != null)
                {
                    column.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Socio: ").SemiBold();
                        text.Span($"{_socio.S_NOMBRE} | ");
                        text.Span("RFC: ").SemiBold();
                        text.Span($"{_socio.S_RFC} | ");
                        text.Span("Empresa: ").SemiBold();
                        text.Span($"{_socio.S_EMPRESA}");
                    });
                    
                    column.Item().Text(text =>
                    {
                        text.Span("Dirección: ").SemiBold();
                        text.Span($"{_socio.S_DIRECCION_PARTICULAR}, {_socio.S_COLONIA_P}, {_socio.S_CIUDAD}");
                    });
                }

                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Fecha de generación: ").SemiBold();
                    text.Span($"{DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            if (!_amortizaciones.Any())
            {
                column.Item().Text("No se encontraron amortizaciones.").FontSize(14);
                return;
            }

            if (!_incluirBitacoras)
            {
                ComposeCompactTable(column.Item());
            }
            else
            {
                foreach (var amortizacion in _amortizaciones)
                {
                    column.Item().PaddingBottom(20).Element(c => ComposeAmortizacion(c, amortizacion));
                }
            }
        });
    }

    private void ComposeCompactTable(IContainer container)
    {
        container.Table(table =>
        {
            // Define columns
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30); // NO.
                columns.RelativeColumn();   // VENCIMIENTO
                columns.RelativeColumn();   // CAPITAL
                columns.RelativeColumn();   // INTERÉS
                columns.RelativeColumn();   // IVA
                columns.RelativeColumn();   // TOTAL
                columns.RelativeColumn();   // SALDO INSOLUTO
                columns.ConstantColumn(60); // ESTADO
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Text("NO.").FontSize(9).SemiBold();
                header.Cell().Text("VENCIMIENTO").FontSize(9).SemiBold();
                header.Cell().Text("CAPITAL").FontSize(9).SemiBold();
                header.Cell().Text("INTERÉS").FontSize(9).SemiBold();
                header.Cell().Text("IVA").FontSize(9).SemiBold();
                header.Cell().Text("TOTAL").FontSize(9).SemiBold();
                header.Cell().Text("SALDO INSOLUTO").FontSize(9).SemiBold();
                header.Cell().Text("ESTADO").FontSize(9).SemiBold();

                header.Cell().ColumnSpan(8).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
            });

            // Rows
            foreach (var amortizacion in _amortizaciones)
            {
                table.Cell().PaddingVertical(5).Text(amortizacion.A_NUMERO.ToString()).FontSize(9);
                table.Cell().PaddingVertical(5).Text(amortizacion.FECHA_VENCIMIENTO ?? "-").FontSize(9);
                table.Cell().PaddingVertical(5).Text(amortizacion.CAPITAL ?? "-").FontSize(9);
                table.Cell().PaddingVertical(5).Text(amortizacion.INTERES ?? "-").FontSize(9);
                table.Cell().PaddingVertical(5).Text(amortizacion.IVA ?? "-").FontSize(9);
                table.Cell().PaddingVertical(5).Text(amortizacion.TOTAL ?? "-").FontSize(9);
                table.Cell().PaddingVertical(5).Text(amortizacion.SALDO_INSOLUTO ?? "-").FontSize(9);

                // Estado Pill
                table.Cell().PaddingVertical(5).Element(c => ComposeEstadoPill(c, amortizacion.Estado));
            }
        });
    }

    private void ComposeEstadoPill(IContainer container, string? estado)
    {
        var normalizedEstado = (estado ?? "").Trim().ToLowerInvariant();
        var color = Colors.Grey.Lighten2;
        var textColor = Colors.Black;

        if (normalizedEstado.Contains("pagad"))
        {
            color = Colors.Green.Medium;
            textColor = Colors.White;
        }
        else if (normalizedEstado.Contains("vencid"))
        {
            color = Colors.Red.Medium;
            textColor = Colors.White;
        }
        else if (normalizedEstado.Contains("vigente"))
        {
            color = Colors.Orange.Medium;
            textColor = Colors.White;
        }

        container
            .Background(color)
            .PaddingVertical(2)
            .PaddingHorizontal(5)
            .AlignCenter()
            .Text(estado ?? "-")
            .FontSize(8)
            .FontColor(textColor)
            .SemiBold();
    }

    private void ComposeAmortizacion(IContainer container, Amortizacion amortizacion)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(column =>
        {
            // Amortizacion Header
            column.Item().Background(Colors.Grey.Lighten3).Padding(5).Text($"Amortización #: {amortizacion.A_NUMERO} - Estado: {amortizacion.Estado ?? "N/A"}")
                .FontSize(14).SemiBold();

            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Text("Fecha Venc:").SemiBold();
                table.Cell().Text(amortizacion.FECHA_VENCIMIENTO ?? "-");
                
                table.Cell().Text("Capital:").SemiBold();
                table.Cell().Text(amortizacion.CAPITAL ?? "-");

                table.Cell().Text("Interés:").SemiBold();
                table.Cell().Text(amortizacion.INTERES ?? "-");

                table.Cell().Text("Total:").SemiBold();
                table.Cell().Text(amortizacion.TOTAL ?? "-");
            });

            // Bitacoras
            if (_incluirBitacoras)
            {
                if (_bitacorasPorAmortizacion.TryGetValue(amortizacion.A_NUMERO, out var bitacoras) && bitacoras.Any())
                {
                    column.Item().PaddingTop(15).Text("Bitácoras de Gestión:").FontSize(12).SemiBold().Underline();
                    
                    foreach (var bitacora in bitacoras)
                    {
                        column.Item().PaddingTop(10).Element(c => ComposeBitacora(c, bitacora));
                    }
                }
                else
                {
                    column.Item().PaddingTop(10).Text("Sin bitácoras registradas.").Italic().FontColor(Colors.Grey.Medium);
                }
            }
        });
    }

    private void ComposeBitacora(IContainer container, Bitacora bitacora)
    {
        container.PaddingLeft(15).BorderLeft(2).BorderColor(Colors.Blue.Lighten2).PaddingLeft(10).Column(column =>
        {
            column.Item().Text(text =>
            {
                text.Span($"Fecha: {bitacora.FechaHoraGestion:dd/MM/yyyy HH:mm} | ").SemiBold();
                text.Span($"Tipo: {bitacora.TipoGestion} | ");
                text.Span($"Resultado: {bitacora.Resultado}");
            });

            if (!string.IsNullOrEmpty(bitacora.Observaciones))
            {
                column.Item().Text($"Obs: {bitacora.Observaciones}").FontSize(9).FontColor(Colors.Grey.Darken3);
            }

            // Evidencias (Images)
            if (_evidenciasPorBitacora.TryGetValue(bitacora.Id, out var evidencias) && evidencias.Any())
            {
                column.Item().PaddingTop(5).Text("Evidencias Adjuntas:").FontSize(10).SemiBold();
                
                column.Item().Row(row =>
                {
                    foreach (var evidencia in evidencias)
                    {
                        var imageBytes = LoadImageBytes(evidencia.Url);
                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            row.AutoItem().PaddingRight(10).Width(150).Image(imageBytes);
                        }
                        else
                        {
                            row.AutoItem().PaddingRight(10).Text($"[Imagen no disponible: {evidencia.Url}]").FontSize(8).FontColor(Colors.Red.Medium);
                        }
                    }
                });
            }
        });
    }

    private byte[]? LoadImageBytes(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        try
        {
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // Synchronous wait is discouraged in real apps, but valid in QuestPDF Compose unless pre-fetched
                // Ideally, images should be pre-fetched in the Service before Compose
                return _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            else
            {
                // Local file
                var fullPath = Path.Combine(_contentRootPath, url.TrimStart('/', '\\'));
                if (File.Exists(fullPath))
                {
                    return File.ReadAllBytes(fullPath);
                }
            }
        }
        catch
        {
            // Ignore errors loading image to not crash the PDF generation
        }

        return null;
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
        });
    }
}
