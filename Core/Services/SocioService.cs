using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Services;

public class SocioService : ISocioService
{
    private readonly ISocioRepository _repository;

    public SocioService(ISocioRepository repository)
    {
        _repository = repository;
    }

    public async Task<DatosSocio?> GetDatosSocioAsync(long sClave)
    {
        var socios = await _repository.GetDatosSocioByClaveAsync(sClave);
        var socio = socios.FirstOrDefault();
        if (socio == null)
        {
            throw new NotFoundException($"No se encontró el socio con clave {sClave}");
        }
        return socio;
    }
}
