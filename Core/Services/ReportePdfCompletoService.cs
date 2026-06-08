using System.Net.Http;
using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Pdf;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;

namespace ApiConcilacionFr.Core.Services;

public class ReportePdfCompletoService : IReportePdfCompletoService
{
    private readonly IReporteRepository _reporteRepository;
    private readonly IBitacoraRepository _bitacoraRepository;
    private readonly IBitacoraArchivoRepository _bitacoraArchivoRepository;
    private readonly ISocioService _socioService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHostEnvironment _env;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReportePdfCompletoService> _logger;

    public ReportePdfCompletoService(
        IReporteRepository reporteRepository,
        IBitacoraRepository bitacoraRepository,
        IBitacoraArchivoRepository bitacoraArchivoRepository,
        ISocioService socioService,
        IHttpClientFactory httpClientFactory,
        IHostEnvironment env,
        IConfiguration configuration,
        ILogger<ReportePdfCompletoService> logger)
    {
        _reporteRepository = reporteRepository;
        _bitacoraRepository = bitacoraRepository;
        _bitacoraArchivoRepository = bitacoraArchivoRepository;
        _socioService = socioService;
        _httpClientFactory = httpClientFactory;
        _env = env;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<byte[]> GenerarReportePdfAsync(int pqClave, long clienteId)
    {
        try
        {
            _logger.LogInformation("Iniciando generación de PDF Completo para el trámite {pqClave}", pqClave);

            var socio = await _socioService.GetDatosSocioAsync(clienteId);

            var amortizaciones = await _reporteRepository.ObtenerAmortizacionAsync(pqClave);
            if (amortizaciones == null || !amortizaciones.Any())
            {
                _logger.LogWarning("No se encontraron amortizaciones para el trámite {pqClave}", pqClave);
                throw new NotFoundException($"No se encontraron amortizaciones para el trámite {pqClave}");
            }

            var amortizacionIds = amortizaciones.Select(a => a.A_NUMERO).Distinct().ToList();

            var filtros = new BitacoraFiltros { 
                AmortizacionIds = amortizacionIds,
                CreditoId = pqClave,
                ClienteId = (int)clienteId
            };
            var paginacion = new PaginationParams { Page = 1, PageSize = 10000 };
            var (bitacoras, _) = await _bitacoraRepository.GetAllAsync(filtros, paginacion);

            var bitacoraIds = bitacoras.Select(b => b.Id).Distinct().ToList();
            var archivos = bitacoraIds.Any()
                ? await _bitacoraArchivoRepository.GetByBitacoraIdsAsync(bitacoraIds)
                : Enumerable.Empty<BitacoraArchivo>();

            var evidencias = archivos.Where(a => a.Tipo.ToUpper() == "EVIDENCIA").ToList();

            var bitacorasPorAmortizacion = bitacoras
                .GroupBy(b => b.AmortizacionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var evidenciasPorBitacora = evidencias
                .GroupBy(e => e.BitacoraId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var seccion = _configuration.GetSection("FileStorage");
            var carpeta = seccion["RutaBase"] ?? "uploads";
            var rutaBaseFisica = Path.IsPathRooted(carpeta)
                ? carpeta
                : Path.Combine(_env.ContentRootPath, carpeta);

            var document = new AmortizacionCompletoDocument(
                pqClave,
                socio,
                amortizaciones.ToList(),
                bitacorasPorAmortizacion,
                evidenciasPorBitacora,
                _httpClientFactory.CreateClient(),
                rutaBaseFisica
            );

            _logger.LogInformation("Generando bytes del PDF Completo...");
            return document.GeneratePdf();
        }
        catch (NotFoundException)
        {
            throw; // Middleware maneja esto como 404
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al generar el reporte PDF Completo para el trámite {pqClave}", pqClave);
            throw new BadRequestException("Ocurrió un error interno al generar el reporte PDF Completo.");
        }
    }

    public async Task<byte[]> GenerarReportePdfCuotaAsync(int pqClave, long clienteId, int aNumero)
    {
        try
        {
            _logger.LogInformation("Iniciando generación de PDF Completo para la cuota {aNumero} del trámite {pqClave}", aNumero, pqClave);

            var socio = await _socioService.GetDatosSocioAsync(clienteId);

            var todasAmortizaciones = await _reporteRepository.ObtenerAmortizacionAsync(pqClave);
            var amortizaciones = todasAmortizaciones?.Where(a => a.A_NUMERO == aNumero).ToList();

            if (amortizaciones == null || !amortizaciones.Any())
            {
                _logger.LogWarning("No se encontró la cuota {aNumero} para el trámite {pqClave}", aNumero, pqClave);
                throw new NotFoundException($"No se encontró la cuota {aNumero} para el trámite {pqClave}");
            }

            var amortizacionIds = new List<int> { aNumero };

            var filtros = new BitacoraFiltros { 
                AmortizacionIds = amortizacionIds,
                CreditoId = pqClave,
                ClienteId = (int)clienteId
            };
            var paginacion = new PaginationParams { Page = 1, PageSize = 10000 };
            var (bitacoras, _) = await _bitacoraRepository.GetAllAsync(filtros, paginacion);

            var bitacoraIds = bitacoras.Select(b => b.Id).Distinct().ToList();
            var archivos = bitacoraIds.Any()
                ? await _bitacoraArchivoRepository.GetByBitacoraIdsAsync(bitacoraIds)
                : Enumerable.Empty<BitacoraArchivo>();

            var evidencias = archivos.Where(a => a.Tipo.ToUpper() == "EVIDENCIA").ToList();

            var bitacorasPorAmortizacion = bitacoras
                .GroupBy(b => b.AmortizacionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var evidenciasPorBitacora = evidencias
                .GroupBy(e => e.BitacoraId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var seccion = _configuration.GetSection("FileStorage");
            var carpeta = seccion["RutaBase"] ?? "uploads";
            var rutaBaseFisica = Path.IsPathRooted(carpeta)
                ? carpeta
                : Path.Combine(_env.ContentRootPath, carpeta);

            var document = new AmortizacionCompletoDocument(
                pqClave,
                socio,
                amortizaciones,
                bitacorasPorAmortizacion,
                evidenciasPorBitacora,
                _httpClientFactory.CreateClient(),
                rutaBaseFisica
            );

            _logger.LogInformation("Generando bytes del PDF Completo por cuota...");
            return document.GeneratePdf();
        }
        catch (NotFoundException)
        {
            throw; // Middleware maneja esto como 404
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al generar el reporte PDF Completo de cuota {aNumero} para trámite {pqClave}", aNumero, pqClave);
            throw new BadRequestException("Ocurrió un error interno al generar el reporte PDF.");
        }
    }
}
