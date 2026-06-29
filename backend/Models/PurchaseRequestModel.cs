namespace backend.Model;

public class PurchaseRequestModel
{
    public FlightSelection?         Flight          { get; set; }
    public FlightSelection?         Flight2         { get; set; }
    public ExternalFlightSelection? ExternalFlight  { get; set; }
    public ExternalFlightSelection? ExternalFlight2 { get; set; }
    public List<PassengerInfo>      Passengers      { get; set; } = [];
    public List<SeatSelection>      SeatSelections  { get; set; } = [];
    public PaymentInfo              Payment         { get; set; } = new();
}

public class FlightSelection
{
    public string   RouteCode  { get; set; } = string.Empty;
    public DateOnly FlightDate { get; set; }
}

public class ExternalFlightSelection
{
    public string  FlightGUID          { get; set; } = string.Empty;
    public string  AirlineName         { get; set; } = string.Empty;
    public string  DepartureTime       { get; set; } = string.Empty;
    public string  ArrivalTime         { get; set; } = string.Empty;
    public string  OriginAirport       { get; set; } = string.Empty;
    public string  DestinationAirport  { get; set; } = string.Empty;
    public decimal TouristPrice        { get; set; }
    public decimal FirstClassPrice     { get; set; }
    public decimal CarryOnPrice        { get; set; }
    public decimal CheckedPrice        { get; set; }
}

public class PaymentInfo
{
    public PaymentMethod Method       { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
}

public class PassengerDuplicateCheckRequest
{
    public string   RouteCode  { get; set; } = string.Empty;
    public DateOnly FlightDate { get; set; }
    public List<PassengerCheckInfo> Passengers { get; set; } = [];
}

public class PassengerCheckInfo
{
    public string   FirstName       { get; set; } = string.Empty;
    public string   LastName        { get; set; } = string.Empty;
    public DateOnly BirthDate       { get; set; }
    public string   PassportCountry { get; set; } = string.Empty;
}
