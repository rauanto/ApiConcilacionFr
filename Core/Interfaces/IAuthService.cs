// Core/Interfaces/IAuthService.cs
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public record LoginRequest(string UsernameOrEmail, string Password);
public record RegisterRequest(string NombreUsuario, string Correo, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);
public record AuthResponse(string Token, string RefreshToken, UsuarioProfile Profile);
public record UsuarioProfile(int Id, string NombreUsuario, string Correo, IEnumerable<string> Roles, IEnumerable<string> Permisos);
public record ChangePasswordRequest(string UserId, string NewPassword);
public record UpdatePasswordRequest(string CurrentPassword, string NewPassword);

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task LogoutAsync(LogoutRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<bool> UpdatePasswordAsync(int userId, UpdatePasswordRequest request);
    Task<UsuarioProfile> GetProfileAsync(int userId);
    Task<IEnumerable<UsuarioBasic>> GetUsuariosAsync();
}

public record UsuarioBasic(int Id, string NombreUsuario);
