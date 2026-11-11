// Infrastructure/Repositories/ReporteRepository.cs

using System.Data;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories;


public class ReporteLavadoRepository : IReporteLavadoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReporteLavadoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<OperacionesInusuales>> ListarReportesLavadoAsync(string fechaInicio, string fechaFin)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "creditos_fincrece.sp_obtener_reportesLavado";
        return await connection.QueryAsync<OperacionesInusuales>(
            sql, 
            new
            { 
                p_fecha_inicio = fechaInicio,
                p_fecha_fin = fechaFin 
            },
            commandType: CommandType.StoredProcedure
            );
    }
}