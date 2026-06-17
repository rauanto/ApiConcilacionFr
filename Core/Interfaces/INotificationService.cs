using ApiConcilacionFr.Common;

namespace ApiConcilacionFr.Core.Interfaces;

public interface INotificationService
{
    Task NotifyBitacoraCreatedAsync(BitacoraNotificationPayload payload);
}
