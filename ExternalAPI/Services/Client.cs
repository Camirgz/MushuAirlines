using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using ExternalAPI.Models;

namespace ExternalAPI.Services;

public class Client : IClient
{
    private readonly HttpClient _httpClient;

    public class BackendException : Exception { public int StatusCode { get; set; } public BackendException(int status, string msg) : base(msg) { StatusCode = status; } }

    public Client(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<object>> GetFlightsAsync(string destination, DateTime targetEarliest, DateTime targetLatest, int passengers, string apiKey)
    {
        var searchStart = targetEarliest.AddDays(-1).ToString("yyyy-MM-dd");
        var searchEnd = targetLatest.AddDays(1).ToString("yyyy-MM-dd");

        var url = $"/api/InternalFlights?destination={destination}&earliest={searchStart}&latest={searchEnd}&passengers={passengers}&apiKey={apiKey}";

        var response = await _httpClient.GetAsync(url);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new BackendException(401, "Unauthorized access to internal core.");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new BackendException(500, errorContent);
        }

        var content = await response.Content.ReadAsStringAsync();
        var backendResult = JsonSerializer.Deserialize<BackendResponseData>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (backendResult?.Flights == null)
            return new List<object>();

        var cultureEs = new System.Globalization.CultureInfo("es-ES");
        return backendResult.Flights
            .Where(f =>
            {
                string searchDayEs = targetLatest.ToString("dddd", cultureEs).ToLower().Trim();

                if (!string.IsNullOrWhiteSpace(f.Frequency))
                {
                    var frequencyDays = f.Frequency.ToLower()
                                                    .Split(',')
                                                    .Select(d => d.Trim())
                                                    .ToList();

                    return frequencyDays.Contains(searchDayEs);
                }

                return false;
            })
            .Select(f => (object)new
            {
                flightGUID = f.FlightGUID,
                departureTime = targetEarliest.ToString("yyyy-MM-dd") + "T" + f.DepartureTime,
                arrivalTime = targetLatest.ToString("yyyy-MM-dd") + "T" + f.ArrivalTime,
                duration = f.Duration,
                departureAirport = new
                {
                    code = f.OriginAirport?.Code,
                    name = f.OriginAirport?.AirportName,
                    city = f.OriginAirport?.City
                },
                arrivalAirport = new
                {
                    code = f.ArrivalAirport?.Code,
                    name = f.ArrivalAirport?.AirportName,
                    city = f.ArrivalAirport?.City
                },
                touristPrice = f.PriceEconomyClass,
                firstClassPrice = f.PriceFirstClass,
                carryOnPrice = f.HandbagPrice,
                checkedPrice = f.BagPrice
            }).ToList();
    }

    public async Task<object?> RegisterConsumerAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/InternalFlights/register", request);

        if (!response.IsSuccessStatusCode)
            throw new BackendException((int)response.StatusCode, "Could not register consumer in backend core.");

        return await response.Content.ReadFromJsonAsync<object>();
    }
}