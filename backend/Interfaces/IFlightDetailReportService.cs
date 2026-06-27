using backend.DTOs;

namespace backend.Interfaces
{
    public interface IFlightDetailReportService
    {
        List<FlightDetailReportDto> GetFlightDetail(FlightDetailReportFilterDto filter);
        byte[] ExportToExcel(FlightDetailReportFilterDto filter);
        byte[] ExportToPdf(FlightDetailReportFilterDto filter);
    }
}
