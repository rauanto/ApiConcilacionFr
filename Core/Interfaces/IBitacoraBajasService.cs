using ApiConcilacionFr.Common;
using ApiConcilacionFr.Domain.Entities;
using FluentValidation;

namespace ApiConcilacionFr.Core.Interfaces;

public record CreateBitacoraBajasRequest(
    long CreditoId,
    long ClienteId,
    double MontoOtorgado,
    double SaldoCapital,
    double SaldoInsoluto,
    double CapitalVencido,
    int AmorticacionesVencidas,
    double InteresCobrado,
    string DiasVencidos,
    string CarteraVencidaContable,
    string Demanda,
    string Estatus,
    DateTime FechaAlta,
    DateTime FechaVencimiento,
    DateTime InicioCobranza,
    DateTime UltCobranza,
    int? GrupoId,
    string? Obervaciones,
    string NombreCliente,
    string? Sindicato,
    int Baja,
    DateOnly? FechaRealBaja
);

public record UpdateBitacoraBajaRequest(
    int Baja,
    string? Obervaciones,
    DateOnly? FechaRealBaja
);

public record BitacoraBajasResponse(
    long Id,
    long CreditoId,
    long ClienteId,
    double MontoOtorgado,
    double SaldoCapital,
    double SaldoInsoluto,
    double CapitalVencido,
    int AmorticacionesVencidas,
    double InteresCobrado,
    string DiasVencidos,
    string CarteraVencidaContable,
    string Demanda,
    string Estatus,
    DateTime FechaAlta,
    DateTime FechaVencimiento,
    DateTime InicioCobranza,
    DateTime UltCobranza,
    long GestorId,
    DateTime CreatedAt,
    int? GrupoId,
    string? Obervaciones,
    int? Baja,
    string NombreCliente,
    string? Sindicato,
    DateOnly? FechaRealBaja
);

public class CreateBitacoraBajasValidator : AbstractValidator<CreateBitacoraBajasRequest>
{
    public CreateBitacoraBajasValidator()
    {
        RuleFor(x => x.CreditoId).GreaterThan(0).WithMessage("CreditoId debe ser mayor a 0.");
        RuleFor(x => x.ClienteId).GreaterThan(0).WithMessage("ClienteId debe ser mayor a 0.");
        RuleFor(x => x.DiasVencidos).NotEmpty().MaximumLength(10);
        RuleFor(x => x.CarteraVencidaContable).NotEmpty().MaximumLength(35);
        RuleFor(x => x.Demanda).NotEmpty().MaximumLength(35);
        RuleFor(x => x.Estatus).NotEmpty().MaximumLength(35);
        RuleFor(x => x.FechaAlta).NotEmpty();
        RuleFor(x => x.FechaVencimiento).NotEmpty();
        RuleFor(x => x.InicioCobranza).NotEmpty();
        RuleFor(x => x.UltCobranza).NotEmpty();
    }
}

public interface IBitacoraBajasService
{
    Task<PagedResponse<BitacoraBajasResponse>> GetAllAsync(PaginationParams paginacion, string grupos);
    Task<BitacoraBajasResponse> CreateAsync(CreateBitacoraBajasRequest request, long gestorId);
    Task<BitacoraBajasResponse> UpdateAsync(long creditoId, UpdateBitacoraBajaRequest request);
}
