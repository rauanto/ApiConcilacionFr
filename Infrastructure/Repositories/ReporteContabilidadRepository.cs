using System.Collections.Generic;
using System.Threading.Tasks;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;
using System.Data;


namespace ApiConcilacionFr.Infrastructure.Repositories
{
    public class ReporteContabilidadRepository : IReporteContabilidadRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ReporteContabilidadRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ReporteContabilidad>> ListarReportesContabilidadAsync(string sufijoMes)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string procedimiento = "sp_obtener_saldos_contables";
            return await connection.QueryAsync<ReporteContabilidad>(
                procedimiento,
                new { p_mes = sufijoMes },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<ReporteContabilidad>> ListarReportesContabilidadPorEtapaAsync(
            string sufijoMes,
            string etapa)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string procedimiento = "sp_obtener_saldos_contables_por_etapa";
            return await connection.QueryAsync<ReporteContabilidad>(
                procedimiento,
                new { p_mes = sufijoMes, p_etapa = etapa },
                commandType: CommandType.StoredProcedure);
        }
    }
}