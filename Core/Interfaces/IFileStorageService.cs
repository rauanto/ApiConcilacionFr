using Microsoft.AspNetCore.Http;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> GuardarGrabacionAsync(IFormFile archivo, int bitacoraId);
    Task<string> GuardarEvidenciaAsync(IFormFile archivo, int bitacoraId);
    void EliminarArchivo(string? rutaRelativa);
}
