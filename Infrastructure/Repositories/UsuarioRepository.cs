// Infrastructure/Repositories/UsuarioRepository.cs
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UsuarioRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Usuario>(
            "SELECT * FROM autentificacion.Usuarios WHERE Id = @Id", 
            new { Id = id });
    }

    public async Task<Usuario?> GetByTokenAsync(string usernameOrEmail)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Usuario>(
            "SELECT * FROM autentificacion.Usuarios WHERE Correo = @UserOrEmail OR NombreUsuario = @UserOrEmail", 
            new { UserOrEmail = usernameOrEmail });
    }

    public async Task<Usuario?> GetByUsernameAsync(string username)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Usuario>(
            "SELECT * FROM autentificacion.Usuarios WHERE NombreUsuario = @Username", 
            new { Username = username });
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Usuario>(
            "SELECT * FROM autentificacion.Usuarios WHERE Correo = @Email", 
            new { Email = email });
    }

    public async Task<int> CreateAsync(Usuario usuario)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = @"
            INSERT INTO autentificacion.Usuarios 
            (NombreUsuario, Correo, PasswordHash, Rol, Activo, FechaCreacion) 
            VALUES (@NombreUsuario, @Correo, @PasswordHash, @Rol, @Activo, @FechaCreacion);
            SELECT LAST_INSERT_ID();";

        return await connection.ExecuteScalarAsync<int>(sql, usuario);
    }

    public async Task<bool> UpdateAsync(Usuario usuario)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = @"
            UPDATE autentificacion.Usuarios 
            SET NombreUsuario = @NombreUsuario, 
                Correo = @Correo, 
                PasswordHash = @PasswordHash, 
                Rol = @Rol, 
                Activo = @Activo
            WHERE Id = @Id";

        var result = await connection.ExecuteAsync(sql, usuario);
        return result > 0;
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QueryAsync<Usuario>(
            "SELECT Id, NombreUsuario FROM autentificacion.Usuarios");
    }
}
