using backend.Model;

namespace backend.Interfaces;

public interface IRouteCreationService
{
    RouteCreationModel GetRouteByCode(string code);
    int? FindExistingScheduledFlight(string routeCode, DateTime date);
    int  GetOrCreateScheduledFlight(string routeCode, DateTime date);
}
