using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IBitacoraBajasRepository
{
    Task<(IEnumerable<BitacoraBajas> Items, int TotalRecords)> GetAllAsync(int limit, int offset,string grupos);
    Task<long> CreateAsync(BitacoraBajas bitacoraBaja);
    Task<BitacoraBajas?> GetByIdAsync(long id);
    Task<BitacoraBajas?> UpdateAsync(long creditoId, int baja, string? obervaciones);
}
