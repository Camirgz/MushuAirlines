using backend.Model;

namespace backend.Interfaces
{
    public interface IPurchaseConfirmationRepository
    {
        PurchaseConfirmationModel GetPurchase(int purchaseId);
        int GetPurchaseIdByReservationCode(string reservationCode);
        List<SeatClassSubtotal> GetPurchaseDetails(int purchaseId);
        List<BaggageSubtotal> GetPurchaseBaggageDetails(int purchaseId);
        List<PassengerBaggageDetail> GetPassengerBaggageDetails(int purchaseId);
        List<BagPricing> GetBagPricingByPurchaseId(int purchaseId);
        void AddCheckedBagsToTickets(int purchaseId, List<PassengerBaggageUpdate> updates, decimal bagPrice, decimal totalCharged);
    }
}