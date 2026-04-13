// Core/Interfaces/IAuditHelper.cs
namespace ApiConcilacionFr.Core.Services; // Asegúrate de que el namespace coincida

public interface IAuditHelper
{
    // Eliminamos el parámetro 'string usuario' porque ahora se obtiene internamente
    Task ExecuteWithAuditAsync(
        string entidad, 
        string id, 
        string operacion, 
        object? anterior, 
        object? nuevo, 
        Func<Task> action);
}