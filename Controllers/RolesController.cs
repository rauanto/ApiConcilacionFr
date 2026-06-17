using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiConcilacionFr.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;

    public RolesController(IRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    /// <summary>
    /// Obtiene todos los roles
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Rol>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _rolesService.GetAllRolesAsync();
        return Ok(ApiResponse<IEnumerable<Rol>>.Success(roles));
    }

    /// <summary>
    /// Obtiene un rol por su ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Rol>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRolById(int id)
    {
        var rol = await _rolesService.GetRolByIdAsync(id);
        if (rol == null)
            return NotFound(ApiResponse<object>.Failure($"Rol con ID {id} no encontrado."));

        return Ok(ApiResponse<Rol>.Success(rol));
    }

    /// <summary>
    /// Crea un nuevo rol
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Rol>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRol([FromBody] CreateRolRequest request)
    {
        var rol = await _rolesService.CreateRolAsync(request);
        return CreatedAtAction(nameof(GetRolById), new { id = rol.Id }, ApiResponse<Rol>.Success(rol, "Rol creado exitosamente."));
    }

    /// <summary>
    /// Actualiza un rol existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Rol>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRol(int id, [FromBody] UpdateRolRequest request)
    {
        var rol = await _rolesService.UpdateRolAsync(id, request);
        return Ok(ApiResponse<Rol>.Success(rol, "Rol actualizado exitosamente."));
    }

    /// <summary>
    /// Obtiene todos los permisos disponibles en el sistema
    /// </summary>
    [HttpGet("permisos")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Permiso>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermisos()
    {
        var permisos = await _rolesService.GetAllPermisosAsync();
        return Ok(ApiResponse<IEnumerable<Permiso>>.Success(permisos));
    }

    /// <summary>
    /// Obtiene los permisos asignados a un rol específico
    /// </summary>
    [HttpGet("{id}/permisos")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Permiso>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermisosByRolId(int id)
    {
        var permisos = await _rolesService.GetPermisosByRolIdAsync(id);
        return Ok(ApiResponse<IEnumerable<Permiso>>.Success(permisos));
    }

    /// <summary>
    /// Asigna una lista de permisos a un rol
    /// </summary>
    [HttpPost("{id}/permisos")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AsignarPermisosARol(int id, [FromBody] AssignPermisosRequest request)
    {
        await _rolesService.AsignarPermisosARolAsync(id, request);
        return Ok(ApiResponse<bool>.Success(true, "Permisos asignados exitosamente al rol."));
    }

    /// <summary>
    /// Obtiene los roles asignados a un usuario específico
    /// </summary>
    [HttpGet("~/api/usuarios/{usuarioId}/roles")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Rol>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesByUsuarioId(int usuarioId)
    {
        var roles = await _rolesService.GetRolesByUsuarioIdAsync(usuarioId);
        return Ok(ApiResponse<IEnumerable<Rol>>.Success(roles));
    }

    /// <summary>
    /// Asigna una lista de roles a un usuario (sobrescribe los anteriores)
    /// </summary>
    [HttpPost("~/api/usuarios/{usuarioId}/roles")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AsignarRolesAUsuario(int usuarioId, [FromBody] AssignRolesRequest request)
    {
        await _rolesService.AsignarRolesAUsuarioAsync(usuarioId, request);
        return Ok(ApiResponse<bool>.Success(true, "Roles asignados exitosamente al usuario."));
    }

    /// <summary>
    /// Elimina un rol específico de un usuario
    /// </summary>
    [HttpDelete("~/api/usuarios/{usuarioId}/roles/{rolId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoverRolDeUsuario(int usuarioId, int rolId)
    {
        await _rolesService.RemoverRolDeUsuarioAsync(usuarioId, rolId);
        return Ok(ApiResponse<bool>.Success(true, "Rol removido exitosamente del usuario."));
    }
}
