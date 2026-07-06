using ApiConcilacionFr.Common;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface ISolicitudBajaRepository
{
    Task<IEnumerable<ReporteCartera>> GetListaBajasAsync();
    Task<IEnumerable<SolicitudBaja>> GetAllAsync();
    Task<SolicitudBaja?> GetByIdAsync(long id);
    Task<long> CreateAsync(SolicitudBaja entity);
    Task<bool> UpdateAsync(SolicitudBaja entity);
    Task<bool> PatchAsync(long id, PatchSolicitudBajaRequest request);
    Task<bool> DeleteAsync(long id);
}

public interface ISolicitudBajaService
{
    Task<IEnumerable<ReporteCartera>> GetListaBajasAsync();
    Task<IEnumerable<SolicitudBajaResponse>> GetAllAsync();
    Task<SolicitudBajaResponse?> GetByIdAsync(long id);
    Task<SolicitudBajaResponse> CreateAsync(CreateSolicitudBajaRequest request, int solicitanteId);
    Task<SolicitudBajaResponse?> UpdateAsync(long id, UpdateSolicitudBajaRequest request);
    Task<SolicitudBajaResponse?> PatchAsync(long id, PatchSolicitudBajaRequest request);
    Task<bool> DeleteAsync(long id);
}
