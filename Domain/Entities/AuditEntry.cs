// Domain/Entities/AuditEntry.cs
namespace ApiConcilacionFr.Domain.Entities;

public class AuditEntry
{
    public string Entidad { get; set; } = string.Empty;
    public string EntidadId { get; set; } = string.Empty;
    public string Operacion { get; set; } = string.Empty;
    public object? ValoresAnteriores { get; set; }
    public object? ValoresNuevos { get; set; }
    public string Usuario { get; set; } = string.Empty;
}