using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Infrastructure.Pdf;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;

namespace ApiConcilacionFr.Core.Services;

public sealed class ReporteContabilidadPdfService : IReporteContabilidadPdfService
{
    private readonly IReporteContabilidadService _reporteContabilidadService;
    private readonly ILogger<ReporteContabilidadPdfService> _logger;

    public ReporteContabilidadPdfService(
        IReporteContabilidadService reporteContabilidadService,
        ILogger<ReporteContabilidadPdfService> logger)
    {
        _reporteContabilidadService = reporteContabilidadService;
        _logger = logger;
    }

    public async Task<byte[]> GenerarReportePdfAsync(
        int mes,
        string etapa,
        IReadOnlyCollection<int>? sGrupos,
        bool indice)
    {
        var reportes = (await _reporteContabilidadService
            .ListarReportesContabilidadPorEtapaAsync(mes, etapa, sGrupos)).ToList();

        if (reportes.Count == 0)
        {
            throw new NotFoundException(
                $"No se encontraron saldos contables para MES{mes} y la etapa {etapa}.");
        }

        _logger.LogInformation(
            "Generando reporte PDF de saldos contables para MES{Mes}, etapa {Etapa}, registros {Total}",
            mes,
            etapa,
            reportes.Count);

        return new ReporteContabilidadDocument(mes, etapa, reportes, indice).GeneratePdf();
    }
}
