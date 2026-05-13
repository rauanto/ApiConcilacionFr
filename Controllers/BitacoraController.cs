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
public class BitacoraController : ControllerBase
{
    private readonly IBitacoraService _service;

    public BitacoraController(IBitacoraService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene registros de bitácora con paginación y filtros opcionales.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BitacoraResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] BitacoraFiltros filtros,
        [FromQuery] PaginationParams paginacion)
    {
        var result = await _service.GetAllAsync(filtros, paginacion);
        return Ok(ApiResponse<IEnumerable<BitacoraResponse>>.SuccessPaged(
            result.Items, result.Meta, "Bitácoras obtenidas exitosamente."));
    }

    /// <summary>
    /// Obtiene un registro de bitácora por su ID, incluyendo todos sus archivos adjuntos.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<BitacoraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var bitacora = await _service.GetByIdAsync(id);
        if (bitacora is null)
            return NotFound(ApiResponse<object>.Failure($"Bitácora con id {id} no encontrada."));
        return Ok(ApiResponse<BitacoraResponse>.Success(bitacora));
    }

    /// <summary>
    /// Registra una nueva gestión en bitácora.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BitacoraResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateBitacoraRequest request)
    {
        if (!TryGetGestorId(out int gestorId))
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));

        var created = await _service.CreateAsync(request, gestorId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<BitacoraResponse>.Success(created, "Bitácora registrada exitosamente."));
    }

    /// <summary>
    /// Actualiza completamente un registro de bitácora.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<BitacoraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBitacoraRequest request)
    {
        if (!TryGetGestorId(out int gestorId))
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));

        var updated = await _service.UpdateAsync(id, request, gestorId);
        return Ok(ApiResponse<BitacoraResponse>.Success(updated, "Bitácora actualizada exitosamente."));
    }

    /// <summary>
    /// Elimina un registro de bitácora y todos sus archivos adjuntos.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return Ok(ApiResponse<bool>.Success(result, "Bitácora eliminada exitosamente."));
    }

    /// <summary>
    /// Agrega una grabación a la bitácora. Se pueden adjuntar múltiples grabaciones.
    /// Formatos aceptados: mp3, wav, mp4, ogg, m4a, webm, avi, mov, aac.
    /// </summary>
    [HttpPost("{id:int}/grabacion")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104_857_600)] // 100 MB
    [ProducesResponseType(typeof(ApiResponse<BitacoraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubirGrabacion(int id, IFormFile archivo)
    {
        var updated = await _service.SubirGrabacionAsync(id, archivo);
        return Ok(ApiResponse<BitacoraResponse>.Success(updated, "Grabación agregada exitosamente."));
    }

    /// <summary>
    /// Agrega una evidencia a la bitácora. Se pueden adjuntar múltiples evidencias.
    /// Formatos aceptados: jpg, jpeg, png, gif, webp, pdf.
    /// </summary>
    [HttpPost("{id:int}/evidencia")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20_971_520)] // 20 MB
    [ProducesResponseType(typeof(ApiResponse<BitacoraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubirEvidencia(int id, IFormFile archivo)
    {
        var updated = await _service.SubirEvidenciaAsync(id, archivo);
        return Ok(ApiResponse<BitacoraResponse>.Success(updated, "Evidencia agregada exitosamente."));
    }

    /// <summary>
    /// Elimina un archivo (grabación o evidencia) específico de la bitácora.
    /// </summary>
    [HttpDelete("{id:int}/archivos/{archivoId:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EliminarArchivo(int id, int archivoId)
    {
        var result = await _service.EliminarArchivoAsync(id, archivoId);
        return Ok(ApiResponse<bool>.Success(result, "Archivo eliminado exitosamente."));
    }

    private bool TryGetGestorId(out int gestorId)
    {
        var value = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(value, out gestorId) && gestorId > 0;
    }
}
