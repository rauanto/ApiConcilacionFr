using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IBitacoraArchivoRepository
{
    Task<IEnumerable<BitacoraArchivo>> GetByBitacoraIdAsync(int bitacoraId);
    Task<IEnumerable<BitacoraArchivo>> GetByBitacoraIdsAsync(IEnumerable<int> bitacoraIds);
    Task<BitacoraArchivo?> GetByIdAsync(int id);
    Task<BitacoraArchivo> AddAsync(BitacoraArchivo archivo);
    Task<bool> DeleteAsync(int id);
}
