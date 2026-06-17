using System.ComponentModel.DataAnnotations;

namespace ApiConcilacionFr.Common;

public class CreateRolRequest
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio")]
    [MaxLength(50)]
    public string Nombre { get; set; } = null!;

    [MaxLength(255)]
    public string? Descripcion { get; set; }
}

public class UpdateRolRequest : CreateRolRequest
{
    public bool Activo { get; set; }
}

public class AssignPermisosRequest
{
    [Required]
    public List<int> PermisosIds { get; set; } = new();
}

public class AssignRolesRequest
{
    [Required]
    public List<int> RolesIds { get; set; } = new();
}
