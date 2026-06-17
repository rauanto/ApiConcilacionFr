// Core/Interfaces/IReporteRepository.cs
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IReporteRepository
{
    Task<IEnumerable<ReporteCartera>> GetCarteraPorGrupoAsync(string grupos);
    Task<IEnumerable<ReporteCartera>> GetCarteraPorGrupoCobranzaAsync(int grupoCobranzaId);
    Task<IEnumerable<ReporteCarteraEjecutivo>> GetCarteraEjecutivosAsync(int usuarioId, string rol);
    Task<IEnumerable<Amortizacion>> ObtenerAmortizacionAsync(int pqClave);
    Task<IEnumerable<ReporteLiquidadosgrupo>> GetLiquidadosGrupoAsync(DateTime fechaInicio, DateTime? fechaFin, string rol, int usuarioId);
    Task<IEnumerable<ReporteLiquidadosAcreditados>> GetLiquidadosAcreditadosAsync(DateTime fechaInicio, DateTime? fechaFin, string rol, int usuarioId, int grupo);
    Task<IEnumerable<ReporteCarteraEjecutivoHistorico>> GetCarteraEjecutivoHistoricoAsync(DateTime fechaReporte, int usuarioId, string rol,int tipoReporte);
    Task<IEnumerable<ReporteOtorgadosGrupo>> GetOtorgadosGrupoAsync(DateTime fechaInicio, DateTime? fechaFin, string rol, int usuarioId);
    Task<IEnumerable<ReporteOtorgadosAcreditados>> GetOtorgadosAcreditadosAsync(DateTime fechaInicio, DateTime? fechaFin, string rol, int usuarioId, int grupo);
    Task<IEnumerable<ReporteCarteraHisotoricoGrupo>> GetCarteraEjecutivoHistoricoGrupoAsync(DateTime fechaReporte, int usuarioId, string rol,int tipoReporte);
    Task<IEnumerable<ReporteCarteraEjecutivoHistoricoAcreditados>> GetCarteraEjecutivoHistoricoAcreditadosAsync(DateTime fechaReporte, int S_GRUPO,int tipoReporte);
    Task<IEnumerable<string>> GetFechasReporteCarteraHistoricoAsync();
}
