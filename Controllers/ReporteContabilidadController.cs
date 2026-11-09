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
        private readonly IReporteContabilidadExcelService _reporteContabilidadExcelService;

        public ReporteContabilidadController(
            IReporteContabilidadService reporteContabilidadService,
            IReporteContabilidadPdfService reporteContabilidadPdfService,
            IReporteContabilidadExcelService reporteContabilidadExcelService)
        {
            _reporteContabilidadService = reporteContabilidadService;
            _reporteContabilidadPdfService = reporteContabilidadPdfService;
            _reporteContabilidadExcelService = reporteContabilidadExcelService;
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
            [FromQuery] string etapa = "todas",
            [FromQuery(Name = "S_GRUPO")] int[]? sGrupos = null,
            [FromQuery] bool indice = false)
        {
            var pdfBytes = await _reporteContabilidadPdfService.GenerarReportePdfAsync(
                mes,
                etapa,
                sGrupos,
                indice);
            var etapaArchivo = etapa.Trim().ToLowerInvariant();
            return File(
                pdfBytes,
                "application/pdf",
                $"ReporteSaldosContables_MES{mes}_Etapa_{etapaArchivo}.pdf");
        }

        [HttpGet("reportes-contabilidad/excel")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        public async Task<IActionResult> GenerarReporteContabilidadExcel(
            [FromQuery, Range(0, 12)] int mes,
            [FromQuery] string etapa = "todas",
            [FromQuery(Name = "S_GRUPO")] int[]? sGrupos = null,
            [FromQuery] bool indice = false)
        {
            var excelBytes = await _reporteContabilidadExcelService.GenerarReporteExcelAsync(
                mes,
                etapa,
                sGrupos,
                indice);
            var etapaArchivo = etapa.Trim().ToLowerInvariant();
            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ReporteSaldosContables_MES{mes}_Etapa_{etapaArchivo}.xlsx");
        }
    }
}