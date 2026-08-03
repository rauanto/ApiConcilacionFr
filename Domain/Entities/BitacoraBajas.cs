namespace ApiConcilacionFr.Domain.Entities;

public class BitacoraBajas
{
    public long Id { get; set; }
    public long CreditoId { get; set; }
    public long ClienteId { get; set; }
    public double MontoOtorgado { get; set; }
    public double SaldoCapital { get; set; }
    public double SaldoInsoluto { get; set; }
    public double CapitalVencido { get; set; }
    public int AmorticacionesVencidas { get; set; }
    public double InteresCobrado { get; set; }
    public string DiasVencidos { get; set; } = string.Empty;
    public string CarteraVencidaContable { get; set; } = string.Empty;
    public string Demanda { get; set; } = string.Empty;
    public string Estatus { get; set; } = string.Empty;
    public DateTime FechaAlta { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime InicioCobranza { get; set; }
    public DateTime UltCobranza { get; set; }
    public long GestorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? GrupoId { get; set; }
    public string? Obervaciones { get; set; }
    public int? Baja { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string? Sindicato { get; set; } = string.Empty;
    public DateOnly? FechaRealBaja { get; set; }
}
