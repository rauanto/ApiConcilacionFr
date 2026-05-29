namespace ApiConcilacionFr.Domain.Entities;

public class ReporteCarteraEjecutivoHistoricoAcreditados
{
    public int id { get; set; }
    public DateTime fecha_corte { get; set; }
    public DateTime fecha_registro { get; set; }
    public string? ejecutivo_asignado { get; set; }
    public double monto_colocado { get; set; }
    public double saldo_cartera { get; set; }
    public double capital_vencido { get; set; }
    public double saldo_final { get; set; }
    public string? tipo { get; set; }
    public int s_grupo { get; set; }
    public string? nombre_grupo { get; set; }
    
    public string? S_NOMBRE { get; set; }
    public int cliente { get; set; }
    public int credito { get; set; }
    

    public int? id_usuario { get; set; }
}
