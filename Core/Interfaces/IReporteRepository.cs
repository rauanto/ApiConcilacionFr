// Core/Interfaces/IReporteRepository.cs
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IReporteRepository
{
    Task<IEnumerable<ReporteCartera>> GetCarteraPorGrupoAsync(string grupos);
    Task<IEnumerable<ReporteCarteraEjecutivo>> GetCarteraEjecutivosAsync(int usuarioId, string rol);
    Task<IEnumerable<Amortizacion>> ObtenerAmortizacionAsync(int pqClave);
}
