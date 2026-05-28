using System.Data;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ApiConcilacionFr.Infrastructure.Database;
using Dapper;
using ApiConcilacionFr.Core.Services;

namespace ApiConcilacionFr.Infrastructure.Repositories;

public class BitacoraBajasRepository : IBitacoraBajasRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IAuditHelper _auditHelper;

    public BitacoraBajasRepository(IDbConnectionFactory connectionFactory, IAuditHelper auditHelper)
    {
        _connectionFactory = connectionFactory;
        _auditHelper = auditHelper;
    }

    public async Task<(IEnumerable<BitacoraBajas> Items, int TotalRecords)> GetAllAsync(int limit, int offset, string grupos)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var parameters = new DynamicParameters();
        parameters.Add("p_limit", limit);
        parameters.Add("p_offset", offset);
        parameters.Add("p_lista_grupos", grupos);

        using var multi = await connection.QueryMultipleAsync(
            "bitacora.sp_listar_bitacora_bajas", 
            parameters, 
            commandType: CommandType.StoredProcedure);

        var items = await multi.ReadAsync<BitacoraBajas>();
        var totalRecords = await multi.ReadFirstAsync<int>();

        return (items, totalRecords);
    }

    public async Task<long> CreateAsync(BitacoraBajas bitacoraBaja)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("p_credito_id", bitacoraBaja.CreditoId);
        parameters.Add("p_cliente_id", bitacoraBaja.ClienteId);
        parameters.Add("p_monto_otorgado", bitacoraBaja.MontoOtorgado);
        parameters.Add("p_saldo_capital", bitacoraBaja.SaldoCapital);
        parameters.Add("p_saldo_insoluto", bitacoraBaja.SaldoInsoluto);
        parameters.Add("p_capital_vencido", bitacoraBaja.CapitalVencido);
        parameters.Add("p_amorticaciones_vencidas", bitacoraBaja.AmorticacionesVencidas);
        parameters.Add("p_interes_cobrado", bitacoraBaja.InteresCobrado);
        parameters.Add("p_dias_vencidos", bitacoraBaja.DiasVencidos);
        parameters.Add("p_cartera_vencida_contable", bitacoraBaja.CarteraVencidaContable);
        parameters.Add("p_demanda", bitacoraBaja.Demanda);
        parameters.Add("p_estatus", bitacoraBaja.Estatus);
        parameters.Add("p_fecha_alta", bitacoraBaja.FechaAlta);
        parameters.Add("p_fecha_vencimiento", bitacoraBaja.FechaVencimiento);
        parameters.Add("p_inicio_cobranza", bitacoraBaja.InicioCobranza);
        parameters.Add("p_ult_cobranza", bitacoraBaja.UltCobranza);
        parameters.Add("p_gestor_id", bitacoraBaja.GestorId);
        parameters.Add("p_grupo_id", bitacoraBaja.GrupoId);
        parameters.Add("p_obervaciones", bitacoraBaja.Obervaciones);
        parameters.Add("p_nombre_cliente", bitacoraBaja.NombreCliente);
        parameters.Add("p_sindicato", bitacoraBaja.Sindicato);
        parameters.Add("p_baja", bitacoraBaja.Baja);

        var id = await connection.QuerySingleAsync<long>(
            "bitacora.sp_insertar_bitacora_baja",
            parameters,
            commandType: CommandType.StoredProcedure);

        return id;
    }

    public async Task<BitacoraBajas?> GetByIdAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM bitacora.bitacora_bajas WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<BitacoraBajas>(sql, new { Id = id });
    }

    public async Task<BitacoraBajas?> UpdateAsync(long creditoId, int baja, string? obervaciones)
    {
        var estadoAnterior = await GetByCreditoIdAsync(creditoId);

        BitacoraBajas? resultado = null;

        await _auditHelper.ExecuteWithAuditAsync(
            "BitacoraBajas",
            creditoId.ToString(),
            "UPDATE",
            estadoAnterior,
            new { Baja = baja, Obervaciones = obervaciones },
            async () =>
            {
                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("p_credito_id", creditoId);
                parameters.Add("p_baja", baja);
                parameters.Add("p_obervaciones", obervaciones);

                using var multi = await connection.QueryMultipleAsync(
                    "bitacora.sp_actualizar_bitacora_baja",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var rowsAffected = await multi.ReadFirstAsync<int>();
                if (rowsAffected > 0)
                    resultado = await multi.ReadSingleOrDefaultAsync<BitacoraBajas>();
            });

        return resultado;
    }

    private async Task<BitacoraBajas?> GetByCreditoIdAsync(long creditoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM bitacora.bitacora_bajas WHERE credito_id = @CreditoId LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<BitacoraBajas>(sql, new { CreditoId = creditoId });
    }
}
