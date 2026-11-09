using ApiConcilacionFr.Common;
using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ClosedXML.Excel;

namespace ApiConcilacionFr.Core.Services;

public sealed class ReporteContabilidadExcelService : IReporteContabilidadExcelService
{
    private static readonly string[] Encabezados =
    [
        "ETAPA",
        "CUENTA",
        "DESCRIPCION",
        "S_CLAVE",
        "S_GRUPO",
        "GRUPO",
        "SALDO CAPITAL CONTABLE",
        "SALDO CAPITAL PROSIGO",
        "DIFERENCIA",
        "PRESTAMOS"
    ];

    private readonly IReporteContabilidadService _reporteContabilidadService;

    public ReporteContabilidadExcelService(IReporteContabilidadService reporteContabilidadService)
    {
        _reporteContabilidadService = reporteContabilidadService;
    }

    public async Task<byte[]> GenerarReporteExcelAsync(
        int mes,
        string etapa,
        IReadOnlyCollection<int>? sGrupos,
        bool indice)
    {
        var reportes = (await _reporteContabilidadService
            .ListarReportesContabilidadPorEtapaAsync(mes, etapa, sGrupos)).ToList();

        if (reportes.Count == 0)
        {
            throw new NotFoundException(
                $"No se encontraron saldos contables para MES{mes} y la etapa {etapa}.");
        }

        using var workbook = new XLWorkbook();
        var grupos = reportes
            .GroupBy(reporte => reporte.S_GRUPO)
            .OrderBy(grupo => NombreGrupo(grupo.First()), StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(grupo => grupo.Key)
            .ToList();

        if (indice)
            CrearIndice(workbook, grupos);

        foreach (var grupo in grupos)
        {
            var worksheet = workbook.Worksheets.Add(CrearNombreHoja(grupo.Key, NombreGrupo(grupo.First())));
            CrearHojaGrupo(worksheet, mes, etapa, grupo);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void CrearIndice(
        XLWorkbook workbook,
        IReadOnlyList<IGrouping<int, ReporteContabilidad>> grupos)
    {
        var worksheet = workbook.Worksheets.Add("Indice");
        worksheet.Cell(1, 1).Value = "Índice de grupos";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        worksheet.Cell(3, 1).Value = "S_GRUPO";
        worksheet.Cell(3, 2).Value = "GRUPO";
        worksheet.Cell(3, 3).Value = "HOJA";
        worksheet.Range(3, 1, 3, 3).Style.Font.Bold = true;

        var row = 4;
        foreach (var grupo in grupos)
        {
            worksheet.Cell(row, 1).Value = grupo.Key;
            worksheet.Cell(row, 2).Value = NombreGrupo(grupo.First());
            worksheet.Cell(row, 3).Value = CrearNombreHoja(grupo.Key, NombreGrupo(grupo.First()));
            row++;
        }

        worksheet.Columns().AdjustToContents();
    }

    private static void CrearHojaGrupo(
        IXLWorksheet worksheet,
        int mes,
        string etapa,
        IEnumerable<ReporteContabilidad> reportes)
    {
        worksheet.Cell(1, 1).Value = "Reporte de saldos contables";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        worksheet.Cell(2, 1).Value = $"MES{mes} | Etapa: {etapa}";

        for (var column = 0; column < Encabezados.Length; column++)
            worksheet.Cell(4, column + 1).Value = Encabezados[column];

        var fila = 5;
        foreach (var reporte in reportes
                     .OrderBy(reporte => reporte.ETAPA)
                     .ThenBy(reporte => reporte.CUENTA))
        {
            worksheet.Cell(fila, 1).Value = reporte.ETAPA;
            worksheet.Cell(fila, 2).Value = reporte.CUENTA ?? "-";
            worksheet.Cell(fila, 3).Value = reporte.C_DESCRIPCION ?? "-";
            worksheet.Cell(fila, 4).Value = reporte.S_CLAVE;
            worksheet.Cell(fila, 5).Value = reporte.S_GRUPO;
            worksheet.Cell(fila, 6).Value = reporte.GRUPO ?? "SIN GRUPO";
            worksheet.Cell(fila, 7).Value = reporte.SALDO;
            worksheet.Cell(fila, 8).Value = reporte.SALDO_INSOLUTO;
            worksheet.Cell(fila, 9).Value = reporte.DIFERENCIA;
            worksheet.Cell(fila, 10).Value = reporte.PRESTAMOS ?? "-";
            fila++;
        }

        var ultimaFila = fila - 1;
        worksheet.Range(4, 1, 4, Encabezados.Length).Style.Font.Bold = true;
        worksheet.Range(4, 1, ultimaFila, Encabezados.Length).SetAutoFilter();
        worksheet.Range(5, 7, ultimaFila, 9).Style.NumberFormat.Format = "#,##0.00";
        worksheet.SheetView.FreezeRows(4);
        worksheet.Columns().AdjustToContents();
        worksheet.Column(3).Width = Math.Min(worksheet.Column(3).Width, 50);
        worksheet.Column(10).Width = Math.Min(worksheet.Column(10).Width, 40);
    }

    private static string CrearNombreHoja(int sGrupo, string nombreGrupo)
    {
        var caracteresInvalidos = new[] { '[', ']', '*', '?', '/', '\\', ':' };
        var nombreSeguro = new string(nombreGrupo
            .Select(caracter => caracteresInvalidos.Contains(caracter) ? '_' : caracter)
            .ToArray())
            .Trim(' ', '\'');
        if (string.IsNullOrWhiteSpace(nombreSeguro))
            nombreSeguro = "SIN_GRUPO";

        var prefijo = $"G{sGrupo}_";
        var maximoNombre = 31 - prefijo.Length;
        if (nombreSeguro.Length > maximoNombre)
            nombreSeguro = nombreSeguro[..maximoNombre];

        return prefijo + nombreSeguro;
    }

    private static string NombreGrupo(ReporteContabilidad reporte) =>
        string.IsNullOrWhiteSpace(reporte.GRUPO) ? "SIN GRUPO" : reporte.GRUPO;
}
