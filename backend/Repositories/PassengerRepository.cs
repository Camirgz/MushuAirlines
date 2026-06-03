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

    public async Task<int?> FindPassengerByDocumentAsync(string passportNumber)
    {
        using var connection = new SqlConnection(_connectionString);

        const string query = @"
            SELECT p.Id
            FROM   Passenger p
            INNER JOIN Person per ON per.Id = p.Id
            WHERE  per.Ssn = @PassportNumber";

        return await connection.QueryFirstOrDefaultAsync<int?>(query, new { PassportNumber = passportNumber });
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
                INSERT INTO Person (Id, FirstName, LastName, Ssn, Nationality)
                VALUES (@Id, @FirstName, @LastName, @Ssn, @Nationality)";

            await connection.ExecuteAsync(insertPerson, new
            {
                Id          = userId,
                data.FirstName,
                data.LastName,
                Ssn         = data.PassportNumber,
                Nationality = string.Empty
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
}
