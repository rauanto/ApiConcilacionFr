using ApiConcilacionFr.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ApiConcilacionFr.Infrastructure.Hubs;

// Descomentar el [Authorize] si la conexión al hub requiere token JWT
// [Authorize]
public class NotificationHub : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        // Lógica al conectar (ej. registrar usuario en un grupo si se usa [Authorize])
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Lógica al desconectar
        await base.OnDisconnectedAsync(exception);
    }
}
