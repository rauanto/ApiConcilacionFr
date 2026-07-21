using System.Collections.Generic;
using System.Threading.Tasks;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces
{
    public interface IProvisionRepository
    {
        Task<IEnumerable<ReporteProvisionDto>> ObtenerReporteProvisionesAsync(string listaPrestamos);
    }
}
