// Core/Services/AuthService.cs
using System.Security.Cryptography;
using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;

namespace ApiConcilacionFr.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepository.GetByTokenAsync(request.UsernameOrEmail);

        if (usuario == null || !usuario.Activo)
            throw new UnauthorizedException("Credenciales incorrectas o usuario inactivo.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            throw new UnauthorizedException("Credenciales incorrectas.");

        var accessToken = JwtExtensions.GenerateToken(usuario, _configuration);
        var refreshToken = await CreateAndStoreRefreshTokenAsync(usuario.Id);
        var profile = new UsuarioProfile(usuario.Id, usuario.NombreUsuario, usuario.Correo, usuario.Rol);

        return new AuthResponse(accessToken, refreshToken, profile);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _usuarioRepository.GetByUsernameAsync(request.NombreUsuario);
        if (existingUser != null)
            throw new ConflictException("El nombre de usuario ya está en uso.");

        var existingEmail = await _usuarioRepository.GetByEmailAsync(request.Correo);
        if (existingEmail != null)
            throw new ConflictException("El correo ya está en uso.");

        var nuevoUsuario = new Domain.Entities.Usuario
        {
            NombreUsuario = request.NombreUsuario,
            Correo = request.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = "Usuario",
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        nuevoUsuario.Id = await _usuarioRepository.CreateAsync(nuevoUsuario);

        var accessToken = JwtExtensions.GenerateToken(nuevoUsuario, _configuration);
        var refreshToken = await CreateAndStoreRefreshTokenAsync(nuevoUsuario.Id);
        var profile = new UsuarioProfile(nuevoUsuario.Id, nuevoUsuario.NombreUsuario, nuevoUsuario.Correo, nuevoUsuario.Rol);

        return new AuthResponse(accessToken, refreshToken, profile);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (storedToken == null || storedToken.Revocado || storedToken.Expiracion <= DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token inválido o expirado.");

        var usuario = await _usuarioRepository.GetByIdAsync(storedToken.UsuarioId);
        if (usuario == null || !usuario.Activo)
            throw new UnauthorizedException("Usuario inactivo o no encontrado.");

        // Rotación: invalidar el token actual y emitir uno nuevo
        await _refreshTokenRepository.RevokeAsync(request.RefreshToken);

        var newAccessToken = JwtExtensions.GenerateToken(usuario, _configuration);
        var newRefreshToken = await CreateAndStoreRefreshTokenAsync(usuario.Id);
        var profile = new UsuarioProfile(usuario.Id, usuario.NombreUsuario, usuario.Correo, usuario.Rol);

        return new AuthResponse(newAccessToken, newRefreshToken, profile);
    }

    public async Task LogoutAsync(LogoutRequest request)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (storedToken != null && !storedToken.Revocado)
            await _refreshTokenRepository.RevokeAsync(request.RefreshToken);
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        if (!int.TryParse(request.UserId, out int parsedUserId))
            throw new ArgumentException("UserId inválido.");

        var usuario = await _usuarioRepository.GetByIdAsync(parsedUserId);
        if (usuario == null)
            throw new NotFoundException("Usuario no encontrado.");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        return await _usuarioRepository.UpdateAsync(usuario);
    }

    public async Task<UsuarioProfile> GetProfileAsync(int userId)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null)
            throw new NotFoundException("Usuario no encontrado.");

        return new UsuarioProfile(usuario.Id, usuario.NombreUsuario, usuario.Correo, usuario.Rol);
    }

    public async Task<IEnumerable<UsuarioBasic>> GetUsuariosAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        return usuarios.Select(u => new UsuarioBasic(u.Id, u.NombreUsuario));
    }

    private async Task<string> CreateAndStoreRefreshTokenAsync(int usuarioId)
    {
        var tokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expirationDays = double.Parse(
            _configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");

        await _refreshTokenRepository.CreateAsync(new Domain.Entities.RefreshToken
        {
            Token = tokenValue,
            UsuarioId = usuarioId,
            Expiracion = DateTime.UtcNow.AddDays(expirationDays),
            Revocado = false,
            FechaCreacion = DateTime.UtcNow
        });

        return tokenValue;
    }
}
