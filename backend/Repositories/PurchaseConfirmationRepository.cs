using backend.Model;
using backend.Interfaces;

namespace backend.Repositories
{
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

            string query = @"
            SELECT p.Id AS PurchaseId,
            per.FirstName + ' ' + per.LastName AS FullName,
            pass.InternationalId AS PassportNumber,
            p.Email,
            p.ReservationCode,
            p.InvoiceNumber,
            p.PaymentMethod,
            p.TotalPaid,
            p.TotalSeats,
            CAST(sf.Id AS VARCHAR) AS FlightNumber,
            at.AircraftType,
            fs.OriginAirport,
            fs.DestinationAirport,
            CAST(fs.DepartureDate AS DATETIME) + CAST(fs.DepartureTime AS DATETIME) AS DepartureDate,
            CAST(fs.ArrivalDate AS DATETIME) + CAST(fs.ArrivalTime AS DATETIME) AS ArrivalDate,
            i.Layover
            FROM Purchase p
            INNER JOIN Passenger pa ON p.PassengerId = pa.Id
            INNER JOIN Person per ON pa.Id = per.Id
            INNER JOIN Passport pass ON pa.Id = pass.PassengerHas
            INNER JOIN Itinerary i ON p.BookingCode = i.BookingCode
            INNER JOIN ItineraryScheduledFlight isf ON i.BookingCode = isf.BookingCode
            INNER JOIN ScheduledFlight sf ON isf.ScheduledId = sf.Id
            INNER JOIN Aircraft a ON sf.AircraftCode = a.Code
            INNER JOIN AircraftType at ON a.Type = at.Id
            INNER JOIN FlightScheduleHasScheduledFlight fshsf ON sf.Id = fshsf.ScheduledFlightId
            INNER JOIN FlightSchedule fs ON fshsf.FlightScheduleId = fs.Id
            WHERE p.Id = @PurchaseId";

            var purchase = connection.QueryFirstOrDefault <PurchaseConfirmationModel>(query, new{PurchaseId = purchaseId});
            if (purchase != null)
            {
                purchase.Details = GetPurchaseDetails(purchaseId);
            }
            return purchase;

        }

        public List<SeatClassSubtotal>GetPurchaseDetails(int purchaseId)
        {
            using var connection =new SqlConnection(connectionString);
            string query = @"
            SELECT SeatClass,SeatCount,Subtotal
            FROM PurchaseDetail
            WHERE PurchaseId = @PurchaseId
            ";

            return connection.Query<SeatClassSubtotal>(query,new{PurchaseId = purchaseId}).ToList();
        }
    }
}