using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces
{
    public interface IReporteContabilidadRepository
    {
        Task<IEnumerable<ReporteContabilidad>> ListarReportesContabilidadAsync(string sufijoMes);
        Task<IEnumerable<ReporteContabilidad>> ListarReportesContabilidadPorEtapaAsync(
            string sufijoMes,
            string etapa,
            IReadOnlyCollection<int>? sGrupos);
    }
}