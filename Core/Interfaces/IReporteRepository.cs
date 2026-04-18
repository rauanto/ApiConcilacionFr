// Core/Interfaces/IReporteRepository.cs
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IReporteRepository
{
    Task<IEnumerable<ReporteCartera>> GetCarteraPorGrupoAsync(string grupos);
    Task<IEnumerable<ReporteCarteraEjecutivo>> GetCarteraEjecutivosAsync(int usuarioId, string rol);
    Task<IEnumerable<Amortizacion>> ObtenerAmortizacionAsync(int pqClave);

    Task<IEnumerable<ReporteLiquidadosgrupo>> GetLiquidadosGrupoAsync(DateTime fechaInicio, string rol, int usuarioId);

    Task<IEnumerable<ReporteLiquidadosAcreditados>> GetLiquidadosAcreditadosAsync(DateTime fechaInicio, string rol, int usuarioId, int grupo);

    Task<IEnumerable<ReporteCarteraEjecutivoHistorico>> GetCarteraEjecutivoHistoricoAsync(int mes, int anio, int usuarioId, string rol);

    Task<IEnumerable<ReporteOtorgadosGrupo>> GetOtorgadosGrupoAsync(DateTime fechaInicio, string rol, int usuarioId);

    Task<IEnumerable<ReporteOtorgadosAcreditados>> GetOtorgadosAcreditadosAsync(DateTime fechaInicio, string rol, int usuarioId, int grupo);
}
