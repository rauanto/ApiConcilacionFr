using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories;

public class RolesRepository : IRolesRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RolesRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Rol?> GetRolByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Rol>(
            "SELECT * FROM autentificacion.Roles WHERE Id = @Id", new { Id = id });
    }

    public async Task<Rol?> GetRolByNombreAsync(string nombre)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Rol>(
            "SELECT * FROM autentificacion.Roles WHERE Nombre = @Nombre", new { Nombre = nombre });
    }

    public async Task<IEnumerable<Rol>> GetAllRolesAsync()
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QueryAsync<Rol>("SELECT * FROM autentificacion.Roles");
    }

    public async Task<int> CreateRolAsync(Rol rol)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = @"
            INSERT INTO autentificacion.Roles (Nombre, Descripcion, Activo, FechaCreacion) 
            VALUES (@Nombre, @Descripcion, @Activo, @FechaCreacion);
            SELECT LAST_INSERT_ID();";
        return await connection.ExecuteScalarAsync<int>(sql, rol);
    }

    public async Task<bool> UpdateRolAsync(Rol rol)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = @"
            UPDATE autentificacion.Roles 
            SET Nombre = @Nombre, Descripcion = @Descripcion, Activo = @Activo 
            WHERE Id = @Id";
        var result = await connection.ExecuteAsync(sql, rol);
        return result > 0;
    }

    public async Task AsignarPermisosARolAsync(int rolId, IEnumerable<int> permisosIds)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Primero, borrar los permisos actuales
            await connection.ExecuteAsync(
                "DELETE FROM autentificacion.Roles_Permisos WHERE RolId = @RolId",
                new { RolId = rolId }, transaction);

            // Luego, insertar los nuevos
            if (permisosIds.Any())
            {
                var insertSql = "INSERT INTO autentificacion.Roles_Permisos (RolId, PermisoId) VALUES (@RolId, @PermisoId)";
                var parameters = permisosIds.Select(pId => new { RolId = rolId, PermisoId = pId }).ToList();
                await connection.ExecuteAsync(insertSql, parameters, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task AsignarRolesAUsuarioAsync(int usuarioId, IEnumerable<int> rolesIds)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(
                "DELETE FROM autentificacion.Usuarios_Roles WHERE UsuarioId = @UsuarioId",
                new { UsuarioId = usuarioId }, transaction);

            if (rolesIds.Any())
            {
                var insertSql = "INSERT INTO autentificacion.Usuarios_Roles (UsuarioId, RolId) VALUES (@UsuarioId, @RolId)";
                var parameters = rolesIds.Select(rId => new { UsuarioId = usuarioId, RolId = rId }).ToList();
                await connection.ExecuteAsync(insertSql, parameters, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task RemoverRolDeUsuarioAsync(int usuarioId, int rolId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await connection.ExecuteAsync(
            "DELETE FROM autentificacion.Usuarios_Roles WHERE UsuarioId = @UsuarioId AND RolId = @RolId",
            new { UsuarioId = usuarioId, RolId = rolId });
    }

    public async Task<IEnumerable<Permiso>> GetAllPermisosAsync()
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QueryAsync<Permiso>("SELECT * FROM autentificacion.Permisos");
    }

    public async Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(int rolId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = @"
            SELECT p.* FROM autentificacion.Permisos p
            INNER JOIN autentificacion.Roles_Permisos rp ON p.Id = rp.PermisoId
            WHERE rp.RolId = @RolId";
        return await connection.QueryAsync<Permiso>(sql, new { RolId = rolId });
    }

    public async Task<IEnumerable<Rol>> GetRolesByUsuarioIdAsync(int usuarioId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = @"
            SELECT r.* FROM autentificacion.Roles r
            INNER JOIN autentificacion.Usuarios_Roles ur ON r.Id = ur.RolId
            WHERE ur.UsuarioId = @UsuarioId";
        return await connection.QueryAsync<Rol>(sql, new { UsuarioId = usuarioId });
    }
}
