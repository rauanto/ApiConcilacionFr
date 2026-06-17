using ApiConcilacionFr.Common;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Interfaces;

public interface IRolesService
{
    Task<IEnumerable<Rol>> GetAllRolesAsync();
    Task<Rol?> GetRolByIdAsync(int id);
    Task<Rol> CreateRolAsync(CreateRolRequest request);
    Task<Rol> UpdateRolAsync(int id, UpdateRolRequest request);
    
    Task AsignarPermisosARolAsync(int rolId, AssignPermisosRequest request);
    Task AsignarRolesAUsuarioAsync(int usuarioId, AssignRolesRequest request);
    Task RemoverRolDeUsuarioAsync(int usuarioId, int rolId);

    Task<IEnumerable<Permiso>> GetAllPermisosAsync();
    Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(int rolId);
    Task<IEnumerable<Rol>> GetRolesByUsuarioIdAsync(int usuarioId);
}
