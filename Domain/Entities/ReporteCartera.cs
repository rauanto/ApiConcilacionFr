// Domain/Entities/ReporteCartera.cs
namespace ApiConcilacionFr.Domain.Entities;

public class ReporteCartera
{
    public string? P_NOMBRE { get; set; }
    public int? S_CLAVE { get; set; }
    public int? PQ_CLAVE { get; set; }
    public double? p { get; set; }
    public string? sindicato { get; set; }
    public string? convenio { get; set; }
    public double? importe { get; set; }
    public double? importe2 { get; set; }
    public int? cla_pobla { get; set; }
    public int? G { get; set; }
    public double? resultado { get; set; }
    public double? Interes_cobrado { get; set; }
    public double? PQ_IMPORTE { get; set; }
    public double? PQ_PTOS_TASA_NORMAL { get; set; }
    public int? DIAS_OPERADOS { get; set; }
    public int? MIN_AMORTIZA { get; set; }
    public int? MAX_AMORTIZA { get; set; }
    public int? PQ_NUM_AMORTIZACIONES { get; set; }
    public int? PQ_AMORTIZACION_GRACIA { get; set; }
    public double? PQ_SOBRE_TASA_MORATORIA { get; set; }
    public DateTime? PQ_FECHA_VENCIMIENTO { get; set; }
    public DateTime PQ_FECHA_OPERACION { get; set; }
    public int? PQ_TIPO_CALCULO_INTERES { get; set; }
    public DateTime? A_FECHA_INICIO { get; set; }
    public DateTime? A_FECHA_VENCIMIENTO { get; set; }
    public double? A_SALDO_INSOLUTO { get; set; }
    public int? AMORTIZACIONES_VENCIDAS { get; set; }
    public double? CAPITAL_VENCIDO { get; set; }
    public int? AMORT_X_VENCER { get; set; }
    public double calculo_interes_moratorio { get; set; }
    public string? S_NOMBRE { get; set; }
    public int? PQ_LITIGIO { get; set; }
    public string? estatus { get; set; }
    public string? cartera_vencida_contable { get; set; }
    public string? demanda { get; set; }
    public string? ejecutivo_asignado { get; set; }
    public string? INTERES_BASE { get; set; }
    public string? Mont_int_cobrado { get; set; }
    public string? VENCIDA1 { get; set; }
}
