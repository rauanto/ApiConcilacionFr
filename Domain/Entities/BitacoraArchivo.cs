namespace ApiConcilacionFr.Domain.Entities;

public class BitacoraArchivo
{
    public int Id { get; set; }
    public int BitacoraId { get; set; }
    public string Tipo { get; set; } = string.Empty;       // GRABACION | EVIDENCIA
    public string Url { get; set; } = string.Empty;
    public string NombreOriginal { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
