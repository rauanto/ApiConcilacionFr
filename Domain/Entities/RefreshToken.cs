namespace ApiConcilacionFr.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public DateTime Expiracion { get; set; }
    public bool Revocado { get; set; } = false;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
