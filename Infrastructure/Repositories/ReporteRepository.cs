// Infrastructure/Repositories/ReporteRepository.cs
using System.Data;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories;

public class ReporteRepository : IReporteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ReporteRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ReporteCartera>> GetCarteraPorGrupoAsync(string grupos)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        
        // Dapper maneja el stored procedure automáticamente cuando se le indica CommandType.StoredProcedure
        // Sin embargo, si la base de datos principal de este proc no es creditos_fincrece_conciliacion
        // sino que debes llamarlo incluyendo el nombre del Schema como "fincrece.sp_...", se debe de poner directo
        // En este caso llamamos mediante el método habitual.
        var parametros = new DynamicParameters();
        parametros.Add("grupos_param", grupos); // Deberá pasarse con el nombre correcto de parámetro definido en tu sp, aquí uso raw query por ser más seguro si no sabemos el nombre.

        // Haciendo un Call crudo directo como lo mandaste en el ejemplo:
        var sql = "CALL sp_ReporteCarteraPorGrupo(@Grupos);";
        
        return await connection.QueryAsync<ReporteCartera>(sql, new { Grupos = grupos });
    }

    public async Task<IEnumerable<ReporteCarteraEjecutivo>> GetCarteraEjecutivosAsync(int usuarioId, string rol)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "CALL sp_reporte_cartera_ejecutivo(@UsuarioId, @Rol);";
        return await connection.QueryAsync<ReporteCarteraEjecutivo>(sql, new { UsuarioId = usuarioId, Rol = rol });
    }

    public async Task<IEnumerable<Amortizacion>> ObtenerAmortizacionAsync(int pqClave)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "CALL sp_obtener_amortizacion(@PqClave);";
        return await connection.QueryAsync<Amortizacion>(sql, new { PqClave = pqClave });
    }
}
