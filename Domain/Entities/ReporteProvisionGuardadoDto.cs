using System;

namespace ApiConcilacionFr.Domain.Entities
{
    public class ReporteProvisionGuardadoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Estatus { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string ListaPrestamos { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
