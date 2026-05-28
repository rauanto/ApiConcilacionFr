using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiConcilacionFr.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BitacoraBajasController : ControllerBase
{
    private readonly IBitacoraBajasService _service;

    public BitacoraBajasController(IBitacoraBajasService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene registros de bitácora bajas con paginación.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BitacoraBajasResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginacion, [FromQuery] string? grupos = null)
    {
        var result = await _service.GetAllAsync(paginacion, grupos ?? "");
        return Ok(ApiResponse<IEnumerable<BitacoraBajasResponse>>.SuccessPaged(
            result.Items, result.Meta, "Registros de bitácora bajas obtenidos exitosamente."));
    }

    /// <summary>
    /// Registra una nueva gestión en bitácora bajas.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BitacoraBajasResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateBitacoraBajasRequest request)
    {
        if (!TryGetGestorId(out long gestorId))
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));

        var created = await _service.CreateAsync(request, gestorId);
        return Created(string.Empty,
            ApiResponse<BitacoraBajasResponse>.Success(created, "Registro de bitácora bajas creado exitosamente."));
    }

    /// <summary>
    /// Actualiza el campo baja y la observación de un registro de bitácora bajas.
    /// </summary>
    [HttpPut("{creditoId:long}")]
    [ProducesResponseType(typeof(ApiResponse<BitacoraBajasResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(long creditoId, [FromBody] UpdateBitacoraBajaRequest request)
    {
        var updated = await _service.UpdateAsync(creditoId, request);
        return Ok(ApiResponse<BitacoraBajasResponse>.Success(updated, "Registro de bitácora bajas actualizado exitosamente."));
    }

    private bool TryGetGestorId(out long gestorId)
    {
        var value = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(value, out gestorId) && gestorId > 0;
    }
}
