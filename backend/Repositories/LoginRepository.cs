using backend.Model;
using System.Data;
using System.Data.SqlClient;

namespace backend.Repositories
{
    using backend.Model;
    using Dapper;
    using System.Data.SqlClient;
    public class LoginRepository
    {
        private readonly string _connectionString;

        public LoginRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString =
                builder.Configuration.GetConnectionString("LoginContext");
        }

        public string GetPasswordHash(string username)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"SELECT Password 
                            FROM AccountEmployee
                            WHERE Username = @Username";

            return connection.QueryFirstOrDefault<string>(query, new
            {
                Username = username
            });
        }
        public string GetUserRole(string username)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"
                SELECT 
                    CASE 
                        WHEN EXISTS (SELECT 1 FROM Administrator A JOIN AccountEmployee AE ON A.Id = AE.Id WHERE AE.Username = @Username) THEN 'Administrator'
                        ELSE 'Operator'
                    END";

            return connection.ExecuteScalar<string>(query, new { Username = username });
        }
    }
}