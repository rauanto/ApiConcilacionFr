using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ApiConcilacionFr.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly IReporteRepository _reporteRepo;
    private readonly IReportePdfSimpleService _reportePdfSimpleService;
    private readonly IReportePdfCompletoService _reportePdfCompletoService;

    public ReportesController(
        IReporteRepository reporteRepo, 
        IReportePdfSimpleService reportePdfSimpleService,
        IReportePdfCompletoService reportePdfCompletoService)
    {
        _reporteRepo = reporteRepo;
        _reportePdfSimpleService = reportePdfSimpleService;
        _reportePdfCompletoService = reportePdfCompletoService;
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

    /// <summary>
    /// Genera un reporte PDF de las amortizaciones de un trámite/cliente, incluyendo bitácoras.
    /// </summary>
    [HttpGet("amortizacion/{pqClave}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAmortizacionPdf(int pqClave, [FromQuery] long clienteId)
    {
        var pdfBytes = await _reportePdfCompletoService.GenerarReportePdfAsync(pqClave, clienteId);
        
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            return NotFound(ApiResponse<object>.Failure("No se encontraron datos para generar el PDF."));
        }

        return File(pdfBytes, "application/pdf", $"ReporteAmortizaciones_Completo_{pqClave}.pdf");
    }

    /// <summary>
    /// Genera un reporte PDF de una única amortización de un trámite/cliente, incluyendo bitácoras.
    /// </summary>
    [HttpGet("amortizacion/{pqClave}/cuota/{aNumero}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAmortizacionCuotaPdf(int pqClave, int aNumero, [FromQuery] long clienteId)
    {
        var pdfBytes = await _reportePdfCompletoService.GenerarReportePdfCuotaAsync(pqClave, clienteId, aNumero);
        
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            return NotFound(ApiResponse<object>.Failure("No se encontraron datos para generar el PDF."));
        }

        return File(pdfBytes, "application/pdf", $"ReporteAmortizacion_{pqClave}_Cuota_{aNumero}.pdf");
    }

    /// <summary>
    /// Genera un reporte PDF de las amortizaciones de un trámite/cliente SIN incluir bitácoras.
    /// </summary>
    [HttpGet("amortizacion/{pqClave}/pdf-simple")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAmortizacionPdfSimple(int pqClave, [FromQuery] long clienteId)
    {
        var pdfBytes = await _reportePdfSimpleService.GenerarReportePdfAsync(pqClave, clienteId);
        
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            return NotFound(ApiResponse<object>.Failure("No se encontraron datos para generar el PDF."));
        }

        return File(pdfBytes, "application/pdf", $"ReporteAmortizaciones_Simple_{pqClave}.pdf");
    }

    #region Liquidados Por grupo y Nombre
    /// <summary>
    /// Obtiene el reporte de liquidados por grupo pasándole la fecha de inicio, el usuario y el rol.
    /// </summary>
    [HttpGet("liquidadosGrupo")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReporteLiquidadosgrupo>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiquidadosGrupo([FromQuery] DateTime fechaInicio, [FromQuery] DateTime? fechaFin)
    {

        var rolName = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        if (fechaInicio == DateTime.MinValue)
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar una fecha de inicio válida."));
        }


        var resultado = await _reporteRepo.GetLiquidadosGrupoAsync(fechaInicio, fechaFin, rolName, userId);

        return Ok(ApiResponse<IEnumerable<ReporteLiquidadosgrupo>>.Success(resultado, "Reporte de liquidados por grupo obtenido con éxito."));
    }

    [HttpGet("liquidadosAcreditados")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReporteLiquidadosAcreditados>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLiquidadosAcreditados([FromQuery] DateTime fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] int grupo)
    {
        var rolName = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        if (fechaInicio == DateTime.MinValue)
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar una fecha de inicio válida."));
        }

        if (grupo == 0)
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar un grupo válido."));
        }

        var resultado = await _reporteRepo.GetLiquidadosAcreditadosAsync(fechaInicio, fechaFin, rolName, userId, grupo);

        return Ok(ApiResponse<IEnumerable<ReporteLiquidadosAcreditados>>.Success(resultado, "Reporte de liquidados por grupo obtenido con éxito."));
    }


    #endregion

    #region Otorgados por grupo y nombre
    [HttpGet("otorgadosGrupo")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReporteOtorgadosGrupo>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOtorgadosGrupo([FromQuery] DateTime fechaInicio, [FromQuery] DateTime? fechaFin)
    {

        var rolName = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        if (fechaInicio == DateTime.MinValue)
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar una fecha de inicio válida."));
        }


        var resultado = await _reporteRepo.GetOtorgadosGrupoAsync(fechaInicio, fechaFin, rolName, userId);

        return Ok(ApiResponse<IEnumerable<ReporteOtorgadosGrupo>>.Success(resultado, "Reporte de otorgados por grupo obtenido con éxito."));
    }

    [HttpGet("otorgadosAcreditados")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReporteOtorgadosAcreditados>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOtorgadosAcreditados([FromQuery] DateTime fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] int grupo)
    {
        var rolName = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        if (fechaInicio == DateTime.MinValue)
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar una fecha de inicio válida."));
        }

        if (grupo == 0)
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar un grupo válido."));
        }

        var resultado = await _reporteRepo.GetOtorgadosAcreditadosAsync(fechaInicio, fechaFin, rolName, userId, grupo);

        return Ok(ApiResponse<IEnumerable<ReporteOtorgadosAcreditados>>.Success(resultado, "Reporte de otorgados por grupo obtenido con éxito."));
    }

    #endregion

    #region historico cartera grupo
    [HttpGet("carteraEjecutivoHistoricoGrupo")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReporteCarteraHisotoricoGrupo>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCarteraEjecutivoHistoricoGrupo([FromQuery] DateTime fechaReporte, [FromQuery] int tipo_reporte)
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

        var resultado = await _reporteRepo.GetCarteraEjecutivoHistoricoGrupoAsync(fechaReporte, userId, rolName, tipo_reporte);

        return Ok(ApiResponse<IEnumerable<ReporteCarteraHisotoricoGrupo>>.Success(resultado, "Reporte de cartera de ejecutivos histórico por grupo obtenido con éxito."));
    }

    #endregion


    #region Historico detalle por grupo
    [HttpGet("carteraEjecutivoHistoricoAcreditado")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReporteCarteraEjecutivoHistoricoAcreditados>>),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCarteraEjecutivoHistoricoAcreditados([FromQuery] DateTime fechaReporte, [FromQuery] int S_GRUPO,[FromQuery] int tipo_reporte ){
        if (fechaReporte == DateTime.MinValue)
        {
            return BadRequest(ApiResponse<object>.Failure("Debes proporcionar una fecha válida."));
        }


        var resultado =
            await _reporteRepo.GetCarteraEjecutivoHistoricoAcreditadosAsync(fechaReporte, S_GRUPO, tipo_reporte);
        
        return Ok(ApiResponse<IEnumerable<ReporteCarteraEjecutivoHistoricoAcreditados>>.Success(resultado,"Reporte de Cartera historico por acreditados"));

    }



    #endregion
}
