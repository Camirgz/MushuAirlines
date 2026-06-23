using Dapper;
using System.Data.SqlClient;
using backend.Interfaces;
using backend.Model;

namespace backend.Repositories
{
    public class ReservationLoginRepository : IReservationLoginRepository
    {
        private readonly string connectionString;

        public ReservationLoginRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("LoginContext");
        }

        public bool ReservationExists(ReservationLoginModel model)
        {
            using var connection = new SqlConnection(connectionString);

            string query = @"
                SELECT COUNT(*)
                FROM Purchase PU
                INNER JOIN Passenger PA
                    ON PA.Id = PU.PassengerId
                INNER JOIN Person PE
                    ON PE.Id = PA.Id
                INNER JOIN Itinerary I
                    ON I.PassengerBooks = PA.Id
                WHERE
                    PU.ReservationCode = @ReservationCode
                    AND PE.FirstName = @FirstName
                    AND PE.LastName = @LastName
                    AND I.Status = 'Active'
            ";

            return connection.ExecuteScalar<int>(query, model) > 0;
        }
    }
}