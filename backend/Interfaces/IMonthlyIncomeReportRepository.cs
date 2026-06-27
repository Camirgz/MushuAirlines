using backend.DTOs;

namespace backend.Interfaces
{
    public interface IMonthlyIncomeReportRepository
    {
        List<MonthlyIncomeReportDto> GetMonthlyIncome(MonthlyIncomeReportFilterDto filter);
    }
}
