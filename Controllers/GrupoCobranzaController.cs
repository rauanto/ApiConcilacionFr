using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace ApiConcilacionFr.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GrupoCobranzaController : ControllerBase
{
    private readonly IGrupoCobranzaRepository _grupoRepo;

    public GrupoCobranzaController(IGrupoCobranzaRepository grupoRepo)
    {
        _grupoRepo = grupoRepo;
    }

    /// <summary>
    /// Obtener todos los grupos de cobranza
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<GrupoCobranza>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var grupos = await _grupoRepo.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<GrupoCobranza>>.Success(grupos, "Grupos obtenidos exitosamente."));
    }

    /// <summary>
    /// Obtener un grupo de cobranza por su ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GrupoCobranza>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var grupo = await _grupoRepo.GetByIdAsync(id);
        if (grupo == null)
            return NotFound(ApiResponse<object>.Failure("Grupo no encontrado."));

        return Ok(ApiResponse<GrupoCobranza>.Success(grupo, "Grupo obtenido exitosamente."));
    }

    public record CreateGrupoCobranzaRequest(string Nombre, string Descripcion, DateTime FechaCobranza);

    /// <summary>
    /// Crear un nuevo grupo de cobranza (POST)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateGrupoCobranzaRequest request)
    {
        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        var grupo = new GrupoCobranza
        {
            Nombre = request.Nombre,
            UsuarioCreoId = userId,
            Descripcion = request.Descripcion,
            FechaCobranza = request.FechaCobranza
        };

        var result = await _grupoRepo.CreateAsync(grupo);
        return Ok(ApiResponse<bool>.Success(result, "Grupo de cobranza creado exitosamente."));
    }

    public record UpdateGrupoCobranzaRequest(int Id, string Nombre, string Descripcion, DateTime FechaCobranza);

    /// <summary>
    /// Reemplazar todos los datos de un grupo (PUT)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGrupoCobranzaRequest request)
    {
        if (id != request.Id)
            return BadRequest(ApiResponse<object>.Failure("El ID de la ruta no coincide con el cuerpo."));

        var userIdString = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        var existente = await _grupoRepo.GetByIdAsync(id);
        if (existente == null)
            return NotFound(ApiResponse<object>.Failure("Grupo no encontrado."));

        var grupo = new GrupoCobranza
        {
            Id = request.Id,
            Nombre = request.Nombre,
            UsuarioCreoId = userId,
            Descripcion = request.Descripcion,
            FechaCobranza = request.FechaCobranza
        };

        var result = await _grupoRepo.UpdateAsync(grupo);
        return Ok(ApiResponse<bool>.Success(result, "Grupo actualizado completamente."));
    }

    public record PatchGrupoCobranzaRequest(string Descripcion);

    /// <summary>
    /// Actualizar parcialmente un grupo (PATCH)
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchGrupoCobranzaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Descripcion))
            return BadRequest(ApiResponse<object>.Failure("La descripción es obligatoria."));

        var existente = await _grupoRepo.GetByIdAsync(id);
        if (existente == null)
            return NotFound(ApiResponse<object>.Failure("Grupo no encontrado."));

        var result = await _grupoRepo.PatchDescripcionAsync(id, request.Descripcion);
        return Ok(ApiResponse<bool>.Success(result, "Descripción del grupo modificada exitosamente."));
    }

    /// <summary>
    /// Eliminar un grupo (DELETE)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id)
    {
        var existente = await _grupoRepo.GetByIdAsync(id);
        if (existente == null)
            return NotFound(ApiResponse<object>.Failure("Grupo no encontrado."));

        var result = await _grupoRepo.DeleteAsync(id);
        return Ok(ApiResponse<bool>.Success(result, "Grupo eliminado exitosamente."));
    }
}
