using backend.Model;

namespace backend.Interfaces;

public interface IPurchaseService
{
    Task<bool> IsFlightAvailableAsync(string routeCode, DateOnly flightDate, int firstClassCount, int economyCount);
    Task<List<string>> CheckPassengerDuplicatesAsync(string routeCode, DateOnly flightDate, IEnumerable<PassengerCheckInfo> passengers);
    Task<PurchaseResponseModel> CreatePurchaseAsync(PurchaseRequestModel request);
}
