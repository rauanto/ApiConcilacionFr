// Core/Services/AuditHelper.cs
using Audit.Core;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens; // O System.IdentityModel.Tokens.Jwt
using ApiConcilacionFr.Core.Interfaces;
namespace ApiConcilacionFr.Core.Services;

public class AuditHelper : IAuditHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task ExecuteWithAuditAsync(string entidad, string id, string operacion, 
                                          object? anterior, object? nuevo, 
                                          Func<Task> action)
    {
        // Obtención automática del usuario desde el JWT
        var user = _httpContextAccessor.HttpContext?.User;
        var userName = user?.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value 
                      ?? user?.FindFirst(ClaimTypes.Name)?.Value 
                      ?? "Sistema/Anonimo";

        using (var scope = await AuditScope.CreateAsync(new AuditScopeOptions
        {
            EventType = operacion,
            ExtraFields = new
            {
                Entidad = entidad,
                EntidadId = id,
                Usuario = userName,
                ValoresAnteriores = anterior
            }
        }))
        {
            try
            {
                await action();
                scope.SetCustomField("ValoresNuevos", nuevo);
            }
            catch (Exception)
            {
                scope.Discard();
                throw;
            }
        }
    }
}