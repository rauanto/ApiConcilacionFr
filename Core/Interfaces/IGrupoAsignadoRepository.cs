// Core/Interfaces/IGrupoAsignadoRepository.cs
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IGrupoAsignadoRepository
{
    Task<IEnumerable<GrupoAsignado>> GetAllAsync();
    Task<IEnumerable<GrupoAsignado>> GetByUsuarioIdAsync(int usuarioId);
    Task<GrupoAsignado?> GetByIdAsync(string sGrupo);
    Task<bool> CreateAsync(GrupoAsignado grupo);
    Task<bool> UpdateAsync(GrupoAsignado grupo);
    Task<bool> PatchNombreAsync(string sGrupo, string nuevoNombre);
    Task<bool> DeleteAsync(string sGrupo);
}
