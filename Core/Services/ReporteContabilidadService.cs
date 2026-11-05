using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Services;

public class ReporteContabilidadService : IReporteContabilidadService
{
    private const int MesMinimo = 0;
    private const int MesMaximo = 12;
    private readonly IReporteContabilidadRepository _repository;

    public ReporteContabilidadService(IReporteContabilidadRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<ReporteContabilidad>> ListarReportesContabilidadAsync(int mes)
    {
        var sufijoMes = ConvertirMesASufijo(mes);
        return _repository.ListarReportesContabilidadAsync(sufijoMes);
    }

    public Task<IEnumerable<ReporteContabilidad>> ListarReportesContabilidadPorEtapaAsync(int mes, string etapa)
    {
        var sufijoMes = ConvertirMesASufijo(mes);
        var etapaNormalizada = ConvertirEtapa(etapa);
        return _repository.ListarReportesContabilidadPorEtapaAsync(sufijoMes, etapaNormalizada);
    }

    private static string ConvertirMesASufijo(int mes)
    {
        if (mes < MesMinimo || mes > MesMaximo)
        {
            throw new ArgumentOutOfRangeException(
                nameof(mes),
                mes,
                $"El mes debe estar entre {MesMinimo} y {MesMaximo}.");
        }

        return $"MES{mes}";
    }

    private static string ConvertirEtapa(string etapa)
    {
        if (string.IsNullOrWhiteSpace(etapa))
        {
            throw new BadRequestException("La etapa debe ser 1, 2, 3 o TODAS.");
        }

        var etapaNormalizada = etapa.Trim().ToUpperInvariant();
        if (etapaNormalizada is not ("1" or "2" or "3" or "TODAS"))
        {
            throw new BadRequestException("La etapa debe ser 1, 2, 3 o TODAS.");
        }

        return etapaNormalizada;
    }
}
