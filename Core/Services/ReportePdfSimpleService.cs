using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Infrastructure.Pdf;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;

namespace ApiConcilacionFr.Core.Services;

public class ReportePdfSimpleService : IReportePdfSimpleService
{
    private readonly IReporteRepository _reporteRepository;
    private readonly ISocioService _socioService;
    private readonly ILogger<ReportePdfSimpleService> _logger;

    public ReportePdfSimpleService(
        IReporteRepository reporteRepository,
        ISocioService socioService,
        ILogger<ReportePdfSimpleService> logger)
    {
        _reporteRepository = reporteRepository;
        _socioService = socioService;
        _logger = logger;
    }

    public async Task<byte[]> GenerarReportePdfAsync(int pqClave, long clienteId)
    {
        try
        {
            _logger.LogInformation("Iniciando generación de PDF Simple para el trámite {pqClave}", pqClave);

            var socio = await _socioService.GetDatosSocioAsync(clienteId);

            var amortizaciones = await _reporteRepository.ObtenerAmortizacionAsync(pqClave);
            if (amortizaciones == null || !amortizaciones.Any())
            {
                _logger.LogWarning("No se encontraron amortizaciones para el trámite {pqClave}", pqClave);
                throw new NotFoundException($"No se encontraron amortizaciones para el trámite {pqClave}");
            }

            var document = new AmortizacionSimpleDocument(pqClave, socio, amortizaciones.ToList());

            _logger.LogInformation("Generando bytes del PDF Simple...");
            return document.GeneratePdf();
        }
        catch (NotFoundException)
        {
            throw; // Re-throw para que el middleware lo atrape como 404
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al generar el reporte PDF Simple para el trámite {pqClave}", pqClave);
            throw new BadRequestException("Ocurrió un error interno al generar el reporte PDF.");
        }
    }
}
