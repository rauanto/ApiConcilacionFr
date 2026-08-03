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

        public async Task<int> GuardarReporteAsync(ReporteGuardadoRequest request, string listaPrestamosStr, IEnumerable<ReporteProvisionDto> detalles)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sqlCabecera = @"
                    INSERT INTO bitacora.ReporteProvisionGuardado (Nombre, Estatus, FechaInicio, FechaFin, ListaPrestamos, FechaCreacion)
                    VALUES (@Nombre, @Estatus, @FechaInicio, @FechaFin, @ListaPrestamos, NOW());
                    SELECT LAST_INSERT_ID();";

                int reporteId = await connection.ExecuteScalarAsync<int>(sqlCabecera, new
                {
                    request.Nombre,
                    request.Estatus,
                    request.FechaInicio,
                    request.FechaFin,
                    ListaPrestamos = listaPrestamosStr
                }, transaction);

                if (detalles != null && detalles.Any())
                {
                    var sqlDetalle = @"
                        INSERT INTO bitacora.ReporteProvisionDetalleGuardado (
                            ReporteId, S_GRUPO, GRUPO, PRESTAMO, CREDITO_OTORGADO, CREDITO_LIQUIDADO, 
                            AMORTIZA_NUMERO, AMORTIZA_INICIO, AMORTIZA_VENCIMIENTO, S_CLAVE, S_NOMBRE, 
                            PRIMERA_AMORTIZACION, IMPORTE_AMORTIZACION, ABONADO, SALDO_AMORTIZACION, 
                            DIAS_ATRASO, MESES_ATRASO, ULTIMO_PAGO, DIAS_SIN_PAGAR, PAGO_SOSTENIDO
                        ) VALUES (
                            @ReporteId, @S_GRUPO, @GRUPO, @PRESTAMO, @CREDITO_OTORGADO, @CREDITO_LIQUIDADO, 
                            @AMORTIZA_NUMERO, @AMORTIZA_INICIO, @AMORTIZA_VENCIMIENTO, @S_CLAVE, @S_NOMBRE, 
                            @PRIMERA_AMORTIZACION, @IMPORTE_AMORTIZACION, @ABONADO, @SALDO_AMORTIZACION, 
                            @DIAS_ATRASO, @MESES_ATRASO, @ULTIMO_PAGO, @DIAS_SIN_PAGAR, @PAGO_SOSTENIDO
                        );";
                    
                    var pDetalles = detalles.Select(d => new
                    {
                        ReporteId = reporteId,
                        d.S_GRUPO, d.GRUPO, d.PRESTAMO, d.CREDITO_OTORGADO, d.CREDITO_LIQUIDADO,
                        d.AMORTIZA_NUMERO, d.AMORTIZA_INICIO, d.AMORTIZA_VENCIMIENTO, d.S_CLAVE, d.S_NOMBRE,
                        d.PRIMERA_AMORTIZACION, d.IMPORTE_AMORTIZACION, d.ABONADO, d.SALDO_AMORTIZACION,
                        d.DIAS_ATRASO, d.MESES_ATRASO, d.ULTIMO_PAGO, d.DIAS_SIN_PAGAR, d.PAGO_SOSTENIDO
                    }).ToList();

                    await connection.ExecuteAsync(sqlDetalle, pDetalles, transaction);
                }

                transaction.Commit();
                return reporteId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<ReporteProvisionDto>> ObtenerReporteGuardadoAsync(int reporteId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT 
                    S_GRUPO, GRUPO, PRESTAMO, CREDITO_OTORGADO, CREDITO_LIQUIDADO, 
                    AMORTIZA_NUMERO, AMORTIZA_INICIO, AMORTIZA_VENCIMIENTO, S_CLAVE, S_NOMBRE, 
                    PRIMERA_AMORTIZACION, IMPORTE_AMORTIZACION, ABONADO, SALDO_AMORTIZACION, 
                    DIAS_ATRASO, MESES_ATRASO, ULTIMO_PAGO, DIAS_SIN_PAGAR, PAGO_SOSTENIDO
                FROM bitacora.ReporteProvisionDetalleGuardado
                WHERE ReporteId = @ReporteId;";
            
            return await connection.QueryAsync<ReporteProvisionDto>(sql, new { ReporteId = reporteId });
        }

        public async Task<IEnumerable<ReporteProvisionGuardadoDto>> ListarReportesGuardadosAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var sql = @"
                SELECT Id, Nombre, Estatus, FechaInicio, FechaFin, ListaPrestamos, FechaCreacion
                FROM bitacora.ReporteProvisionGuardado
                ORDER BY FechaCreacion DESC;";
                
            return await connection.QueryAsync<ReporteProvisionGuardadoDto>(sql);
        }
    }
}
