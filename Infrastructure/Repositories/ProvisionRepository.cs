using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories
{
    public class ProvisionRepository : IProvisionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProvisionRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<ReporteProvisionDto>> ObtenerReporteProvisionesAsync(string listaPrestamos, DateTime? fechaInicio, DateTime? fechaFin)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = "CALL sp_ReporteProvicion(@p_lista_prestamos, @p_fecha_inicio, @p_fecha_fin);";
            
            var result = await connection.QueryAsync<ReporteProvisionDto>(sql, new 
            { 
                p_lista_prestamos = listaPrestamos,
                p_fecha_inicio = fechaInicio,
                p_fecha_fin = fechaFin
            });
            
            return result;
        }
    }
}
