// Infrastructure/Repositories/GrupoAsignadoRepository.cs
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Core.Services;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories;

public class GrupoAsignadoRepository : IGrupoAsignadoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    private readonly IAuditHelper _auditHelper;

    public GrupoAsignadoRepository(IDbConnectionFactory connectionFactory, IAuditHelper auditHelper)
    {
        _connectionFactory = connectionFactory;
        _auditHelper = auditHelper;
    }

    public async Task<IEnumerable<GrupoAsignado>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = @"
            SELECT ga.S_GRUPO, ga.nombre_grupo, ga.usuario_id,
                   COALESCE(u.NombreUsuario, 'Sin agregar') AS nombre_usuario
            FROM autentificacion.grupo_asignado ga
            LEFT JOIN autentificacion.Usuarios u ON u.Id = ga.usuario_id";
        return await connection.QueryAsync<GrupoAsignado>(sql);
    }

    public async Task<IEnumerable<GrupoAsignado>> GetByUsuarioIdAsync(int usuarioId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = @"
            SELECT ga.S_GRUPO, ga.nombre_grupo, ga.usuario_id,
                   COALESCE(u.NombreUsuario, 'Sin agregar') AS nombre_usuario
            FROM autentificacion.grupo_asignado ga
            LEFT JOIN autentificacion.Usuarios u ON u.Id = ga.usuario_id
            WHERE ga.usuario_id = @UsuarioId";
        return await connection.QueryAsync<GrupoAsignado>(sql, new { UsuarioId = usuarioId });
    }

    public async Task<GrupoAsignado?> GetByIdAsync(string sGrupo)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = @"
            SELECT ga.S_GRUPO, ga.nombre_grupo, ga.usuario_id,
                   COALESCE(u.NombreUsuario, 'Sin agregar') AS nombre_usuario
            FROM autentificacion.grupo_asignado ga
            LEFT JOIN autentificacion.Usuarios u ON u.Id = ga.usuario_id
            WHERE ga.S_GRUPO = @SGrupo";
        return await connection.QuerySingleOrDefaultAsync<GrupoAsignado>(sql, new { SGrupo = sGrupo });
    }

    public async Task<bool> CreateAsync(GrupoAsignado grupo)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = @"
            INSERT INTO autentificacion.grupo_asignado (S_GRUPO, nombre_grupo, usuario_id) 
            VALUES (@S_GRUPO, @nombre_grupo, @usuario_id)";
            
        var rows = await connection.ExecuteAsync(sql, grupo);
        return rows > 0;
    }


    

    public async Task<bool> UpdateAsync(GrupoAsignado grupo)
    {
        var estadoAnterior = await GetByIdAsync(grupo.S_GRUPO);

        await _auditHelper.ExecuteWithAuditAsync(
            "GrupoAsignado", 
            grupo.S_GRUPO, 
            "UPDATE", 
            estadoAnterior, 
            grupo, 
            async () => 
            {
                using var connection = await _connectionFactory.CreateOpenConnectionAsync();
                var sql = @"UPDATE autentificacion.grupo_asignado 
                            SET nombre_grupo = @nombre_grupo, usuario_id = @usuario_id 
                            WHERE S_GRUPO = @S_GRUPO";
                await connection.ExecuteAsync(sql, grupo);
            });

        return true;
    }



    public async Task<bool> PatchNombreAsync(string sGrupo, string nuevoNombre)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = @"
            UPDATE autentificacion.grupo_asignado 
            SET nombre_grupo = @NombreGrupo 
            WHERE S_GRUPO = @SGrupo";

        var rows = await connection.ExecuteAsync(sql, new { NombreGrupo = nuevoNombre, SGrupo = sGrupo });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(string sGrupo)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "DELETE FROM autentificacion.grupo_asignado WHERE S_GRUPO = @SGrupo";
        var rows = await connection.ExecuteAsync(sql, new { SGrupo = sGrupo });
        return rows > 0;
    }
}
