using backend.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class RouteCreationRepository : IFlightRepository
    {
        private readonly string _connectionString;

        public RouteCreationRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString =
                builder.Configuration.GetConnectionString("LoginContext");
        }

        public RouteCreationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LoginContext");
        }

        public IEnumerable<RouteDbModel> GetAll(string date = null)
        {
            using var connection = new SqlConnection(_connectionString);
            if (date == null)
                return connection.Query<RouteDbModel>("SELECT * FROM Route").ToList();

            return connection.Query<RouteDbModel>(@"
                SELECT r.*
                FROM Route r
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ScheduledFlight sf
                    JOIN Aircraft a ON sf.AircraftCode = a.Code
                    WHERE sf.RouteCode = r.Code
                      AND sf.DepartureDate = @Date
                      AND sf.BookedSeats >= (a.EconomyRows * a.EconomySeatsPerRow
                                           + a.FirstClassRows * a.FirstClassSeatsPerRow)
                )", new { Date = date }).ToList();
        }

        public void InsertRoute(RouteCreationModel route)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"
                INSERT INTO Route
                (
                    Code,
                    OriginAirport,
                    DestinationAirport,
                    DepartureTime,
                    ArrivalTime,
                    Duration,
                    AircraftTypeId,
                    Frequency,
                    PriceFirstClass,
                    PriceEconomy,
                    HandBagPrice,
                    HandBagWeight,
                    BagPrice,
                    BagWeight,
                    BagMultiplier,
                    StartDate,
                    FinalizationDate,
                    EconomyClassCapacity,
                    FirstClassCapacity,
                    OriginCity,
                    DestinationCity
                )
                VALUES
                (
                    @Code,
                    @OriginAirport,
                    @DestinationAirport,
                    @DepartureTime,
                    @ArrivalTime,
                    @Duration,
                    @AircraftTypeId,
                    @Frequency,
                    @PriceFirstClass,
                    @PriceEconomy,
                    @HandBagPrice,
                    @HandBagWeight,
                    @BagPrice,
                    @BagWeight,
                    @BagMultiplier,
                    @StartDate,
                    @FinalizationDate,
                    @EconomyClassCapacity,
                    @FirstClassCapacity,
                    @OriginCity,
                    @DestinationCity
                );";

            connection.Execute(query, new
            {
                route.Code,
                route.OriginAirport,
                route.DestinationAirport,
                route.DepartureTime,
                route.ArrivalTime,
                route.Duration,
                route.AircraftTypeId,
                frequency = string.Join(",", route.Frequency),
                route.PriceFirstClass,
                route.PriceEconomy,
                route.HandBagPrice,
                route.HandBagWeight,
                route.BagPrice,
                route.BagWeight,
                route.BagMultiplier,
                route.StartDate,
                route.FinalizationDate,
                route.EconomyClassCapacity,
                route.FirstClassCapacity,
                route.OriginCity,
                route.DestinationCity
            });
        }

        public List<RouteDbModel> GetRoutes()
        {
            var query = "SELECT * FROM Route";

            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<RouteDbModel>(query).ToList();
            }
        }
    }
}