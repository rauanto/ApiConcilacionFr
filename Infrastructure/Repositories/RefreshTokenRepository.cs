using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RefreshTokenRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(RefreshToken token)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = @"
            INSERT INTO autentificacion.RefreshTokens (Token, UsuarioId, Expiracion, Revocado, FechaCreacion)
            VALUES (@Token, @UsuarioId, @Expiracion, @Revocado, @FechaCreacion)";
        await connection.ExecuteAsync(sql, token);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<RefreshToken>(
            "SELECT * FROM autentificacion.RefreshTokens WHERE Token = @Token",
            new { Token = token });
    }

    public async Task RevokeAsync(string token)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await connection.ExecuteAsync(
            "UPDATE autentificacion.RefreshTokens SET Revocado = 1 WHERE Token = @Token",
            new { Token = token });
    }

    public async Task RevokeAllByUserAsync(int usuarioId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await connection.ExecuteAsync(
            "UPDATE autentificacion.RefreshTokens SET Revocado = 1 WHERE UsuarioId = @UsuarioId",
            new { UsuarioId = usuarioId });
    }
}
