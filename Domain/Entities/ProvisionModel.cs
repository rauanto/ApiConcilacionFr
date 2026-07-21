namespace ApiConcilacionFr.Domain.Entities
{
    public class ProvisionModel
    {
        public string? Acreditado { get; set; }
        public string? Nombre_Cliente { get; set; }
        public string? Tramite { get; set; }
        public string? Descripcion_Prestamo { get; set; }
        public double? Amortizacion { get; set; }
        public string? Fecha_Operacion { get; set; }
        public string? Fecha_Vencimiento { get; set; }
        public string? Fecha_Liquidacion { get; set; }
        public double? Dias_Ope { get; set; }
        public double? Tasa_Normal { get; set; }
        public double? Saldo_Promedio { get; set; }
        public double? Interes_Ordinario { get; set; }
        public double? IVA { get; set; }
        public double? Total { get; set; }
        public string? Observaciones { get; set; }
    }
}
