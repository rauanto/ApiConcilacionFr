using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiConcilacionFr.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SociosController : ControllerBase
{
    private readonly ISocioService _service;

    public SociosController(ISocioService service)
    {
        _service = service;
    }

    /// <summary>
    /// Obtiene los detalles de un socio (acreditado) por su clave.
    /// </summary>
    [HttpGet("{clave:long}")]
    [ProducesResponseType(typeof(ApiResponse<DatosSocio>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSocioByClave(long clave)
    {
        var socio = await _service.GetDatosSocioAsync(clave);
        return Ok(ApiResponse<DatosSocio>.Success(socio, "Datos del socio obtenidos exitosamente."));
    }
}
