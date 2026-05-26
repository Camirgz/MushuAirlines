using backend.Model;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AirportController : ControllerBase
{
    private readonly AirportService airportService;

    public AirportController()
    {
        airportService = new AirportService();
    }

    [HttpGet]
    public ActionResult GetAirports()
    {
        var airports = airportService.GetAirports();
        return Ok(airports);
    }

    [HttpGet("countries")]
    public ActionResult GetCountries()
    {
        var countries = airportService.GetCountries();
        return Ok(countries);
    }

    [HttpGet("cities")]
    public ActionResult GetCitiesByCountry([FromQuery] string country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return BadRequest("Debe seleccionar un país.");
        }

        var cities = airportService.GetCitiesByCountry(country);
        return Ok(cities);
    }

    [HttpPost]
    public ActionResult CreateAirport([FromBody] AirportModel airport)
    {
        if (airport == null)
        {
            return BadRequest("Debe ingresar los datos del aeropuerto.");
        }

        var result = airportService.CreateAirport(airport);

        if (string.IsNullOrEmpty(result))
        {
            return Ok("Aeropuerto creado correctamente");
        }

        return BadRequest(result);
    }
}
