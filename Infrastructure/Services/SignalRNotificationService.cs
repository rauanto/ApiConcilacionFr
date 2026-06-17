using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ApiConcilacionFr.Infrastructure.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub, INotificationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyBitacoraCreatedAsync(BitacoraNotificationPayload payload)
    {
        // Notificar a todos los clientes conectados
        // (Podría enviarse solo a ciertos grupos, como administradores o gestores específicos)
        await _hubContext.Clients.All.ReceiveBitacoraNotification(payload);
    }
}
