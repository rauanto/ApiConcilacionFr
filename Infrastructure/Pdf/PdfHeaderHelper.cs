using ApiConcilacionFr.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

namespace ApiConcilacionFr.Infrastructure.Pdf;

public static class PdfHeaderHelper
{
    public static void ComposeSocioHeader(IContainer container, int pqClave, DatosSocio? socio, string reportTitle)
    {
        // Un ligero padding inferior para separar el encabezado del contenido principal
        container.PaddingBottom(15).Column(column =>
        {
            // 1. Título del Reporte y Fecha (Extremos opuestos)
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(reportTitle)
                    .FontSize(18).Black().FontColor(Colors.Blue.Darken3);
                
                row.AutoItem().PaddingTop(5).Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
            });

            // 2. Subtítulo: Trámite / Crédito
            column.Item().PaddingBottom(10).Text($"Trámite / Crédito No. {pqClave}")
                .FontSize(12).SemiBold().FontColor(Colors.Grey.Darken2);

            // 3. Tarjeta de Información del Socio (Info Card)
            if (socio != null)
            {
                column.Item()
                    .Background(Colors.Grey.Lighten4)
                    .BorderLeft(3).BorderColor(Colors.Blue.Medium) // Acento de color
                    .Padding(10)
                    .Column(card =>
                    {
                        // Nombre del socio como encabezado de la tarjeta
                        card.Item().PaddingBottom(10)
                            .Text(socio.S_NOMBRE)
                            .FontSize(12).SemiBold().FontColor(Colors.Black);

                        // Grid de 3 columnas para los datos
                        card.Item().Row(row =>
                        {
                            // Columna 1: Identificación y Contacto
                            row.RelativeItem().Column(c =>
                            {
                                // SOLUCIÓN: Usar c.Item() para obtener el IContainer
                                ComposeField(c.Item(), "RFC", socio.S_RFC);
                                ComposeField(c.Item(), "CORREO", socio.S_FIRMA2);
                                ComposeField(c.Item(), "TELÉFONOS", socio.S_TELEFONOS);
                            });

                            // Columna 2: Laboral
                            row.RelativeItem().Column(c =>
                            {
                                ComposeField(c.Item(), "EMPRESA", socio.S_EMPRESA);
                                ComposeField(c.Item(), "PUESTO", socio.S_PUESTO);
                                ComposeField(c.Item(), "NUM. EMPLEADO", socio.S_NUM_EMPLEADOS);
                            });

                            // Columna 3: Direcciones
                            row.RelativeItem(1.5f).Column(c =>
                            {
                                var dirParticular = $"{socio.S_DIRECCION_PARTICULAR}, {socio.S_COLONIA_P}, {socio.S_CIUDAD}, C.P. {socio.S_CODIGO_POSTAL}";
                                ComposeField(c.Item(), "DIR. PARTICULAR", dirParticular);
                                
                                var domFiscal = $"{socio.S_DOMICILIO_FISCAL}, {socio.S_COLONIA_F}";
                                ComposeField(c.Item(), "DOM. FISCAL", domFiscal);
                            });
                        });
                    });
            }
        });
    }

    /// <summary>
    /// Helper privado para dibujar un campo con su etiqueta pequeña arriba y el valor abajo.
    /// </summary>
    private static void ComposeField(IContainer container, string label, string? value)
    {
        var displayValue = string.IsNullOrWhiteSpace(value) ? "-" : value;
        
        container.PaddingBottom(6).Column(col =>
        {
            col.Item().Text(label).FontSize(7).SemiBold().FontColor(Colors.Grey.Medium);
            col.Item().Text(displayValue).FontSize(9).FontColor(Colors.Grey.Darken3);
        });
    }
}