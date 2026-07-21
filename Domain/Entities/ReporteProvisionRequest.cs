using System.Collections.Generic;

namespace ApiConcilacionFr.Domain.Entities
{
    public class ReporteProvisionRequest
    {
        // En lugar de un solo string grande, podemos recibir una lista de strings para mejor tipado en JSON,
        // y luego en el backend la unimos con string.Join(",", ListaPrestamos).
        // Sin embargo, si el usuario manda directamente un string delimitado por comas, usaremos un string.
        public List<string> ListaPrestamos { get; set; } = new List<string>();
    }
}
