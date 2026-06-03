using backend.Interfaces;
using backend.Model;
using Dapper;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class AircraftTypeRepository : IAircraftTypeRepository
    {
        private readonly string _connectionString;

        public AircraftTypeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LoginContext");
        }

        public IEnumerable<AircraftTypeOptionModel> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"
                SELECT
                    aty.Id,
                    aty.AircraftType AS Name,
                    a.Model AS DefaultModel,
                    a.MaxTakeOffWeight AS DefaultWeightKg,
                    a.EconomyRows AS DefaultEconomyRows,
                    a.EconomySeatsPerRow AS DefaultEconomySeatsPerRow,
                    a.FirstClassRows AS DefaultFirstClassRows,
                    a.FirstClassSeatsPerRow AS DefaultFirstClassSeatsPerRow
                FROM AircraftType aty
                LEFT JOIN Aircraft a
                    ON a.[Type] = aty.Id
                    AND a.Code = (
                        SELECT MIN(Code)
                        FROM Aircraft
                        WHERE [Type] = aty.Id
                    )";

            return connection.Query<AircraftTypeOptionModel>(query);
        }
    }
}
