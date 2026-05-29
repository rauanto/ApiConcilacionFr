using System.Net.Http;
using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Pdf;
using Microsoft.Extensions.Hosting;
using QuestPDF.Fluent;

namespace ApiConcilacionFr.Core.Services;

public class ReportePdfService : IReportePdfService
{
    private readonly IReporteRepository _reporteRepository;
    private readonly IBitacoraRepository _bitacoraRepository;
    private readonly IBitacoraArchivoRepository _bitacoraArchivoRepository;
    private readonly ISocioService _socioService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHostEnvironment _env;

    public ReportePdfService(
        IReporteRepository reporteRepository,
        IBitacoraRepository bitacoraRepository,
        IBitacoraArchivoRepository bitacoraArchivoRepository,
        ISocioService socioService,
        IHttpClientFactory httpClientFactory,
        IHostEnvironment env)
    {
        _reporteRepository = reporteRepository;
        _bitacoraRepository = bitacoraRepository;
        _bitacoraArchivoRepository = bitacoraArchivoRepository;
        _socioService = socioService;
        _httpClientFactory = httpClientFactory;
        _env = env;
    }

    public async Task<byte[]> GenerarReporteAmortizacionesPdfAsync(int pqClave, long clienteId, bool incluirBitacoras = true)
    {
        // 0. Obtener datos del socio (cliente)
        var socio = await _socioService.GetDatosSocioAsync(clienteId);

        // 1. Obtener las amortizaciones
        var amortizaciones = await _reporteRepository.ObtenerAmortizacionAsync(pqClave);
        if (amortizaciones == null || !amortizaciones.Any())
        {
            return Array.Empty<byte>(); // o lanzar excepción según sea el diseño
        }

        var bitacorasPorAmortizacion = new Dictionary<int, List<Bitacora>>();
        var evidenciasPorBitacora = new Dictionary<int, List<BitacoraArchivo>>();

        if (incluirBitacoras)
        {
            var amortizacionIds = amortizaciones.Select(a => a.A_NUMERO).Distinct().ToList();

            // 2. Obtener las bitácoras asociadas a estas amortizaciones
            var filtros = new BitacoraFiltros { AmortizacionIds = amortizacionIds };
            // Asumiendo paginación amplia para obtener todas
            var paginacion = new PaginationParams { Page = 1, PageSize = 10000 };
            var (bitacoras, _) = await _bitacoraRepository.GetAllAsync(filtros, paginacion);

            // 3. Obtener los archivos de las bitácoras
            var bitacoraIds = bitacoras.Select(b => b.Id).Distinct().ToList();
            var archivos = bitacoraIds.Any()
                ? await _bitacoraArchivoRepository.GetByBitacoraIdsAsync(bitacoraIds)
                : Enumerable.Empty<Domain.Entities.BitacoraArchivo>();

            // Filtramos solo EVIDENCIA (ignoramos audios/GRABACION)
            var evidencias = archivos.Where(a => a.Tipo.ToUpper() == "EVIDENCIA").ToList();

            bitacorasPorAmortizacion = bitacoras
                .GroupBy(b => b.AmortizacionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            evidenciasPorBitacora = evidencias
                .GroupBy(e => e.BitacoraId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        // 4. Crear el documento
        var document = new AmortizacionReporteDocument(
            pqClave,
            socio,
            amortizaciones.ToList(),
            bitacorasPorAmortizacion,
            evidenciasPorBitacora,
            _httpClientFactory.CreateClient(),
            _env.ContentRootPath,
            incluirBitacoras
        );

        // 5. Generar PDF
        return document.GeneratePdf();
    }
}
