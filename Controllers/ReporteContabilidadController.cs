using ApiConcilacionFr.Common;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;


namespace ApiConcilacionFr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Assuming you want it protected, remove if not
    public class ReporteContabilidadController : ControllerBase
    {
        private readonly IReporteContabilidadService _reporteContabilidadService;
        private readonly IReporteContabilidadPdfService _reporteContabilidadPdfService;

        public ReporteContabilidadController(
            IReporteContabilidadService reporteContabilidadService,
            IReporteContabilidadPdfService reporteContabilidadPdfService)
        {
            _reporteContabilidadService = reporteContabilidadService;
            _reporteContabilidadPdfService = reporteContabilidadPdfService;
        }

        [HttpGet("reportes-contabilidad")]
        public async Task<IActionResult> ListarReportesContabilidadGuardados([FromQuery, Range(0, 12)] int mes)
        {
            try
            {
                var reportes = await _reporteContabilidadService.ListarReportesContabilidadAsync(mes);
                return Ok(ApiResponse<IEnumerable<ReporteContabilidad>>.Success(reportes, "Reportes de contabilidad listados con éxito."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al listar los reportes de contabilidad: {ex.Message}");
            }
        }

        [HttpGet("reportes-contabilidad/pdf")]
        [Produces("application/pdf")]
        public async Task<IActionResult> GenerarReporteContabilidadPdf(
            [FromQuery, Range(0, 12)] int mes,
            [FromQuery] string etapa = "todas")
        {
            var pdfBytes = await _reporteContabilidadPdfService.GenerarReportePdfAsync(mes, etapa);
            var etapaArchivo = etapa.Trim().ToLowerInvariant();
            return File(
                pdfBytes,
                "application/pdf",
                $"ReporteSaldosContables_MES{mes}_Etapa_{etapaArchivo}.pdf");
        }
    }
}