using Dapper;
using System.Data.SqlClient;
using backend.Interfaces;
using backend.Model;

namespace backend.Repositories
{
    public class ReservationCancellationRepository
        : IReservationCancellationRepository
    {
        private readonly string connectionString;

        public ReservationCancellationRepository(IConfiguration configuration)
        {
            connectionString =
                configuration.GetConnectionString("LoginContext")!;
        }

        public CancellationReservationModel? GetReservation(string reservationCode)
        {
            using var connection =
                new SqlConnection(connectionString);

            string sql = @"
               SELECT
                    p.ReservationCode,
                    p.Email,
                    per.FirstName + ' ' + per.LastName AS FullName
                FROM Purchase p
                INNER JOIN Passenger pa
                    ON p.PassengerId = pa.Id
                INNER JOIN Person per
                    ON pa.Id = per.Id
                WHERE p.ReservationCode = @ReservationCode;
            ";

            return connection.QueryFirstOrDefault<CancellationReservationModel>(
                sql,
                new
                {
                    reservationCode
                });
        }

       public void CancelReservation(string token)
        {
            using var connection = new SqlConnection(connectionString);

            connection.Execute(
                "CancelReservation",
                new { ReservationCode = token }, 
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}