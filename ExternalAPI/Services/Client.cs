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

        return backendResult.Flights
            /*
            .Where(f => {
                if (DateTime.TryParse(f.ArrivalTime, out var flightArrival))
                {
                    if (flightArrival < targetEarliest || flightArrival > targetLatest)
                        return false;

                    if (!string.IsNullOrWhiteSpace(f.Frequency))
                    {
                        string diaEs = flightArrival.ToString("dddd", new CultureInfo("es-ES")).ToLower();
                        string diaEn = flightArrival.ToString("dddd", new CultureInfo("en-US")).ToLower();
                        string freq = f.Frequency.ToLower();

                        if (diaEs.Contains("miér") || diaEs.Contains("mier")) return freq.Contains("mié") || freq.Contains("mie") || freq.Contains("wed");
                        if (diaEs.Contains("sáb") || diaEs.Contains("sab")) return freq.Contains("sáb") || freq.Contains("sab") || freq.Contains("sat");

                        return freq.Contains(diaEs) || freq.Contains(diaEn) ||
                               (diaEn == "sunday" && freq.Contains("domingo")) ||
                               (diaEn == "monday" && freq.Contains("lunes")) ||
                               (diaEn == "tuesday" && freq.Contains("martes")) ||
                               (diaEn == "thursday" && freq.Contains("jueves")) ||
                               (diaEn == "friday" && freq.Contains("viernes"));
                    }
                    return true;
                }
                return false;
            })
            */
            .Select(f => (object)new
            {
                flightGUID = f.FlightGUID,
                departureTime = f.DepartureTime,
                arrivalTime = f.ArrivalTime,
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