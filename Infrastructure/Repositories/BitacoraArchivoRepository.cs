using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories;

public class BitacoraArchivoRepository : IBitacoraArchivoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BitacoraArchivoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private const string SelectColumns = @"
        id              AS Id,
        bitacora_id     AS BitacoraId,
        tipo            AS Tipo,
        url             AS Url,
        nombre_original AS NombreOriginal,
        created_at      AS CreatedAt";

    public async Task<IEnumerable<BitacoraArchivo>> GetByBitacoraIdAsync(int bitacoraId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QueryAsync<BitacoraArchivo>(
            $"SELECT {SelectColumns} FROM bitacora.bitacora_archivos WHERE bitacora_id = @BitacoraId ORDER BY created_at ASC",
            new { BitacoraId = bitacoraId });
    }

    public async Task<IEnumerable<BitacoraArchivo>> GetByBitacoraIdsAsync(IEnumerable<int> bitacoraIds)
    {
        var ids = bitacoraIds.ToList();
        if (ids.Count == 0) return [];

        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QueryAsync<BitacoraArchivo>(
            $"SELECT {SelectColumns} FROM bitacora.bitacora_archivos WHERE bitacora_id IN @Ids ORDER BY created_at ASC",
            new { Ids = ids });
    }

    public async Task<BitacoraArchivo?> GetByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<BitacoraArchivo>(
            $"SELECT {SelectColumns} FROM bitacora.bitacora_archivos WHERE id = @Id",
            new { Id = id });
    }

    public async Task<BitacoraArchivo> AddAsync(BitacoraArchivo archivo)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = @"
            INSERT INTO bitacora.bitacora_archivos (bitacora_id, tipo, url, nombre_original, created_at)
            VALUES (@BitacoraId, @Tipo, @Url, @NombreOriginal, @CreatedAt);
            SELECT LAST_INSERT_ID();";

        var newId = await connection.ExecuteScalarAsync<int>(sql, archivo);
        return (await GetByIdAsync(newId))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var rows = await connection.ExecuteAsync(
            "DELETE FROM bitacora.bitacora_archivos WHERE id = @Id",
            new { Id = id });
        return rows > 0;
    }
}
