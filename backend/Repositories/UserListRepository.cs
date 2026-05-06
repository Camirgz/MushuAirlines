using backend.Model;
using Dapper;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class UserListRepository
    {
        private readonly string _connectionString;

        public UserListRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("LoginContext");
        }

        public (List<UserListItemModel> Users, int TotalCount) GetUsers(int page, int pageSize, string? search)
        {
            using var connection = new SqlConnection(_connectionString);

            var searchParam = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            int offset = (page - 1) * pageSize;

            const string cte = @"
                WITH UserData AS (
                    SELECT
                        p.FirstName + ' ' + p.LastName AS FullName,
                        p.Ssn,
                        COALESCE(ae.Username, pa_sub.Email, '') AS Email,
                        CASE
                            WHEN adm.Id IS NOT NULL THEN 'Administrador'
                            WHEN op.Id  IS NOT NULL THEN 'Operador'
                            ELSE 'Desconocido'
                        END AS Role
                    FROM Employee e
                    INNER JOIN Person p          ON p.Id   = e.Id
                    LEFT JOIN  AccountEmployee ae ON ae.Id  = e.Id
                    LEFT JOIN (
                        SELECT EmployeeId, MIN(Email) AS Email
                        FROM   PendingAccount
                        GROUP BY EmployeeId
                    ) pa_sub ON pa_sub.EmployeeId = e.Id
                    LEFT JOIN Administrator adm   ON adm.Id = e.Id
                    LEFT JOIN Operator      op    ON op.Id  = e.Id
                )";

            const string where = @"
                WHERE @Search IS NULL
                   OR FullName LIKE '%' + @Search + '%'
                   OR Ssn      LIKE '%' + @Search + '%'
                   OR Email    LIKE '%' + @Search + '%'";

            int total = connection.ExecuteScalar<int>(
                cte + " SELECT COUNT(*) FROM UserData " + where,
                new { Search = searchParam });

            var users = connection.Query<UserListItemModel>(
                cte + @"
                SELECT * FROM UserData"
                + where + @"
                ORDER BY FullName
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                new { Search = searchParam, Offset = offset, PageSize = pageSize })
                .ToList();

            return (users, total);
        }
    }
}
