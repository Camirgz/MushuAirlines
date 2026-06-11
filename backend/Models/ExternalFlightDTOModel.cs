namespace backend.Model;

public class ExternalFlightDTO
{
    public string FlightGUID { get; set; }
    public string DepartureTime { get; set; }
    public string ArrivalTime { get; set; }
    public string Duration { get; set; }
    public AirportDTO OriginAirport { get; set; }
    public AirportDTO ArrivalAirport { get; set; }
    public decimal PriceEconomyClass { get; set; }
    public decimal PriceFirstClass { get; set; }
    public decimal HandbagPrice { get; set; }
    public decimal BagPrice { get; set; }
    public string Frequency { get; set; } = string.Empty;
}

public class AirportDTO
{
    public string Code { get; set; }
    public string AirportName { get; set; }
    public string City { get; set; }
}