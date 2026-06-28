using backend.Interfaces;
using backend.Model;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class AircraftRepository : IAircraftRepository
    {
        private readonly string _connectionString;

        public AircraftRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LoginContext");
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
                    a.EconomyRows,
                    a.EconomySeatsPerRow,
                    a.FirstClassRows,
                    a.FirstClassSeatsPerRow,
                    (
                        a.EconomyRows * a.EconomySeatsPerRow
                        + a.FirstClassRows * a.FirstClassSeatsPerRow
                    ) AS Capacity
                FROM Aircraft a
                JOIN AircraftType aty ON a.[Type] = aty.Id
                WHERE a.IsDeleted = 0
                ORDER BY a.Model;";

            return connection.Query<AircraftResponseModel>(query);
        }

        public AircraftResponseModel? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"
                SELECT
                    a.Code AS Id,
                    a.Model,
                    aty.AircraftType AS Type,
                    a.MaxTakeOffWeight AS WeightKg,
                    a.EconomyRows,
                    a.EconomySeatsPerRow,
                    a.FirstClassRows,
                    a.FirstClassSeatsPerRow,
                    (
                        a.EconomyRows * a.EconomySeatsPerRow
                        + a.FirstClassRows * a.FirstClassSeatsPerRow
                    ) AS Capacity
                FROM Aircraft a
                JOIN AircraftType aty ON a.[Type] = aty.Id
                WHERE a.Code = @Id
                AND a.IsDeleted = 0;";

            return connection.QueryFirstOrDefault<AircraftResponseModel>(
                query,
                new { Id = id }
            );
        }

        public bool AircraftExists(string model, string type)
        {
            using var connection = new SqlConnection(_connectionString);

            int count = connection.ExecuteScalar<int>(@"
                SELECT COUNT(*)
                FROM Aircraft a
                JOIN AircraftType aty ON a.[Type] = aty.Id
                WHERE a.Model = @Model
                AND aty.AircraftType = @TypeName
                AND a.IsDeleted = 0",
                new
                {
                    Model = model,
                    TypeName = type
                });

            return count > 0;
        }

        public void Create(CreateAircraftRequestModel aircraft)
        {
            using var connection = new SqlConnection(_connectionString);

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
                    (
                        Code,
                        Model,
                        [Type],
                        MaxTakeOffWeight,
                        EconomyRows,
                        EconomySeatsPerRow,
                        FirstClassRows,
                        FirstClassSeatsPerRow
                    )
                SELECT
                    ISNULL(MAX(Code), 0) + 1,
                    @Model,
                    @TypeId,
                    @WeightKg,
                    @EconomyRows,
                    @EconomySeatsPerRow,
                    @FirstClassRows,
                    @FirstClassSeatsPerRow
                FROM Aircraft",
                new
                {
                    Model = aircraft.Model,
                    TypeId = typeId,
                    WeightKg = aircraft.WeightKg,
                    EconomyRows = aircraft.EconomyRows,
                    EconomySeatsPerRow = aircraft.EconomySeatsPerRow,
                    FirstClassRows = aircraft.FirstClassRows ?? 0,
                    FirstClassSeatsPerRow = aircraft.FirstClassSeatsPerRow ?? 1
                });
        }

        public void Update(int id, UpdateAircraftRequestModel aircraft)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"
                UPDATE Aircraft
                SET
                    MaxTakeOffWeight = @WeightKg,
                    EconomyRows = @EconomyRows,
                    EconomySeatsPerRow = @EconomySeatsPerRow,
                    FirstClassRows = @FirstClassRows,
                    FirstClassSeatsPerRow = @FirstClassSeatsPerRow
                WHERE Code = @Id";

            connection.Execute(query, new
            {
                Id = id,
                WeightKg = aircraft.WeightKg,
                EconomyRows = aircraft.EconomyRows,
                EconomySeatsPerRow = aircraft.EconomySeatsPerRow,
                FirstClassRows = aircraft.FirstClassRows,
                FirstClassSeatsPerRow = aircraft.FirstClassSeatsPerRow
            });
        }

        public bool Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            try
            {
                bool wasDeleted = connection.ExecuteScalar<bool>(
                    "DeleteAircraft",
                    new { Code = id },
                    commandType: CommandType.StoredProcedure
                );

                return wasDeleted;
            }
            catch
            {
                throw;
            }
        }
    }
}
