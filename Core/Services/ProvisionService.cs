using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;
using Dapper;

namespace ApiConcilacionFr.Core.Services
{
    public class ProvisionService : IProvisionService
    {
        private readonly IProvisionRepository _provisionRepository;
        private readonly IMemoryCache _cache;

        public ProvisionService(IProvisionRepository provisionRepository, IMemoryCache cache)
        {
            _provisionRepository = provisionRepository;
            _cache = cache;
        }

        public byte[] ProcesarReporteProvision(IFormFile file)
        {
            var currentDetails = new List<ProvisionModel>();

            using (var stream = file.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = false
                        }
                    });

                    if (result.Tables.Count == 0)
                        throw new Exception("El archivo Excel no contiene hojas.");

                    var table = result.Tables[0];
                    string? currentLoanId = null;
                    string? currentLoanDesc = null;
                    string? currentClientId = null;
                    string? currentClientName = null;

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        var row = table.Rows[i];
                        
                        // Extract non-null, non-empty values
                        var valoresFila = new List<object>();
                        for(int col = 0; col < table.Columns.Count; col++)
                        {
                            if(row[col] != DBNull.Value && row[col] != null)
                            {
                                string strVal = row[col].ToString()?.Trim() ?? "";
                                if(!string.IsNullOrEmpty(strVal))
                                {
                                    valoresFila.Add(row[col]);
                                }
                            }
                        }

                        if(valoresFila.Count == 0) continue;

                        var col0 = row[0] != DBNull.Value ? row[0].ToString()?.Trim() : "";

                        if (col0 == "Total Pres.")
                        {
                            if(valoresFila.Count >= 3)
                            {
                                currentLoanId = valoresFila[1].ToString()?.Replace("/", "");
                                currentLoanDesc = valoresFila[2].ToString();
                                foreach (var d in currentDetails)
                                {
                                    if (d.Tramite == null)
                                    {
                                        d.Tramite = currentLoanId;
                                        d.Descripcion_Prestamo = currentLoanDesc;
                                    }
                                }
                            }
                        }
                        else if (col0 == "Acreditado")
                        {
                            if(valoresFila.Count >= 3)
                            {
                                currentClientId = valoresFila[1].ToString();
                                currentClientName = valoresFila[2].ToString();
                                foreach (var d in currentDetails)
                                {
                                    if (d.Acreditado == null)
                                    {
                                        d.Acreditado = currentClientId;
                                        d.Nombre_Cliente = currentClientName;
                                    }
                                }
                            }
                        }
                        else if (string.IsNullOrEmpty(col0) && valoresFila.Count >= 8)
                        {
                            if(double.TryParse(valoresFila[0].ToString(), out double amortizacion))
                            {
                                string fechaOp = ParseDateToString(valoresFila[1]);
                                string fechaVenc = ParseDateToString(valoresFila[2]);
                                
                                string fechaLiq = "";
                                int offset = 0;

                                string val3Str = valoresFila[3].ToString() ?? "";
                                if (val3Str.Contains("/"))
                                {
                                    fechaLiq = ParseDateToString(valoresFila[3]);
                                    offset = 1;
                                }
                                
                                try
                                {
                                    double? diasOpe = ParseDoubleOrNull(valoresFila[3 + offset]);
                                    double? tasa = ParseDoubleOrNull(valoresFila[4 + offset]);
                                    double? saldo = ParseDoubleOrNull(valoresFila[5 + offset]);
                                    double? interes = ParseDoubleOrNull(valoresFila[6 + offset]);
                                    double? iva = ParseDoubleOrNull(valoresFila[7 + offset]);
                                    double? total = ParseDoubleOrNull(valoresFila[8 + offset]);
                                    string obs = valoresFila.Count > 9 + offset ? valoresFila[9 + offset].ToString() ?? "" : "";

                                    var detail = new ProvisionModel
                                    {
                                        Acreditado = null,
                                        Nombre_Cliente = null,
                                        Tramite = null,
                                        Descripcion_Prestamo = null,
                                        Amortizacion = amortizacion,
                                        Fecha_Operacion = fechaOp,
                                        Fecha_Vencimiento = fechaVenc,
                                        Fecha_Liquidacion = fechaLiq,
                                        Dias_Ope = diasOpe,
                                        Tasa_Normal = tasa,
                                        Saldo_Promedio = saldo,
                                        Interes_Ordinario = interes,
                                        IVA = iva,
                                        Total = total,
                                        Observaciones = obs
                                    };

                                    currentDetails.Add(detail);
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Procesado");

                string[] headers = {
                    "Acreditado", "Nombre_Cliente", "Tramite", "Descripcion_Prestamo",
                    "Amortizacion", "Fecha_Operacion", "Fecha_Vencimiento", "Fecha_Liquidacion", "Dias_Ope",
                    "Tasa_Normal", "Saldo_Promedio", "Interes_Ordinario", "IVA", "Total", "Observaciones"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headers[i];
                }

                for (int i = 0; i < currentDetails.Count; i++)
                {
                    var item = currentDetails[i];
                    int row = i + 2;

                    worksheet.Cell(row, 1).Value = item.Acreditado;
                    worksheet.Cell(row, 2).Value = item.Nombre_Cliente;
                    worksheet.Cell(row, 3).Value = item.Tramite;
                    worksheet.Cell(row, 4).Value = item.Descripcion_Prestamo;
                    worksheet.Cell(row, 5).Value = item.Amortizacion;
                    
                    // Format explicitly to avoid issues in the frontend reading the excel cells
                    worksheet.Cell(row, 6).Value = item.Fecha_Operacion;
                    worksheet.Cell(row, 7).Value = item.Fecha_Vencimiento;
                    worksheet.Cell(row, 8).Value = item.Fecha_Liquidacion;
                    
                    worksheet.Cell(row, 9).Value = item.Dias_Ope;
                    worksheet.Cell(row, 10).Value = item.Tasa_Normal;
                    worksheet.Cell(row, 11).Value = item.Saldo_Promedio;
                    worksheet.Cell(row, 12).Value = item.Interes_Ordinario;
                    worksheet.Cell(row, 13).Value = item.IVA;
                    worksheet.Cell(row, 14).Value = item.Total;
                    worksheet.Cell(row, 15).Value = item.Observaciones;
                }

                worksheet.Columns().AdjustToContents();

                using (var ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }

        private string ParseDateToString(object obj)
        {
            if (obj == null || obj == DBNull.Value) return "";

            if (obj is DateTime dt)
            {
                return dt.ToString("dd/MM/yyyy");
            }
            if (obj is double d)
            {
                try
                {
                    return DateTime.FromOADate(d).ToString("dd/MM/yyyy");
                }
                catch
                {
                    return d.ToString();
                }
            }

            string s = obj.ToString() ?? "";
            if (DateTime.TryParse(s, out DateTime parsed))
            {
                return parsed.ToString("dd/MM/yyyy");
            }
            if (double.TryParse(s, out double parsedDouble))
            {
                try
                {
                    return DateTime.FromOADate(parsedDouble).ToString("dd/MM/yyyy");
                }
                catch
                {
                }
            }

            return s;
        }

        private double? ParseDoubleOrNull(object obj)
        {
            if (obj == null || obj == DBNull.Value) return null;
            if (double.TryParse(obj.ToString(), out double d)) return d;
            return null;
        }

        private static string GenerarCacheKey(string listaPrestamos, DateTime? inicio, DateTime? fin)
        {
            string raw = $"{listaPrestamos}_{inicio:yyyyMMdd}_{fin:yyyyMMdd}";
            if (raw.Length <= 64) return $"provision_rep_{raw}";
            return $"provision_rep_{Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw)))}";
        }

        public async Task<IEnumerable<ReporteProvisionDto>> ObtenerReporteProvisionesAsync(ReporteProvisionRequest request, CancellationToken cancellationToken = default)
        {
            string listaPrestamos = request?.ListaPrestamos != null && request.ListaPrestamos.Count > 0
                ? string.Join(",", request.ListaPrestamos.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).OrderBy(x => x))
                : "";
            string cacheKey = GenerarCacheKey(listaPrestamos, request?.FechaInicio, request?.FechaFin);

            if (_cache.TryGetValue(cacheKey, out IEnumerable<ReporteProvisionDto>? cached) && cached != null)
            {
                return cached;
            }

            var result = await _provisionRepository.ObtenerReporteProvisionesAsync(listaPrestamos, request?.FechaInicio, request?.FechaFin, cancellationToken);
            var list = result.AsList();
            if (list.Count > 0)
            {
                _cache.Set(cacheKey, list, TimeSpan.FromMinutes(3));
            }

            return list;
        }

        public async Task<ReporteProvisionFiltrosDto> ObtenerFiltrosReporteProvisionesAsync(ReporteProvisionRequest request, CancellationToken cancellationToken = default)
        {
            IEnumerable<ReporteProvisionDto> datos;
            if (request?.ReporteId.HasValue == true && request.ReporteId.Value > 0)
            {
                datos = await ObtenerReporteGuardadoAsync(request.ReporteId.Value, cancellationToken);
            }
            else
            {
                datos = await ObtenerReporteProvisionesAsync(request, cancellationToken);
            }

            var filtros = new ReporteProvisionFiltrosDto();

            var sGrupoSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var grupoSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var pagoSostenidoSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var interesAmortizacionSet = new HashSet<decimal>();
            var ivaAmortizacionSet = new HashSet<decimal>();

            DateOnly? minCreditoOtorgado = null, maxCreditoOtorgado = null;
            DateOnly? minCreditoLiquidado = null, maxCreditoLiquidado = null;
            DateOnly? minAmortizaInicio = null, maxAmortizaInicio = null;
            DateOnly? minAmortizaVencimiento = null, maxAmortizaVencimiento = null;
            DateOnly? minUltimoPago = null, maxUltimoPago = null;

            decimal? minPrestamo = null, maxPrestamo = null;
            decimal? minSClave = null, maxSClave = null;
            decimal? minAmortizaNumero = null, maxAmortizaNumero = null;
            decimal? minPrimeraAmortizacion = null, maxPrimeraAmortizacion = null;
            decimal? minImporteAmortizacion = null, maxImporteAmortizacion = null;
            decimal? minAbonado = null, maxAbonado = null;
            decimal? minSaldoAmortizacion = null, maxSaldoAmortizacion = null;
            decimal? minInteresAmortizacion = null, maxInteresAmortizacion = null;
            decimal? minIvaAmortizacion = null, maxIvaAmortizacion = null;
            decimal? minDiasAtraso = null, maxDiasAtraso = null;
            decimal? minMesesAtraso = null, maxMesesAtraso = null;
            decimal? minDiasSinPagar = null, maxDiasSinPagar = null;

            bool hasData = false;

            // Recorrido único O(N) para todos los campos
            foreach (var row in datos)
            {
                hasData = true;

                // Catálogos
                if (!string.IsNullOrWhiteSpace(row.S_GRUPO)) sGrupoSet.Add(row.S_GRUPO.Trim());
                if (!string.IsNullOrWhiteSpace(row.GRUPO)) grupoSet.Add(row.GRUPO.Trim());
                if (!string.IsNullOrWhiteSpace(row.PAGO_SOSTENIDO)) pagoSostenidoSet.Add(row.PAGO_SOSTENIDO.Trim());

                // Fechas
                if (row.CREDITO_OTORGADO.HasValue)
                {
                    var val = row.CREDITO_OTORGADO.Value;
                    if (!minCreditoOtorgado.HasValue || val < minCreditoOtorgado.Value) minCreditoOtorgado = val;
                    if (!maxCreditoOtorgado.HasValue || val > maxCreditoOtorgado.Value) maxCreditoOtorgado = val;
                }
                if (row.CREDITO_LIQUIDADO.HasValue)
                {
                    var val = row.CREDITO_LIQUIDADO.Value;
                    if (!minCreditoLiquidado.HasValue || val < minCreditoLiquidado.Value) minCreditoLiquidado = val;
                    if (!maxCreditoLiquidado.HasValue || val > maxCreditoLiquidado.Value) maxCreditoLiquidado = val;
                }
                if (row.AMORTIZA_INICIO.HasValue)
                {
                    var val = row.AMORTIZA_INICIO.Value;
                    if (!minAmortizaInicio.HasValue || val < minAmortizaInicio.Value) minAmortizaInicio = val;
                    if (!maxAmortizaInicio.HasValue || val > maxAmortizaInicio.Value) maxAmortizaInicio = val;
                }
                if (row.AMORTIZA_VENCIMIENTO.HasValue)
                {
                    var val = row.AMORTIZA_VENCIMIENTO.Value;
                    if (!minAmortizaVencimiento.HasValue || val < minAmortizaVencimiento.Value) minAmortizaVencimiento = val;
                    if (!maxAmortizaVencimiento.HasValue || val > maxAmortizaVencimiento.Value) maxAmortizaVencimiento = val;
                }
                if (row.ULTIMO_PAGO.HasValue)
                {
                    var val = row.ULTIMO_PAGO.Value;
                    if (!minUltimoPago.HasValue || val < minUltimoPago.Value) minUltimoPago = val;
                    if (!maxUltimoPago.HasValue || val > maxUltimoPago.Value) maxUltimoPago = val;
                }

                // Strings numéricos (PRESTAMO y S_CLAVE)
                if (!string.IsNullOrEmpty(row.PRESTAMO) && decimal.TryParse(row.PRESTAMO, out var pVal) && pVal > 0)
                {
                    if (!minPrestamo.HasValue || pVal < minPrestamo.Value) minPrestamo = pVal;
                    if (!maxPrestamo.HasValue || pVal > maxPrestamo.Value) maxPrestamo = pVal;
                }
                if (!string.IsNullOrEmpty(row.S_CLAVE) && decimal.TryParse(row.S_CLAVE, out var scVal) && scVal > 0)
                {
                    if (!minSClave.HasValue || scVal < minSClave.Value) minSClave = scVal;
                    if (!maxSClave.HasValue || scVal > maxSClave.Value) maxSClave = scVal;
                }

                // Números
                if (row.AMORTIZA_NUMERO.HasValue)
                {
                    var val = row.AMORTIZA_NUMERO.Value;
                    if (!minAmortizaNumero.HasValue || val < minAmortizaNumero.Value) minAmortizaNumero = val;
                    if (!maxAmortizaNumero.HasValue || val > maxAmortizaNumero.Value) maxAmortizaNumero = val;
                }
                if (row.PRIMERA_AMORTIZACION.HasValue)
                {
                    var val = row.PRIMERA_AMORTIZACION.Value;
                    if (!minPrimeraAmortizacion.HasValue || val < minPrimeraAmortizacion.Value) minPrimeraAmortizacion = val;
                    if (!maxPrimeraAmortizacion.HasValue || val > maxPrimeraAmortizacion.Value) maxPrimeraAmortizacion = val;
                }
                if (row.IMPORTE_AMORTIZACION.HasValue)
                {
                    var val = row.IMPORTE_AMORTIZACION.Value;
                    if (!minImporteAmortizacion.HasValue || val < minImporteAmortizacion.Value) minImporteAmortizacion = val;
                    if (!maxImporteAmortizacion.HasValue || val > maxImporteAmortizacion.Value) maxImporteAmortizacion = val;
                }
                if (row.ABONADO.HasValue)
                {
                    var val = row.ABONADO.Value;
                    if (!minAbonado.HasValue || val < minAbonado.Value) minAbonado = val;
                    if (!maxAbonado.HasValue || val > maxAbonado.Value) maxAbonado = val;
                }
                if (row.SALDO_AMORTIZACION.HasValue)
                {
                    var val = row.SALDO_AMORTIZACION.Value;
                    if (!minSaldoAmortizacion.HasValue || val < minSaldoAmortizacion.Value) minSaldoAmortizacion = val;
                    if (!maxSaldoAmortizacion.HasValue || val > maxSaldoAmortizacion.Value) maxSaldoAmortizacion = val;
                }
                if (row.INTERES_AMORTIZACION.HasValue)
                {
                    var val = row.INTERES_AMORTIZACION.Value;
                    if (!minInteresAmortizacion.HasValue || val < minInteresAmortizacion.Value) minInteresAmortizacion = val;
                    if (!maxInteresAmortizacion.HasValue || val > maxInteresAmortizacion.Value) maxInteresAmortizacion = val;
                    interesAmortizacionSet.Add(val);
                }
                if (row.IVA_AMORTIZACION.HasValue)
                {
                    var val = row.IVA_AMORTIZACION.Value;
                    if (!minIvaAmortizacion.HasValue || val < minIvaAmortizacion.Value) minIvaAmortizacion = val;
                    if (!maxIvaAmortizacion.HasValue || val > maxIvaAmortizacion.Value) maxIvaAmortizacion = val;
                    ivaAmortizacionSet.Add(val);
                }
                if (row.DIAS_ATRASO.HasValue)
                {
                    var val = row.DIAS_ATRASO.Value;
                    if (!minDiasAtraso.HasValue || val < minDiasAtraso.Value) minDiasAtraso = val;
                    if (!maxDiasAtraso.HasValue || val > maxDiasAtraso.Value) maxDiasAtraso = val;
                }
                if (row.MESES_ATRASO.HasValue)
                {
                    var val = row.MESES_ATRASO.Value;
                    if (!minMesesAtraso.HasValue || val < minMesesAtraso.Value) minMesesAtraso = val;
                    if (!maxMesesAtraso.HasValue || val > maxMesesAtraso.Value) maxMesesAtraso = val;
                }
                if (row.DIAS_SIN_PAGAR.HasValue)
                {
                    var val = row.DIAS_SIN_PAGAR.Value;
                    if (!minDiasSinPagar.HasValue || val < minDiasSinPagar.Value) minDiasSinPagar = val;
                    if (!maxDiasSinPagar.HasValue || val > maxDiasSinPagar.Value) maxDiasSinPagar = val;
                }
            }

            if (!hasData) return filtros;

            // Ordenamiento únicamente sobre los valores distintos filtrados
            filtros.S_GRUPO.Valores = sGrupoSet.OrderBy(x => x).ToList();
            filtros.GRUPO.Valores = grupoSet.OrderBy(x => x).ToList();
            filtros.PAGO_SOSTENIDO.Valores = pagoSostenidoSet.OrderBy(x => x).ToList();

            filtros.CREDITO_OTORGADO.Min = minCreditoOtorgado;
            filtros.CREDITO_OTORGADO.Max = maxCreditoOtorgado;
            filtros.CREDITO_LIQUIDADO.Min = minCreditoLiquidado;
            filtros.CREDITO_LIQUIDADO.Max = maxCreditoLiquidado;
            filtros.AMORTIZA_INICIO.Min = minAmortizaInicio;
            filtros.AMORTIZA_INICIO.Max = maxAmortizaInicio;
            filtros.AMORTIZA_VENCIMIENTO.Min = minAmortizaVencimiento;
            filtros.AMORTIZA_VENCIMIENTO.Max = maxAmortizaVencimiento;
            filtros.ULTIMO_PAGO.Min = minUltimoPago;
            filtros.ULTIMO_PAGO.Max = maxUltimoPago;

            filtros.PRESTAMO.Min = minPrestamo;
            filtros.PRESTAMO.Max = maxPrestamo;
            filtros.S_CLAVE.Min = minSClave;
            filtros.S_CLAVE.Max = maxSClave;
            filtros.AMORTIZA_NUMERO.Min = minAmortizaNumero;
            filtros.AMORTIZA_NUMERO.Max = maxAmortizaNumero;
            filtros.PRIMERA_AMORTIZACION.Min = minPrimeraAmortizacion;
            filtros.PRIMERA_AMORTIZACION.Max = maxPrimeraAmortizacion;
            filtros.IMPORTE_AMORTIZACION.Min = minImporteAmortizacion;
            filtros.IMPORTE_AMORTIZACION.Max = maxImporteAmortizacion;
            filtros.ABONADO.Min = minAbonado;
            filtros.ABONADO.Max = maxAbonado;
            filtros.SALDO_AMORTIZACION.Min = minSaldoAmortizacion;
            filtros.SALDO_AMORTIZACION.Max = maxSaldoAmortizacion;
            filtros.INTERES_AMORTIZACION.Min = minInteresAmortizacion;
            filtros.INTERES_AMORTIZACION.Max = maxInteresAmortizacion;
            filtros.INTERES_AMORTIZACION.Valores = interesAmortizacionSet.OrderBy(x => x).ToList();
            filtros.IVA_AMORTIZACION.Min = minIvaAmortizacion;
            filtros.IVA_AMORTIZACION.Max = maxIvaAmortizacion;
            filtros.IVA_AMORTIZACION.Valores = ivaAmortizacionSet.OrderBy(x => x).ToList();
            filtros.DIAS_ATRASO.Min = minDiasAtraso;
            filtros.DIAS_ATRASO.Max = maxDiasAtraso;
            filtros.MESES_ATRASO.Min = minMesesAtraso;
            filtros.MESES_ATRASO.Max = maxMesesAtraso;
            filtros.DIAS_SIN_PAGAR.Min = minDiasSinPagar;
            filtros.DIAS_SIN_PAGAR.Max = maxDiasSinPagar;

            return filtros;
        }

        public async Task<int> GenerarYGuardarReporteAsync(ReporteGuardadoRequest request)
        {
            var detalles = await ObtenerReporteProvisionesAsync(request);
            string listaPrestamosStr = request?.ListaPrestamos != null ? string.Join(",", request.ListaPrestamos) : "";
            
            return await _provisionRepository.GuardarReporteAsync(request, listaPrestamosStr, detalles);
        }

        public async Task<IEnumerable<ReporteProvisionDto>> ObtenerReporteGuardadoAsync(int reporteId, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"reporte_provision_guardado_{reporteId}";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<ReporteProvisionDto>? cachedReporte) && cachedReporte != null)
            {
                return cachedReporte;
            }

            var reporte = await _provisionRepository.ObtenerReporteGuardadoAsync(reporteId, cancellationToken);
            if (reporte != null && reporte.Any())
            {
                _cache.Set(cacheKey, reporte, TimeSpan.FromMinutes(60));
            }

            return reporte ?? Enumerable.Empty<ReporteProvisionDto>();
        }

        public async Task<IEnumerable<ReporteProvisionGuardadoDto>> ListarReportesGuardadosAsync()
        {
            return await _provisionRepository.ListarReportesGuardadosAsync();
        }
    }
}
