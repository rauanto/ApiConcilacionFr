namespace ApiConcilacionFr.Domain.Entities;

public class GrupoCobranza
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int UsuarioCreoId { get; set; }
    public string UsuarioCreoNombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCobranza { get; set; }
    public DateTime FechaCreacion { get; set; }
}
