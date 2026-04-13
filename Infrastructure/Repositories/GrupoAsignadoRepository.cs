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
    
    /// <summary>
    /// Obtiene todos los grupos asignados.
    /// </summary>
    /// <returns>Una colección de objetos GrupoAsignado.</returns>
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

    /// <summary>
    /// Obtiene los grupos asignados a un usuario específico.
    /// </summary>
    /// <param name="usuarioId">El ID del usuario.</param>
    /// <returns>Una colección de objetos GrupoAsignado.</returns>
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

    /// <summary>
    /// Obtiene un grupo asignado por su ID.
    /// </summary>
    /// <param name="sGrupo">El ID del grupo.</param>
    /// <returns>El objeto GrupoAsignado si se encuentra, de lo contrario null.</returns>
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

    /// <summary>
    /// Crea un nuevo grupo asignado.
    /// </summary>
    /// <param name="grupo">El objeto GrupoAsignado a crear.</param>
    /// <returns>True si la creación fue exitosa, de lo contrario false.</returns>
    public async Task<bool> CreateAsync(GrupoAsignado grupo)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = @"
            INSERT INTO autentificacion.grupo_asignado (S_GRUPO, nombre_grupo, usuario_id) 
            VALUES (@S_GRUPO, @nombre_grupo, @usuario_id)";
            
        var rows = await connection.ExecuteAsync(sql, grupo);
        return rows > 0;
    }


    
    /// <summary>
    /// Actualiza un grupo asignado existente.
    /// </summary>
    /// <param name="grupo">El objeto GrupoAsignado con los datos actualizados.</param>
    /// <returns>True si la actualización fue exitosa, de lo contrario false.</returns>
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


    /// <summary>
    /// Actualiza parcialmente el nombre de un grupo asignado.
    /// </summary>
    /// <param name="sGrupo">El ID del grupo.</param>
    /// <param name="nuevoNombre">El nuevo nombre del grupo.</param>
    /// <returns>True si la actualización fue exitosa, de lo contrario false.</returns>
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

    /// <summary>
    /// Elimina un grupo asignado.
    /// </summary>
    /// <param name="sGrupo">El ID del grupo a eliminar.</param>
    /// <returns>True si la eliminación fue exitosa, de lo contrario false.</returns>
    public async Task<bool> DeleteAsync(string sGrupo)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "DELETE FROM autentificacion.grupo_asignado WHERE S_GRUPO = @SGrupo";
        var rows = await connection.ExecuteAsync(sql, new { SGrupo = sGrupo });
        return rows > 0;
    }
}
