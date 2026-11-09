namespace ApiConcilacionFr.Core.Interfaces;

public interface IReporteContabilidadExcelService
{
    Task<byte[]> GenerarReporteExcelAsync(
        int mes,
        string etapa,
        IReadOnlyCollection<int>? sGrupos,
        bool indice);
}
