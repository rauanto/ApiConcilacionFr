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
}
