using backend.Model;

namespace backend.Interfaces
{
    public interface IPurchaseConfirmationRepository
    {
        PurchaseConfirmationModel GetPurchase(int purchaseId);
        List<SeatClassSubtotal> GetPurchaseDetails(int purchaseId);
        List<BaggageSubtotal> GetPurchaseBaggageDetails(int purchaseId);
        List<PassengerBaggageDetail> GetPassengerBaggageDetails(int purchaseId);
    }
}