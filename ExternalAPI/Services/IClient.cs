using ExternalAPI.Models;

namespace ExternalAPI.Services;

public interface IClient
{
    Task<List<object>> GetFlightsAsync(string destination, DateTime targetEarliest, DateTime targetLatest, int passengers, string apiKey);
    Task<object?> RegisterConsumerAsync(RegisterRequest request);
}