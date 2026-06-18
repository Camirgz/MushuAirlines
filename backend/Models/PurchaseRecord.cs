namespace backend.Model;

public class PassengerIdentityRecord
{
    public string    FullName       { get; set; } = string.Empty;
    public DateTime? BirthDate      { get; set; }
    public string    PassportCountry { get; set; } = string.Empty;
}

public class PurchaseRecord
{
    public int PassengerId { get; set; }
    public int BookingCode { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string Email { get; set; } = string.Empty;
    public decimal TotalPaid { get; set; }
    public int TotalSeats { get; set; }
    public DateTime PurchaseDate { get; set; }
}
