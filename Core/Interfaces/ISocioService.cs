using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface ISocioService
{
    Task<DatosSocio?> GetDatosSocioAsync(long sClave);
}
