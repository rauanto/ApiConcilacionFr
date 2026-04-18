
namespace ApiConcilacionFr.Domain.Entities;
public class ReporteOtorgadosAcreditados
{
    public int? credito { get; set; }
    public string? PQ_FECHA_OPERACION { get; set; }
    public int? cliente { get; set; }
    public int? S_GRUPO { get; set; }

    public double monto_otorgado { get; set; }

    public string? S_NOMBRE { get; set; }
    public string? ejecutivo_asignado { get; set; }
}