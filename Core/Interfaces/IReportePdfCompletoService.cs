namespace ApiConcilacionFr.Core.Interfaces;

public interface IReportePdfCompletoService
{
    Task<byte[]> GenerarReportePdfAsync(int pqClave, long clienteId);
}
