using ApiConcilacionFr.Common;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace ApiConcilacionFr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Assuming you want it protected, remove if not
    public class ReporteContabilidadController : ControllerBase
    {
        private readonly IReporteContabilidadRepository _reporteContabilidadRepository;

        public ReporteContabilidadController(IReporteContabilidadRepository reporteContabilidadRepository)
        {
            _reporteContabilidadRepository = reporteContabilidadRepository;
        }

        [HttpGet("reportes-contabilidad")]
        public async Task<IActionResult> ListarReportesContabilidadGuardados()
        {
            try
            {
                var reportes = await _reporteContabilidadRepository.ListarReportesContabilidadGuardadosAsync();
                return Ok(ApiResponse<IEnumerable<ReporteContabilidad>>.Success(reportes, "Reportes de contabilidad listados con éxito."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al listar los reportes de contabilidad: {ex.Message}");
            }
        }
    }
}