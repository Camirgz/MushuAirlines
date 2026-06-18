using Microsoft.AspNetCore.Mvc;
using ExternalAPI.Models;
using ExternalAPI.Services;

namespace ExternalAPI.Controllers;

[ApiController]
[Route("api/external")]
public class ExternalApiController : ControllerBase
{
    private readonly IClient _client;

    public ExternalApiController(IClient client)
    {
        _client = client;
    }

    [HttpGet]
    public async Task<IActionResult> GetFlights(
        [FromQuery] string destination,
        [FromQuery] string earliestDeparture,
        [FromQuery] string latestDeparture,
        [FromQuery] int quantityOfPassengers,
        [FromQuery] string apiKey = "")
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(destination)) return BadRequest(new { code = "MISSING_AIRPORTS", description = "Destination airport is required" });

        if (!DateTime.TryParse(earliestDeparture, out var targetEarliest) || !DateTime.TryParse(latestDeparture, out var targetLatest))
            return BadRequest(new { code = "INVALID_DATE_FORMAT", description = "Dates must be in ISO format YYYY-MM-DDThh:mm" });

        if (quantityOfPassengers == null) return BadRequest(new { code = "MISSING_PASSENGERS", description = "quantityOfPassengers is required" });

        if (quantityOfPassengers < 1 || quantityOfPassengers == null) return BadRequest(new { code = "INVALID_PASSENGERS", description = "quantityOfPassengers is required and it's value must be >= 1" });
        if (targetEarliest > targetLatest) return BadRequest(new { code = "INVALID_DATE_RANGE", description = "earliestDeparture must be before latestDeparture" });

        try
        {
            var formattedFlights = await _client.GetFlightsAsync(destination, targetEarliest, targetLatest, quantityOfPassengers, apiKey);
            return Ok(new { flights = formattedFlights });
        }
        catch (Client.BackendException ex)
        {
            if (ex.StatusCode == 401) return Unauthorized();
            return StatusCode(500, new { code = "INTERNAL_SERVER_ERROR", description = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { code = "INTERNAL_SERVER_ERROR", description = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterConsumer([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
            return BadRequest(new { code = "MISSING_NAME", description = "Name is required" });

        try
        {
            var result = await _client.RegisterConsumerAsync(request);
            return Ok(result);
        }
        catch (Client.BackendException ex)
        {
            return StatusCode(ex.StatusCode, new { code = "INTERNAL_SERVER_ERROR", description = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { code = "INTERNAL_SERVER_ERROR", description = ex.Message });
        }
    }
}