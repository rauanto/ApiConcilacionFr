using ApiConcilacionFr.Common;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ApiConcilacionFr.Core.Interfaces;

public record CreateBitacoraRequest(
    int AmortizacionId,
    int CreditoId,
    int ClienteId,
    int? MedioContactoId,
    DateTime FechaHoraGestion,
    string TipoGestion,
    string Sentido,
    string Resultado,
    int? DuracionSegundos,
    string? MensajeEnviado,
    string? Asunto,
    string? RespuestaCliente,
    DateTime? PromesaFechaPago,
    decimal? PromesaMonto,
    string? UrlGrabacion,
    string? UrlEvidencia,
    string? Observaciones,
    decimal? GeolocalizacionLat,
    decimal? GeolocalizacionLng,
    DateTime? FechaCobro,
    long? GrupoId,
    string? DiasVencidos,
    string? CarteraVencidaContable,
    string? Demanda,
    string? Estatus
);

public record UpdateBitacoraRequest(
    int AmortizacionId,
    int CreditoId,
    int ClienteId,
    int? MedioContactoId,
    DateTime FechaHoraGestion,
    string TipoGestion,
    string Sentido,
    string Resultado,
    int? DuracionSegundos,
    string? MensajeEnviado,
    string? Asunto,
    string? RespuestaCliente,
    DateTime? PromesaFechaPago,
    decimal? PromesaMonto,
    bool? PromesaCumplida,
    string? UrlGrabacion,
    string? UrlEvidencia,
    string? Observaciones,
    decimal? GeolocalizacionLat,
    decimal? GeolocalizacionLng,
    DateTime? FechaCobro,
    long? GrupoId,
    string? DiasVencidos,
    string? CarteraVencidaContable,
    string? Demanda,
    string? Estatus
);

public record BitacoraResponse(
    int Id,
    int AmortizacionId,
    int CreditoId,
    int ClienteId,
    int GestorId,
    int? MedioContactoId,
    DateTime FechaHoraGestion,
    string TipoGestion,
    string Sentido,
    string Resultado,
    int? DuracionSegundos,
    string? MensajeEnviado,
    string? Asunto,
    string? RespuestaCliente,
    DateTime? PromesaFechaPago,
    decimal? PromesaMonto,
    bool? PromesaCumplida,
    string? UrlGrabacion,
    string? UrlEvidencia,
    string? Observaciones,
    decimal? GeolocalizacionLat,
    decimal? GeolocalizacionLng,
    DateTime CreatedAt,
    DateTime? FechaCobro,
    long? GrupoId,
    string? DiasVencidos,
    string? CarteraVencidaContable,
    string? Demanda,
    string? Estatus
);

public class CreateBitacoraValidator : AbstractValidator<CreateBitacoraRequest>
{
    private static readonly string[] TiposGestion =
        ["LLAMADA", "SMS", "WHATSAPP", "EMAIL", "VISITA", "CARTA", "BUROFAX"];

    private static readonly string[] Sentidos = ["SALIENTE", "ENTRANTE"];

    private static readonly string[] Resultados =
    [
        "CONTACTO_EFECTIVO", "CONTACTO_TERCERO", "NO_CONTESTA", "BUZON",
        "NUMERO_EQUIVOCADO", "NUMERO_FUERA_SERVICIO", "RECHAZA_LLAMADA",
        "PROMESA_PAGO", "NEGOCIACION", "NEGATIVA_PAGO", "REPROGRAMACION",
        "ENVIADO", "ENTREGADO", "LEIDO", "RESPONDIDO"
    ];

    public CreateBitacoraValidator()
    {
        RuleFor(x => x.AmortizacionId).GreaterThan(0).WithMessage("AmortizacionId debe ser mayor a 0.");
        RuleFor(x => x.CreditoId).GreaterThan(0).WithMessage("CreditoId debe ser mayor a 0.");
        RuleFor(x => x.ClienteId).GreaterThan(0).WithMessage("ClienteId debe ser mayor a 0.");
        RuleFor(x => x.FechaHoraGestion).NotEmpty().WithMessage("FechaHoraGestion es obligatoria.");
        RuleFor(x => x.TipoGestion)
            .Must(v => TiposGestion.Contains(v))
            .WithMessage($"TipoGestion debe ser uno de: {string.Join(", ", TiposGestion)}.");
        RuleFor(x => x.Sentido)
            .Must(v => Sentidos.Contains(v))
            .WithMessage($"Sentido debe ser: {string.Join(" o ", Sentidos)}.");
        RuleFor(x => x.Resultado)
            .Must(v => Resultados.Contains(v))
            .WithMessage($"Resultado debe ser uno de: {string.Join(", ", Resultados)}.");
        RuleFor(x => x.DuracionSegundos)
            .GreaterThanOrEqualTo(0).When(x => x.DuracionSegundos.HasValue)
            .WithMessage("DuracionSegundos no puede ser negativa.");
        RuleFor(x => x.PromesaMonto)
            .GreaterThan(0).When(x => x.PromesaMonto.HasValue)
            .WithMessage("PromesaMonto debe ser mayor a 0.");
        RuleFor(x => x.Asunto).MaximumLength(255).When(x => x.Asunto != null);
        RuleFor(x => x.UrlGrabacion).MaximumLength(500).When(x => x.UrlGrabacion != null);
        RuleFor(x => x.UrlEvidencia).MaximumLength(500).When(x => x.UrlEvidencia != null);
        RuleFor(x => x.Estatus).MaximumLength(30).When(x => x.Estatus != null);
    }
}

public class UpdateBitacoraValidator : AbstractValidator<UpdateBitacoraRequest>
{
    private static readonly string[] TiposGestion =
        ["LLAMADA", "SMS", "WHATSAPP", "EMAIL", "VISITA", "CARTA", "BUROFAX"];

    private static readonly string[] Sentidos = ["SALIENTE", "ENTRANTE"];

    private static readonly string[] Resultados =
    [
        "CONTACTO_EFECTIVO", "CONTACTO_TERCERO", "NO_CONTESTA", "BUZON",
        "NUMERO_EQUIVOCADO", "NUMERO_FUERA_SERVICIO", "RECHAZA_LLAMADA",
        "PROMESA_PAGO", "NEGOCIACION", "NEGATIVA_PAGO", "REPROGRAMACION",
        "ENVIADO", "ENTREGADO", "LEIDO", "RESPONDIDO"
    ];

    public UpdateBitacoraValidator()
    {
        RuleFor(x => x.AmortizacionId).GreaterThan(0).WithMessage("AmortizacionId debe ser mayor a 0.");
        RuleFor(x => x.CreditoId).GreaterThan(0).WithMessage("CreditoId debe ser mayor a 0.");
        RuleFor(x => x.ClienteId).GreaterThan(0).WithMessage("ClienteId debe ser mayor a 0.");
        RuleFor(x => x.FechaHoraGestion).NotEmpty().WithMessage("FechaHoraGestion es obligatoria.");
        RuleFor(x => x.TipoGestion)
            .Must(v => TiposGestion.Contains(v))
            .WithMessage($"TipoGestion debe ser uno de: {string.Join(", ", TiposGestion)}.");
        RuleFor(x => x.Sentido)
            .Must(v => Sentidos.Contains(v))
            .WithMessage($"Sentido debe ser: {string.Join(" o ", Sentidos)}.");
        RuleFor(x => x.Resultado)
            .Must(v => Resultados.Contains(v))
            .WithMessage($"Resultado debe ser uno de: {string.Join(", ", Resultados)}.");
        RuleFor(x => x.DuracionSegundos)
            .GreaterThanOrEqualTo(0).When(x => x.DuracionSegundos.HasValue)
            .WithMessage("DuracionSegundos no puede ser negativa.");
        RuleFor(x => x.PromesaMonto)
            .GreaterThan(0).When(x => x.PromesaMonto.HasValue)
            .WithMessage("PromesaMonto debe ser mayor a 0.");
        RuleFor(x => x.Asunto).MaximumLength(255).When(x => x.Asunto != null);
        RuleFor(x => x.UrlGrabacion).MaximumLength(500).When(x => x.UrlGrabacion != null);
        RuleFor(x => x.UrlEvidencia).MaximumLength(500).When(x => x.UrlEvidencia != null);
        RuleFor(x => x.Estatus).MaximumLength(30).When(x => x.Estatus != null);
    }
}

public interface IBitacoraService
{
    Task<PagedResponse<BitacoraResponse>> GetAllAsync(BitacoraFiltros filtros, PaginationParams paginacion);
    Task<BitacoraResponse?> GetByIdAsync(int id);
    Task<BitacoraResponse> CreateAsync(CreateBitacoraRequest request, int gestorId);
    Task<BitacoraResponse> UpdateAsync(int id, UpdateBitacoraRequest request, int gestorId);
    Task<bool> DeleteAsync(int id);
    Task<BitacoraResponse> SubirGrabacionAsync(int id, IFormFile archivo);
    Task<BitacoraResponse> SubirEvidenciaAsync(int id, IFormFile archivo);
}
