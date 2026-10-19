using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces
{
    public interface IProvisionRepository
    {
        Task<IEnumerable<ReporteProvisionDto>> ObtenerReporteProvisionesAsync(string listaPrestamos, DateTime? fechaInicio, DateTime? fechaFin, System.Threading.CancellationToken cancellationToken = default);
        Task<int> GuardarReporteAsync(ReporteGuardadoRequest request, string listaPrestamosStr, IEnumerable<ReporteProvisionDto> detalles);
        Task<IEnumerable<ReporteProvisionDto>> ObtenerReporteGuardadoAsync(int reporteId, System.Threading.CancellationToken cancellationToken = default);
        Task<IEnumerable<ReporteProvisionGuardadoDto>> ListarReportesGuardadosAsync();
    }
}
