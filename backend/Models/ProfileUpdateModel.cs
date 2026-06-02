namespace backend.Model;

public class ProfileUpdateModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Ssn { get; set; }
    public string Nationality { get; set; }

    // Only administrator could change
    public double Salary { get; set; }
    public string WorkSchedule { get; set; }
    public string Permissions { get; set; }
    public string Role { get; set; }
}
