namespace ApiConcilacionFr.Infrastructure.Repositories;

using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Core.Services;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

public class GrupoCobranzaRepository : IGrupoCobranzaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IAuditHelper _auditHelper;

    public GrupoCobranzaRepository(IDbConnectionFactory connectionFactory, IAuditHelper auditHelper)
    {
        _connectionFactory = connectionFactory;
        _auditHelper = auditHelper;
    }

    public async Task<IEnumerable<GrupoCobranza>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = @"
            SELECT gc.id as Id, gc.nombre as Nombre, gc.usuario_creo_id as UsuarioCreoId, 
                   COALESCE(u.NombreUsuario, 'Desconocido') as UsuarioCreoNombre, 
                   gc.descripcion as Descripcion, gc.fecha_cobranza as FechaCobranza, gc.fecha_creacion as FechaCreacion 
            FROM bitacora.grupo_cobranza gc
            LEFT JOIN autentificacion.Usuarios u ON gc.usuario_creo_id = u.Id";
        return await connection.QueryAsync<GrupoCobranza>(sql);
    }

    public async Task<GrupoCobranza?> GetByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = @"
            SELECT gc.id as Id, gc.nombre as Nombre, gc.usuario_creo_id as UsuarioCreoId, 
                   COALESCE(u.NombreUsuario, 'Desconocido') as UsuarioCreoNombre, 
                   gc.descripcion as Descripcion, gc.fecha_cobranza as FechaCobranza, gc.fecha_creacion as FechaCreacion 
            FROM bitacora.grupo_cobranza gc
            LEFT JOIN autentificacion.Usuarios u ON gc.usuario_creo_id = u.Id
            WHERE gc.id = @Id";
        return await connection.QuerySingleOrDefaultAsync<GrupoCobranza>(sql, new { Id = id });
    }

    public async Task<bool> CreateAsync(GrupoCobranza grupo)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = @"
            INSERT INTO bitacora.grupo_cobranza (nombre, usuario_creo_id, descripcion, fecha_cobranza) 
            VALUES (@Nombre, @UsuarioCreoId, @Descripcion, @FechaCobranza)";
        var rows = await connection.ExecuteAsync(sql, grupo);
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(GrupoCobranza grupo)
    {
        var estadoAnterior = await GetByIdAsync(grupo.Id);
        int rowsAffected = 0;

        await _auditHelper.ExecuteWithAuditAsync(
            "GrupoCobranza", 
            grupo.Id.ToString(), 
            "UPDATE", 
            estadoAnterior, 
            grupo, 
            async () => 
            {
                using var connection = await _connectionFactory.CreateOpenConnectionAsync();
                const string sql = @"
                    UPDATE bitacora.grupo_cobranza 
                    SET nombre = @Nombre,
                        usuario_creo_id = @UsuarioCreoId, 
                        descripcion = @Descripcion, 
                        fecha_cobranza = @FechaCobranza 
                    WHERE id = @Id";
                rowsAffected = await connection.ExecuteAsync(sql, grupo);
            });

        return rowsAffected > 0;
    }

    public async Task<bool> PatchDescripcionAsync(int id, string nuevaDescripcion)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = @"
            UPDATE bitacora.grupo_cobranza 
            SET descripcion = @Descripcion 
            WHERE id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Descripcion = nuevaDescripcion, Id = id });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = "DELETE FROM bitacora.grupo_cobranza WHERE id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
