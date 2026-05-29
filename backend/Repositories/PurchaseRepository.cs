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
}
