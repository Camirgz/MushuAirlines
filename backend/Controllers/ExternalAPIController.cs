using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/external")]
public class ExternalApiController : ControllerBase
{
	private readonly ExternalApiService _service;

	public ExternalApiController()
	{
		_service = new ExternalApiService();
	}

	[HttpGet]
	public IActionResult GetFlights(
		[FromQuery] string origin,
		[FromQuery] string destination,
		[FromQuery] string earliestDeparture,
		[FromQuery] string latestDeparture,
		[FromQuery] int quantityOfPassengers = 1,
		[FromQuery] string apiKey = "")
	{
		// 401 - auth
		if (string.IsNullOrWhiteSpace(apiKey) || !_service.ValidateApiKey(apiKey))
			return Unauthorized();

		// 400 - validation
		var (valid, errorCode, errorDescription) = _service.ValidateQueryParams(
			origin, destination, earliestDeparture, latestDeparture, quantityOfPassengers);

		if (!valid)
			return BadRequest(new { code = errorCode, description = errorDescription });

		// 500 - server
		try
		{
			var flights = _service.GetFlights(
				origin, destination, earliestDeparture, latestDeparture, quantityOfPassengers);

			return Ok(new { flights });
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex);
			return StatusCode(500);
		}
	}

	[HttpPost("register")]
	public IActionResult RegisterConsumer([FromBody] RegisterRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Name))
			return BadRequest(new { code = "MISSING_NAME", description = "Name is required" });

		try
		{
			var apiKey = _service.RegisterConsumer(request.Name);
			return Ok(new { apiKey });
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.ToString());
			return StatusCode(500, new { error = ex.Message });
		}
	}

	public class RegisterRequest
	{
		public string Name { get; set; }
	}
}