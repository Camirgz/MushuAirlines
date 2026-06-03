using backend.Model;

namespace backend.Interfaces;

public interface IPurchaseService
{
    Task<bool> IsFlightAvailableAsync(string routeCode, DateOnly flightDate, int requestedCount);
    Task<PurchaseResponseModel> CreatePurchaseAsync(PurchaseRequestModel request);
}
