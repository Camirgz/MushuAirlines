using backend.Model;

namespace backend.Interfaces
{
    public interface IPurchaseConfirmationRepository
    {
        PurchaseConfirmationModel GetPurchase(int purchaseId);
        List<PurchaseDetailModel> GetPurchaseDetails(int purchaseId);
    }
}