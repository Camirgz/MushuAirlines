using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/flights")]
public class FlightsController : ControllerBase
{
    private readonly FlightAggregatorService _flightAggregatorService;

    public FlightsController(FlightAggregatorService flightAggregatorService)
    {
        _flightAggregatorService = flightAggregatorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFlights(
        [FromQuery] string date = null,
        [FromQuery] string origin = null,
        [FromQuery] string originType = null,
        [FromQuery] string destination = null,
        [FromQuery] string destinationType = null)
    {
        try
        {
            var localFlights = _flightAggregatorService.GetAllFlights(date, origin, originType, destination, destinationType);
            var externalFlights = await _flightAggregatorService.GetExternalFlightsAsync(destination, destinationType, date);

            return Ok(new
            {
                flights = localFlights,
                externalFlights = externalFlights
            });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return StatusCode(500);
        }
    }
}
