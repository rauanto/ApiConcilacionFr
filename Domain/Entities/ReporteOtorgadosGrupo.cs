
namespace ApiConcilacionFr.Domain.Entities;
public class ReporteOtorgadosGrupo
{
    public int? S_GRUPO { get; set; }
    public string? nombre_grupo { get; set; }
    public DateTime PQ_FECHA_OPERACION { get; set; }
    public int total_clientes { get; set; }
    public int total_prestamos { get; set; }
    public double monto_otorgado { get; set; }
    public string? ejecutivo_asignado { get; set; }
}