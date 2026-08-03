using ApiConcilacionFr.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiConcilacionFr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Assuming you want it protected, remove if not
    public class ProvisionesController : ControllerBase
    {
        private readonly IProvisionService _provisionService;

        public ProvisionesController(IProvisionService provisionService)
        {
            _provisionService = provisionService;
        }

        [HttpPost("procesar")]
        public IActionResult ProcesarReporteProvision(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se ha enviado ningún archivo.");

            try
            {
                var excelBytes = _provisionService.ProcesarReporteProvision(file);
                
                string originalName = Path.GetFileNameWithoutExtension(file.FileName);
                string newFileName = $"{originalName}_Procesado.xlsx";

                return File(
                    excelBytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    newFileName
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al procesar el archivo: {ex.Message}");
            }
        }
        [HttpPost("reporte")]
        public async Task<IActionResult> ObtenerReporteProvision([FromBody] ApiConcilacionFr.Domain.Entities.ReporteProvisionRequest request)
        {
            try
            {
                var result = await _provisionService.ObtenerReporteProvisionesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al generar el reporte: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera filtros para un reporte de provisión sin guardar el resultado.
        /// </summary>
        [HttpPost("filtros")]
        [ProducesResponseType(typeof(ApiConcilacionFr.Domain.Entities.ReporteProvisionFiltrosDto), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ObtenerFiltrosReporteProvision([FromBody] ApiConcilacionFr.Domain.Entities.ReporteProvisionRequest request)
        {
            try
            {
                var result = await _provisionService.ObtenerFiltrosReporteProvisionesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al generar los filtros: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera y guarda un reporte de provisión en la base de datos.
        /// </summary>
        [HttpPost("guardar")]
        [ProducesResponseType(typeof(object), 200)] // Devuelve { ReporteId, Mensaje }
        [ProducesResponseType(500)]
        public async Task<IActionResult> GuardarReporteProvision([FromBody] ApiConcilacionFr.Domain.Entities.ReporteGuardadoRequest request)
        {
            try
            {
                var id = await _provisionService.GenerarYGuardarReporteAsync(request);
                return Ok(new { ReporteId = id, Mensaje = "Reporte guardado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al guardar el reporte: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el detalle de un reporte de provisión previamente guardado.
        /// </summary>
        [HttpGet("guardado/{id}")]
        [ProducesResponseType(typeof(IEnumerable<ApiConcilacionFr.Domain.Entities.ReporteProvisionDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ObtenerReporteGuardado(int id)
        {
            try
            {
                var result = await _provisionService.ObtenerReporteGuardadoAsync(id);
                if (result == null || !result.Any())
                    return NotFound($"No se encontró un reporte guardado con el ID {id}.");
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al consultar el reporte guardado: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene la lista de todos los reportes de provisión guardados (cabeceras).
        /// </summary>
        [HttpGet("guardados")]
        [ProducesResponseType(typeof(IEnumerable<ApiConcilacionFr.Domain.Entities.ReporteProvisionGuardadoDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ListarReportesGuardados()
        {
            try
            {
                var result = await _provisionService.ListarReportesGuardadosAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al listar los reportes guardados: {ex.Message}");
            }
        }
    }
}
