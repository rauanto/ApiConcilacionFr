namespace ApiConcilacionFr.Core.Interfaces;

public interface IReporteContabilidadPdfService
{
    Task<byte[]> GenerarReportePdfAsync(
        int mes,
        string etapa,
        IReadOnlyCollection<int>? sGrupos,
        bool indice);
}
