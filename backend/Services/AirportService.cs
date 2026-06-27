using backend.DTOs;
using backend.Interfaces;
using backend.Model;

namespace backend.Services;

public class AirportService : IAirportService
{
    private readonly IAirportRepository _airportRepository;

    public AirportService(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
    }

    public List<AirportModel> GetAirports()
    {
        return _airportRepository.GetAirports();
    }

    public List<AirportCatalogDto> GetCountries()
    {
        return _airportRepository.GetCountries();
    }

    public List<AirportCatalogDto> GetCitiesByCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return new List<AirportCatalogDto>();
        }

        return _airportRepository.GetCitiesByCountry(country.Trim());
    }

    public string CreateAirport(AirportModel airport)
    {
        if (airport == null)
        {
            return "Debe ingresar los datos del aeropuerto.";
        }

        airport.Normalize();

        string validationMessage = airport.ValidateForCreation();

        if (!string.IsNullOrEmpty(validationMessage))
        {
            return validationMessage;
        }

        if (!_airportRepository.CityBelongsToCountry(airport.Country, airport.City))
        {
            return "La ciudad seleccionada no pertenece al país seleccionado.";
        }

        if (_airportRepository.AirportCodeExists(airport.Code))
        {
            return "Ya existe un aeropuerto con ese código.";
        }

        if (_airportRepository.AirportNameExists(
            airport.AirportName,
            airport.Country,
            airport.City
        ))
        {
            return "Ya existe un aeropuerto con ese nombre en la misma ciudad y país.";
        }

        _airportRepository.InsertAirport(airport);

        return string.Empty;
    }

    public string UpdateAirportName(string code, string airportName)
    {
        string normalizedCode = AirportModel.NormalizeCode(code);
        string normalizedAirportName = airportName?.Trim() ?? string.Empty;

        string codeValidation = AirportModel.ValidateCode(normalizedCode);

        if (!string.IsNullOrWhiteSpace(codeValidation))
        {
            return codeValidation;
        }

        string nameValidation = AirportModel.ValidateAirportName(normalizedAirportName);

        if (!string.IsNullOrWhiteSpace(nameValidation))
        {
            return nameValidation;
        }

        AirportModel? currentAirport = _airportRepository.GetAirportByCode(normalizedCode);

        if (currentAirport == null)
        {
            return "No existe un aeropuerto con ese código.";
        }

        if (string.Equals(
            currentAirport.AirportName.Trim(),
            normalizedAirportName,
            StringComparison.OrdinalIgnoreCase
        ))
        {
            return "El nuevo nombre del aeropuerto debe ser diferente al nombre actual.";
        }

        if (_airportRepository.AirportNameExistsInLocationExceptCode(
            normalizedAirportName,
            currentAirport.Country,
            currentAirport.City,
            normalizedCode
        ))
        {
            return "Ya existe un aeropuerto con ese nombre en la misma ciudad y país.";
        }

        bool wasUpdated = _airportRepository.UpdateAirportName(
            normalizedCode,
            normalizedAirportName
        );

        if (!wasUpdated)
        {
            return "No se pudo actualizar el nombre del aeropuerto.";
        }

        return string.Empty;
    }

    public string DeleteAirport(string code)
    {
        string normalizedCode = AirportModel.NormalizeCode(code);

        string codeValidation = AirportModel.ValidateCode(normalizedCode);

        if (!string.IsNullOrWhiteSpace(codeValidation))
        {
            return codeValidation;
        }

        AirportModel? airport = _airportRepository.GetAirportByCode(normalizedCode);

        if (airport == null)
        {
            return "No existe un aeropuerto con ese código.";
        }

        try
        {
            bool wasDeleted = _airportRepository.DeleteAirport(normalizedCode);

            if (!wasDeleted)
            {
                return "No se encontró el aeropuerto que desea eliminar.";
            }

            return string.Empty;
        }
        catch
        {
            return "No se pudo eliminar el aeropuerto.";
        }
    }

    public List<AirportSuggestionDto> GetSuggestions(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<AirportSuggestionDto>();
        }

        return _airportRepository.GetSuggestions(query.Trim());
    }
}
