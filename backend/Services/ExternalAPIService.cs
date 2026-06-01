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

            return new ExternalFlightDTO
            {
                FlightGUID = rowDict.ContainsKey("FlightGUID") ? Convert.ToString(rowDict["FlightGUID"])! : "UNKNOWN",
                DepartureTime = rowDict.ContainsKey("DepartureTime") && rowDict["DepartureTime"] != null
                    ? Convert.ToDateTime(rowDict["DepartureTime"]).ToString("yyyy-MM-ddTHH:mm") : string.Empty,
                ArrivalTime = rowDict.ContainsKey("ArrivalTime") && rowDict["ArrivalTime"] != null
                    ? Convert.ToDateTime(rowDict["ArrivalTime"]).ToString("yyyy-MM-ddTHH:mm") : string.Empty,
                Duration = rowDict.ContainsKey("Duration") ? Convert.ToString(rowDict["Duration"])! : "00:00",
                OriginAirport = new AirportDTO
                {
                    Code = rowDict.ContainsKey("OriginAirport") ? Convert.ToString(rowDict["OriginAirport"])! : "UNK",
                    AirportName = rowDict.ContainsKey("OriginName") && rowDict["OriginName"] != null ? Convert.ToString(rowDict["OriginName"])! : "Default Airport",
                    City = rowDict.ContainsKey("OriginCity") && rowDict["OriginCity"] != null ? Convert.ToString(rowDict["OriginCity"])! : "Unknown City"
                },
                ArrivalAirport = new AirportDTO
                {
                    Code = rowDict.ContainsKey("DestinationAirport") ? Convert.ToString(rowDict["DestinationAirport"])! : "UNK",
                    AirportName = rowDict.ContainsKey("DestinationName") && rowDict["DestinationName"] != null ? Convert.ToString(rowDict["DestinationName"])! : "Default Airport",
                    City = rowDict.ContainsKey("DestinationCity") && rowDict["DestinationCity"] != null ? Convert.ToString(rowDict["DestinationCity"])! : "Unknown City"
                },
                PriceEconomyClass = rowDict.ContainsKey("PriceEconomy") && rowDict["PriceEconomy"] != null ? Convert.ToDecimal(rowDict["PriceEconomy"]) : 0m,
                PriceFirstClass = rowDict.ContainsKey("PriceFirstClass") && rowDict["PriceFirstClass"] != null ? Convert.ToDecimal(rowDict["PriceFirstClass"]) : 0m,
                HandbagPrice = rowDict.ContainsKey("HandBagPrice") && rowDict["HandBagPrice"] != null ? Convert.ToDecimal(rowDict["HandBagPrice"]) : 0m,
                BagPrice = rowDict.ContainsKey("BagPrice") && rowDict["BagPrice"] != null ? Convert.ToDecimal(rowDict["BagPrice"]) : 0m
            };
        }).ToList();
    }
}