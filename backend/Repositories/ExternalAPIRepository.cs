using backend.Model;
using Dapper;
using System.Data.SqlClient;

namespace backend.Repositories;

public class ExternalApiRepository
{
    private readonly string _connectionString;

    public ExternalApiRepository()
    {
        var builder = WebApplication.CreateBuilder();
        _connectionString = builder.Configuration.GetConnectionString("LoginContext");
    }

    public bool ValidateApiKey(string apiKeyHashed)
    {
        using var connection = new SqlConnection(_connectionString);
        var result = connection.QueryFirstOrDefault<int>(
            "SELECT COUNT(1) FROM dbo.ExternalApiConsumers WHERE ApiKeyHash = @ApiKeyHash AND IsActive = 1",
            new { ApiKeyHash = apiKeyHashed }
        );
        return result > 0;
    }

    public void InsertConsumer(APIConsumerModel consumer)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Execute(@"
            INSERT INTO ExternalApiConsumers (Name, ApiKeyHash, IsActive)
            VALUES (@Name, @ApiKeyHash, @IsActive)",
            consumer
        );
    }

    public List<dynamic> GetFlights(string origin, string destination,
        DateTime earliestDeparture, DateTime latestDeparture)
    {
        using var connection = new SqlConnection(_connectionString);
        return connection.Query(@"
        SELECT
            r.Code AS FlightGUID,
            r.DepartureTime,
            r.ArrivalTime,
            r.Duration,
            r.OriginAirport,
            r.DestinationAirport,
            r.PriceEconomy,
            r.PriceFirstClass,
            r.HandBagPrice,
            r.BagPrice,
            ao.AirportName AS OriginName,
            ao.City AS OriginCity,
            ad.AirportName AS DestinationName,
            ad.City AS DestinationCity
        FROM dbo.Route r
        JOIN dbo.Airport ao ON r.OriginAirport = ao.Code
        JOIN dbo.Airport ad ON r.DestinationAirport = ad.Code
        WHERE r.OriginAirport = @Origin
          AND r.DestinationAirport = @Destination
          AND r.StartDate <= @LatestDeparture
          AND r.FinalizationDate >= @EarliestDeparture",
            new
            {
                Origin = origin,
                Destination = destination,
                EarliestDeparture = earliestDeparture,
                LatestDeparture = latestDeparture
            }
        ).ToList();
    }
}