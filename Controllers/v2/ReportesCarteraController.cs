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
    public async Task<IActionResult> GetHistoricoCartera([FromQuery] int mes, [FromQuery] int anio)
    {
        var rolName = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        if (mes == 0 || anio == 0)
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar un mes y un año válidos."));
        }

        var resultado = await _reporteRepo.GetCarteraEjecutivoHistoricoAsync(mes, anio, userId, rolName);

        return Ok(ApiResponse<IEnumerable<ReporteCarteraEjecutivoHistorico>>.Success(resultado, "Reporte de cartera histórico generado con éxito."));
    }


}
