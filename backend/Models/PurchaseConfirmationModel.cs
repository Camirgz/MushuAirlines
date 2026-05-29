namespace backend.Model
{
    public class PurchaseConfirmationModel
    {
        public int PurchaseId { get; set; }
        public string FullName { get; set; }
        public int PassportNumber { get; set; }
        public string Email { get; set; }
        public string ReservationCode { get; set; }
        public string InvoiceNumber { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalPaid { get; set; }
        public int TotalSeats { get; set; }
        public string FlightNumber { get; set; }
        public string AircraftType { get; set; }
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string Layover { get; set; }
        public List<SeatClassSubtotal> Details { get; set; } = new();
    }
}