using System.Net.Http.Json;
using System.Text.Json;
using backend.Model;
using Microsoft.Extensions.Configuration;

namespace backend.Services;

public interface IExternalAirlinesService
{
    Task<IEnumerable<ExternalAirlineFlightDto>> GetExternalFlightsAsync(
        string destination,
        string? date);

    Task BookExternalFlightAsync(string airlineName, ExternalOrderRequest order);
}

public class ExternalAirlinesService : IExternalAirlinesService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly List<ExternalAirlineConfig> _airlines;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ExternalAirlinesService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _airlines = configuration.GetSection("ExternalAirlines")
            .Get<List<ExternalAirlineConfig>>() ?? new List<ExternalAirlineConfig>();
    }

    public async Task<IEnumerable<ExternalAirlineFlightDto>> GetExternalFlightsAsync(
        string destination,
        string? date)
    {
        DateTime targetDate = DateTime.TryParse(date, out var parsed) ? parsed : DateTime.Today;
        string earliest = targetDate.ToString("yyyy-MM-ddT00:00");
        string latest = targetDate.ToString("yyyy-MM-ddT23:59");

        var tasks = _airlines.Select(airline => FetchFromAirlineAsync(airline, destination, earliest, latest));
        var results = await Task.WhenAll(tasks);

        return results.SelectMany(r => r);
    }

    public async Task BookExternalFlightAsync(string airlineName, ExternalOrderRequest order)
    {
        var airline = _airlines.FirstOrDefault(a =>
            string.Equals(a.Name, airlineName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Aerolínea externa '{airlineName}' no configurada.");

        order.ApiKey = airline.ApiKey;

        var client   = _httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync(
            $"{airline.BaseUrl}/api/external/order", order, _jsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"La aerolínea '{airlineName}' rechazó la reserva ({(int)response.StatusCode}): {body}");
        }
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
            });
        }
        catch
        {
            return Enumerable.Empty<ExternalAirlineFlightDto>();
        }
    }
}
