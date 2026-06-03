using backend.DTOs;
using backend.Model;

namespace backend.Interfaces;

public interface IAirportRepository
{
    List<AirportModel> GetAirports();
    List<AirportCatalogDto> GetCountries();
    List<AirportCatalogDto> GetCitiesByCountry(string country);
    AirportModel? GetAirportByCode(string code);
    bool CityBelongsToCountry(string country, string city);
    bool AirportCodeExists(string code);
    bool AirportNameExists(string airportName, string country, string city);
    bool AirportNameExistsInLocationExceptCode(
        string airportName,
        string country,
        string city,
        string excludedCode
    );
    void InsertAirport(AirportModel airport);
    bool UpdateAirportName(string code, string airportName);
    List<AirportSuggestionDto> GetSuggestions(string query);
}
