namespace backend.Model;

public class PurchaseRequestModel
{
    public FlightSelection       Flight         { get; set; } = new();
    public FlightSelection?      Flight2        { get; set; }
    public List<PassengerInfo>   Passengers     { get; set; } = [];
    public List<SeatSelection>   SeatSelections { get; set; } = [];
    public PaymentInfo           Payment        { get; set; } = new();
}

public class FlightSelection
{
    public string   RouteCode  { get; set; } = string.Empty;
    public DateOnly FlightDate { get; set; }
}

public class PaymentInfo
{
    public string Method       { get; set; } = string.Empty;
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
