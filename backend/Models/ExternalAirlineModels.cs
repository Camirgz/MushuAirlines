using System.Text.Json.Serialization;

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

public class ExternalOrderRequest
{
    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonPropertyName("flightGUID")]
    public string FlightGUID { get; set; } = string.Empty;

    [JsonPropertyName("firstClass")]
    public bool FirstClass { get; set; }

    [JsonPropertyName("passengers")]
    public List<ExternalOrderPassenger> Passengers { get; set; } = [];

    [JsonPropertyName("buyer")]
    public ExternalOrderBuyer Buyer { get; set; } = new();

    [JsonPropertyName("payment")]
    public ExternalOrderPayment Payment { get; set; } = new();
}

public class ExternalOrderPassenger
{
    [JsonPropertyName("carryOn")]
    public bool CarryOn { get; set; }

    [JsonPropertyName("checked")]
    public int Checked { get; set; }

    [JsonPropertyName("passport")]
    public string Passport { get; set; } = string.Empty;

    [JsonPropertyName("passportExpirationDate")]
    public string PassportExpirationDate { get; set; } = string.Empty;

    [JsonPropertyName("paspoertCountry")]
    public string PassportCountry { get; set; } = string.Empty;

    [JsonPropertyName("fisrtName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("lastName2")]
    public string? LastName2 { get; set; }

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("birthDate")]
    public string BirthDate { get; set; } = string.Empty;
}

public class ExternalOrderBuyer
{
    [JsonPropertyName("nationality")]
    public string Nationality { get; set; } = string.Empty;

    [JsonPropertyName("fisrtName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("lastName2")]
    public string? LastName2 { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}

public class ExternalOrderPayment
{
    [JsonPropertyName("cardNunber")]
    public string CardNumber { get; set; } = string.Empty;

    [JsonPropertyName("cardEspiration")]
    public string CardExpiration { get; set; } = string.Empty;

    [JsonPropertyName("cvv")]
    public string Cvv { get; set; } = string.Empty;

    [JsonPropertyName("cardHolderName")]
    public string CardHolderName { get; set; } = string.Empty;
}
