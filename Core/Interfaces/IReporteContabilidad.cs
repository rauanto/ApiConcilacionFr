using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces
{
    public interface IReporteContabilidadRepository
    {
        Task<IEnumerable<ReporteContabilidad>> ListarReportesContabilidadGuardadosAsync();
    }
}