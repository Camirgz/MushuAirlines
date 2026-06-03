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

            var purchase = connection.QueryFirstOrDefault<PurchaseConfirmationModel>(
                "GetPurchase",
                new { PurchaseId = purchaseId },
                commandType: CommandType.StoredProcedure
            );

            if (purchase != null)
            {
                purchase.Details = GetPurchaseDetails(purchaseId);
            }

            return purchase;
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
    }
}