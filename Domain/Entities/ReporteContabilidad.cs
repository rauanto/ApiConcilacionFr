namespace ApiConcilacionFr.Domain.Entities;

public class ReporteContabilidad
{
    public string CUENTA { get; set; }
    public string C_DESCRIPCION { get; set; }
    public double SALDO { get; set; }
    public decimal SALDO_INSOLUTO { get; set; }
    public decimal DIFERENCIA { get; set; }
    public int S_CLAVE { get; set; }
    public int S_GRUPO { get; set; }
    public int ETAPA { get; set; }
    public string GRUPO { get; set; }

    public string? PRESTAMOS { get; set; }

}