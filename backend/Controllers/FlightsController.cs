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
    public IActionResult GetFlights([FromQuery] string date = null)
    {
        try
        {
            var flights = _flightAggregatorService.GetAllFlights(date);
            return Ok(flights);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return StatusCode(500);
        }
    }
}
