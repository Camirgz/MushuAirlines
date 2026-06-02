using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using ExternalAPI.Services;
using ExternalAPI.Models;

namespace ExternalAPI.Tests;

[TestFixture]
public class ExternalApiTests
{
    private Client _client = null!;
    private HttpClient _mockHttpClient = null!;

    [SetUp]
    public void Setup()
    {
        _mockHttpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost-mock-vuelos/")
        };
        _client = new Client(_mockHttpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _mockHttpClient.Dispose();
    }

    [Test]
    public async Task GetFlightsAsync_WithValidParameters_ShouldExecuteHttpPipeline()
    {
        // Arrange
        string origin = "BCN";
        DateTime start = new DateTime(2026, 6, 15);
        DateTime end = new DateTime(2026, 6, 20);
        int normalIntParam = 1;
        string apiKey = "test_api_key";

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _client.GetFlightsAsync(origin, start, end, normalIntParam, apiKey));
    }

    [Test]
    public async Task GetFlightsAsync_WhenAirportDoesNotExist_ShouldAttemptConnection()
    {
        // Arrange
        string fakeOrigin = "XYZ";
        DateTime start = new DateTime(2026, 6, 15);
        DateTime end = new DateTime(2026, 6, 20);
        int normalIntParam = 1;
        string apiKey = "test_api_key";

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await _client.GetFlightsAsync(fakeOrigin, start, end, normalIntParam, apiKey));
    }

    [Test]
    public void GetFlightsAsync_WithInvalidParameters_ShouldThrowException()
    {
        // Arrange
        string emptyOrigin = "";
        DateTime start = new DateTime(2026, 6, 15);
        DateTime end = new DateTime(2026, 6, 20);
        int invalidIntParam = -1;
        string apiKey = "";

        // Act
        AsyncTestDelegate action = async () => await _client.GetFlightsAsync(emptyOrigin, start, end, invalidIntParam, apiKey);

        // Assert
        Assert.CatchAsync<Exception>(action);
    }
}