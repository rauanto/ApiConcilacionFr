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
    }
}
