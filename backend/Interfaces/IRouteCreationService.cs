using backend.Model;

namespace backend.Interfaces;

public interface IRouteCreationService
{
    string CreateRoute(RouteCreationModel route);
    List<RouteCreationModel> GetRoutes();
    RouteCreationModel? GetRouteByCode(string code);
    int? FindExistingScheduledFlight(string routeCode, DateTime date);
    int GetOrCreateScheduledFlight(string routeCode, DateTime date);
    string DeleteRoute(string code);
}
