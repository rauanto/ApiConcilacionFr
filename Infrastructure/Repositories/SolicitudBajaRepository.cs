using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories;

public class SolicitudBajaRepository : ISolicitudBajaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SolicitudBajaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ReporteCartera>> GetListaBajasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        // Llama al procedimiento almacenado
        const string sql = "sp_solicitudBajasLista";
        return await connection.QueryAsync<ReporteCartera>(sql, commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<SolicitudBaja>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM solicitud_baja ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<SolicitudBaja>(sql);
    }

    public async Task<SolicitudBaja?> GetByIdAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM solicitud_baja WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<SolicitudBaja>(sql, new { Id = id });
    }

    public async Task<long> CreateAsync(SolicitudBaja entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO solicitud_baja 
            (CreditoId, ClienteId, Obervaciones, Baja, solicitante, solventado, GrupoId, NombreCliente)
            VALUES 
            (@CreditoId, @ClienteId, @Obervaciones, @Baja, @Solicitante, @Solventado, @GrupoId, @NombreCliente);
            SELECT LAST_INSERT_ID();";

        return await connection.ExecuteScalarAsync<long>(sql, entity);
    }

    public async Task<bool> UpdateAsync(SolicitudBaja entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE solicitud_baja 
            SET CreditoId = @CreditoId,
                ClienteId = @ClienteId,
                Obervaciones = @Obervaciones,
                Baja = @Baja,
                solventado = @Solventado,
                GrupoId = @GrupoId,
                NombreCliente = @NombreCliente
            WHERE id = @Id";

        var affected = await connection.ExecuteAsync(sql, entity);
        return affected > 0;
    }

    public async Task<bool> PatchAsync(long id, PatchSolicitudBajaRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Build dynamic query
        var updateFields = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (request.Obervaciones != null)
        {
            updateFields.Add("Obervaciones = @Obervaciones");
            parameters.Add("Obervaciones", request.Obervaciones);
        }
        if (request.Baja.HasValue)
        {
            updateFields.Add("Baja = @Baja");
            parameters.Add("Baja", request.Baja.Value);
        }
        if (request.Solventado.HasValue)
        {
            updateFields.Add("solventado = @Solventado");
            parameters.Add("Solventado", request.Solventado.Value);
        }

        if (!updateFields.Any()) return true;
        var sql = $"UPDATE solicitud_baja SET {string.Join(", ", updateFields)} WHERE id = @Id";
        var affected = await connection.ExecuteAsync(sql, parameters);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM solicitud_baja WHERE id = @Id";
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
}
