using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using ApiConcilacionFr.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ApiConcilacionFr.Infrastructure.Pdf;

public class AmortizacionCompletoDocument : IDocument
{
    private readonly int _pqClave;
    private readonly DatosSocio? _socio;
    private readonly List<Amortizacion> _amortizaciones;
    private readonly Dictionary<int, List<Bitacora>> _bitacorasPorAmortizacion;
    private readonly Dictionary<int, List<BitacoraArchivo>> _evidenciasPorBitacora;
    private readonly HttpClient _httpClient;
    private readonly string _rutaBaseFisica;

    public AmortizacionCompletoDocument(
        int pqClave,
        DatosSocio? socio,
        List<Amortizacion> amortizaciones,
        Dictionary<int, List<Bitacora>> bitacorasPorAmortizacion,
        Dictionary<int, List<BitacoraArchivo>> evidenciasPorBitacora,
        HttpClient httpClient,
        string rutaBaseFisica)
    {
        _pqClave = pqClave;
        _socio = socio;
        _amortizaciones = amortizaciones;
        _bitacorasPorAmortizacion = bitacorasPorAmortizacion;
        _evidenciasPorBitacora = evidenciasPorBitacora;
        _httpClient = httpClient;
        _rutaBaseFisica = rutaBaseFisica;
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
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Arial));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    private void ComposeHeader(IContainer container)
    {
        PdfHeaderHelper.ComposeSocioHeader(container, _pqClave, _socio, "Expediente Detallado de Amortizaciones");
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            if (!_amortizaciones.Any())
            {
                column.Item().Text("No se encontraron amortizaciones.")
                      .FontSize(12).FontColor(Colors.Grey.Medium).Italic();
                return;
            }

            foreach (var amortizacion in _amortizaciones)
            {
                column.Item().PaddingBottom(25).Element(c => ComposeAmortizacionCard(c, amortizacion));
            }
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

    private void ComposeAmortizacionCard(IContainer container, Amortizacion amortizacion)
    {
        // Contenedor principal de la tarjeta con borde sutil
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Column(column =>
        {
            // Encabezado de la Amortización
            column.Item().Background(Colors.Grey.Lighten4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Row(row =>
            {
                row.RelativeItem().Text($"Amortización #{amortizacion.A_NUMERO}")
                   .FontSize(14).SemiBold().FontColor(Colors.Blue.Darken3);

                // Badge de Estado a la derecha
                row.AutoItem().Element(c => ComposeEstadoPill(c, amortizacion.Estado));
            });

            // Cuerpo de la tarjeta
            column.Item().Padding(10).Column(body =>
            {
                // Detalles financieros en un Grid (Inlined)
                body.Item().PaddingBottom(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeDataField(c, "VENCIMIENTO", amortizacion.FECHA_VENCIMIENTO));
                    row.RelativeItem().Element(c => ComposeDataField(c, "CAPITAL", amortizacion.CAPITAL));
                    row.RelativeItem().Element(c => ComposeDataField(c, "INTERÉS", amortizacion.INTERES));
                    row.RelativeItem().Element(c => ComposeDataField(c, "TOTAL A PAGAR", amortizacion.TOTAL, true));
                });

                // Sección de Bitácoras
                body.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten3);
                
                if (_bitacorasPorAmortizacion.TryGetValue(amortizacion.A_NUMERO, out var bitacoras) && bitacoras.Any())
                {
                    body.Item().PaddingTop(10).PaddingBottom(5)
                        .Text("HISTORIAL DE GESTIÓN").FontSize(9).SemiBold().FontColor(Colors.Grey.Medium);
                    
                    foreach (var bitacora in bitacoras)
                    {
                        body.Item().PaddingTop(8).Element(c => ComposeBitacoraTimeline(c, bitacora));
                    }
                }
                else
                {
                    body.Item().PaddingTop(10).Text("No hay bitácoras de gestión registradas para esta amortización.")
                        .FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                }
            });
        });
    }

    private void ComposeBitacoraTimeline(IContainer container, Bitacora bitacora)
    {
        // Efecto visual de línea de tiempo con un color de borde más resaltado
        container.BorderLeft(3).BorderColor(Colors.Blue.Lighten2).PaddingLeft(12).PaddingBottom(12).Column(column =>
        {
            // 1. Meta-datos principales de la bitácora
            column.Item().Text(text =>
            {
                text.Span($"{bitacora.FechaHoraGestion:dd/MM/yyyy HH:mm}").SemiBold().FontColor(Colors.Black);
                text.Span($" • {bitacora.TipoGestion} ").SemiBold().FontColor(Colors.Blue.Darken2);
                
                if (!string.IsNullOrEmpty(bitacora.Sentido))
                    text.Span($"({bitacora.Sentido}) ").FontColor(Colors.Grey.Medium).FontSize(8);
                    
                text.Span($"• Resultado: {bitacora.Resultado}").FontColor(Colors.Grey.Darken3).SemiBold();

                if (!string.IsNullOrEmpty(bitacora.Estatus))
                    text.Span($" • Est: {bitacora.Estatus}").FontColor(Colors.Grey.Medium).Italic().FontSize(8);
            });

            // 2. Información Detallada (Asunto, Mensaje, Respuesta)
            if (!string.IsNullOrEmpty(bitacora.Asunto) || 
                !string.IsNullOrEmpty(bitacora.MensajeEnviado) || 
                !string.IsNullOrEmpty(bitacora.RespuestaCliente))
            {
                column.Item().PaddingTop(4).Column(msgCol =>
                {
                    if (!string.IsNullOrEmpty(bitacora.Asunto))
                        msgCol.Item().Text($"Asunto: {bitacora.Asunto}").FontSize(8).FontColor(Colors.Grey.Darken3).SemiBold();
                        
                    if (!string.IsNullOrEmpty(bitacora.MensajeEnviado))
                        msgCol.Item().Text($"Mensaje: {bitacora.MensajeEnviado}").FontSize(8).FontColor(Colors.Grey.Darken2);
                        
                    if (!string.IsNullOrEmpty(bitacora.RespuestaCliente))
                        msgCol.Item().Text($"Respuesta: {bitacora.RespuestaCliente}").FontSize(8).FontColor(Colors.Blue.Darken1).Italic();
                });
            }

            // 3. Promesa de Pago
            if (bitacora.PromesaMonto.HasValue || bitacora.PromesaFechaPago.HasValue)
            {
                column.Item().PaddingTop(4).Row(row =>
                {
                    var promesaTxt = "Promesa de Pago: ";
                    if (bitacora.PromesaMonto.HasValue) promesaTxt += $"{bitacora.PromesaMonto.Value:C} ";
                    if (bitacora.PromesaFechaPago.HasValue) promesaTxt += $"para el {bitacora.PromesaFechaPago.Value:dd/MM/yyyy} ";
                    
                    row.AutoItem().Text(promesaTxt).FontSize(8).FontColor(Colors.Green.Darken2).SemiBold();

                    if (bitacora.PromesaCumplida.HasValue)
                    {
                        var cumplidaColor = bitacora.PromesaCumplida.Value ? Colors.Green.Medium : Colors.Red.Medium;
                        var cumplidaTxt = bitacora.PromesaCumplida.Value ? "CUMPLIDA" : "INCUMPLIDA";
                        
                        row.AutoItem().PaddingLeft(5).Background(cumplidaColor).PaddingHorizontal(3)
                           .Text(cumplidaTxt).FontSize(7).FontColor(Colors.White).Bold();
                    }
                });
            }

            // 4. Demanda
            if (!string.IsNullOrEmpty(bitacora.Demanda))
            {
                column.Item().PaddingTop(2).Text($"Demanda: {bitacora.Demanda}").FontSize(8).FontColor(Colors.Red.Medium).SemiBold();
            }

            // 5. Observaciones Generales
            if (!string.IsNullOrEmpty(bitacora.Observaciones))
            {
                column.Item().PaddingTop(4).Text($"\"{bitacora.Observaciones}\"")
                    .FontSize(9).Italic().FontColor(Colors.Grey.Darken2);
            }

            // 6. Galería de Evidencias
            if (_evidenciasPorBitacora.TryGetValue(bitacora.Id, out var evidencias) && evidencias.Any())
            {
                column.Item().PaddingTop(8).Inlined(inlined =>
                {
                    inlined.Spacing(10);

                    foreach (var evidencia in evidencias)
                    {
                        var imageBytes = LoadImageBytes(evidencia.Url);
                        
                        // Contenedor individual para cada imagen/error
                        var imgContainer = inlined.Item().Width(120).Height(120).Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4);

                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            imgContainer.Image(imageBytes).FitArea();
                        }
                        else
                        {
                            imgContainer.AlignCenter().AlignMiddle().Padding(5)
                                .Text("Imagen no disponible").FontSize(7).FontColor(Colors.Grey.Medium).AlignCenter();
                        }
                    }
                });
            }
        });
    }

    private void ComposeDataField(IContainer container, string label, string? value, bool highlight = false)
    {
        container.Column(col =>
        {
            col.Item().Text(label).FontSize(7).SemiBold().FontColor(Colors.Grey.Medium);
            
            var textConfig = col.Item().Text(value ?? "-").FontSize(9);
            if (highlight) 
                textConfig.SemiBold().FontColor(Colors.Black);
            else 
                textConfig.FontColor(Colors.Grey.Darken3);
        });
    }

    private void ComposeEstadoPill(IContainer container, string? estado)
    {
        var categoria = ObtenerCategoriaEstado(estado);
        
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
            bgColor = Colors.Amber.Lighten4;
            txtColor = Colors.Amber.Darken4;
        }

        container
            .Background(bgColor)
            .PaddingVertical(2)
            .PaddingHorizontal(6)
            .AlignCenter()
            .Text((estado ?? "-").ToUpper())
            .FontSize(8)
            .FontColor(txtColor)
            .Bold();
    }

    private byte[]? LoadImageBytes(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        try
        {
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            else
            {
                var relativePath = url;
                if (relativePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                    relativePath = relativePath.Substring("/uploads/".Length);
                else if (relativePath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                    relativePath = relativePath.Substring("uploads/".Length);

                var fullPath = Path.Combine(_rutaBaseFisica, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(fullPath))
                {
                    return File.ReadAllBytes(fullPath);
                }
            }
        }
        catch { /* Silencioso para no romper el PDF */ }

        return null;
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