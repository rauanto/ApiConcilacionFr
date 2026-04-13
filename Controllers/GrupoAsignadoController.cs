using System.Security.Claims;
using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ApiConcilacionFr.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GrupoAsignadoController : ControllerBase
{
    private readonly IGrupoAsignadoRepository _grupoRepo;

    public GrupoAsignadoController(IGrupoAsignadoRepository grupoRepo)
    {
        _grupoRepo = grupoRepo;
    }

    /// <summary>
    /// Traer grupos. Si el usuario es Admin, trae todos. De lo contrario, solo los suyos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GrupoAsignado>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGrupos()
    {
        var rolName = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        IEnumerable<GrupoAsignado> grupos;

        // Validar lógica de rol
        if (rolName != null && string.Equals(rolName, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            grupos = await _grupoRepo.GetAllAsync();
        }
        else
        {
            // Roles como "Cartera", "Usuario" o cualquier otro, solo ven los asignados a ellos
            grupos = await _grupoRepo.GetByUsuarioIdAsync(userId);
        }

        return Ok(ApiResponse<IEnumerable<GrupoAsignado>>.Success(grupos, "Grupos obtenidos exitosamente"));
    }

    public record CreateGrupoRequest(string S_GRUPO, string nombre_grupo, int? usuario_id);

    /// <summary>
    /// Crear un nuevo grupo (S_GRUPO debe ser proveído manualmente)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateGrupoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.S_GRUPO))
            return BadRequest(ApiResponse<object>.Failure("El campo S_GRUPO es obligatorio y debe ser manual."));

        // Validar si ya existe
        var existente = await _grupoRepo.GetByIdAsync(request.S_GRUPO);
        if (existente != null)
            return Conflict(ApiResponse<object>.Failure("Ya existe un grupo con ese S_GRUPO."));

        var grupo = new GrupoAsignado
        {
            S_GRUPO = request.S_GRUPO,
            nombre_grupo = request.nombre_grupo,
            usuario_id = request.usuario_id
        };

        var result = await _grupoRepo.CreateAsync(grupo);
        return Ok(ApiResponse<bool>.Success(result, "Grupo creado exitosamente."));
    }

    public record UpdateGrupoRequest(string S_GRUPO, string nombre_grupo, int? usuario_id);

    /// <summary>
    /// Reemplazar todos los datos de un grupo (PUT)
    /// </summary>
    [HttpPut("{sGrupo}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(string sGrupo, [FromBody] UpdateGrupoRequest request)
    {
        if (sGrupo != request.S_GRUPO)
            return BadRequest(ApiResponse<object>.Failure("El S_GRUPO de la ruta no coincide con el cuerpo."));

        var existente = await _grupoRepo.GetByIdAsync(sGrupo);
        if (existente == null)
            return NotFound(ApiResponse<object>.Failure("Grupo no encontrado."));

        var grupo = new GrupoAsignado
        {
            S_GRUPO = request.S_GRUPO,
            nombre_grupo = request.nombre_grupo,
            usuario_id = request.usuario_id
        };

        var result = await _grupoRepo.UpdateAsync(grupo);
        return Ok(ApiResponse<bool>.Success(result, "Grupo actualizado completamente."));
    }

    public record PatchGrupoRequest(string nombre_grupo);

    /// <summary>
    /// Actualizar parcialmente un grupo (Solo el nombre) (PATCH)
    /// </summary>
    [HttpPatch("{sGrupo}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Patch(string sGrupo, [FromBody] PatchGrupoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.nombre_grupo))
            return BadRequest(ApiResponse<object>.Failure("El nombre del grupo es obligatorio."));

        var existente = await _grupoRepo.GetByIdAsync(sGrupo);
        if (existente == null)
            return NotFound(ApiResponse<object>.Failure("Grupo no encontrado."));

        var result = await _grupoRepo.PatchNombreAsync(sGrupo, request.nombre_grupo);
        return Ok(ApiResponse<bool>.Success(result, "Nombre del grupo modificado exitosamente."));
    }

    /// <summary>
    /// Eliminar un grupo
    /// </summary>
    [HttpDelete("{sGrupo}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(string sGrupo)
    {
        var existente = await _grupoRepo.GetByIdAsync(sGrupo);
        if (existente == null)
            return NotFound(ApiResponse<object>.Failure("Grupo no encontrado."));

        var result = await _grupoRepo.DeleteAsync(sGrupo);
        return Ok(ApiResponse<bool>.Success(result, "Grupo eliminado exitosamente."));
    }
}
