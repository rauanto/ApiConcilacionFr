namespace ApiConcilacionFr.Core.Interfaces;

using ApiConcilacionFr.Domain.Entities;

public interface IGrupoCobranzaCreditoRepository
{
    Task<bool> CreateAsync(GrupoCobranzaCredito asignacion);
    Task<bool> DeleteAsync(int id);
    Task<GrupoCobranza?> GetGrupoByCreditoIdAsync(int creditoId);
}
