using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ApiConcilacionFr.Core.Services;

public class BitacoraService : IBitacoraService
{
    private readonly IBitacoraRepository _repo;
    private readonly IBitacoraArchivoRepository _archivoRepo;
    private readonly IValidator<CreateBitacoraRequest> _createValidator;
    private readonly IValidator<UpdateBitacoraRequest> _updateValidator;
    private readonly IFileStorageService _fileStorage;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BitacoraService(
        IBitacoraRepository repo,
        IBitacoraArchivoRepository archivoRepo,
        IValidator<CreateBitacoraRequest> createValidator,
        IValidator<UpdateBitacoraRequest> updateValidator,
        IFileStorageService fileStorage,
        IHttpContextAccessor httpContextAccessor)
    {
        _repo = repo;
        _archivoRepo = archivoRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _fileStorage = fileStorage;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResponse<BitacoraResponse>> GetAllAsync(BitacoraFiltros filtros, PaginationParams paginacion)
    {
        var (items, total) = await _repo.GetAllAsync(filtros, paginacion);
        var itemList = items.ToList();

        if (itemList.Count == 0)
            return new PagedResponse<BitacoraResponse>([], total, paginacion);

        var archivos = await _archivoRepo.GetByBitacoraIdsAsync(itemList.Select(b => b.Id));
        var archivosPorBitacora = archivos
            .GroupBy(a => a.BitacoraId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<BitacoraArchivo>)g.ToList());

        return new PagedResponse<BitacoraResponse>(
            itemList.Select(b => ToResponse(b, archivosPorBitacora.GetValueOrDefault(b.Id, []))),
            total, paginacion);
    }

    public async Task<BitacoraResponse?> GetByIdAsync(int id)
    {
        var bitacora = await _repo.GetByIdAsync(id);
        if (bitacora is null) return null;
        var archivos = await _archivoRepo.GetByBitacoraIdAsync(id);
        return ToResponse(bitacora, archivos.ToList());
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
            PromesaFechaPago = request.PromesaFechaPago?.ToDateTime(TimeOnly.MinValue),
            PromesaMonto = request.PromesaMonto,
            Observaciones = request.Observaciones,
            GeolocalizacionLat = request.GeolocalizacionLat,
            GeolocalizacionLng = request.GeolocalizacionLng,
            FechaCobro = request.FechaCobro?.ToDateTime(TimeOnly.MinValue),
            GrupoId = request.GrupoId,
            DiasVencidos = request.DiasVencidos,
            CarteraVencidaContable = request.CarteraVencidaContable,
            Demanda = request.Demanda,
            Estatus = request.Estatus,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repo.CreateAsync(entity);
        return ToResponse(created, []);
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
        existing.PromesaFechaPago = request.PromesaFechaPago?.ToDateTime(TimeOnly.MinValue);
        existing.PromesaMonto = request.PromesaMonto;
        existing.PromesaCumplida = request.PromesaCumplida;
        existing.Observaciones = request.Observaciones;
        existing.GeolocalizacionLat = request.GeolocalizacionLat;
        existing.GeolocalizacionLng = request.GeolocalizacionLng;
        existing.FechaCobro = request.FechaCobro?.ToDateTime(TimeOnly.MinValue);
        existing.GrupoId = request.GrupoId;
        existing.DiasVencidos = request.DiasVencidos;
        existing.CarteraVencidaContable = request.CarteraVencidaContable;
        existing.Demanda = request.Demanda;
        existing.Estatus = request.Estatus;

        var updated = await _repo.UpdateAsync(existing);
        var archivos = await _archivoRepo.GetByBitacoraIdAsync(id);
        return ToResponse(updated, archivos.ToList());
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Bitácora con id {id} no encontrada.");

        return await _repo.DeleteAsync(id);
    }

    public async Task<BitacoraResponse> SubirGrabacionAsync(int id, IFormFile archivo)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Bitácora con id {id} no encontrada.");

        var url = await _fileStorage.GuardarGrabacionAsync(archivo, id);
        await _archivoRepo.AddAsync(new BitacoraArchivo
        {
            BitacoraId = id,
            Tipo = "GRABACION",
            Url = url,
            NombreOriginal = archivo.FileName,
            CreatedAt = DateTime.UtcNow
        });

        var bitacora = await _repo.GetByIdAsync(id);
        var archivos = await _archivoRepo.GetByBitacoraIdAsync(id);
        return ToResponse(bitacora!, archivos.ToList());
    }

    public async Task<BitacoraResponse> SubirEvidenciaAsync(int id, IFormFile archivo)
    {
        _ = await _repo.GetByIdAsync(id)
            ?? throw new NotFoundException($"Bitácora con id {id} no encontrada.");

        var url = await _fileStorage.GuardarEvidenciaAsync(archivo, id);
        await _archivoRepo.AddAsync(new BitacoraArchivo
        {
            BitacoraId = id,
            Tipo = "EVIDENCIA",
            Url = url,
            NombreOriginal = archivo.FileName,
            CreatedAt = DateTime.UtcNow
        });

        var bitacora = await _repo.GetByIdAsync(id);
        var archivos = await _archivoRepo.GetByBitacoraIdAsync(id);
        return ToResponse(bitacora!, archivos.ToList());
    }

    public async Task<bool> EliminarArchivoAsync(int bitacoraId, int archivoId)
    {
        _ = await _repo.GetByIdAsync(bitacoraId)
            ?? throw new NotFoundException($"Bitácora con id {bitacoraId} no encontrada.");

        var archivo = await _archivoRepo.GetByIdAsync(archivoId)
            ?? throw new NotFoundException($"Archivo con id {archivoId} no encontrado.");

        if (archivo.BitacoraId != bitacoraId)
            throw new BadRequestException($"El archivo {archivoId} no pertenece a la bitácora {bitacoraId}.");

        _fileStorage.EliminarArchivo(archivo.Url);
        return await _archivoRepo.DeleteAsync(archivoId);
    }

    private BitacoraResponse ToResponse(Bitacora b, IReadOnlyList<BitacoraArchivo> archivos) => new(
        b.Id,
        b.AmortizacionId,
        b.CreditoId,
        b.ClienteId,
        b.GestorId,
        b.MedioContactoId,
        b.FechaHoraGestion,
        b.TipoGestion ?? string.Empty,
        b.Sentido ?? string.Empty,
        b.Resultado ?? string.Empty,
        b.DuracionSegundos,
        b.MensajeEnviado,
        b.Asunto,
        b.RespuestaCliente,
        b.PromesaFechaPago.HasValue ? DateOnly.FromDateTime(b.PromesaFechaPago.Value) : null,
        b.PromesaMonto,
        b.PromesaCumplida,
        b.Observaciones,
        b.GeolocalizacionLat,
        b.GeolocalizacionLng,
        b.CreatedAt,
        b.FechaCobro.HasValue ? DateOnly.FromDateTime(b.FechaCobro.Value) : null,
        b.GrupoId,
        b.DiasVencidos,
        b.CarteraVencidaContable,
        b.Demanda,
        b.Estatus,
        archivos.Select(ToArchivoResponse).ToList()
    );

    private BitacoraArchivoResponse ToArchivoResponse(BitacoraArchivo a) => new(
        a.Id,
        a.BitacoraId,
        a.Tipo,
        ToAbsoluteUrl(a.Url) ?? a.Url,
        a.NombreOriginal,
        a.CreatedAt
    );

    private string? ToAbsoluteUrl(string? rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa)) return null;
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null) return rutaRelativa;
        return $"{request.Scheme}://{request.Host}{rutaRelativa}";
    }
}
