using backend.Model;

namespace backend.Interfaces;

public interface IPurchaseRepository
{
    Task<bool> IsSeatAvailableAsync(int scheduledFlightId, int seatNumber);
    Task<bool> ReservationCodeExistsAsync(string code);
    Task<int> CreatePurchaseAsync(PurchaseRecord record);
    Task CreatePurchaseDetailAsync(int purchaseId, string seatClass, int count, decimal subtotal);
    Task CreateTicketAsync(int scheduledFlightId, int passengerHas, int seatNumber);
    Task LinkItineraryToScheduledFlightAsync(int bookingCode, int scheduledFlightId);
}
