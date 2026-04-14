namespace ApiConcilacionFr.Domain.Entities;


public class ReporteLiquidadosgrupo
{
    public int S_GRUPO { get; set; }
    public string? nombre_grupo { get; set; }
    public DateTime PQ_FECHA_LIQUIDACION { get; set; }
    public int total_clientes { get; set; }
    public int total_prestamos { get; set; }
    public double monto_liquidado { get; set; }
    public string? ejecutivo_asignado { get; set; }
}