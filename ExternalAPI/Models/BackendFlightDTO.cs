namespace ExternalAPI.Models;

public class BackendResponseData
{
    public List<BackendFlightDTO> Flights { get; set; } = new();
}

public class BackendFlightDTO
{
    public string FlightGUID { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public AirportLocalDTO? OriginAirport { get; set; }
    public AirportLocalDTO? ArrivalAirport { get; set; }
    public decimal PriceEconomyClass { get; set; }
    public decimal PriceFirstClass { get; set; }
    public decimal HandbagPrice { get; set; }
    public decimal BagPrice { get; set; }
    public string Frequency { get; set; } = string.Empty;
}

public class AirportLocalDTO
{
    public string Code { get; set; } = string.Empty;
    public string AirportName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
}