namespace backend.Model
{
    public class PendingAccountModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Ssn { get; set; }
        public string Nationality { get; set; }

        public double? Salary { get; set; }
        public string WorkSchedule { get; set; }
        public string Permissions { get; set; }

        public string Email { get; set; }
        public string Role { get; set; }

        public string? VerificationToken { get; set; }
        public bool IsVerified { get; set; }
    }
}