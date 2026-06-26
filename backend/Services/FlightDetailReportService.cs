using backend.DTOs;
using backend.Interfaces;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace backend.Services
{
    public class FlightDetailReportService : IFlightDetailReportService
    {
        private readonly IFlightDetailReportRepository _repository;

        public FlightDetailReportService(IFlightDetailReportRepository repository)
        {
            _repository = repository;
        }

        public List<FlightDetailReportDto> GetFlightDetail(FlightDetailReportFilterDto filter)
        {
            return _repository.GetFlightDetail(filter);
        }

        public byte[] ExportToExcel(FlightDetailReportFilterDto filter)
        {
            var rows = _repository.GetFlightDetail(filter);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Vuelo Detallado");

            var headers = new[]
            {
                "Fecha", "Origen", "Destino", "Código de Vuelo",
                "Pasajeros Primera Clase", "Pasajeros Turista", "Aerolínea",
                "Venta Pasajeros", "Venta Equipajes", "Total Venta"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = sheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromArgb(0xE8, 0x63, 0x1A);
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = 2;
            foreach (var item in rows)
            {
                bool isTotalsRow = item.Fecha == null;

                sheet.Cell(row, 1).Value  = isTotalsRow ? "TOTALES" : item.Fecha!.Value.ToString("dd/MM/yyyy");
                sheet.Cell(row, 2).Value  = item.Origen   ?? string.Empty;
                sheet.Cell(row, 3).Value  = item.Destino  ?? string.Empty;
                sheet.Cell(row, 4).Value  = item.CodigoVuelo ?? string.Empty;
                sheet.Cell(row, 5).Value  = item.PasajerosPrimeraClase;
                sheet.Cell(row, 6).Value  = item.PasajerosEconomia;
                sheet.Cell(row, 7).Value  = item.Aerolinea;
                sheet.Cell(row, 8).Value  = item.VentaPasajeros;
                sheet.Cell(row, 9).Value  = item.VentaEquipajes;
                sheet.Cell(row, 10).Value = item.TotalVenta;

                sheet.Cell(row, 8).Style.NumberFormat.Format  = "#,##0.00";
                sheet.Cell(row, 9).Style.NumberFormat.Format  = "#,##0.00";
                sheet.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00";

                if (isTotalsRow)
                {
                    for (int col = 1; col <= 10; col++)
                    {
                        sheet.Cell(row, col).Style.Font.Bold = true;
                        sheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0xF4, 0xF6, 0xF8);
                    }
                }

                row++;
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportToPdf(FlightDetailReportFilterDto filter)
        {
            var rows      = _repository.GetFlightDetail(filter);
            var dataRows  = rows.Where(r => r.Fecha != null).ToList();
            var totals    = rows.FirstOrDefault(r => r.Fecha == null);

            var orange = QuestPDF.Helpers.Colors.Orange.Medium;
            var lightGray = "#F4F6F8";

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(24);
                    page.DefaultTextStyle(t => t.FontSize(8).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Mushu Airlines — Reporte de Vuelo Detallado")
                            .Bold().FontSize(14).FontColor(orange);
                        col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(7).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        col.Item().PaddingTop(6).LineHorizontal(1).LineColor(orange);
                    });

                    page.Content().PaddingTop(12).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(QuestPDF.Helpers.Colors.Orange.Medium)
                             .Padding(4).AlignCenter();

                        static IContainer DataCell(IContainer c) =>
                            c.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten3)
                             .Padding(4);

                        static IContainer TotalsCell(IContainer c) =>
                            c.Background("#F4F6F8").Padding(4);

                        var headers = new[] { "Fecha", "Origen", "Destino", "Código Vuelo",
                            "1ª Clase", "Turista", "Aerolínea",
                            "Venta Pasajeros", "Venta Equipajes", "Total Venta" };

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                                header.Cell().Element(HeaderCell)
                                    .Text(h).Bold().FontColor(Colors.White).FontSize(7);
                        });

                        foreach (var item in dataRows)
                        {
                            table.Cell().Element(DataCell).Text(item.Fecha!.Value.ToString("dd/MM/yyyy"));
                            table.Cell().Element(DataCell).Text(item.Origen ?? "");
                            table.Cell().Element(DataCell).Text(item.Destino ?? "");
                            table.Cell().Element(DataCell).Text(item.CodigoVuelo ?? "");
                            table.Cell().Element(DataCell).AlignCenter().Text(item.PasajerosPrimeraClase.ToString());
                            table.Cell().Element(DataCell).AlignCenter().Text(item.PasajerosEconomia.ToString());
                            table.Cell().Element(DataCell).Text(item.Aerolinea);
                            table.Cell().Element(DataCell).AlignRight().Text(item.VentaPasajeros.ToString("N2"));
                            table.Cell().Element(DataCell).AlignRight().Text(item.VentaEquipajes.ToString("N2"));
                            table.Cell().Element(DataCell).AlignRight().Text(item.TotalVenta.ToString("N2"));
                        }

                        if (totals != null)
                        {
                            table.Cell().ColumnSpan(4).Element(TotalsCell).Text("TOTALES").Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignCenter().Text(totals.PasajerosPrimeraClase.ToString()).Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignCenter().Text(totals.PasajerosEconomia.ToString()).Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).Text("").Bold();
                            table.Cell().Element(TotalsCell).AlignRight().Text(totals.VentaPasajeros.ToString("N2")).Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignRight().Text(totals.VentaEquipajes.ToString("N2")).Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignRight().Text(totals.TotalVenta.ToString("N2")).Bold().FontColor(orange);
                        }
                    });

                    page.Footer().AlignRight()
                        .Text(t =>
                        {
                            t.Span("Página ").FontSize(7).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                            t.CurrentPageNumber().FontSize(7).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                            t.Span(" de ").FontSize(7).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                            t.TotalPages().FontSize(7).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        });
                });
            });

            return doc.GeneratePdf();
        }
    }
}
