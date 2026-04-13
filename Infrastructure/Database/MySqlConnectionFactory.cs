// Infrastructure/Database/MySqlConnectionFactory.cs
using System.Data;
using MySqlConnector;

namespace ApiConcilacionFr.Infrastructure.Database;

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionString 'DefaultConnection' no encontrada.");
    }

    public IDbConnection CreateConnection()
        => new MySqlConnection(_connectionString);

    public async Task<IDbConnection> CreateOpenConnectionAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}