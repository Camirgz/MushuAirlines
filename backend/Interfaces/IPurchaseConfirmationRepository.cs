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
        void AddCheckedBagsToTickets(int purchaseId, List<PassengerBaggageAddition> additions, decimal unitPrice);
    }
}