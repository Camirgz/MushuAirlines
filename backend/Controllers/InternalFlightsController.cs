using backend.Services;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace backend.Controller;

[Route("api/[controller]")]
[ApiController]
public class InternalFlightsController : ControllerBase
{
    private readonly ExternalApiService _service;

    public InternalFlightsController(ExternalApiService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult GetInternalFlights(
        [FromQuery] string destination = "",
        [FromQuery] string detination = "",
        [FromQuery] string earliest = "",
        [FromQuery] string latest = "",
        [FromQuery] int passengers = 1,
        [FromQuery] string apiKey = "")
    {
        if (string.IsNullOrWhiteSpace(apiKey) || !_service.ValidateApiKey(apiKey))
            return Unauthorized();

        try
        {
            string finalDestination = (!string.IsNullOrWhiteSpace(destination) ? destination : detination).Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(finalDestination))
                return BadRequest(new { error = "El parámetro de destino es requerido." });

            var flights = _service.GetFlights(finalDestination);
            return Ok(new { flights });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Ocurrió un error interno al procesar los vuelos." });
        }
    }

    [HttpPost("register")]
    public IActionResult RegisterInternalConsumer([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Name))
            return BadRequest(new { code = "MISSING_NAME", description = "Name is required" });

        try
        {
            var apiKey = _service.RegisterConsumer(request.Name);
            return Ok(new { apiKey });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    public class RegisterRequest
    {
        public string Name { get; set; } = default!;
    }
}