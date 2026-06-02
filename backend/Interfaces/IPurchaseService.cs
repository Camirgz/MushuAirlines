using backend.Model;

namespace backend.Interfaces;

public interface IPurchaseService
{
    Task<PurchaseResponseModel> CreatePurchaseAsync(PurchaseRequestModel request);
}
