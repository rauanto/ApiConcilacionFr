using ApiConcilacionFr.Common;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public record BitacoraFiltros(
    int? CreditoId = null,
    int? ClienteId = null,
    int? GestorId = null,
    DateTime? FechaDesde = null,
    DateTime? FechaHasta = null
);

public interface IBitacoraRepository
{
    Task<(IEnumerable<Bitacora> Items, int Total)> GetAllAsync(BitacoraFiltros filtros, PaginationParams paginacion);
    Task<Bitacora?> GetByIdAsync(int id);
    Task<Bitacora> CreateAsync(Bitacora bitacora);
    Task<Bitacora> UpdateAsync(Bitacora bitacora);
    Task<bool> DeleteAsync(int id);
}
