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

// Used internally between the service and repository layers.
public class PassengerBaggageUpdate
{
    public string PassengerFullName { get; set; } = string.Empty;
    public int ExtraBags { get; set; }
    public decimal ExtraCost { get; set; }
}

public class BagPricing
{
    public decimal BagPrice { get; set; }
    public decimal BagMultiplier { get; set; }
}
