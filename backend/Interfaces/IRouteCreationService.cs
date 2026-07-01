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

    void GetOrCreateExternalRoute(
        string flightGuid,
        string airlineName,
        string originAirport,
        string destinationAirport,
        string departureTime,
        string arrivalTime,
        string duration,
        decimal priceFirstClass,
        decimal priceEconomy,
        decimal handBagPrice,
        decimal bagPrice);
}
