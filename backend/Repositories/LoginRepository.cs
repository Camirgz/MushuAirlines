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

        public bool ValidateUser(LoginModel login)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"SELECT COUNT(*) 
                             FROM AccountEmployee
                             WHERE username = @Username 
                             AND password = @Password";

            int count = connection.ExecuteScalar<int>(query, new
            {
                Username = login.Username,
                Password = login.Password
            });

            return count > 0;
        }
    }
}