using backend.Model;

namespace backend.Interfaces;

public interface IPurchaseRepository
{
    Task<bool> IsSeatAvailableAsync(int scheduledFlightId, int seatNumber);
    Task<bool> HasAvailableSeatsAsync(int scheduledFlightId, int requestedCount);
    Task<List<int>> GetNextAvailableSeatNumbersAsync(int scheduledFlightId, int count);
    Task<bool> ReservationCodeExistsAsync(string code);
    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber);
    Task<int> CreatePurchaseAsync(PurchaseRecord record);
    Task CreatePurchaseDetailAsync(int purchaseId, string seatClass, int count, decimal subtotal);
    Task CreatePurchaseBaggageDetailAsync(int purchaseId, string baggageType, int quantity, decimal unitPrice, decimal subtotal);
    Task CreateTicketAsync(int scheduledFlightId, int passengerHas, int seatNumber);
    Task CreateTicketBaggageAsync(int scheduledFlightId, int passengerId, int bookingCode, int handBagCount, int checkedBagCount, decimal subtotal);
    Task LinkItineraryToScheduledFlightAsync(int bookingCode, int scheduledFlightId);
    Task<List<PassengerIdentityRecord>> GetPassengerIdentitiesOnFlightAsync(int scheduledFlightId);
    Task UpdateFlightBookingAsync(int scheduledFlightId, int firstPassengerId, int seatCount);
    Task<int> GetBookedSeatsAsync(int scheduledFlightId);
    Task<int> GetAircraftCapacityByTypeAsync(string aircraftTypeId);
}
