namespace ApiConcilacionFr.Core.Interfaces;

public interface IReportePdfSimpleService
{
    Task<byte[]> GenerarReportePdfAsync(int pqClave, long clienteId);
}
