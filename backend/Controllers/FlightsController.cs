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
    public IActionResult GetFlights(
        [FromQuery] string date = null,
        [FromQuery] string origin = null,
        [FromQuery] string originType = null,
        [FromQuery] string destination = null,
        [FromQuery] string destinationType = null)
    {
        try
        {
            var flights = _flightAggregatorService.GetAllFlights(date, origin, originType, destination, destinationType);
            return Ok(flights);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return StatusCode(500);
        }
    }
}
