using backend.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class RouteCreationRepository
    {
        private readonly string _connectionString;

        public RouteCreationRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString =
                builder.Configuration.GetConnectionString("LoginContext");
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
                    BagMultiplier
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
                    @BagMultiplier
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
                route.BagMultiplier
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