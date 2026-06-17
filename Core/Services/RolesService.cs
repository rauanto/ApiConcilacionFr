using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;

namespace ApiConcilacionFr.Core.Services;

public class RolesService : IRolesService
{
    private readonly IRolesRepository _rolesRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public RolesService(IRolesRepository rolesRepository, IUsuarioRepository usuarioRepository)
    {
        _rolesRepository = rolesRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<IEnumerable<Rol>> GetAllRolesAsync()
    {
        return await _rolesRepository.GetAllRolesAsync();
    }

    public async Task<Rol?> GetRolByIdAsync(int id)
    {
        return await _rolesRepository.GetRolByIdAsync(id);
    }

    public async Task<Rol> CreateRolAsync(CreateRolRequest request)
    {
        var existing = await _rolesRepository.GetRolByNombreAsync(request.Nombre);
        if (existing != null)
            throw new BadRequestException($"El rol con nombre '{request.Nombre}' ya existe.");

        var rol = new Rol
        {
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        var id = await _rolesRepository.CreateRolAsync(rol);
        rol.Id = id;
        return rol;
    }

    public async Task<Rol> UpdateRolAsync(int id, UpdateRolRequest request)
    {
        var rol = await _rolesRepository.GetRolByIdAsync(id);
        if (rol == null)
            throw new NotFoundException($"No se encontró el rol con ID {id}.");

        // Validar si el nombre nuevo ya está en uso por otro rol
        var existingName = await _rolesRepository.GetRolByNombreAsync(request.Nombre);
        if (existingName != null && existingName.Id != id)
            throw new BadRequestException($"El rol con nombre '{request.Nombre}' ya existe.");

        rol.Nombre = request.Nombre;
        rol.Descripcion = request.Descripcion;
        rol.Activo = request.Activo;

        await _rolesRepository.UpdateRolAsync(rol);
        return rol;
    }

    public async Task AsignarPermisosARolAsync(int rolId, AssignPermisosRequest request)
    {
        var rol = await _rolesRepository.GetRolByIdAsync(rolId);
        if (rol == null)
            throw new NotFoundException($"No se encontró el rol con ID {rolId}.");

        // Validar que los permisos existan
        var allPermisos = (await _rolesRepository.GetAllPermisosAsync()).ToList();
        var validPermisosIds = allPermisos.Select(p => p.Id).ToHashSet();
        
        var invalidPermisos = request.PermisosIds.Where(id => !validPermisosIds.Contains(id)).ToList();
        if (invalidPermisos.Any())
            throw new BadRequestException($"Los siguientes permisos no existen: {string.Join(", ", invalidPermisos)}");

        await _rolesRepository.AsignarPermisosARolAsync(rolId, request.PermisosIds);
    }

    public async Task AsignarRolesAUsuarioAsync(int usuarioId, AssignRolesRequest request)
    {
        var user = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (user == null)
            throw new NotFoundException($"No se encontró el usuario con ID {usuarioId}.");

        // Validar que los roles existan
        var allRoles = (await _rolesRepository.GetAllRolesAsync()).ToList();
        var validRolesIds = allRoles.Select(r => r.Id).ToHashSet();
        
        var invalidRoles = request.RolesIds.Where(id => !validRolesIds.Contains(id)).ToList();
        if (invalidRoles.Any())
            throw new BadRequestException($"Los siguientes roles no existen: {string.Join(", ", invalidRoles)}");

        await _rolesRepository.AsignarRolesAUsuarioAsync(usuarioId, request.RolesIds);
    }

    public async Task RemoverRolDeUsuarioAsync(int usuarioId, int rolId)
    {
        var user = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (user == null)
            throw new NotFoundException($"No se encontró el usuario con ID {usuarioId}.");

        var rol = await _rolesRepository.GetRolByIdAsync(rolId);
        if (rol == null)
            throw new NotFoundException($"No se encontró el rol con ID {rolId}.");

        await _rolesRepository.RemoverRolDeUsuarioAsync(usuarioId, rolId);
    }

    public async Task<IEnumerable<Permiso>> GetAllPermisosAsync()
    {
        return await _rolesRepository.GetAllPermisosAsync();
    }

    public async Task<IEnumerable<Permiso>> GetPermisosByRolIdAsync(int rolId)
    {
        return await _rolesRepository.GetPermisosByRolIdAsync(rolId);
    }

    public async Task<IEnumerable<Rol>> GetRolesByUsuarioIdAsync(int usuarioId)
    {
        return await _rolesRepository.GetRolesByUsuarioIdAsync(usuarioId);
    }
}
