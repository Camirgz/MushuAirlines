using backend.Interfaces;
using backend.Model;
using Dapper;
using System.Data.SqlClient;

namespace backend.Repositories;

public class UserListRepository : IUserListRepository
{
    private readonly string _connectionString;

    public UserListRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("LoginContext");
    }

    public (List<UserManagementItemModel> Users, int TotalCount) GetUsers(
        int page,
        int pageSize,
        string? search
    )
    {
        using var connection = new SqlConnection(_connectionString);

        var searchParam = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        int offset = (page - 1) * pageSize;

        const string cte = @"
            WITH UserData AS (
                SELECT
                    e.Id,
                    p.FirstName,
                    p.LastName,
                    p.FirstName + ' ' + p.LastName AS FullName,
                    p.Ssn,
                    p.Nationality,
                    COALESCE(ae.Username, pa_sub.Email, '') AS Email,
                    e.Salary,
                    e.WorkSchedule,
                    e.Permissions,
                    CASE
                        WHEN adm.Id IS NOT NULL THEN 'Administrator'
                        WHEN op.Id IS NOT NULL THEN 'Operator'
                        ELSE 'Unknown'
                    END AS Role
                FROM Employee e
                INNER JOIN Person p ON p.Id = e.Id
                LEFT JOIN AccountEmployee ae ON ae.Id = e.Id
                LEFT JOIN (
                    SELECT EmployeeId, MIN(Email) AS Email
                    FROM PendingAccount
                    GROUP BY EmployeeId
                ) pa_sub ON pa_sub.EmployeeId = e.Id
                LEFT JOIN Administrator adm ON adm.Id = e.Id
                LEFT JOIN Operator op ON op.Id = e.Id
            )";

        const string where = @"
            WHERE @Search IS NULL
               OR FullName LIKE '%' + @Search + '%'
               OR FirstName LIKE '%' + @Search + '%'
               OR LastName LIKE '%' + @Search + '%'
               OR Ssn LIKE '%' + @Search + '%'
               OR Email LIKE '%' + @Search + '%'";

        int total = connection.ExecuteScalar<int>(
            cte + " SELECT COUNT(*) FROM UserData " + where,
            new { Search = searchParam }
        );

        var users = connection.Query<UserManagementItemModel>(
            cte + @"
            SELECT *
            FROM UserData "
            + where + @"
            ORDER BY FullName
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            new
            {
                Search = searchParam,
                Offset = offset,
                PageSize = pageSize
            }
        ).ToList();

        return (users, total);
    }

    public bool UpdateUser(int employeeId, UserManagementUpdateModel user)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            bool exists = connection.ExecuteScalar<int>(
                @"
                SELECT COUNT(1)
                FROM Employee
                WHERE Id = @EmployeeId;
                ",
                new { EmployeeId = employeeId },
                transaction
            ) > 0;

            if (!exists)
            {
                transaction.Rollback();
                return false;
            }

            ValidateDuplicatedSsn(connection, transaction, employeeId, user.Ssn!);

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
                    user.FirstName,
                    user.LastName,
                    user.Ssn,
                    user.Nationality,
                    EmployeeId = employeeId
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
                    Salary = user.Salary!.Value,
                    user.WorkSchedule,
                    user.Permissions,
                    EmployeeId = employeeId
                },
                transaction
            );

            UpdateRole(connection, transaction, employeeId, user.Role!);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
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

    public bool DeleteUser(int employeeId)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

    }

}
