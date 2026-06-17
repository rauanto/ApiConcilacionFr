namespace ApiConcilacionFr.Core.Interfaces;

using ApiConcilacionFr.Domain.Entities;

public interface IGrupoCobranzaRepository
{
    Task<IEnumerable<GrupoCobranza>> GetAllAsync();
    Task<GrupoCobranza?> GetByIdAsync(int id);
    Task<bool> CreateAsync(GrupoCobranza grupo);
    Task<bool> UpdateAsync(GrupoCobranza grupo);
    Task<bool> PatchDescripcionAsync(int id, string nuevaDescripcion);
    Task<bool> DeleteAsync(int id);
}
