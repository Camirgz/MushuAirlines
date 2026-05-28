namespace backend.Model;

public class PurchaseRequestModel
{
    public FlightSelection Flight { get; set; } = new();
    public List<PassengerInfo> Passengers { get; set; } = [];
    public List<SeatSelection> SeatSelections { get; set; } = [];
    public PaymentInfo Payment { get; set; } = new();
}

public class FlightSelection
{
    public string RouteCode { get; set; } = string.Empty;
    public DateOnly FlightDate { get; set; }
}

public class PaymentInfo
{
    public string Method { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}
