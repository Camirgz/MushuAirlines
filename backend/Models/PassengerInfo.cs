namespace backend.Model;

public class PassengerInfo
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string PassportCountry { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int HandBagCount    { get; set; }
    public int CheckedBagCount { get; set; }
}
