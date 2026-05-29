using System.Threading.Tasks;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IReportePdfService
{
    Task<byte[]> GenerarReporteAmortizacionesPdfAsync(int pqClave, long clienteId, bool incluirBitacoras = true);
}
