using Dapper;
using System.Data.SqlClient;
using backend.Interfaces;
using backend.Model;

namespace backend.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly string _connectionString;

    public PurchaseRepository()
    {
        var builder = WebApplication.CreateBuilder();
        _connectionString = builder.Configuration.GetConnectionString("LoginContext")!;
    }

    public async Task<bool> IsSeatAvailableAsync(int scheduledFlightId, int seatNumber)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
            SELECT COUNT(*)
            FROM   Ticket
            WHERE  ScheduledId = @ScheduledId
            AND    SeatNumber  = @SeatNumber";

        int taken = await connection.ExecuteScalarAsync<int>(query, new
        {
            ScheduledId = scheduledFlightId,
            SeatNumber  = seatNumber
        });

        return taken == 0;
    }

    public async Task<bool> HasAvailableSeatsAsync(int scheduledFlightId, int requestedCount)
    {
        using var connection = new SqlConnection(_connectionString);

        // Tickets already booked on this flight
        const string bookedQuery = @"
            SELECT COUNT(SeatNumber)
            FROM   Ticket
            WHERE  ScheduledId = @ScheduledFlightId";

        int booked = await connection.ExecuteScalarAsync<int>(bookedQuery,
            new { ScheduledFlightId = scheduledFlightId });

        // Total capacity from the route linked to this scheduled flight.
        // Returns null when the route row is not found — treated as fail-open
        // so bad data never silently blocks a purchase.
        const string capacityQuery = @"
            SELECT r.EconomyClassCapacity + r.FirstClassCapacity
            FROM   ScheduledFlight sf
            JOIN   Route           r  ON sf.RouteCode = r.Code
            WHERE  sf.Id = @ScheduledFlightId";

        int? capacity = await connection.QueryFirstOrDefaultAsync<int?>(capacityQuery,
            new { ScheduledFlightId = scheduledFlightId });

        if (capacity == null || capacity == 0)
            return true; // Cannot determine capacity — let the purchase attempt decide

        return (capacity.Value - booked) >= requestedCount;
    }

    public async Task<List<int>> GetNextAvailableSeatNumbersAsync(int scheduledFlightId, int count)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
            SELECT ISNULL(MAX(SeatNumber), 0)
            FROM   Ticket
            WHERE  ScheduledId = @ScheduledFlightId";

        int maxUsed = await connection.ExecuteScalarAsync<int>(query,
            new { ScheduledFlightId = scheduledFlightId });

        return Enumerable.Range(maxUsed + 1, count).ToList();
    }

    public async Task<bool> ReservationCodeExistsAsync(string code)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
            SELECT COUNT(*)
            FROM   Purchase
            WHERE  ReservationCode = @Code";

        int count = await connection.ExecuteScalarAsync<int>(query, new { Code = code });
        return count > 0;
    }

    public async Task<int> CreatePurchaseAsync(PurchaseRecord record)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
            INSERT INTO Purchase
                (PassengerId, BookingCode, ReservationCode, InvoiceNumber,
                 PaymentMethod, Email, TotalPaid, TotalSeats, PurchaseDate)
            VALUES
                (@PassengerId, @BookingCode, @ReservationCode, @InvoiceNumber,
                 @PaymentMethod, @Email, @TotalPaid, @TotalSeats, @PurchaseDate);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await connection.ExecuteScalarAsync<int>(query, record);
    }

    public async Task CreatePurchaseDetailAsync(
        int purchaseId, string seatClass, int count, decimal subtotal)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
            INSERT INTO PurchaseDetail (PurchaseId, SeatClass, SeatCount, Subtotal)
            VALUES (@PurchaseId, @SeatClass, @SeatCount, @Subtotal)";

        await connection.ExecuteAsync(query, new
        {
            PurchaseId = purchaseId,
            SeatClass  = seatClass,
            SeatCount  = count,
            Subtotal   = subtotal
        });
    }

    public async Task CreateTicketAsync(
        int scheduledFlightId, int passengerHas, int seatNumber)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
            INSERT INTO Ticket (ScheduledId, PassengerHas, SeatNumber)
            VALUES (@ScheduledId, @PassengerHas, @SeatNumber)";

        await connection.ExecuteAsync(query, new
        {
            ScheduledId  = scheduledFlightId,
            PassengerHas = passengerHas,
            SeatNumber   = seatNumber
        });
    }

    public async Task LinkItineraryToScheduledFlightAsync(
        int bookingCode, int scheduledFlightId)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
            INSERT INTO ItineraryScheduledFlight (ScheduledId, BookingCode)
            VALUES (@ScheduledId, @BookingCode)";

        await connection.ExecuteAsync(query, new
        {
            ScheduledId = scheduledFlightId,
            BookingCode = bookingCode
        });
    }

    public async Task<List<PassengerIdentityRecord>> GetPassengerIdentitiesOnFlightAsync(int scheduledFlightId)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
            SELECT per.FirstName + ' ' + per.LastName AS FullName,
                   per.BirthDate,
                   per.Nationality AS PassportCountry
            FROM   Ticket     t
            JOIN   Passenger  pa  ON t.PassengerHas = pa.Id
            JOIN   Person     per ON pa.Id           = per.Id
            WHERE  t.ScheduledId = @ScheduledFlightId";

        var identities = await connection.QueryAsync<PassengerIdentityRecord>(query,
            new { ScheduledFlightId = scheduledFlightId });

        return identities.ToList();
    }
}
