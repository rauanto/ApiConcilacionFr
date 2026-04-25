using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Core.Services;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;

namespace ApiConcilacionFr.Infrastructure.Repositories;

public class BitacoraRepository : IBitacoraRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IAuditHelper _auditHelper;

    public BitacoraRepository(IDbConnectionFactory connectionFactory, IAuditHelper auditHelper)
    {
        _connectionFactory = connectionFactory;
        _auditHelper = auditHelper;
    }

    private const string SelectColumns = @"
        id                  AS Id,
        amortizacion_id     AS AmortizacionId,
        credito_id          AS CreditoId,
        cliente_id          AS ClienteId,
        gestor_id           AS GestorId,
        medio_contacto_id   AS MedioContactoId,
        fecha_hora_gestion  AS FechaHoraGestion,
        tipo_gestion        AS TipoGestion,
        sentido             AS Sentido,
        resultado           AS Resultado,
        duracion_segundos   AS DuracionSegundos,
        mensaje_enviado     AS MensajeEnviado,
        asunto              AS Asunto,
        respuesta_cliente   AS RespuestaCliente,
        promesa_fecha_pago  AS PromesaFechaPago,
        promesa_monto       AS PromesaMonto,
        promesa_cumplida    AS PromesaCumplida,
        url_grabacion       AS UrlGrabacion,
        url_evidencia       AS UrlEvidencia,
        observaciones       AS Observaciones,
        geolocalizacion_lat AS GeolocalizacionLat,
        geolocalizacion_lng AS GeolocalizacionLng,
        created_at                  AS CreatedAt,
        fecha_cobro                 AS FechaCobro,
        grupo_id                    AS GrupoId,
        dias_vencidos               AS DiasVencidos,
        cartera_vencida_contable    AS CarteraVencidaContable,
        demanda                     AS Demanda";

    private const string WhereFilters = @"
        WHERE (@CreditoId IS NULL OR credito_id = @CreditoId)
          AND (@ClienteId IS NULL OR cliente_id = @ClienteId)
          AND (@GestorId  IS NULL OR gestor_id  = @GestorId)
          AND (@FechaDesde IS NULL OR fecha_hora_gestion >= @FechaDesde)
          AND (@FechaHasta IS NULL OR fecha_hora_gestion <= @FechaHasta)";

    public async Task<(IEnumerable<Bitacora> Items, int Total)> GetAllAsync(
        BitacoraFiltros filtros, PaginationParams paginacion)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();

        var parameters = new
        {
            filtros.CreditoId,
            filtros.ClienteId,
            filtros.GestorId,
            filtros.FechaDesde,
            filtros.FechaHasta,
            paginacion.PageSize,
            Offset = (paginacion.Page - 1) * paginacion.PageSize
        };

        var countSql = $"SELECT COUNT(*) FROM bitacora.bitacora_gestion {WhereFilters}";
        var dataSql = $@"SELECT {SelectColumns}
                         FROM bitacora.bitacora_gestion {WhereFilters}
                         ORDER BY created_at DESC
                         LIMIT @PageSize OFFSET @Offset";

        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<Bitacora>(dataSql, parameters);

        return (items, total);
    }

    public async Task<Bitacora?> GetByIdAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var sql = $"SELECT {SelectColumns} FROM bitacora.bitacora_gestion WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Bitacora>(sql, new { Id = id });
    }

    public async Task<Bitacora> CreateAsync(Bitacora bitacora)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        const string sql = @"
            INSERT INTO bitacora.bitacora_gestion (
                amortizacion_id, credito_id, cliente_id, gestor_id, medio_contacto_id,
                fecha_hora_gestion, tipo_gestion, sentido, resultado, duracion_segundos,
                mensaje_enviado, asunto, respuesta_cliente, promesa_fecha_pago, promesa_monto,
                url_grabacion, url_evidencia, observaciones, geolocalizacion_lat, geolocalizacion_lng,
                fecha_cobro, grupo_id, dias_vencidos, cartera_vencida_contable, demanda
            ) VALUES (
                @AmortizacionId, @CreditoId, @ClienteId, @GestorId, @MedioContactoId,
                @FechaHoraGestion, @TipoGestion, @Sentido, @Resultado, @DuracionSegundos,
                @MensajeEnviado, @Asunto, @RespuestaCliente, @PromesaFechaPago, @PromesaMonto,
                @UrlGrabacion, @UrlEvidencia, @Observaciones, @GeolocalizacionLat, @GeolocalizacionLng,
                @FechaCobro, @GrupoId, @DiasVencidos, @CarteraVencidaContable, @Demanda
            );
            SELECT LAST_INSERT_ID();";

        var newId = await connection.ExecuteScalarAsync<int>(sql, bitacora);
        return (await GetByIdAsync(newId))!;
    }

    public async Task<Bitacora> UpdateAsync(Bitacora bitacora)
    {
        var estadoAnterior = await GetByIdAsync(bitacora.Id);

        await _auditHelper.ExecuteWithAuditAsync(
            "Bitacora",
            bitacora.Id.ToString(),
            "UPDATE",
            estadoAnterior,
            bitacora,
            async () =>
            {
                using var connection = await _connectionFactory.CreateOpenConnectionAsync();
                const string sql = @"
                    UPDATE bitacora_gestion SET
                        amortizacion_id     = @AmortizacionId,
                        credito_id          = @CreditoId,
                        cliente_id          = @ClienteId,
                        gestor_id           = @GestorId,
                        medio_contacto_id   = @MedioContactoId,
                        fecha_hora_gestion  = @FechaHoraGestion,
                        tipo_gestion        = @TipoGestion,
                        sentido             = @Sentido,
                        resultado           = @Resultado,
                        duracion_segundos   = @DuracionSegundos,
                        mensaje_enviado     = @MensajeEnviado,
                        asunto              = @Asunto,
                        respuesta_cliente   = @RespuestaCliente,
                        promesa_fecha_pago  = @PromesaFechaPago,
                        promesa_monto       = @PromesaMonto,
                        promesa_cumplida    = @PromesaCumplida,
                        url_grabacion       = @UrlGrabacion,
                        url_evidencia       = @UrlEvidencia,
                        observaciones       = @Observaciones,
                        geolocalizacion_lat         = @GeolocalizacionLat,
                        geolocalizacion_lng         = @GeolocalizacionLng,
                        fecha_cobro                 = @FechaCobro,
                        grupo_id                    = @GrupoId,
                        dias_vencidos               = @DiasVencidos,
                        cartera_vencida_contable    = @CarteraVencidaContable,
                        demanda                     = @Demanda
                    WHERE id = @Id";
                await connection.ExecuteAsync(sql, bitacora);
            });

        return (await GetByIdAsync(bitacora.Id))!;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        var rows = await connection.ExecuteAsync(
            "DELETE FROM bitacora.bitacora_gestion WHERE id = @Id",
            new { Id = id });
        return rows > 0;
    }

    public async Task<Bitacora> UpdateUrlGrabacionAsync(int id, string url)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await connection.ExecuteAsync(
            "UPDATE bitacora.bitacora_gestion SET url_grabacion = @Url WHERE id = @Id",
            new { Url = url, Id = id });
        return (await GetByIdAsync(id))!;
    }

    public async Task<Bitacora> UpdateUrlEvidenciaAsync(int id, string url)
    {
        using var connection = await _connectionFactory.CreateOpenConnectionAsync();
        await connection.ExecuteAsync(
            "UPDATE bitacora.bitacora_gestion SET url_evidencia = @Url WHERE id = @Id",
            new { Url = url, Id = id });
        return (await GetByIdAsync(id))!;
    }
}
