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

        const string query = @"
            SELECT CASE
                WHEN (r.EconomyClassCapacity + r.FirstClassCapacity) = 0 THEN 1
                WHEN (r.EconomyClassCapacity + r.FirstClassCapacity) - COUNT(t.SeatNumber) >= @RequestedCount THEN 1
                ELSE 0
            END
            FROM      ScheduledFlight sf
            JOIN      Route  r ON sf.RouteCode = r.Code
            LEFT JOIN Ticket t ON t.ScheduledId = sf.Id
            WHERE     sf.Id = @ScheduledFlightId
            GROUP BY  r.EconomyClassCapacity, r.FirstClassCapacity";

        var result = await connection.QueryFirstOrDefaultAsync<int?>(query, new
        {
            ScheduledFlightId = scheduledFlightId,
            RequestedCount    = requestedCount
        });

        return result is null or 1;
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

    public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
            SELECT COUNT(*)
            FROM   Purchase
            WHERE  InvoiceNumber = @InvoiceNumber";

        int count = await connection.ExecuteScalarAsync<int>(query, new { InvoiceNumber = invoiceNumber });
        return count > 0;
    }

    public async Task<int> ExecutePurchaseTransactionAsync(PurchaseTransactionData data)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            const string getNextBookingCode = @"
                SELECT ISNULL(MAX(BookingCode), 0) + 1
                FROM   Itinerary WITH (UPDLOCK, HOLDLOCK)";

            var bookingCode = await connection.ExecuteScalarAsync<int>(
                getNextBookingCode, transaction: transaction);

            const string insertItinerary = @"
                INSERT INTO Itinerary (BookingCode, PassengerBooks)
                VALUES (@BookingCode, @PassengerId)";

            await connection.ExecuteAsync(insertItinerary, new
            {
                BookingCode = bookingCode,
                PassengerId = data.Record.PassengerId
            }, transaction);

            const string insertPurchase = @"
                INSERT INTO Purchase
                    (PassengerId, BookingCode, ReservationCode, InvoiceNumber,
                     PaymentMethod, Email, TotalPaid, TotalSeats, PurchaseDate)
                VALUES
                    (@PassengerId, @BookingCode, @ReservationCode, @InvoiceNumber,
                     @PaymentMethod, @Email, @TotalPaid, @TotalSeats, @PurchaseDate);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var purchaseId = await connection.ExecuteScalarAsync<int>(insertPurchase, new
            {
                data.Record.PassengerId,
                BookingCode     = bookingCode,
                data.Record.ReservationCode,
                data.Record.InvoiceNumber,
                PaymentMethod   = data.Record.PaymentMethod.ToString(),
                data.Record.Email,
                data.Record.TotalPaid,
                data.Record.TotalSeats,
                data.Record.PurchaseDate
            }, transaction);

            const string insertDetail = @"
                INSERT INTO PurchaseDetail (PurchaseId, SeatClass, SeatCount, Subtotal)
                VALUES (@PurchaseId, @SeatClass, @SeatCount, @Subtotal)";

            await connection.ExecuteAsync(insertDetail, data.Details.Select(d => new
            {
                PurchaseId = purchaseId,
                SeatClass  = d.SeatClass.ToString(),
                SeatCount  = d.SeatCount,
                Subtotal   = d.Subtotal
            }), transaction);

            const string insertBaggageDetail = @"
                INSERT INTO PurchaseBaggageDetail (PurchaseId, BaggageType, Quantity, UnitPrice, Subtotal)
                VALUES (@PurchaseId, @BaggageType, @Quantity, @UnitPrice, @Subtotal)";

            await connection.ExecuteAsync(insertBaggageDetail, data.BaggageDetails.Select(b => new
            {
                PurchaseId  = purchaseId,
                BaggageType = b.Type.ToString(),
                Quantity    = b.Quantity,
                UnitPrice   = b.UnitPrice,
                Subtotal    = b.Subtotal
            }), transaction);

            const string insertTicket = @"
                INSERT INTO Ticket (ScheduledId, PassengerHas, SeatNumber, SeatClass)
                VALUES (@ScheduledId, @PassengerHas, @SeatNumber, @SeatClass)";

            await connection.ExecuteAsync(insertTicket, data.Tickets1.Select(t => new
            {
                ScheduledId  = t.ScheduledFlightId,
                PassengerHas = t.PassengerId,
                SeatNumber   = t.SeatNumber,
                SeatClass    = t.SeatClass
            }), transaction);

            const string insertTicketBaggage = @"
                INSERT INTO TicketBaggage (ScheduledFlightId, PassengerId, BookingCode, HandBagCount, CheckedBagCount, BaggageSubtotal)
                VALUES (@ScheduledFlightId, @PassengerId, @BookingCode, @HandBagCount, @CheckedBagCount, @BaggageSubtotal)";

            await connection.ExecuteAsync(insertTicketBaggage, data.TicketBaggage1.Select(tb => new
            {
                tb.ScheduledFlightId,
                tb.PassengerId,
                BookingCode     = bookingCode,
                tb.HandBagCount,
                tb.CheckedBagCount,
                BaggageSubtotal = tb.Subtotal
            }), transaction);

            const string insertItineraryFlight = @"
                INSERT INTO ItineraryScheduledFlight (ScheduledId, BookingCode)
                VALUES (@ScheduledId, @BookingCode)";

            await connection.ExecuteAsync(insertItineraryFlight, new
            {
                ScheduledId = data.ScheduledId1,
                BookingCode = bookingCode
            }, transaction);

            if (data.Tickets2 != null)
            {
                await connection.ExecuteAsync(insertTicket, data.Tickets2.Select(t => new
                {
                    ScheduledId  = t.ScheduledFlightId,
                    PassengerHas = t.PassengerId,
                    SeatNumber   = t.SeatNumber,
                    SeatClass    = t.SeatClass
                }), transaction);

                await connection.ExecuteAsync(insertTicketBaggage, data.TicketBaggage2!.Select(tb => new
                {
                    tb.ScheduledFlightId,
                    tb.PassengerId,
                    BookingCode     = bookingCode,
                    tb.HandBagCount,
                    tb.CheckedBagCount,
                    BaggageSubtotal = tb.Subtotal
                }), transaction);

                await connection.ExecuteAsync(insertItineraryFlight, new
                {
                    ScheduledId = data.ScheduledId2!.Value,
                    BookingCode = bookingCode
                }, transaction);
            }

            transaction.Commit();
            return purchaseId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateFlightBookingAsync(int scheduledFlightId, int firstPassengerId, int seatCount)
    {
        using var connection = new SqlConnection(_connectionString);

        await connection.ExecuteAsync(@"
            UPDATE ScheduledFlight
            SET    BookedSeats = BookedSeats + @SeatCount
            WHERE  Id = @ScheduledFlightId",
            new { ScheduledFlightId = scheduledFlightId, SeatCount = seatCount });

        await connection.ExecuteAsync(@"
            UPDATE FlightSchedule
            SET    PassengerBooked = @PassengerId
            WHERE  Id IN (
                SELECT FlightScheduleId
                FROM   FlightScheduleHasScheduledFlight
                WHERE  ScheduledFlightId = @ScheduledFlightId
            )
            AND PassengerBooked IS NULL",
            new { ScheduledFlightId = scheduledFlightId, PassengerId = firstPassengerId });
    }

    public async Task<int> GetBookedSeatsAsync(int scheduledFlightId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(@"
            SELECT ISNULL(BookedSeats, 0) FROM ScheduledFlight WHERE Id = @Id",
            new { Id = scheduledFlightId });
    }

    public async Task<int> GetBookedSeatsByClassAsync(int scheduledFlightId, string seatClass)
    {
        using var connection = new SqlConnection(_connectionString);
        const string query = @"
            SELECT COUNT(*)
            FROM   Ticket
            WHERE  ScheduledId = @ScheduledFlightId
            AND    SeatClass   = @SeatClass";

        return await connection.ExecuteScalarAsync<int>(query, new
        {
            ScheduledFlightId = scheduledFlightId,
            SeatClass         = seatClass
        });
    }

    public async Task<int> GetAircraftCapacityByTypeAsync(string aircraftTypeId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.ExecuteScalarAsync<int>(@"
            SELECT TOP 1
                ISNULL(a.EconomyRows * a.EconomySeatsPerRow
                       + a.FirstClassRows * a.FirstClassSeatsPerRow, 0)
            FROM Aircraft a
            JOIN AircraftType aty ON a.[Type] = aty.Id
            WHERE aty.AircraftType = @AircraftTypeId",
            new { AircraftTypeId = aircraftTypeId });
    }

    public async Task<(int FirstClass, int Economy)> GetAircraftCapacityByClassAsync(string aircraftTypeId)
    {
        using var connection = new SqlConnection(_connectionString);
        var row = await connection.QueryFirstOrDefaultAsync(@"
            SELECT TOP 1
                ISNULL(a.FirstClassRows * a.FirstClassSeatsPerRow, 0) AS FirstClass,
                ISNULL(a.EconomyRows    * a.EconomySeatsPerRow,    0) AS Economy
            FROM Aircraft a
            JOIN AircraftType aty ON a.[Type] = aty.Id
            WHERE aty.AircraftType = @AircraftTypeId",
            new { AircraftTypeId = aircraftTypeId });

        if (row == null) return (0, 0);
        return ((int)row.FirstClass, (int)row.Economy);
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
