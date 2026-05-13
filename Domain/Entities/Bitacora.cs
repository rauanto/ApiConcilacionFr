namespace ApiConcilacionFr.Domain.Entities;

public class Bitacora
{
    public int Id { get; set; }
    public int AmortizacionId { get; set; }
    public int CreditoId { get; set; }
    public int ClienteId { get; set; }
    public int GestorId { get; set; }
    public int? MedioContactoId { get; set; }
    public DateTime FechaHoraGestion { get; set; }
    public string TipoGestion { get; set; }
    public string Sentido { get; set; }
    public string Resultado { get; set; }
    public int? DuracionSegundos { get; set; }
    public string? MensajeEnviado { get; set; }
    public string? Asunto { get; set; }
    public string? RespuestaCliente { get; set; }
    public DateTime? PromesaFechaPago { get; set; }
    public decimal? PromesaMonto { get; set; }
    public bool? PromesaCumplida { get; set; }
    public string? Observaciones { get; set; }
    public decimal? GeolocalizacionLat { get; set; }
    public decimal? GeolocalizacionLng { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FechaCobro { get; set; }
    public long? GrupoId { get; set; }
    public string? DiasVencidos { get; set; }
    public string? CarteraVencidaContable { get; set; }
    public string? Demanda { get; set; }
    public string? Estatus { get; set; }
}