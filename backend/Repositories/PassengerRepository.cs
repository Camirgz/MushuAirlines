using Dapper;
using System.Data.SqlClient;
using backend.Interfaces;
using backend.Model;

namespace backend.Repositories;

public class PassengerRepository : IPassengerRepository
{
    private readonly string _connectionString;

    public PassengerRepository()
    {
        var builder = WebApplication.CreateBuilder();
        _connectionString = builder.Configuration.GetConnectionString("LoginContext")!;
    }

    public async Task<int> CreatePassengerAsync(PassengerInfo data)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {
            const string insertUser = @"
                INSERT INTO [User] DEFAULT VALUES;
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int userId = await connection.ExecuteScalarAsync<int>(
                insertUser, transaction: transaction);

            const string insertPerson = @"
                INSERT INTO Person (Id, FirstName, LastName, Ssn, Nationality, BirthDate)
                VALUES (@Id, @FirstName, @LastName, @Ssn, @Nationality, @BirthDate)";

            await connection.ExecuteAsync(insertPerson, new
            {
                Id          = userId,
                data.FirstName,
                data.LastName,
                Ssn         = Guid.NewGuid().ToString("N")[..20],
                Nationality = data.PassportCountry,
                BirthDate   = data.BirthDate.ToDateTime(TimeOnly.MinValue)
            }, transaction);

            const string insertPassenger = @"
                INSERT INTO Passenger (Id) VALUES (@Id)";

            await connection.ExecuteAsync(insertPassenger, new { Id = userId }, transaction);

            await transaction.CommitAsync();
            return userId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeletePassengersAsync(IEnumerable<int> passengerIds)
    {
        var ids = passengerIds.ToList();
        if (ids.Count == 0) return;

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            DELETE FROM Passenger WHERE Id IN @Ids;
            DELETE FROM Person    WHERE Id IN @Ids;
            DELETE FROM [User]    WHERE Id IN @Ids;";

        await connection.ExecuteAsync(sql, new { Ids = ids });
    }
}
