using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Services;

public class SolicitudBajaService : ISolicitudBajaService
{
    private readonly ISolicitudBajaRepository _repository;
    private readonly INotificationService _notificationService;

    public SolicitudBajaService(ISolicitudBajaRepository repository, INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<ReporteCartera>> GetListaBajasAsync()
    {
        return await _repository.GetListaBajasAsync();
    }

    public async Task<IEnumerable<SolicitudBajaResponse>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToResponse);
    }

    public async Task<SolicitudBajaResponse?> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToResponse(entity);
    }

    public async Task<SolicitudBajaResponse> CreateAsync(CreateSolicitudBajaRequest request, int solicitanteId)
    {
        var entity = new SolicitudBaja
        {
            CreditoId = request.CreditoId,
            ClienteId = request.ClienteId,
            Obervaciones = request.Obervaciones,
            Baja = 1, // Default por bd
            Solicitante = solicitanteId,
            GrupoId = request.GrupoId,
            NombreCliente = request.NombreCliente,
            CreatedAt = DateTime.UtcNow
        };

        var newId = await _repository.CreateAsync(entity);
        entity.Id = newId;

        var response = MapToResponse(entity);

        // Notificar creación vía SignalR
        await _notificationService.SendNotificationAsync($"Nueva solicitud de baja creada para el crédito {request.CreditoId}");

        return response;
    }

    public async Task<SolicitudBajaResponse?> UpdateAsync(long id, UpdateSolicitudBajaRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        existing.CreditoId = request.CreditoId;
        existing.ClienteId = request.ClienteId;
        existing.Obervaciones = request.Obervaciones;
        if (request.Baja.HasValue) existing.Baja = request.Baja.Value;
        if (request.Solventado.HasValue) existing.Solventado = request.Solventado.Value;
        existing.GrupoId = request.GrupoId;
        existing.NombreCliente = request.NombreCliente;

        var updated = await _repository.UpdateAsync(existing);
        if (updated)
        {
            // Notificar actualización vía SignalR
            await _notificationService.SendNotificationAsync($"Solicitud de baja {id} actualizada");
            return MapToResponse(existing);
        }

        return null;
    }

    public async Task<SolicitudBajaResponse?> PatchAsync(long id, PatchSolicitudBajaRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return null;

        var updated = await _repository.PatchAsync(id, request);
        if (updated)
        {
            // Reload para respuesta completa
            existing = await _repository.GetByIdAsync(id);
            if (existing != null)
            {
                await _notificationService.SendNotificationAsync($"Solicitud de baja {id} modificada");
                return MapToResponse(existing);
            }
        }
        
        return null;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static SolicitudBajaResponse MapToResponse(SolicitudBaja entity)
    {
        return new SolicitudBajaResponse
        {
            Id = entity.Id,
            CreditoId = entity.CreditoId,
            ClienteId = entity.ClienteId,
            Obervaciones = entity.Obervaciones,
            Baja = entity.Baja,
            CreatedAt = entity.CreatedAt,
            Solicitante = entity.Solicitante,
            Solventado = entity.Solventado,
            GrupoId = entity.GrupoId,
            NombreCliente = entity.NombreCliente
        };
    }
}
