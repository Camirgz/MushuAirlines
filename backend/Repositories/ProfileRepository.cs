using backend.Interfaces;
using backend.Model;
using Dapper;
using System.Data.SqlClient;

namespace backend.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly string _connectionString;

    public ProfileRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("LoginContext");
    }

    public ProfileModel? GetProfileByUsername(string username)
    {
        const string query = @"
            SELECT TOP 1
                p.FirstName,
                p.LastName,
                p.Ssn,
                p.Nationality,
                ae.Username AS Email,
                e.Salary,
                e.WorkSchedule,
                e.Permissions,
                CASE
                    WHEN adm.Id IS NOT NULL THEN 'Administrator'
                    WHEN op.Id IS NOT NULL THEN 'Operator'
                    ELSE 'Unknown'
                END AS Role
            FROM AccountEmployee ae
            INNER JOIN Employee e ON e.Id = ae.Id
            INNER JOIN Person p ON p.Id = e.Id
            LEFT JOIN Administrator adm ON adm.Id = e.Id
            LEFT JOIN Operator op ON op.Id = e.Id
            WHERE ae.Username = @Username;
        ";

        using var connection = new SqlConnection(_connectionString);

        return connection.QueryFirstOrDefault<ProfileModel>(
            query,
            new { Username = username }
        );
    }

    public bool UpdateBasicProfile(string username, ProfileUpdateModel profile)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            int? employeeId = GetEmployeeIdByUsername(connection, transaction, username);

            if (employeeId == null)
            {
                transaction.Rollback();
                return false;
            }

            ValidateDuplicatedSsn(connection, transaction, employeeId.Value, profile.Ssn);

            connection.Execute(
                @"
                UPDATE Person
                SET
                    FirstName = @FirstName,
                    LastName = @LastName,
                    Ssn = @Ssn,
                    Nationality = @Nationality
                WHERE Id = @EmployeeId;
                ",
                new
                {
                    profile.FirstName,
                    profile.LastName,
                    profile.Ssn,
                    profile.Nationality,
                    EmployeeId = employeeId.Value
                },
                transaction
            );

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public bool UpdateFullProfile(string username, ProfileUpdateModel profile)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            int? employeeId = GetEmployeeIdByUsername(connection, transaction, username);

            if (employeeId == null)
            {
                transaction.Rollback();
                return false;
            }

            ValidateDuplicatedSsn(connection, transaction, employeeId.Value, profile.Ssn);

            connection.Execute(
                @"
                UPDATE Person
                SET
                    FirstName = @FirstName,
                    LastName = @LastName,
                    Ssn = @Ssn,
                    Nationality = @Nationality
                WHERE Id = @EmployeeId;
                ",
                new
                {
                    profile.FirstName,
                    profile.LastName,
                    profile.Ssn,
                    profile.Nationality,
                    EmployeeId = employeeId.Value
                },
                transaction
            );

            connection.Execute(
                @"
                UPDATE Employee
                SET
                    Salary = @Salary,
                    WorkSchedule = @WorkSchedule,
                    Permissions = @Permissions
                WHERE Id = @EmployeeId;
                ",
                new
                {
                    profile.Salary,
                    profile.WorkSchedule,
                    profile.Permissions,
                    EmployeeId = employeeId.Value
                },
                transaction
            );

            UpdateRole(connection, transaction, employeeId.Value, profile.Role);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static int? GetEmployeeIdByUsername(
        SqlConnection connection,
        SqlTransaction transaction,
        string username
    )
    {
        return connection.ExecuteScalar<int?>(
            @"
            SELECT Id
            FROM AccountEmployee
            WHERE Username = @Username;
            ",
            new { Username = username },
            transaction
        );
    }

    private static void ValidateDuplicatedSsn(
        SqlConnection connection,
        SqlTransaction transaction,
        int employeeId,
        string ssn
    )
    {
        int duplicatedSsn = connection.ExecuteScalar<int>(
            @"
            SELECT COUNT(1)
            FROM Person
            WHERE Ssn = @Ssn
              AND Id <> @EmployeeId;
            ",
            new
            {
                Ssn = ssn,
                EmployeeId = employeeId
            },
            transaction
        );

        if (duplicatedSsn > 0)
        {
            throw new InvalidOperationException("Ya existe otro usuario con ese SSN.");
        }
    }

    private static void UpdateRole(
        SqlConnection connection,
        SqlTransaction transaction,
        int employeeId,
        string role
    )
    {
        if (role == "Administrator")
        {
            connection.Execute(
                "DELETE FROM Operator WHERE Id = @EmployeeId;",
                new { EmployeeId = employeeId },
                transaction
            );

            int exists = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Administrator WHERE Id = @EmployeeId;",
                new { EmployeeId = employeeId },
                transaction
            );

            if (exists == 0)
            {
                connection.Execute(
                    "INSERT INTO Administrator (Id) VALUES (@EmployeeId);",
                    new { EmployeeId = employeeId },
                    transaction
                );
            }

            return;
        }

        if (role == "Operator")
        {
            connection.Execute(
                "DELETE FROM Administrator WHERE Id = @EmployeeId;",
                new { EmployeeId = employeeId },
                transaction
            );

            int exists = connection.ExecuteScalar<int>(
                "SELECT COUNT(1) FROM Operator WHERE Id = @EmployeeId;",
                new { EmployeeId = employeeId },
                transaction
            );

            if (exists == 0)
            {
                connection.Execute(
                    "INSERT INTO Operator (Id) VALUES (@EmployeeId);",
                    new { EmployeeId = employeeId },
                    transaction
                );
            }
        }
    }
}