using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using FluentValidation;

namespace ApiConcilacionFr.Core.Services;

public class BitacoraService : IBitacoraService
{
    private readonly IBitacoraRepository _repo;
    private readonly IValidator<CreateBitacoraRequest> _createValidator;
    private readonly IValidator<UpdateBitacoraRequest> _updateValidator;

    public BitacoraService(
        IBitacoraRepository repo,
        IValidator<CreateBitacoraRequest> createValidator,
        IValidator<UpdateBitacoraRequest> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResponse<BitacoraResponse>> GetAllAsync(BitacoraFiltros filtros, PaginationParams paginacion)
    {
        var (items, total) = await _repo.GetAllAsync(filtros, paginacion);
        return new PagedResponse<BitacoraResponse>(items.Select(ToResponse), total, paginacion);
    }

    public async Task<BitacoraResponse?> GetByIdAsync(int id)
    {
        var bitacora = await _repo.GetByIdAsync(id);
        return bitacora is null ? null : ToResponse(bitacora);
    }

    public async Task<BitacoraResponse> CreateAsync(CreateBitacoraRequest request, int gestorId)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new BadRequestException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var entity = new Bitacora
        {
            AmortizacionId = request.AmortizacionId,
            CreditoId = request.CreditoId,
            ClienteId = request.ClienteId,
            GestorId = gestorId,
            MedioContactoId = request.MedioContactoId,
            FechaHoraGestion = request.FechaHoraGestion,
            TipoGestion = request.TipoGestion,
            Sentido = request.Sentido,
            Resultado = request.Resultado,
            DuracionSegundos = request.DuracionSegundos,
            MensajeEnviado = request.MensajeEnviado,
            Asunto = request.Asunto,
            RespuestaCliente = request.RespuestaCliente,
            PromesaFechaPago = request.PromesaFechaPago,
            PromesaMonto = request.PromesaMonto,
            UrlGrabacion = request.UrlGrabacion,
            UrlEvidencia = request.UrlEvidencia,
            Observaciones = request.Observaciones,
            GeolocalizacionLat = request.GeolocalizacionLat,
            GeolocalizacionLng = request.GeolocalizacionLng,
            FechaCobro = request.FechaCobro,
            GrupoId = request.GrupoId,
            DiasVencidos = request.DiasVencidos,
            CarteraVencidaContable = request.CarteraVencidaContable,
            Demanda = request.Demanda,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(entity);
        return ToResponse(created);
    }

    public async Task<BitacoraResponse> UpdateAsync(int id, UpdateBitacoraRequest request, int gestorId)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new BadRequestException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        var existing = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Bitácora con id {id} no encontrada.");

        existing.AmortizacionId = request.AmortizacionId;
        existing.CreditoId = request.CreditoId;
        existing.ClienteId = request.ClienteId;
        existing.GestorId = gestorId;
        existing.MedioContactoId = request.MedioContactoId;
        existing.FechaHoraGestion = request.FechaHoraGestion;
        existing.TipoGestion = request.TipoGestion;
        existing.Sentido = request.Sentido;
        existing.Resultado = request.Resultado;
        existing.DuracionSegundos = request.DuracionSegundos;
        existing.MensajeEnviado = request.MensajeEnviado;
        existing.Asunto = request.Asunto;
        existing.RespuestaCliente = request.RespuestaCliente;
        existing.PromesaFechaPago = request.PromesaFechaPago;
        existing.PromesaMonto = request.PromesaMonto;
        existing.PromesaCumplida = request.PromesaCumplida;
        existing.UrlGrabacion = request.UrlGrabacion;
        existing.UrlEvidencia = request.UrlEvidencia;
        existing.Observaciones = request.Observaciones;
        existing.GeolocalizacionLat = request.GeolocalizacionLat;
        existing.GeolocalizacionLng = request.GeolocalizacionLng;
        existing.FechaCobro = request.FechaCobro;
        existing.GrupoId = request.GrupoId;
        existing.DiasVencidos = request.DiasVencidos;
        existing.CarteraVencidaContable = request.CarteraVencidaContable;
        existing.Demanda = request.Demanda;

        var updated = await _repo.UpdateAsync(existing);
        return ToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Bitácora con id {id} no encontrada.");

        return await _repo.DeleteAsync(id);
    }

    private static BitacoraResponse ToResponse(Bitacora b) => new(
        b.Id,
        b.AmortizacionId,
        b.CreditoId,
        b.ClienteId,
        b.GestorId,
        b.MedioContactoId,
        b.FechaHoraGestion,
        b.TipoGestion,
        b.Sentido,
        b.Resultado,
        b.DuracionSegundos,
        b.MensajeEnviado,
        b.Asunto,
        b.RespuestaCliente,
        b.PromesaFechaPago,
        b.PromesaMonto,
        b.PromesaCumplida,
        b.UrlGrabacion,
        b.UrlEvidencia,
        b.Observaciones,
        b.GeolocalizacionLat,
        b.GeolocalizacionLng,
        b.CreatedAt,
        b.FechaCobro,
        b.GrupoId,
        b.DiasVencidos,
        b.CarteraVencidaContable,
        b.Demanda
    );
}
