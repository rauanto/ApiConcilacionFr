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

        public ReporteContabilidadController(IReporteContabilidadService reporteContabilidadService)
        {
            _reporteContabilidadService = reporteContabilidadService;
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
    }
}