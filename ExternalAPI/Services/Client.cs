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

        var cultureEs = new CultureInfo("es-ES");
        var results = new List<object>();

        for (var day = targetEarliest.Date; day <= targetLatest.Date; day = day.AddDays(1))
        {
            string dayOfWeekEs = day.ToString("dddd", cultureEs).ToLower().Trim();

            foreach (var f in backendResult.Flights)
            {
                if (string.IsNullOrWhiteSpace(f.Frequency))
                    continue;

                var frequencyDays = f.Frequency.ToLower()
                                               .Split(',')
                                               .Select(d => d.Trim())
                                               .ToList();

                if (!frequencyDays.Contains(dayOfWeekEs))
                    continue;

                if (!TimeSpan.TryParse(f.DepartureTime, out var depSpan))
                    depSpan = TimeSpan.Zero;
                if (!TimeSpan.TryParse(f.ArrivalTime, out var arrSpan))
                    arrSpan = TimeSpan.Zero;

                var departureDateTime = day + depSpan;

                var arrivalDate = arrSpan < depSpan ? day.AddDays(1) : day;
                var arrivalDateTime = arrivalDate + arrSpan;

                if (departureDateTime < targetEarliest || departureDateTime > targetLatest)
                    continue;

                results.Add(new
                {
                    flightGUID = f.FlightGUID,
                    departureTime = departureDateTime.ToString("yyyy-MM-ddTHH:mm"),
                    arrivalTime = arrivalDateTime.ToString("yyyy-MM-ddTHH:mm"),
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
                });
            }
        }

        return results;
    }

    public async Task<object?> RegisterConsumerAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/InternalFlights/register", request);

        if (!response.IsSuccessStatusCode)
            throw new BackendException((int)response.StatusCode, "Could not register consumer in backend core.");

        return await response.Content.ReadFromJsonAsync<object>();
    }
}