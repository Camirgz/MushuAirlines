namespace backend.Model;

public class ExternalAirlineConfig
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}

public class ExternalAirlineResponse
{
    public List<ExternalAirlineFlight> Flights { get; set; } = new();
}

public class ExternalAirlineFlight
{
    public string FlightGUID { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public ExternalAirlineAirport? DepartureAirport { get; set; }
    public ExternalAirlineAirport? ArrivalAirport { get; set; }
    public decimal TouristPrice { get; set; }
    public decimal FirstClassPrice { get; set; }
    public decimal CarryOnPrice { get; set; }
    public decimal CheckedPrice { get; set; }
}

public class ExternalAirlineAirport
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class ExternalAirlineFlightDto
{
    public string Airline { get; set; } = string.Empty;
    public string FlightGUID { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public ExternalAirlineAirport? DepartureAirport { get; set; }
    public ExternalAirlineAirport? ArrivalAirport { get; set; }
    public decimal TouristPrice { get; set; }
    public decimal FirstClassPrice { get; set; }
    public decimal CarryOnPrice { get; set; }
    public decimal CheckedPrice { get; set; }
}
