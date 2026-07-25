using ApiConcilacionFr.Core.Interfaces;
using ApiConcilacionFr.Domain.Entities;
using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Text.RegularExpressions;

namespace ApiConcilacionFr.Core.Services
{
    public class ProvisionService : IProvisionService
    {
        private readonly IProvisionRepository _provisionRepository;

        public ProvisionService(IProvisionRepository provisionRepository)
        {
            _provisionRepository = provisionRepository;
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

        public async Task<IEnumerable<ReporteProvisionDto>> ObtenerReporteProvisionesAsync(ReporteProvisionRequest request)
        {
            string listaPrestamos = request?.ListaPrestamos != null ? string.Join(",", request.ListaPrestamos) : "";
            return await _provisionRepository.ObtenerReporteProvisionesAsync(listaPrestamos, request?.FechaInicio, request?.FechaFin);
        }
    }
}
