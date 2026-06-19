namespace ApiConcilacionFr.Domain.Entities;


public class ReporteLiquidadosAcreditados
{
    public string? credito { get; set; }
    public string? cliente { get; set; }
    public int S_GRUPO { get; set; }
    public string? nombre_grupo { get; set; }
    public DateTime PQ_FECHA_LIQUIDACION { get; set; }
    public DateTime PQ_FECHA_OPERACION { get; set; }
    public string? S_NOMBRE { get; set; }
    public string? ejecutivo_asignado { get; set; }
    public double monto_credito { get; set; }
    public double? monto_cobrado_liquidado { get; set; }

}