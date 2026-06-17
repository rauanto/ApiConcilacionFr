namespace ApiConcilacionFr.Common;

public record BitacoraNotificationPayload(
    int Id,
    int GestorId,
    string TipoGestion,
    string Resultado,
    DateTime CreatedAt
);
