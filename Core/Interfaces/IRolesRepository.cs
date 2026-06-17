using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IRolesRepository
{
    Task<Rol?> GetRolByIdAsync(int id);
    Task<Rol?> GetRolByNombreAsync(string nombre);
    Task<IEnumerable<Rol>> GetAllRolesAsync();
    
    Task<int> CreateRolAsync(Rol rol);
    Task<bool> UpdateRolAsync(Rol rol);
    
    // Asignaciones
    Task AsignarPermisosARolAsync(int rolId, IEnumerable<int> permisosIds);
    Task AsignarRolesAUsuarioAsync(int usuarioId, IEnumerable<int> rolesIds);
    Task RemoverRolDeUsuarioAsync(int usuarioId, int rolId);
    
    // Permisos
    Task<IEnumerable<Permiso>> GetAllPermisosAsync();
    Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(int rolId);
    Task<IEnumerable<Rol>> GetRolesByUsuarioIdAsync(int usuarioId);
}
