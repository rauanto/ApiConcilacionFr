using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;
using System.Data;
namespace ApiConcilacionFr.Infrastructure.Repositories;

public class SocioRepository : ISocioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SocioRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<DatosSocio>> GetDatosSocioByClaveAsync(long sClave)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();

        var sql = "CALL sp_ObtenerDatosSocio(@S_CLAVE);";
        
        return await connection.QueryAsync<DatosSocio>(sql, new { S_CLAVE = sClave });
    }
}
