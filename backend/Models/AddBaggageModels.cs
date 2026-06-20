namespace backend.Model;

public class AddBaggageRequest
{
    public List<PassengerBaggageAddition> Passengers { get; set; } = [];
}

public class PassengerBaggageAddition
{
    public string PassengerFullName { get; set; } = string.Empty;
    public int ExtraBags { get; set; }
}

public class AddBaggageResponse
{
    public decimal TotalCharged { get; set; }
    public int TotalBagsAdded { get; set; }
}
