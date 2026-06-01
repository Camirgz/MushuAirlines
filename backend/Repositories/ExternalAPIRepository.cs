using backend.Model;
using Dapper;
using System.Data.SqlClient;

namespace backend.Repositories;

public class ExternalApiRepository : IExternalApiRepository
{
    private readonly string _connectionString;

    public ExternalApiRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("LoginContext");
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

    public List<dynamic> GetFlights(string destination)
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
                ad.City AS DestinationCity,
                r.Frequency
            FROM dbo.Route r
            LEFT JOIN dbo.Airport ao ON TRIM(r.OriginAirport) = TRIM(ao.Code)
            LEFT JOIN dbo.Airport ad ON TRIM(r.DestinationAirport) = TRIM(ad.Code)
            WHERE TRIM(r.DestinationAirport) = TRIM(@Destination)",
            new { Destination = destination }
        ).ToList();
    }
}