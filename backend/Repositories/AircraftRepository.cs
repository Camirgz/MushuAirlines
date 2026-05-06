using backend.Model;
using Dapper;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class AircraftRepository
    {
        private readonly string _connectionString;

        public AircraftRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("LoginContext");
        }

        public IEnumerable<AircraftResponseModel> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"
                SELECT
                    a.Code AS Id,
                    a.Model,
                    aty.AircraftType AS Type,
                    a.MaxTakeOffWeight AS WeightKg,
                    (a.EconomyRows * a.EconomySeatsPerRow
                        + a.FirstClassRows * a.FirstClassSeatsPerRow) AS Capacity
                FROM Aircraft a
                JOIN AircraftType aty ON a.[Type] = aty.Id";

            return connection.Query<AircraftResponseModel>(query);
        }

        public bool AircraftExists(string model, string type)
        {
            using var connection = new SqlConnection(_connectionString);
            int count = connection.ExecuteScalar<int>(@"
                SELECT COUNT(*)
                FROM Aircraft a
                JOIN AircraftType aty ON a.[Type] = aty.Id
                WHERE a.Model = @Model AND aty.AircraftType = @TypeName",
                new { Model = model, TypeName = type });
            return count > 0;
        }

        public void Create(CreateAircraftRequestModel aircraft)
        {
            using var connection = new SqlConnection(_connectionString);

            // Resolve or create the AircraftType entry
            int? typeId = connection.QueryFirstOrDefault<int?>(
                "SELECT Id FROM AircraftType WHERE AircraftType = @TypeName",
                new { TypeName = aircraft.Type });

            if (typeId == null)
            {
                connection.Execute(@"
                    INSERT INTO AircraftType (Id, AircraftType)
                    SELECT ISNULL(MAX(Id), 0) + 1, @TypeName
                    FROM AircraftType",
                    new { TypeName = aircraft.Type });

                typeId = connection.ExecuteScalar<int>(
                    "SELECT Id FROM AircraftType WHERE AircraftType = @TypeName",
                    new { TypeName = aircraft.Type });
            }

            connection.Execute(@"
                INSERT INTO Aircraft
                    (Code, Model, [Type], MaxTakeOffWeight,
                     EconomyRows, EconomySeatsPerRow,
                     FirstClassRows, FirstClassSeatsPerRow)
                SELECT
                    ISNULL(MAX(Code), 0) + 1,
                    @Model, @TypeId, @WeightKg,
                    @EconomyRows, @EconomySeatsPerRow,
                    @FirstClassRows, @FirstClassSeatsPerRow
                FROM Aircraft",
                new
                {
                    Model                 = aircraft.Model,
                    TypeId                = typeId,
                    WeightKg              = aircraft.WeightKg,
                    EconomyRows           = aircraft.EconomyRows,
                    EconomySeatsPerRow    = aircraft.EconomySeatsPerRow,
                    FirstClassRows        = aircraft.FirstClassRows ?? 0,
                    FirstClassSeatsPerRow = aircraft.FirstClassSeatsPerRow ?? 1
                });
        }
    }
}
