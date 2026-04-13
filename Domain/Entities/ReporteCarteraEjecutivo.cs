// Domain/Entities/ReporteCarteraEjecutivo.cs
namespace ApiConcilacionFr.Domain.Entities;

public class ReporteCarteraEjecutivo
{
    public string? ejecutivo_asignado { get; set; }
    public int? total_clientes { get; set; }
    public int? total_prestamos { get; set; }
    public double? monto_colocado { get; set; }
    public double? saldo_cartera { get; set; }
    public double? capital_vencido { get; set; }
    public double? saldo_final { get; set; }
    public double? porcentaje_calidad { get; set; }
}
