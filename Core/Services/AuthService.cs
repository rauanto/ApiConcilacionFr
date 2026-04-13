// Core/Services/AuthService.cs
using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;

namespace ApiConcilacionFr.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepository.GetByTokenAsync(request.UsernameOrEmail);

        if (usuario == null || !usuario.Activo)
        {
            throw new UnauthorizedException("Credenciales incorrectas o usuario inactivo.");
        }

        bool passValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
        if (!passValida)
        {
            throw new UnauthorizedException("Credenciales incorrectas.");
        }

        var token = JwtExtensions.GenerateToken(usuario, _configuration);
        var profile = new UsuarioProfile(usuario.Id, usuario.NombreUsuario, usuario.Correo, usuario.Rol);

        return new AuthResponse(token, profile);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Verificar existencia por usuario o email
        var existingUser = await _usuarioRepository.GetByUsernameAsync(request.NombreUsuario);
        if (existingUser != null)
        {
            throw new ConflictException("El nombre de usuario ya está en uso.");
        }

        var existingEmail = await _usuarioRepository.GetByEmailAsync(request.Correo);
        if (existingEmail != null)
        {
            throw new ConflictException("El correo ya está en uso.");
        }

        // Crear nuevo usuario
        var nuevoUsuario = new Domain.Entities.Usuario
        {
            NombreUsuario = request.NombreUsuario,
            Correo = request.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = "Usuario",
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        var newUserId = await _usuarioRepository.CreateAsync(nuevoUsuario);
        nuevoUsuario.Id = newUserId;

        // Autologuear después del registro
        var token = JwtExtensions.GenerateToken(nuevoUsuario, _configuration);
        var profile = new UsuarioProfile(nuevoUsuario.Id, nuevoUsuario.NombreUsuario, nuevoUsuario.Correo, nuevoUsuario.Rol);

        return new AuthResponse(token, profile);
    }

    public async Task<UsuarioProfile> GetProfileAsync(int userId)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null)
        {
            throw new NotFoundException("Usuario no encontrado.");
        }

        return new UsuarioProfile(usuario.Id, usuario.NombreUsuario, usuario.Correo, usuario.Rol);
    }

    public async Task<IEnumerable<UsuarioBasic>> GetUsuariosAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        return usuarios.Select(u => new UsuarioBasic(u.Id, u.NombreUsuario));
    }
}
