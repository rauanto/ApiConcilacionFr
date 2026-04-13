// Infrastructure/Database/IDbConnectionFactory.cs
using System.Data;

namespace ApiConcilacionFr.Infrastructure.Database;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<IDbConnection> CreateOpenConnectionAsync();
}