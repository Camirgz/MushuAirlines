using backend.DTOs;
using backend.Interfaces;
using backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Route("api/airport")]
[ApiController]
public class AirportController : ControllerBase
{
    private readonly IAirportService _airportService;

    public AirportController(IAirportService airportService)
    {
        _airportService = airportService;
    }

    [HttpGet]
    public ActionResult<List<AirportModel>> GetAirports()
    {
        var airports = _airportService.GetAirports();

        return Ok(airports);
    }

    [HttpGet("countries")]
    public ActionResult<List<AirportCatalogDto>> GetCountries()
    {
        var countries = _airportService.GetCountries();

        return Ok(countries);
    }

    [HttpGet("cities")]
    public ActionResult<List<AirportCatalogDto>> GetCitiesByCountry(
        [FromQuery] string country
    )
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return BadRequest("Debe seleccionar un país.");
        }

        var cities = _airportService.GetCitiesByCountry(country);

        return Ok(cities);
    }

    [HttpPost]
    public ActionResult CreateAirport([FromBody] AirportModel airport)
    {
        string result = _airportService.CreateAirport(airport);

        if (!string.IsNullOrWhiteSpace(result))
        {
            return BadRequest(result);
        }

        return Ok("Aeropuerto creado correctamente.");
    }

    [HttpPut("{code}")]
    public ActionResult UpdateAirportName(
        [FromRoute] string code,
        [FromBody] AirportModel airport
    )
    {
        if (airport == null)
        {
            return BadRequest("Debe ingresar los datos del aeropuerto.");
        }

        string result = _airportService.UpdateAirportName(
            code,
            airport.AirportName
        );

        if (!string.IsNullOrWhiteSpace(result))
        {
            return BadRequest(result);
        }

        return Ok("Nombre del aeropuerto actualizado correctamente.");
    }
}
