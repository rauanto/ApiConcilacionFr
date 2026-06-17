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


        var parametros = new DynamicParameters();
        parametros.Add("grupos_param", grupos);

        var sql = "CALL sp_ReporteCarteraPorGrupo(@Grupos);";

        return await connection.QueryAsync<ReporteCartera>(sql, new { Grupos = grupos });
    }

    public async Task<IEnumerable<ReporteCartera>> GetCarteraPorGrupoCobranzaAsync(int grupoCobranzaId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "CALL sp_ReporteCarteraPorGrupoCobranza(@GrupoCobranzaId);";
        return await connection.QueryAsync<ReporteCartera>(sql, new { GrupoCobranzaId = grupoCobranzaId });
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

    #region Liquidados por grupo y nombre

    public async Task<IEnumerable<ReporteLiquidadosgrupo>> GetLiquidadosGrupoAsync(DateTime fechaInicio,
        DateTime? fechaFin, string rol, int usuarioId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "CALL sp_reporte_liquidados_grupo(@FechaInicio,@FechaFin,@Rol,@UsuarioId);";
        return await connection.QueryAsync<ReporteLiquidadosgrupo>(sql,
            new { FechaInicio = fechaInicio, FechaFin = fechaFin, Rol = rol, UsuarioId = usuarioId });
    }

    public async Task<IEnumerable<ReporteLiquidadosAcreditados>> GetLiquidadosAcreditadosAsync(DateTime fechaInicio,
        DateTime? fechaFin, string rol, int usuarioId, int grupo)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "CALL sp_reporte_liquidados_grupo_acreditados(@FechaInicio,@FechaFin,@Rol,@UsuarioId,@Grupo);";
        return await connection.QueryAsync<ReporteLiquidadosAcreditados>(sql,
            new { FechaInicio = fechaInicio, FechaFin = fechaFin, Rol = rol, UsuarioId = usuarioId, Grupo = grupo });
    }

    #endregion


    #region Historico Cartera

    public async Task<IEnumerable<ReporteCarteraEjecutivoHistorico>> GetCarteraEjecutivoHistoricoAsync(DateTime fechaReporte,
        int usuarioId, string rol, int tipoReporte)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "CALL sp_consultar_historico_cartera(@p_fecha_reporte, @p_usuario_id, @p_rol,@tipo_reporte);";
        return await connection.QueryAsync<ReporteCarteraEjecutivoHistorico>(sql,
            new { p_fecha_reporte = fechaReporte, p_usuario_id = usuarioId, p_rol = rol, tipo_reporte = tipoReporte });
    }

    public async Task<IEnumerable<ReporteCarteraHisotoricoGrupo>> GetCarteraEjecutivoHistoricoGrupoAsync(DateTime fechaReporte,
        int usuarioId, string rol, int tipoReporte)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "CALL sp_consultar_historico_cartera_grupo(@p_fecha_reporte, @p_usuario_id, @p_rol ,@tipo_reporte);";
        return await connection.QueryAsync<ReporteCarteraHisotoricoGrupo>(sql,
            new { p_fecha_reporte = fechaReporte, p_usuario_id = usuarioId, p_rol = rol, tipo_reporte = tipoReporte });
    }

    public async Task<IEnumerable<ReporteCarteraEjecutivoHistoricoAcreditados>>
        GetCarteraEjecutivoHistoricoAcreditadosAsync(DateTime fechaReporte, int S_GRUPO, int tipoReporte)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql =
            "CALL autentificacion.sp_consultar_historico_cartera_grupo_acreditado(@p_fecha_reporte,@tipo_reporte, @s_grup );";
        return await connection.QueryAsync<ReporteCarteraEjecutivoHistoricoAcreditados>(sql,
            new { p_fecha_reporte = fechaReporte, tipo_reporte = tipoReporte, s_grup = S_GRUPO, });
    }

    public async Task<IEnumerable<string>> GetFechasReporteCarteraHistoricoAsync()
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "SELECT DATE_FORMAT(fecha_corte, '%Y-%m-%d') as fecha_reporte FROM autentificacion.reporte_cartera_historico GROUP BY fecha_reporte ORDER BY fecha_reporte DESC;";
        return await connection.QueryAsync<string>(sql);
    }

    #endregion


    #region Otorgados por grupo y nombre

    public async Task<IEnumerable<ReporteOtorgadosGrupo>> GetOtorgadosGrupoAsync(DateTime fechaInicio,
        DateTime? fechaFin, string rol, int usuarioId)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "CALL sp_reporte_otorgados_grupo(@FechaInicio,@FechaFin,@Rol,@UsuarioId);";
        return await connection.QueryAsync<ReporteOtorgadosGrupo>(sql,
            new { FechaInicio = fechaInicio, FechaFin = fechaFin, Rol = rol, UsuarioId = usuarioId });
    }

    public async Task<IEnumerable<ReporteOtorgadosAcreditados>> GetOtorgadosAcreditadosAsync(DateTime fechaInicio,
        DateTime? fechaFin, string rol, int usuarioId, int grupo)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = "CALL sp_reporte_otorgados_grupo_acreditados(@FechaInicio,@FechaFin,@Rol,@UsuarioId,@Grupo);";
        return await connection.QueryAsync<ReporteOtorgadosAcreditados>(sql,
            new { FechaInicio = fechaInicio, FechaFin = fechaFin, Rol = rol, UsuarioId = usuarioId, Grupo = grupo });
    }

    #endregion
}