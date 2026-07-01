using backend.Model;
using backend.Repositories;

namespace backend.Services;

public class ExternalApiService
{
    private readonly IExternalApiRepository _repo;

    public ExternalApiService(IExternalApiRepository repo)
    {
        _repo = repo;
    }

    public bool ValidateApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return false;
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(apiKey));
        var hash = Convert.ToHexString(bytes).ToLower();
        return _repo.ValidateApiKey(hash);
    }

    public string RegisterConsumer(string name)
    {
        var randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(128);
        var apiKey = Convert.ToHexString(randomBytes).ToLower();

        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(apiKey));
        var hash = Convert.ToHexString(bytes).ToLower();

        var consumer = new APIConsumerModel
        {
            Name = name,
            ApiKeyHash = hash,
            IsActive = true
        };

        _repo.InsertConsumer(consumer);
        return apiKey;
    }

    public List<ExternalFlightDTO> GetFlights(string destination)
    {
        var rows = _repo.GetFlights(destination);

        return rows.Select(r => {
            var rowDict = (IDictionary<string, object>)r;

            string depTime = rowDict.ContainsKey("DepartureTime") && rowDict["DepartureTime"] != null
                ? Convert.ToString(rowDict["DepartureTime"])!.Trim() : "00:00";
            string arrTime = rowDict.ContainsKey("ArrivalTime") && rowDict["ArrivalTime"] != null
                ? Convert.ToString(rowDict["ArrivalTime"])!.Trim() : "00:00";

            return new ExternalFlightDTO
            {
                FlightGUID = rowDict.ContainsKey("FlightGUID") ? Convert.ToString(rowDict["FlightGUID"])! : "UNKNOWN",

                DepartureTime = depTime,
                ArrivalTime = arrTime,

                Duration = rowDict.ContainsKey("Duration") ? Convert.ToString(rowDict["Duration"])! : "00:00",
                OriginAirport = new AirportDTO
                {
                    Code = rowDict.ContainsKey("OriginAirport") ? Convert.ToString(rowDict["OriginAirport"])!.Trim() : "UNK",
                    AirportName = rowDict.ContainsKey("OriginName") && rowDict["OriginName"] != null ? Convert.ToString(rowDict["OriginName"])!.Trim() : "Default Airport",
                    City = rowDict.ContainsKey("OriginCity") && rowDict["OriginCity"] != null ? Convert.ToString(rowDict["OriginCity"])!.Trim() : "Unknown City"
                },
                ArrivalAirport = new AirportDTO
                {
                    Code = rowDict.ContainsKey("DestinationAirport") ? Convert.ToString(rowDict["DestinationAirport"])!.Trim() : "UNK",
                    AirportName = rowDict.ContainsKey("DestinationName") && rowDict["DestinationName"] != null ? Convert.ToString(rowDict["DestinationName"])!.Trim() : "Default Airport",
                    City = rowDict.ContainsKey("DestinationCity") && rowDict["DestinationCity"] != null ? Convert.ToString(rowDict["DestinationCity"])!.Trim() : "Unknown City"
                },
                PriceEconomyClass = rowDict.ContainsKey("PriceEconomy") && rowDict["PriceEconomy"] != null ? Convert.ToDecimal(rowDict["PriceEconomy"]) : 0m,
                PriceFirstClass = rowDict.ContainsKey("PriceFirstClass") && rowDict["PriceFirstClass"] != null ? Convert.ToDecimal(rowDict["PriceFirstClass"]) : 0m,
                HandbagPrice = rowDict.ContainsKey("HandBagPrice") && rowDict["HandBagPrice"] != null ? Convert.ToDecimal(rowDict["HandBagPrice"]) : 0m,
                BagPrice = rowDict.ContainsKey("BagPrice") && rowDict["BagPrice"] != null ? Convert.ToDecimal(rowDict["BagPrice"]) : 0m,
                Frequency = rowDict.ContainsKey("Frequency") ? Convert.ToString(rowDict["Frequency"])! : string.Empty
            };
        }).ToList();
    }

    public async Task<PurchaseResponseModel> OrderFlight(OrderRequest request)
    {
        var flights = _service.GetRoutes();
        string earliest = DateTime.Now.ToString();
        string latest = DateTime.Now.AddDays(7).ToString();

        for (int i = 0; i < flights.Count; i++)
        {
            var flight = flights[i];
            var tasks = _airlines.Select(airline => FetchFromAirlineAsync(airline, flights[i].destination, earliest, latest));
            var results = await Task.WhenAll(tasks);

            return results.SelectMany(r => r);
        }

        throw new InvalidOperationExcelsException();
    }



    private async Task<IEnumerable<ExternalAirlineFlightDto>> FetchFromAirlineAsync(
    ExternalAirlineConfig airline,
    string destination,
    string earliest,
    string latest)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{airline.BaseUrl}/api/external" +
                      $"?destination={Uri.EscapeDataString(destination)}" +
                      $"&earliestDeparture={Uri.EscapeDataString(earliest)}" +
                      $"&latestDeparture={Uri.EscapeDataString(latest)}" +
                      $"&quantityOfPassengers=1" +
                      $"&apiKey={Uri.EscapeDataString(airline.ApiKey)}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<ExternalAirlineFlightDto>();

            var content = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<ExternalAirlineResponse>(content, _jsonOptions);

            if (data?.Flights == null)
                return Enumerable.Empty<ExternalAirlineFlightDto>();

            return data.Flights.Select(f => new ExternalAirlineFlightDto
            {
                Airline = airline.Name,
                FlightGUID = f.FlightGUID,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Duration = f.Duration,
                DepartureAirport = f.DepartureAirport,
                ArrivalAirport = f.ArrivalAirport,
                TouristPrice = f.TouristPrice,
                FirstClassPrice = f.FirstClassPrice,
                CarryOnPrice = f.CarryOnPrice,
                CheckedPrice = f.CheckedPrice
            }).FirstOrDefault() ?? new ExternalAirlineFlightDto();
        }
        catch
        {
            return Enumerable.Empty<ExternalAirlineFlightDto>();
        }
    }
}