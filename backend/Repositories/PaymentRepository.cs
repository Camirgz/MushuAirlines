using System.Data.SqlClient;
using Dapper;

using backend.Interfaces;
using backend.Model;

namespace backend.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string connectionString;

        public PaymentRepository(
            IConfiguration configuration)
        {
            connectionString =
                configuration.GetConnectionString(
                    "LoginContext"
                );
        }

        public int CreatePurchase(
            PurchaseRecord purchase)
        {
            using var connection =
                new SqlConnection(connectionString);

            string query = @"
            INSERT INTO Purchase
            (
                PassengerId,
                BookingCode,
                ReservationCode,
                InvoiceNumber,
                PaymentMethod,
                Email,
                TotalPaid,
                TotalSeats,
                PurchaseDate,
                EmailSent
            )
            VALUES
            (
                @PassengerId,
                @BookingCode,
                @ReservationCode,
                @InvoiceNumber,
                @PaymentMethod,
                @Email,
                @TotalPaid,
                @TotalSeats,
                @PurchaseDate,
                0
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            return connection.QuerySingle<int>(
                query,
                purchase
            );
        }

        public void CreatePurchaseDetail(
            int purchaseId,
            SeatClassSubtotal detail)
        {
            using var connection =
                new SqlConnection(connectionString);

            string query = @"
            INSERT INTO PurchaseDetail
            (
                PurchaseId,
                SeatClass,
                SeatCount,
                Subtotal
            )
            VALUES
            (
                @PurchaseId,
                @SeatClass,
                @SeatCount,
                @Subtotal
            );
            ";

            connection.Execute(
                query,
                new
                {
                    PurchaseId = purchaseId,
                    detail.SeatClass,
                    detail.SeatCount,
                    detail.Subtotal
                }
            );
        }
    }
}