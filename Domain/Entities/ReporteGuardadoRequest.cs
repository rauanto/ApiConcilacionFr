using System;

namespace ApiConcilacionFr.Domain.Entities
{
    public class ReporteGuardadoRequest : ReporteProvisionRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public bool Estatus { get; set; } = true;
    }
}
