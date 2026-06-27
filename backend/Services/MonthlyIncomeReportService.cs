using backend.DTOs;
using backend.Interfaces;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace backend.Services
{
    public class MonthlyIncomeReportService : IMonthlyIncomeReportService
    {
        private readonly IMonthlyIncomeReportRepository _repository;

        public MonthlyIncomeReportService(IMonthlyIncomeReportRepository repository)
        {
            _repository = repository;
        }

        public List<MonthlyIncomeReportDto> GetMonthlyIncome(MonthlyIncomeReportFilterDto filter)
        {
            return _repository.GetMonthlyIncome(filter);
        }

        public byte[] ExportToExcel(MonthlyIncomeReportFilterDto filter)
        {
            var rows = _repository.GetMonthlyIncome(filter);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Ingresos por Mes");

            var headers = new[]
            {
                "Mes", "Cantidad de Vuelos",
                "Pasajeros Primera Clase", "Pasajeros Economía", "Total Pasajeros",
                "Ingresos Tiquetes", "Ingresos Maletas", "Total Ingresos"
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
                bool isTotalsRow = item.Mes == "TOTALES";

                sheet.Cell(row, 1).Value = item.Mes;
                sheet.Cell(row, 2).Value = item.CantidadVuelos;
                sheet.Cell(row, 3).Value = item.TotalPasajerosPrimeraClase;
                sheet.Cell(row, 4).Value = item.TotalPasajerosEconomia;
                sheet.Cell(row, 5).Value = item.TotalPasajeros;
                sheet.Cell(row, 6).Value = item.IngresosTiquetes;
                sheet.Cell(row, 7).Value = item.IngresosMaletas;
                sheet.Cell(row, 8).Value = item.TotalIngresos;

                sheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                sheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
                sheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";

                if (isTotalsRow)
                {
                    for (int col = 1; col <= 8; col++)
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

        public byte[] ExportToPdf(MonthlyIncomeReportFilterDto filter)
        {
            var rows     = _repository.GetMonthlyIncome(filter);
            var dataRows = rows.Where(r => r.Mes != "TOTALES").ToList();
            var totals   = rows.FirstOrDefault(r => r.Mes == "TOTALES");

            var orange = QuestPDF.Helpers.Colors.Orange.Medium;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(24);
                    page.DefaultTextStyle(t => t.FontSize(8).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Mushu Airlines — Reporte de Ingresos por Mes")
                            .Bold().FontSize(14).FontColor(orange);
                        col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(7).FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        col.Item().PaddingTop(6).LineHorizontal(1).LineColor(orange);
                    });

                    page.Content().PaddingTop(12).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
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

                        var headers = new[]
                        {
                            "Mes", "Cant. Vuelos", "1ª Clase", "Economía",
                            "Total Pasajeros", "Ing. Tiquetes", "Ing. Maletas", "Total Ingresos"
                        };

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                                header.Cell().Element(HeaderCell)
                                    .Text(h).Bold().FontColor(Colors.White).FontSize(7);
                        });

                        foreach (var item in dataRows)
                        {
                            table.Cell().Element(DataCell).Text(item.Mes);
                            table.Cell().Element(DataCell).AlignCenter().Text(item.CantidadVuelos.ToString());
                            table.Cell().Element(DataCell).AlignCenter().Text(item.TotalPasajerosPrimeraClase.ToString());
                            table.Cell().Element(DataCell).AlignCenter().Text(item.TotalPasajerosEconomia.ToString());
                            table.Cell().Element(DataCell).AlignCenter().Text(item.TotalPasajeros.ToString());
                            table.Cell().Element(DataCell).AlignRight().Text(item.IngresosTiquetes.ToString("N2"));
                            table.Cell().Element(DataCell).AlignRight().Text(item.IngresosMaletas.ToString("N2"));
                            table.Cell().Element(DataCell).AlignRight().Text(item.TotalIngresos.ToString("N2"));
                        }

                        if (totals != null)
                        {
                            table.Cell().Element(TotalsCell).Text("TOTALES").Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignCenter().Text(totals.CantidadVuelos.ToString()).Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignCenter().Text(totals.TotalPasajerosPrimeraClase.ToString()).Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignCenter().Text(totals.TotalPasajerosEconomia.ToString()).Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignCenter().Text(totals.TotalPasajeros.ToString()).Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignRight().Text(totals.IngresosTiquetes.ToString("N2")).Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignRight().Text(totals.IngresosMaletas.ToString("N2")).Bold().FontColor(orange);
                            table.Cell().Element(TotalsCell).AlignRight().Text(totals.TotalIngresos.ToString("N2")).Bold().FontColor(orange);
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
