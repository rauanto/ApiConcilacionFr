namespace ApiConcilacionFr.Core.Interfaces;

public interface IReportePdfCompletoService
{
    Task<byte[]> GenerarReportePdfAsync(int pqClave, long clienteId);
    Task<byte[]> GenerarReportePdfCuotaAsync(int pqClave, long clienteId, int aNumero);
}
