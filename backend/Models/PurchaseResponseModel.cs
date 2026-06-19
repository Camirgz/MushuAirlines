namespace backend.Model;

public class PurchaseResponseModel
{
    public int    PurchaseId      { get; set; }
    public string ReservationCode { get; set; } = string.Empty;
    public string InvoiceNumber   { get; set; } = string.Empty;
    public decimal TotalPaid      { get; set; }
    public int    TotalSeats      { get; set; }
    public DateTime PurchaseDate  { get; set; }
    public List<SeatClassSubtotal> DetailByClass { get; set; } = [];
    public List<TicketSummary>     Tickets       { get; set; } = [];
}

public class TicketSummary
{
    public string    PassengerFullName { get; set; } = string.Empty;
    public string    SeatNumber        { get; set; } = string.Empty;
    public SeatClass SeatClass         { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
}
