using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ApiConcilacionFr.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly string[] ExtensionesGrabacion =
        [".mp3", ".wav", ".mp4", ".ogg", ".m4a", ".webm", ".avi", ".mov", ".aac"];

    private static readonly string[] ExtensionesEvidencia =
        [".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf"];

    private readonly string _rutaBase;
    private readonly long _tamanoMaxGrabacionBytes;
    private readonly long _tamanoMaxEvidenciaBytes;

    public LocalFileStorageService(IConfiguration configuration, IWebHostEnvironment env)
    {
        var seccion = configuration.GetSection("FileStorage");
        var carpeta = seccion["RutaBase"] ?? "uploads";
        _rutaBase = Path.IsPathRooted(carpeta)
            ? carpeta
            : Path.Combine(env.ContentRootPath, carpeta);

        var mbGrabacion = seccion.GetValue<int>("TamanoMaxGrabacionMb", 100);
        var mbEvidencia = seccion.GetValue<int>("TamanoMaxEvidenciaMb", 20);
        _tamanoMaxGrabacionBytes = mbGrabacion * 1024L * 1024L;
        _tamanoMaxEvidenciaBytes = mbEvidencia * 1024L * 1024L;
    }

    public async Task<string> GuardarGrabacionAsync(IFormFile archivo, int bitacoraId)
    {
        ValidarArchivo(archivo, ExtensionesGrabacion, _tamanoMaxGrabacionBytes, "grabación");

        var subcarpeta = Path.Combine(_rutaBase, "grabaciones");
        return await GuardarAsync(archivo, subcarpeta, bitacoraId, "grabaciones");
    }

    public async Task<string> GuardarEvidenciaAsync(IFormFile archivo, int bitacoraId)
    {
        ValidarArchivo(archivo, ExtensionesEvidencia, _tamanoMaxEvidenciaBytes, "evidencia");

        var subcarpeta = Path.Combine(_rutaBase, "evidencias");
        return await GuardarAsync(archivo, subcarpeta, bitacoraId, "evidencias");
    }

    public void EliminarArchivo(string? rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
            return;

        // rutaRelativa viene como /uploads/grabaciones/filename.mp3
        var rutaFisica = Path.Combine(
            Directory.GetParent(_rutaBase)!.FullName,
            rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(rutaFisica))
            File.Delete(rutaFisica);
    }

    private static void ValidarArchivo(IFormFile archivo, string[] extensionesPermitidas, long tamanoMax, string tipo)
    {
        if (archivo is null || archivo.Length == 0)
            throw new BadRequestException($"Debe proporcionar un archivo de {tipo}.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!extensionesPermitidas.Contains(extension))
            throw new BadRequestException(
                $"Extensión no permitida para {tipo}. Permitidas: {string.Join(", ", extensionesPermitidas)}.");

        if (archivo.Length > tamanoMax)
            throw new BadRequestException(
                $"El archivo de {tipo} supera el tamaño máximo permitido ({tamanoMax / 1024 / 1024} MB).");
    }

    private static async Task<string> GuardarAsync(
        IFormFile archivo, string carpeta, int bitacoraId, string segmento)
    {
        Directory.CreateDirectory(carpeta);

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var nombreArchivo = $"{bitacoraId}_{Guid.NewGuid():N}{extension}";
        var rutaFisica = Path.Combine(carpeta, nombreArchivo);

        await using var stream = new FileStream(rutaFisica, FileMode.Create, FileAccess.Write);
        await archivo.CopyToAsync(stream);

        return $"/uploads/{segmento}/{nombreArchivo}";
    }
}
