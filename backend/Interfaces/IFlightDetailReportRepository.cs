using backend.DTOs;

namespace backend.Interfaces
{
    public interface IFlightDetailReportRepository
    {
        List<FlightDetailReportDto> GetFlightDetail(FlightDetailReportFilterDto filter);
    }
}
