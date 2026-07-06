namespace ApiConcilacionFr.Domain.Entities;

public class SolicitudBaja
{
    public long Id { get; set; }
    public long CreditoId { get; set; }
    public long ClienteId { get; set; }
    public string? Obervaciones { get; set; }
    public long? Baja { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Solicitante { get; set; }
    public int? Solventado { get; set; }
    public int? GrupoId { get; set; }
    public string? NombreCliente { get; set; }
}
