using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ApiConcilacionFr.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IReporteRepository _reporteRepo;

    public ReportesController(IReporteRepository reporteRepo)
    {
        _reporteRepo = reporteRepo;
    }

    public record ReporteCarteraResponse(int TotalRegistros, IEnumerable<ReporteCartera> Registros);

    /// <summary>
    /// Obtiene el reporte de cartera pasándole uno o múltiples grupos (ej. "990035,990036").
    /// </summary>
    [HttpGet("carteraPorGrupo")]
    [ProducesResponseType(typeof(ApiResponse<ReporteCarteraResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCarteraPorGrupo([FromQuery] string grupos)
    {
        if (string.IsNullOrWhiteSpace(grupos))
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar al menos un grupo válido. (s_grupo)"));
        }

        var resultado = await _reporteRepo.GetCarteraPorGrupoAsync(grupos);
        var response = new ReporteCarteraResponse(resultado.Count(), resultado);

        return Ok(ApiResponse<ReporteCarteraResponse>.Success(response, "Reporte de cartera generado con éxito."));
    }

    /// <summary>
    /// Obtiene el reporte de cartera de ejecutivos (dependiendo del rol del usuario).
    /// </summary>
    [HttpGet("carteraEjecutivos")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReporteCarteraEjecutivo>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCarteraEjecutivos()
    {
        var rolName = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        var resultado = await _reporteRepo.GetCarteraEjecutivosAsync(userId, rolName);

        return Ok(ApiResponse<IEnumerable<ReporteCarteraEjecutivo>>.Success(resultado, "Reporte de ejecutivos generado con éxito."));
    }

    /// <summary>
    /// Obtiene la amortización pasándole la clave del trámite.
    /// </summary>
    [HttpGet("amortizacion/{pqClave}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Amortizacion>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAmortizacion(int pqClave)
    {
        var resultado = await _reporteRepo.ObtenerAmortizacionAsync(pqClave);

        return Ok(ApiResponse<IEnumerable<Amortizacion>>.Success(resultado, "Amortización obtenida con éxito."));
    }
}
