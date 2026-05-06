using backend.Model;

namespace backend.Repositories
{
    using Dapper;
    using System.Data.SqlClient;

    public class PendingAccountRepository
    {
        private readonly string _connectionString;

        public PendingAccountRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString =
                builder.Configuration.GetConnectionString("LoginContext");
        }

        public int CreateUser()
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"INSERT INTO [User] DEFAULT VALUES;
                             SELECT CAST(SCOPE_IDENTITY() as int);";

            return connection.ExecuteScalar<int>(query);
        }

        public void CreatePerson(PendingAccountModel model, int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"INSERT INTO Person (Id, FirstName, LastName, Ssn, Nationality)
                             VALUES (@Id, @FirstName, @LastName, @Ssn, @Nationality)";

            connection.Execute(query, new
            {
                Id = id,
                model.FirstName,
                model.LastName,
                model.Ssn,
                model.Nationality
            });
        }

        public void CreateEmployee(PendingAccountModel model, int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"INSERT INTO Employee (Id, Url, Salary, WorkSchedule, Permissions)
                             VALUES (@Id, @Url, @Salary, @WorkSchedule, @Permissions)";

            connection.Execute(query, new
            {
                Id = id,
                model.Url,
                model.Salary,
                model.WorkSchedule,
                model.Permissions
            });
        }

        public void CreateAdministrator(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"INSERT INTO Administrator (Id)
                             VALUES (@Id)";

            connection.Execute(query, new { Id = id });
        }

        public void CreateOperator(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"INSERT INTO Operator (Id)
                             VALUES (@Id)";

            connection.Execute(query, new { Id = id });
        }

        public void SaveInvitation(PendingAccountModel model)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"INSERT INTO PendingAccount (EmployeeId, Email, Role, VerificationToken, IsVerified)
                             VALUES (@EmployeeId, @Email, @Role, @VerificationToken, @IsVerified)";

            connection.Execute(query, model);
        }

        public PendingAccountModel GetByToken(string token)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"SELECT *
                             FROM PendingAccount
                             WHERE VerificationToken = @Token
                             AND IsVerified = 0";

            return connection.QueryFirstOrDefault<PendingAccountModel>(query, new
            {
                Token = token
            });
        }

        public void CreateAccountEmployee(int id, string email, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"INSERT INTO AccountEmployee (Id, Account, Username, Password)
                             VALUES (@Id, 'GeneratedAccount', @Username, @Password)";

            connection.Execute(query, new
            {
                Id = id,
                Username = email,
                Password = password
            });
        }

        public void MarkAsVerified(string token)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"UPDATE PendingAccount
                             SET IsVerified = 1
                             WHERE VerificationToken = @Token";

            connection.Execute(query, new
            {
                Token = token
            });
        }
        public bool EmailExists(string email)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"SELECT COUNT(*) FROM AccountEmployee WHERE Username = @Email";

            int count = connection.ExecuteScalar<int>(query, new { Email = email });

            return count > 0;
        }
    
        public bool PendingEmailExists(string email)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = @"SELECT COUNT(*) 
                            FROM PendingAccount 
                            WHERE Email = @Email
                            AND IsVerified = 0";

            int count = connection.ExecuteScalar<int>(query, new { Email = email });

            return count > 0;
        }
        public void DeletePendingByEmployeeId(int employeeId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("DELETE FROM PendingAccount WHERE EmployeeId = @Id", new { Id = employeeId });
        }

        public void DeleteUserCascade(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            connection.Execute("DELETE FROM Administrator WHERE Id = @Id", new { Id = id });
            connection.Execute("DELETE FROM Operator WHERE Id = @Id", new { Id = id });
            connection.Execute("DELETE FROM Employee WHERE Id = @Id", new { Id = id });
            connection.Execute("DELETE FROM Person WHERE Id = @Id", new { Id = id });
            connection.Execute("DELETE FROM [User] WHERE Id = @Id", new { Id = id });
        }
    }
}