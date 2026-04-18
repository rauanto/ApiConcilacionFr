// Domain/Entities/ReporteCarteraEjecutivo.cs
namespace ApiConcilacionFr.Domain.Entities;

public class ReporteCarteraEjecutivoHistorico
{
    public string? ejecutivo_asignado { get; set; }
    public int? total_clientes { get; set; }
    public int? total_prestamos { get; set; }
    public double? monto_colocado { get; set; }
    public double? saldo_cartera { get; set; }
    public double? capital_vencido { get; set; }
    public double? saldo_final { get; set; }
    public double? porcentaje_calidad { get; set; }

    public int Id { get; set; }
    public DateTime fecha_corte { get; set; }
    public DateTime fecha_registro { get; set; }
}
