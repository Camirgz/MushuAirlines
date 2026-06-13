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

        // Leg 1
        public string FlightNumber { get; set; }
        public string AircraftType { get; set; }
        public string AircraftModel { get; set; }
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
        public string Layover { get; set; }

        // Leg 2 (null for direct flights)
        public string FlightNumber2 { get; set; }
        public string AircraftType2 { get; set; }
        public string AircraftModel2 { get; set; }
        public string OriginAirport2 { get; set; }
        public string DestinationAirport2 { get; set; }
        public DateTime? DepartureDate2 { get; set; }
        public DateTime? ArrivalDate2 { get; set; }

        // Seat details
        public List<SeatClassSubtotal> Details { get; set; } = new();

        // Baggage details
        public List<BaggageSubtotal> BaggageDetails { get; set; } = new();
        public List<PassengerBaggageDetail> PassengerBaggageDetails { get; set; } = new();
        public int HandBaggageCount { get; set; }
        public decimal HandBaggageSubtotal { get; set; }
        public int CheckedBaggageCount { get; set; }
        public decimal CheckedBaggageSubtotal { get; set; }
    }

    public class PassengerBaggageDetail
    {
        public string PassengerFullName { get; set; } = string.Empty;
        public int HandBagCount { get; set; }
        public int CheckedBagCount { get; set; }
        public decimal BaggageSubtotal { get; set; }
    }
}