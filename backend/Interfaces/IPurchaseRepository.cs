using backend.Model;

namespace backend.Interfaces;

public interface IPurchaseRepository
{
    Task<bool> IsSeatAvailableAsync(int scheduledFlightId, int seatNumber);
    Task<bool> HasAvailableSeatsAsync(int scheduledFlightId, int requestedCount);
    Task<List<int>> GetNextAvailableSeatNumbersAsync(int scheduledFlightId, int count);
    Task<bool> ReservationCodeExistsAsync(string code);
    Task<bool> InvoiceNumberExistsAsync(string invoiceNumber);
    Task<int> ExecutePurchaseTransactionAsync(PurchaseTransactionData data);
    Task<List<PassengerIdentityRecord>> GetPassengerIdentitiesOnFlightAsync(int scheduledFlightId);
    Task UpdateFlightBookingAsync(int scheduledFlightId, int firstPassengerId, int seatCount);
    Task<int> GetBookedSeatsAsync(int scheduledFlightId);
    Task<int> GetAircraftCapacityByTypeAsync(string aircraftTypeId);
}
