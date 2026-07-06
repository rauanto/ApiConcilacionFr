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
public class SolicitudBajaController : ControllerBase
{
    private readonly ISolicitudBajaService _service;

    public SolicitudBajaController(ISolicitudBajaService service)
    {
        _service = service;
    }

    [HttpGet("lista")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ApiConcilacionFr.Domain.Entities.ReporteCartera>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListaBajas()
    {
        var result = await _service.GetListaBajasAsync();
        return Ok(ApiResponse<IEnumerable<ApiConcilacionFr.Domain.Entities.ReporteCartera>>.Success(result, "Lista de bajas obtenida exitosamente."));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SolicitudBajaResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<SolicitudBajaResponse>>.Success(result, "Solicitudes de baja obtenidas exitosamente."));
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<SolicitudBajaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.Failure($"No se encontró la solicitud de baja con id {id}."));

        return Ok(ApiResponse<SolicitudBajaResponse>.Success(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SolicitudBajaResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateSolicitudBajaRequest request)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));

        var created = await _service.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, 
            ApiResponse<SolicitudBajaResponse>.Success(created, "Solicitud de baja creada exitosamente."));
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<SolicitudBajaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateSolicitudBajaRequest request)
    {
        var updated = await _service.UpdateAsync(id, request);
        if (updated == null)
            return NotFound(ApiResponse<object>.Failure($"No se encontró la solicitud de baja con id {id} para actualizar."));

        return Ok(ApiResponse<SolicitudBajaResponse>.Success(updated, "Solicitud de baja actualizada exitosamente."));
    }

    [HttpPatch("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<SolicitudBajaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Patch(long id, [FromBody] PatchSolicitudBajaRequest request)
    {
        var patched = await _service.PatchAsync(id, request);
        if (patched == null)
            return NotFound(ApiResponse<object>.Failure($"No se encontró la solicitud de baja con id {id} para modificar."));

        return Ok(ApiResponse<SolicitudBajaResponse>.Success(patched, "Solicitud de baja modificada exitosamente."));
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.Failure($"No se encontró la solicitud de baja con id {id} para eliminar."));

        return Ok(ApiResponse<object>.Success(null, "Solicitud de baja eliminada exitosamente."));
    }

    private bool TryGetUserId(out int userId)
    {
        var value = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out userId) && userId > 0;
    }
}
