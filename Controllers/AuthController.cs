using System.Security.Claims;
using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiConcilacionFr.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Iniciar sesión en el sistema usando nombre de usuario o correo.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Success(result, "Autenticación exitosa"));
    }

    /// <summary>
    /// Registrar un nuevo usuario.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<AuthResponse>.Success(result, "Usuario registrado exitosamente"));
    }

    /// <summary>
    /// Utilidad temporal para generar un hash BCrypt para una contraseña (para crear usuarios en BD o debugear).
    /// </summary>
    [HttpGet("hash/{password}")]
    public IActionResult GenerateHash(string password)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return Ok(ApiResponse<string>.Success(hash, "Copia este hash e insértalo en la Base de Datos"));
    }

    /// <summary>
    /// Obtener los datos del perfil del usuario autenticado.
    /// </summary>
    [HttpGet("perfil")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UsuarioProfile>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        var profile = await _authService.GetProfileAsync(userId);
        return Ok(ApiResponse<UsuarioProfile>.Success(profile, "Perfil recuperado."));
    }

    /// <summary>
    /// Refrescar el token de sesión (Versión simplificada).
    /// </summary>
    [HttpPost("refresh")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken()
    {
        // En una implementación real se espera un RefreshToken.
        // Aquí generamos un nuevo token para el usuario autenticado usando su perfil.
        var userIdString = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized(ApiResponse<object>.Failure("Token inválido o mal formado."));
        }

        var profile = await _authService.GetProfileAsync(userId);

        // Simulamos un login con el username provisto
        // NOTA: Para producción usar un modelo de RefreshToken seguro.
        var newToken = Infrastructure.Auth.JwtExtensions.GenerateToken(new Domain.Entities.Usuario 
        {
            Id = profile.Id,
            NombreUsuario = profile.NombreUsuario,
            Correo = profile.Correo,
            Rol = profile.Rol
        }, HttpContext.RequestServices.GetRequiredService<IConfiguration>());

        return Ok(ApiResponse<AuthResponse>.Success(new AuthResponse(newToken, profile), "Token refrescado"));
    }

    /// <summary>
    /// Cerrar sesión.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        // Con JWT sin estado, el logout normal se hace en frontend borrando el token.
        // Si hay modelo de Refresh Token, acá se invalidaría en DB.
        return Ok(ApiResponse<object>.Success(null, "Cierre de sesión exitoso. Borre el token localmente."));
    }

    /// <summary>
    /// Listar usuarios (solo Id y Nombre)
    /// </summary>
    [HttpGet("usuarios")]
    [Authorize] // or add a policy if needed
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<UsuarioBasic>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsuarios()
    {
        var usuarios = await _authService.GetUsuariosAsync();
        return Ok(ApiResponse<IEnumerable<UsuarioBasic>>.Success(usuarios, "Usuarios listados exitosamente"));
    }
}
