using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces
{
    public interface IProvisionService
    {
        byte[] ProcesarReporteProvision(IFormFile file);
        Task<IEnumerable<ReporteProvisionDto>> ObtenerReporteProvisionesAsync(ReporteProvisionRequest request);
        Task<ReporteProvisionFiltrosDto> ObtenerFiltrosReporteProvisionesAsync(ReporteProvisionRequest request);
        Task<int> GenerarYGuardarReporteAsync(ReporteGuardadoRequest request);
        Task<IEnumerable<ReporteProvisionDto>> ObtenerReporteGuardadoAsync(int reporteId);
        Task<IEnumerable<ReporteProvisionGuardadoDto>> ListarReportesGuardadosAsync();
    }
}
