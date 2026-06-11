using Dapper;
using System.Data;
using System.Data.SqlClient;
using backend.Interfaces;

namespace backend.Repositories;

public class ItineraryRepository : IItineraryRepository
{
    private readonly string _connectionString;

    public ItineraryRepository()
    {
        var builder = WebApplication.CreateBuilder();
        _connectionString = builder.Configuration.GetConnectionString("LoginContext")!;
    }

    // BookingCode has no IDENTITY in the schema, so the next value is derived
    // under UPDLOCK + HOLDLOCK to prevent duplicate keys under concurrent inserts.
    public async Task<int> CreateItineraryAsync(int passengerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            const string getNextCode = @"
                SELECT ISNULL(MAX(BookingCode), 0) + 1
                FROM   Itinerary WITH (UPDLOCK, HOLDLOCK)";

            int bookingCode = await connection.ExecuteScalarAsync<int>(
                getNextCode, transaction: transaction);

            const string insertItinerary = @"
                INSERT INTO Itinerary (BookingCode, PassengerBooks)
                VALUES (@BookingCode, @PassengerId)";

            await connection.ExecuteAsync(insertItinerary, new
            {
                BookingCode = bookingCode,
                PassengerId = passengerId
            }, transaction);

            await transaction.CommitAsync();
            return bookingCode;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
