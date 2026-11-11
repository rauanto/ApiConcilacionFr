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
    public class ReporteLavadoController : ControllerBase
    {
        private readonly IReporteLavadoRepository _reporteLavadoRepository;

        public ReporteLavadoController(IReporteLavadoRepository reporteLavadoRepository)
        {
            _reporteLavadoRepository = reporteLavadoRepository;
        }

        [HttpGet("reportes-operaciones-inusuales")]
        public async Task<IActionResult> ListarReportesLavado([FromQuery] string fechaInicio, [FromQuery] string fechaFin)
        {
            try
            {
                var reportes = await _reporteLavadoRepository.ListarReportesLavadoAsync(fechaInicio, fechaFin);
                return Ok(ApiResponse<IEnumerable<OperacionesInusuales>>.Success(reportes, "Reportes de lavado listados con éxito."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al listar los reportes de lavado: {ex.Message}");
            }
        }
    }
}