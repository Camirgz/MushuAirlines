using backend.Model;
using backend.Interfaces;

namespace backend.Repositories
{
    using System.Data;
    using System.Data.SqlClient;
    using Dapper;
    public class PurchaseConfirmationRepository : IPurchaseConfirmationRepository
    {
        private readonly string connectionString;
        public PurchaseConfirmationRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("LoginContext");
        }

        public PurchaseConfirmationModel GetPurchase(int purchaseId)
        {
            using var connection = new SqlConnection(connectionString);

            const string sql = @"
                WITH Legs AS (
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY
                            CAST(fs.DepartureDate AS DATETIME) + CAST(fs.DepartureTime AS DATETIME)
                        ) AS LegOrder,
                        sf.RouteCode                                                                AS FlightNumber,
                        at.AircraftType                                                             AS AircraftType,
                        a.Model                                                                     AS AircraftModel,
                        fs.OriginAirport,
                        fs.DestinationAirport,
                        CAST(fs.DepartureDate AS DATETIME) + CAST(fs.DepartureTime AS DATETIME)    AS DepartureDate,
                        CAST(fs.ArrivalDate   AS DATETIME) + CAST(fs.ArrivalTime   AS DATETIME)    AS ArrivalDate
                    FROM Purchase p
                    INNER JOIN Itinerary i     ON p.BookingCode       = i.BookingCode
                    INNER JOIN ItineraryScheduledFlight isf ON i.BookingCode = isf.BookingCode
                    INNER JOIN ScheduledFlight sf  ON isf.ScheduledId        = sf.Id
                    INNER JOIN Aircraft a          ON sf.AircraftCode        = a.Code
                    INNER JOIN AircraftType at     ON a.Type                 = at.Id
                    INNER JOIN FlightScheduleHasScheduledFlight fshsf ON sf.Id = fshsf.ScheduledFlightId
                    INNER JOIN FlightSchedule fs   ON fshsf.FlightScheduleId = fs.Id
                    WHERE p.Id = @PurchaseId
                )
                SELECT
                    p.Id                                    AS PurchaseId,
                    per.FirstName + ' ' + per.LastName      AS FullName,
                    pass.InternationalId                    AS PassportNumber,
                    p.Email,
                    p.ReservationCode,
                    p.InvoiceNumber,
                    p.PaymentMethod,
                    p.TotalPaid,
                    p.TotalSeats,
                    l1.FlightNumber,
                    l1.AircraftType,
                    l1.AircraftModel,
                    l1.OriginAirport,
                    l1.DestinationAirport,
                    l1.DepartureDate,
                    l1.ArrivalDate,
                    i.Layover,
                    l2.FlightNumber     AS FlightNumber2,
                    l2.AircraftType     AS AircraftType2,
                    l2.AircraftModel    AS AircraftModel2,
                    l2.OriginAirport    AS OriginAirport2,
                    l2.DestinationAirport AS DestinationAirport2,
                    l2.DepartureDate    AS DepartureDate2,
                    l2.ArrivalDate      AS ArrivalDate2
                FROM Purchase p
                INNER JOIN Passenger pa    ON p.PassengerId = pa.Id
                INNER JOIN Person per      ON pa.Id         = per.Id
                LEFT  JOIN Passport pass   ON pa.Id         = pass.PassengerHas
                INNER JOIN Itinerary i     ON p.BookingCode = i.BookingCode
                INNER JOIN Legs l1         ON l1.LegOrder   = 1
                LEFT  JOIN Legs l2         ON l2.LegOrder   = 2
                WHERE p.Id = @PurchaseId";

            var purchase = connection.QueryFirstOrDefault<PurchaseConfirmationModel>(
                sql, new { PurchaseId = purchaseId });

            if (purchase != null)
            {
                purchase.Details = GetPurchaseDetails(purchaseId);
                purchase.BaggageDetails = GetPurchaseBaggageDetails(purchaseId);
                purchase.PassengerBaggageDetails = GetPassengerBaggageDetails(purchaseId);
                purchase.Tickets = GetPassengerTickets(purchaseId);
                var handBaggage = purchase.BaggageDetails.FirstOrDefault(b => b.Type == BaggageType.HandBaggage);
                if (handBaggage != null)
                {
                    purchase.HandBaggageCount    = handBaggage.Quantity;
                    purchase.HandBaggageSubtotal = handBaggage.Subtotal;
                }

                var checkedBaggage = purchase.BaggageDetails.FirstOrDefault(b => b.Type == BaggageType.CheckedBaggage);
                if (checkedBaggage != null)
                {
                    purchase.CheckedBaggageCount    = checkedBaggage.Quantity;
                    purchase.CheckedBaggageSubtotal = checkedBaggage.Subtotal;
                }

                var legs = GetBagPricingByPurchaseId(purchaseId);
                purchase.BagLegs       = legs;
                purchase.BagPrice      = legs.Sum(l => l.BagPrice);
                purchase.BagMultiplier = legs.FirstOrDefault()?.BagMultiplier ?? 1;
            }

            return purchase;
        }
       
        public int GetPurchaseIdByReservationCode(string reservationCode)
        {
            using var connection = new SqlConnection(connectionString);

            const string query = @"
                SELECT Id
                FROM Purchase
                WHERE ReservationCode = @ReservationCode";

            var purchaseId = connection.QueryFirstOrDefault<int?>(
                query,
                new { ReservationCode = reservationCode });

            if (purchaseId == null)
            {
                throw new Exception("Reserva no encontrada.");
            }

            return purchaseId.Value;
        }

       public List<SeatClassSubtotal> GetPurchaseDetails(int purchaseId)
        {
            using var connection = new SqlConnection(connectionString);

            return connection.Query<SeatClassSubtotal>(
                "GetPurchaseDetails",
                new { PurchaseId = purchaseId },
                commandType: CommandType.StoredProcedure
            ).ToList();
        }

        public List<BaggageSubtotal> GetPurchaseBaggageDetails(int purchaseId)
        {
            using var connection = new SqlConnection(connectionString);

            const string query = @"
                SELECT
                    BaggageType AS Type,
                    Quantity,
                    UnitPrice,
                    Subtotal
                FROM PurchaseBaggageDetail
                WHERE PurchaseId = @PurchaseId
                ORDER BY BaggageType";

            return connection.Query<BaggageSubtotal>(query, new { PurchaseId = purchaseId }).ToList();
        }

        public List<PassengerBaggageDetail> GetPassengerBaggageDetails(int purchaseId)
        {
            using var connection = new SqlConnection(connectionString);

            const string query = @"
                SELECT
                    per.FirstName + ' ' + per.LastName AS PassengerFullName,
                    tb.HandBagCount,
                    tb.CheckedBagCount,
                    tb.BaggageSubtotal
                FROM TicketBaggage tb
                INNER JOIN Passenger pa  ON tb.PassengerId = pa.Id
                INNER JOIN Person    per ON pa.Id          = per.Id
                WHERE tb.BookingCode = (SELECT BookingCode FROM Purchase WHERE Id = @PurchaseId)
                AND tb.ScheduledFlightId = (
                    SELECT TOP 1 isf.ScheduledId
                    FROM Purchase p
                    INNER JOIN ItineraryScheduledFlight isf ON p.BookingCode = isf.BookingCode
                    WHERE p.Id = @PurchaseId
                    ORDER BY isf.ScheduledId
                )
                ORDER BY per.FirstName, per.LastName";

            return connection.Query<PassengerBaggageDetail>(query, new { PurchaseId = purchaseId }).ToList();
        }
        public List<TicketSummary> GetPassengerTickets(int purchaseId)
        {
            using var connection = new SqlConnection(connectionString);

            const string query = @"
                SELECT
                    per.FirstName + ' ' + per.LastName AS PassengerFullName,
                    CAST(t.SeatNumber AS VARCHAR)      AS SeatNumber,
                    t.SeatClass,
                    sf.RouteCode                       AS FlightNumber
                FROM Ticket t
                INNER JOIN ScheduledFlight sf
                    ON sf.Id = t.ScheduledId
                INNER JOIN Passenger pa
                    ON t.PassengerHas = pa.Id
                INNER JOIN Person per
                    ON pa.Id = per.Id
                WHERE t.ScheduledId =
                (
                    SELECT TOP 1 isf.ScheduledId
                    FROM Purchase p
                    INNER JOIN ItineraryScheduledFlight isf
                        ON p.BookingCode = isf.BookingCode
                    WHERE p.Id = @PurchaseId
                    ORDER BY isf.ScheduledId
                )
                AND t.PassengerHas IN
                (
                    SELECT tb.PassengerId
                    FROM TicketBaggage tb
                    WHERE tb.BookingCode =
                    (
                        SELECT BookingCode FROM Purchase WHERE Id = @PurchaseId
                    )
                )
                ORDER BY per.FirstName, per.LastName";

            return connection.Query<TicketSummary>(query, new { PurchaseId = purchaseId }).ToList();
        }

        public List<BagPricing> GetBagPricingByPurchaseId(int purchaseId)
        {
            using var connection = new SqlConnection(connectionString);

            const string sql = @"
                SELECT
                    r.BagPrice,
                    r.BagMultiplier
                FROM Purchase p
                INNER JOIN ItineraryScheduledFlight isf
                    ON p.BookingCode = isf.BookingCode
                INNER JOIN ScheduledFlight sf
                    ON isf.ScheduledId = sf.Id
                INNER JOIN Route r
                    ON sf.RouteCode = r.Code
                WHERE p.Id = @PurchaseId
                ORDER BY isf.ScheduledId";

            var legs = connection.Query<BagPricing>(sql, new { PurchaseId = purchaseId }).ToList();

            if (legs.Count == 0)
                throw new Exception("No se encontró la información de precios para esta compra.");

            return legs;
        }

        public void AddCheckedBagsToTickets(int purchaseId, List<PassengerBaggageUpdate> updates, decimal bagPrice, decimal totalCharged)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();

            const string getBookingCode = @"
                SELECT BookingCode FROM Purchase WHERE Id = @PurchaseId";

            var bookingCode = connection.QueryFirstOrDefault<int?>(
                getBookingCode, new { PurchaseId = purchaseId }, tx)
                ?? throw new Exception("Compra no encontrada.");

            const string updateTicketBaggage = @"
                UPDATE tb
                SET
                    tb.CheckedBagCount = tb.CheckedBagCount + @ExtraBags,
                    tb.BaggageSubtotal = tb.BaggageSubtotal + @ExtraCost
                FROM TicketBaggage tb
                INNER JOIN Passenger pa  ON tb.PassengerId = pa.Id
                INNER JOIN Person    per ON pa.Id          = per.Id
                WHERE tb.BookingCode = @BookingCode
                  AND per.FirstName + ' ' + per.LastName = @PassengerFullName";

            foreach (var update in updates)
            {
                connection.Execute(updateTicketBaggage, new
                {
                    update.ExtraBags,
                    update.ExtraCost,
                    BookingCode = bookingCode,
                    update.PassengerFullName
                }, tx);
            }

            int totalExtraBags = updates.Sum(u => u.ExtraBags);

            const string upsertBaggageDetail = @"
                IF EXISTS (
                    SELECT 1 FROM PurchaseBaggageDetail
                    WHERE PurchaseId = @PurchaseId AND BaggageType = 'CheckedBaggage'
                )
                    UPDATE PurchaseBaggageDetail
                    SET Quantity = Quantity + @TotalExtraBags,
                        Subtotal = Subtotal + @TotalCharged
                    WHERE PurchaseId = @PurchaseId AND BaggageType = 'CheckedBaggage'
                ELSE
                    INSERT INTO PurchaseBaggageDetail (PurchaseId, BaggageType, Quantity, UnitPrice, Subtotal)
                    VALUES (@PurchaseId, 'CheckedBaggage', @TotalExtraBags, @BagPrice, @TotalCharged)";

            connection.Execute(upsertBaggageDetail, new
            {
                PurchaseId = purchaseId,
                TotalExtraBags = totalExtraBags,
                TotalCharged = totalCharged,
                BagPrice = bagPrice
            }, tx);

            const string updateTotalPaid = @"
                UPDATE Purchase SET TotalPaid = TotalPaid + @TotalCharged WHERE Id = @PurchaseId";

            connection.Execute(updateTotalPaid, new
            {
                TotalCharged = totalCharged,
                PurchaseId = purchaseId
            }, tx);

            tx.Commit();
        }
    }
}