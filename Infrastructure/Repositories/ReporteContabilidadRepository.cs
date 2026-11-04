using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;


namespace ApiConcilacionFr.Infrastructure.Repositories
{
    public class ReporteContabilidadRepository : IReporteContabilidadRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ReporteContabilidadRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        //Listar reportes de contabilidad guardados
        public async Task<IEnumerable<ReporteContabilidad>> ListarReportesContabilidadGuardadosAsync()
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            
            const string sql = "call sp_obtener_saldos_contables();";
            
            var result = await connection.QueryAsync<ReporteContabilidad>(sql);
            return result.AsList();
        }
    }
}