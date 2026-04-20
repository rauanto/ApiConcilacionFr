// Core/Interfaces/IAuthService.cs
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public record LoginRequest(string UsernameOrEmail, string Password);
public record RegisterRequest(string NombreUsuario, string Correo, string Password);
public record AuthResponse(string Token, UsuarioProfile Profile);
public record UsuarioProfile(int Id, string NombreUsuario, string Correo, string Rol);
public record ChangePasswordRequest(string UserId, string NewPassword);

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<UsuarioProfile> GetProfileAsync(int userId);
    Task<IEnumerable<UsuarioBasic>> GetUsuariosAsync();
}

public record UsuarioBasic(int Id, string NombreUsuario);
