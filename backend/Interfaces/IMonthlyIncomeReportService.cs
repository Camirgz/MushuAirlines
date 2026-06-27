using backend.DTOs;

namespace backend.Interfaces
{
    public interface IMonthlyIncomeReportService
    {
        List<MonthlyIncomeReportDto> GetMonthlyIncome(MonthlyIncomeReportFilterDto filter);
        byte[] ExportToExcel(MonthlyIncomeReportFilterDto filter);
        byte[] ExportToPdf(MonthlyIncomeReportFilterDto filter);
    }
}
