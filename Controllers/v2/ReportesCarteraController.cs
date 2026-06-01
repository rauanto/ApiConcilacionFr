using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ApiConcilacionFr.Controllers.v2;

[ApiController]
[Route("api/v2/[controller]")]
[Authorize]
public class ReportesCarteraController : ControllerBase
{
    private readonly IReporteRepository _reporteRepo;

    public ReportesCarteraController(IReporteRepository reporteRepo)
    {
        _reporteRepo = reporteRepo;
    }

    [HttpGet("historicoCartera")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReporteCarteraEjecutivoHistorico>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistoricoCartera([FromQuery] DateTime fechaReporte, [FromQuery] int tipoReporte)
    {
        var rolName = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        if (fechaReporte == DateTime.MinValue)
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar una fecha válida."));
        }

        if (tipoReporte == 0)
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar un tipo de reporte válido."));
        }

        var resultado = await _reporteRepo.GetCarteraEjecutivoHistoricoAsync(fechaReporte, userId, rolName,tipoReporte);

        return Ok(ApiResponse<IEnumerable<ReporteCarteraEjecutivoHistorico>>.Success(resultado, "Reporte de cartera histórico generado con éxito."));
    }
    [HttpGet("fechasReporte")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFechasReporte()
    {
        var rolName = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        var resultado = await _reporteRepo.GetFechasReporteCarteraHistoricoAsync();

        return Ok(ApiResponse<IEnumerable<string>>.Success(resultado, "Fechas de reporte obtenidas con éxito."));
    }

}
