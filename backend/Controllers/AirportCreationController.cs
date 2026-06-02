using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AirportCreationController : ControllerBase
{
    private readonly AirportCreationService airportCreationService;

    public AirportCreationController()
    {
        airportCreationService = new AirportCreationService();
    }

    [HttpGet]
    public ActionResult GetAirports()
    {
        var airports = airportCreationService.GetAirports();
        return Ok(airports);
    }

    [HttpGet("countries")]
    public ActionResult GetCountries()
    {
        var countries = airportCreationService.GetCountries();
        return Ok(countries);
    }

    [HttpGet("cities")]
    public ActionResult GetCitiesByCountry([FromQuery] string country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return BadRequest("Debe seleccionar un país.");
        }

        var cities = airportCreationService.GetCitiesByCountry(country);
        return Ok(cities);
    }

    [HttpGet("suggestions")]
    public ActionResult GetSuggestions([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(new List<object>());

        return Ok(airportCreationService.GetSuggestions(q));
    }

    [HttpPost]
    public ActionResult CreateAirport([FromBody] AirportCreationModel airport)
    {
        if (airport == null)
        {
            return BadRequest("Debe ingresar los datos del aeropuerto.");
        }

        var result = airportCreationService.CreateAirport(airport);

        if (string.IsNullOrEmpty(result))
        {
            return Ok("Aeropuerto creado correctamente");
        }

        return BadRequest(result);
    }
}
