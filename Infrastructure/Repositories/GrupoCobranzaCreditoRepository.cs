namespace ApiConcilacionFr.Infrastructure.Repositories;

using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using ApiConcilacionFr.Core.Services;
using Dapper;

public class GrupoCobranzaCreditoRepository : IGrupoCobranzaCreditoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IAuditHelper _auditHelper;

    public GrupoCobranzaCreditoRepository(IDbConnectionFactory connectionFactory, IAuditHelper auditHelper)
    {
        _connectionFactory = connectionFactory;
        _auditHelper = auditHelper;
    }

    public async Task<bool> CreateAsync(GrupoCobranzaCredito asignacion)
    {
        int rows = 0;
        await _auditHelper.ExecuteWithAuditAsync(
            "GrupoCobranzaCredito",
            $"{asignacion.GrupoCobranzaId}-{asignacion.CreditoId}",
            "CREATE",
            null,
            asignacion,
            async () =>
            {
                using var connection = await _connectionFactory.CreateOpenConnectionAsync();
                const string sql = @"
                    INSERT INTO bitacora.grupo_cobranza_credito (grupo_cobranza_id, credito_id) 
                    VALUES (@GrupoCobranzaId, @CreditoId)";
                rows = await connection.ExecuteAsync(sql, asignacion);
            });
        return rows > 0;
    }

    public async Task<bool> DeleteByCreditoIdAsync(int creditoId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = "DELETE FROM bitacora.grupo_cobranza_credito WHERE credito_id = @CreditoId";
        var rows = await connection.ExecuteAsync(sql, new { CreditoId = creditoId });
        return rows > 0;
    }

    public async Task<GrupoCobranza?> GetGrupoByCreditoIdAsync(int creditoId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = @"
            SELECT gc.id as Id, gc.nombre as Nombre, gc.usuario_creo_id as UsuarioCreoId, 
                   COALESCE(u.NombreUsuario, 'Desconocido') as UsuarioCreoNombre, 
                   gc.descripcion as Descripcion, gc.fecha_cobranza as FechaCobranza, gc.fecha_creacion as FechaCreacion 
            FROM bitacora.grupo_cobranza gc
            INNER JOIN bitacora.grupo_cobranza_credito gcc ON gc.id = gcc.grupo_cobranza_id
            LEFT JOIN autentificacion.Usuarios u ON gc.usuario_creo_id = u.Id
            WHERE gcc.credito_id = @CreditoId";
        return await connection.QuerySingleOrDefaultAsync<GrupoCobranza>(sql, new { CreditoId = creditoId });
    }
}
