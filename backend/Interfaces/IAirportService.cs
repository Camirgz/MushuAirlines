using backend.DTOs;
using backend.Model;

namespace backend.Interfaces;

public interface IAirportService
{
    List<AirportModel> GetAirports();
    List<AirportCatalogDto> GetCountries();
    List<AirportCatalogDto> GetCitiesByCountry(string country);
    string CreateAirport(AirportModel airport);
    string UpdateAirportName(string code, string airportName);
    string DeleteAirport(string code);
    List<AirportSuggestionDto> GetSuggestions(string query);
}
