using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface ISocioRepository
{
    Task<IEnumerable<DatosSocio>> GetDatosSocioByClaveAsync(long sClave);
}
