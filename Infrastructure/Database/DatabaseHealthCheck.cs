// Infrastructure/Database/DatabaseHealthCheck.cs
using System.Data;
using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiConcilacionFr.Infrastructure.Database;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseHealthCheck(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();

            var result = await connection.QueryFirstAsync<int>("SELECT 1");

            return result == 1
                ? HealthCheckResult.Healthy("Base de datos OK")
                : HealthCheckResult.Unhealthy("La base de datos no respondió correctamente");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos", ex);
        }
    }
}