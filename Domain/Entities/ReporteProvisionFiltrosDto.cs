using System;
using System.Collections.Generic;

namespace ApiConcilacionFr.Domain.Entities
{
    public class RangoNumeroFiltro
    {
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
    }

    public class RangoFechaFiltro
    {
        public DateOnly? Min { get; set; }
        public DateOnly? Max { get; set; }
    }

    public class CatalogoFiltro
    {
        public List<string> Valores { get; set; } = new List<string>();
    }

    public class ReporteProvisionFiltrosDto
    {
        public CatalogoFiltro S_GRUPO { get; set; } = new CatalogoFiltro();
        public CatalogoFiltro GRUPO { get; set; } = new CatalogoFiltro();
        public RangoNumeroFiltro PRESTAMO { get; set; } = new RangoNumeroFiltro();
        public RangoFechaFiltro CREDITO_OTORGADO { get; set; } = new RangoFechaFiltro();
        public RangoFechaFiltro CREDITO_LIQUIDADO { get; set; } = new RangoFechaFiltro();
        public RangoNumeroFiltro AMORTIZA_NUMERO { get; set; } = new RangoNumeroFiltro();
        public RangoFechaFiltro AMORTIZA_INICIO { get; set; } = new RangoFechaFiltro();
        public RangoFechaFiltro AMORTIZA_VENCIMIENTO { get; set; } = new RangoFechaFiltro();
        public RangoNumeroFiltro S_CLAVE { get; set; } = new RangoNumeroFiltro();
        public RangoNumeroFiltro PRIMERA_AMORTIZACION { get; set; } = new RangoNumeroFiltro();
        public RangoNumeroFiltro IMPORTE_AMORTIZACION { get; set; } = new RangoNumeroFiltro();
        public RangoNumeroFiltro ABONADO { get; set; } = new RangoNumeroFiltro();
        public RangoNumeroFiltro SALDO_AMORTIZACION { get; set; } = new RangoNumeroFiltro();
        public RangoNumeroFiltro DIAS_ATRASO { get; set; } = new RangoNumeroFiltro();
        public RangoNumeroFiltro MESES_ATRASO { get; set; } = new RangoNumeroFiltro();
        public RangoFechaFiltro ULTIMO_PAGO { get; set; } = new RangoFechaFiltro();
        public RangoNumeroFiltro DIAS_SIN_PAGAR { get; set; } = new RangoNumeroFiltro();
        public CatalogoFiltro PAGO_SOSTENIDO { get; set; } = new CatalogoFiltro();
    }
}
