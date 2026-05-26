namespace backend.Model;

public class FlightDto
{
    public string Code { get; set; }
    public string OriginAirport { get; set; }
    public string DestinationAirport { get; set; }
    public string OriginCity { get; set; }
    public string DestinationCity { get; set; }
    public string DepartureTime { get; set; }
    public string ArrivalTime { get; set; }
    public string Duration { get; set; }
    public string AircraftTypeId { get; set; }
    public List<string> Frequency { get; set; }
    public decimal PriceFirstClass { get; set; }
    public decimal PriceEconomy { get; set; }
    public decimal HandBagPrice { get; set; }
    public decimal HandBagWeight { get; set; }
    public decimal BagPrice { get; set; }
    public decimal BagWeight { get; set; }
    public decimal BagMultiplier { get; set; }
    public int EconomyClassCapacity { get; set; }
    public int FirstClassCapacity { get; set; }
}
