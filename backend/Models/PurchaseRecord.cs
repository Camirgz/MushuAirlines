namespace backend.Model;

public class PurchaseRecord
{
    public int PassengerId { get; set; }
    public int BookingCode { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalPaid { get; set; }
    public int TotalSeats { get; set; }
    public DateTime PurchaseDate { get; set; }
}
