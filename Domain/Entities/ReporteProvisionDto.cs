namespace ApiConcilacionFr.Domain.Entities
{
    public class ReporteProvisionDto
    {
        public string? S_GRUPO { get; set; }
        public string? GRUPO { get; set; }
        public string? PRESTAMO { get; set; }
        public DateOnly? CREDITO_OTORGADO { get; set; }
        public DateOnly? CREDITO_LIQUIDADO { get; set; }
        public int? AMORTIZA_NUMERO { get; set; }
        public DateOnly? AMORTIZA_INICIO { get; set; }
        public DateOnly? AMORTIZA_VENCIMIENTO { get; set; }
        public string? S_CLAVE { get; set; }
        public string? S_NOMBRE { get; set; }
        public int? PRIMERA_AMORTIZACION { get; set; }
        public decimal? IMPORTE_AMORTIZACION { get; set; }
        public decimal? ABONADO { get; set; }
        public decimal? SALDO_AMORTIZACION { get; set; }
        public int? DIAS_ATRASO { get; set; }
        public int? MESES_ATRASO { get; set; }
        public DateOnly? ULTIMO_PAGO { get; set; }
        public int? DIAS_SIN_PAGAR { get; set; }
        public string? PAGO_SOSTENIDO { get; set; }
    }
}
