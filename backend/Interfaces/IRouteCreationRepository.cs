using backend.Model;

namespace backend.Interfaces;

public interface IRouteCreationRepository
{
    void InsertRoute(RouteCreationModel route);
    List<RouteDbModel> GetRoutes();
    RouteDbModel? GetRouteByCode(string routeCode);
    IEnumerable<RouteDbModel> GetAll(
        string date = null,
        string origin = null,
        string originType = null,
        string destination = null,
        string destinationType = null
    );

    int GetOrCreateScheduledFlight(string routeCode, DateTime date);
    int? FindExistingScheduledFlight(string routeCode, DateTime date);
    bool DeleteRoute(string code);
}
