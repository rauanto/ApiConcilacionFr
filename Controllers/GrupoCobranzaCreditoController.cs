using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiConcilacionFr.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GrupoCobranzaCreditoController : ControllerBase
{
    private readonly IGrupoCobranzaCreditoRepository _grupoCreditoRepo;

    public GrupoCobranzaCreditoController(IGrupoCobranzaCreditoRepository grupoCreditoRepo)
    {
        _grupoCreditoRepo = grupoCreditoRepo;
    }

    public record CreateGrupoCobranzaCreditoRequest(int GrupoCobranzaId, int CreditoId);

    /// <summary>
    /// Obtener el grupo de cobranza al que pertenece un crédito (GET)
    /// </summary>
    [HttpGet("credito/{creditoId}")]
    [ProducesResponseType(typeof(ApiResponse<GrupoCobranza>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGrupoByCreditoId(int creditoId)
    {
        var grupo = await _grupoCreditoRepo.GetGrupoByCreditoIdAsync(creditoId);
        if (grupo == null)
            return NotFound(ApiResponse<object>.Failure("El crédito no está asignado a ningún grupo."));

        return Ok(ApiResponse<GrupoCobranza>.Success(grupo, "Grupo obtenido exitosamente."));
    }

    /// <summary>
    /// Asignar un crédito a un grupo de cobranza (POST)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateGrupoCobranzaCreditoRequest request)
    {
        var asignacion = new GrupoCobranzaCredito
        {
            GrupoCobranzaId = request.GrupoCobranzaId,
            CreditoId = request.CreditoId
        };

        var result = await _grupoCreditoRepo.CreateAsync(asignacion);
        return Ok(ApiResponse<bool>.Success(result, "Asignación creada exitosamente."));
    }

    /// <summary>
    /// Eliminar la asignación de un crédito a un grupo mediante su CreditoId (DELETE)
    /// </summary>
    [HttpDelete("{creditoId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int creditoId)
    {
        var result = await _grupoCreditoRepo.DeleteByCreditoIdAsync(creditoId);
        if (!result)
            return NotFound(ApiResponse<object>.Failure("Asignación no encontrada o no se pudo eliminar."));

        return Ok(ApiResponse<bool>.Success(result, "Asignación eliminada exitosamente."));
    }
}
