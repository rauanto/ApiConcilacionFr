using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using FluentValidation;

namespace ApiConcilacionFr.Core.Services;

public class BitacoraBajasService : IBitacoraBajasService
{
    private readonly IBitacoraBajasRepository _repository;
    private readonly IValidator<CreateBitacoraBajasRequest> _validator;

    public BitacoraBajasService(
        IBitacoraBajasRepository repository,
        IValidator<CreateBitacoraBajasRequest> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<PagedResponse<BitacoraBajasResponse>> GetAllAsync(PaginationParams paginacion, string grupos)
    {
        var offset = (paginacion.Page - 1) * paginacion.PageSize;
        var (items, totalRecords) = await _repository.GetAllAsync(paginacion.PageSize, offset, grupos   );

        var responses = items.Select(MapToResponse).ToList();

        return new PagedResponse<BitacoraBajasResponse>(responses, totalRecords, paginacion);
    }

    public async Task<BitacoraBajasResponse> CreateAsync(CreateBitacoraBajasRequest request, long gestorId)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new BadRequestException(errors);
        }

        var entidad = new BitacoraBajas
        {
            CreditoId = request.CreditoId,
            ClienteId = request.ClienteId,
            MontoOtorgado = request.MontoOtorgado,
            SaldoCapital = request.SaldoCapital,
            SaldoInsoluto = request.SaldoInsoluto,
            CapitalVencido = request.CapitalVencido,
            AmorticacionesVencidas = request.AmorticacionesVencidas,
            InteresCobrado = request.InteresCobrado,
            DiasVencidos = request.DiasVencidos,
            CarteraVencidaContable = request.CarteraVencidaContable,
            Demanda = request.Demanda,
            Estatus = request.Estatus,
            FechaAlta = request.FechaAlta,
            FechaVencimiento = request.FechaVencimiento,
            InicioCobranza = request.InicioCobranza,
            UltCobranza = request.UltCobranza,
            GrupoId = request.GrupoId,
            Obervaciones = request.Obervaciones,
            GestorId = gestorId,
            Baja = request.Baja,
            FechaRealBaja = request.FechaRealBaja,
            CreatedAt = DateTime.UtcNow,
            NombreCliente = request.NombreCliente,
            Sindicato = request.Sindicato
        };

        var id = await _repository.CreateAsync(entidad);
        var created = await _repository.GetByIdAsync(id);

        if (created == null)
            throw new UnprocessableException("Error al recuperar el registro creado.");

        return MapToResponse(created);
    }

    public async Task<BitacoraBajasResponse> UpdateAsync(long creditoId, UpdateBitacoraBajaRequest request)
    {
        var updated = await _repository.UpdateAsync(creditoId, request.Baja, request.Obervaciones, request.FechaRealBaja);

        if (updated == null)
            throw new NotFoundException($"No se encontró ningún registro con CreditoId {creditoId}.");

        return MapToResponse(updated);
    }

    private static BitacoraBajasResponse MapToResponse(BitacoraBajas entidad)
    {
        return new BitacoraBajasResponse(
            entidad.Id,
            entidad.CreditoId,
            entidad.ClienteId,
            entidad.MontoOtorgado,
            entidad.SaldoCapital,
            entidad.SaldoInsoluto,
            entidad.CapitalVencido,
            entidad.AmorticacionesVencidas,
            entidad.InteresCobrado,
            entidad.DiasVencidos,
            entidad.CarteraVencidaContable,
            entidad.Demanda,
            entidad.Estatus,
            entidad.FechaAlta,
            entidad.FechaVencimiento,
            entidad.InicioCobranza,
            entidad.UltCobranza,
            entidad.GestorId,
            entidad.CreatedAt,
            entidad.GrupoId,
            entidad.Obervaciones,
            entidad.Baja,
            entidad.NombreCliente,
            entidad.Sindicato,
            entidad.FechaRealBaja
        );
    }
}
