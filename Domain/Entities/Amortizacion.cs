namespace ApiConcilacionFr.Domain.Entities;

public class Amortizacion
{
    public int A_NUMERO { get; set; }
    public string? FECHA_VENCIMIENTO { get; set; }
    public string? CAPITAL { get; set; }
    public string? SALDO_INSOLUTO { get; set; }
    public string? IMPORTE { get; set; }
    public string? INTERES { get; set; }
    public string? IVA { get; set; }
    public string? TOTAL { get; set; }
    public string? Estado { get; set; }
}
