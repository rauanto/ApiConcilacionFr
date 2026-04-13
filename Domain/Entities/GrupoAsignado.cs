// Domain/Entities/GrupoAsignado.cs
namespace ApiConcilacionFr.Domain.Entities;

public class GrupoAsignado
{
    public string S_GRUPO { get; set; } = string.Empty;
    public string nombre_grupo { get; set; } = string.Empty;
    public int? usuario_id { get; set; }

    public string? nombre_usuario { get; set; }
}
