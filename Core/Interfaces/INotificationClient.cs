using ApiConcilacionFr.Common;

namespace ApiConcilacionFr.Core.Interfaces;

public interface INotificationClient
{
    Task ReceiveBitacoraNotification(BitacoraNotificationPayload payload);
    Task ReceiveNotification(string message);
}
