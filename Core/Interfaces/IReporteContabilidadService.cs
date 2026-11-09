using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IReporteContabilidadService
{
    Task<IEnumerable<ReporteContabilidad>> ListarReportesContabilidadAsync(int mes);
    Task<IEnumerable<ReporteContabilidad>> ListarReportesContabilidadPorEtapaAsync(
        int mes,
        string etapa,
        IReadOnlyCollection<int>? sGrupos = null);
}
