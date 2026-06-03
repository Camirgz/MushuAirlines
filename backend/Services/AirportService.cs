using backend.DTOs;
using backend.Interfaces;
using backend.Model;
using System.Text.RegularExpressions;

namespace backend.Services;

public class AirportService : IAirportService
{
    private const int AirportCodeLength = 3;
    private const int AirportNameMaxLength = 200;
    private static readonly Regex AirportNameRegex = new(
        @"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s.'-]+$",
        RegexOptions.Compiled
    );
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

        NormalizeAirport(airport);

        string validationMessage = ValidateAirportForCreation(airport);

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
        if (string.IsNullOrWhiteSpace(code))
        {
            return "Debe indicar el código del aeropuerto.";
        }

        if (string.IsNullOrWhiteSpace(airportName))
        {
            return "Debe ingresar el nuevo nombre del aeropuerto.";
        }

        string normalizedCode = code.Trim().ToUpper();
        string normalizedAirportName = airportName.Trim();

        if (normalizedCode.Length != AirportCodeLength)
        {
            return "El código del aeropuerto debe tener exactamente 3 caracteres.";
        }

        if (normalizedAirportName.Length > AirportNameMaxLength)
        {
            return $"El nombre del aeropuerto no puede superar los {AirportNameMaxLength} caracteres.";
        }

        if (!AirportNameRegex.IsMatch(normalizedAirportName))
        {
            return "El nombre del aeropuerto no debe contener caracteres especiales como #, !, %, $.";
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

    private static string ValidateAirportForCreation(AirportModel airport)
    {
        if (string.IsNullOrWhiteSpace(airport.Country))
        {
            return "Debe seleccionar un país.";
        }

        if (string.IsNullOrWhiteSpace(airport.City))
        {
            return "Debe seleccionar una ciudad.";
        }

        if (string.IsNullOrWhiteSpace(airport.AirportName))
        {
            return "Debe ingresar el nombre del aeropuerto.";
        }

        if (string.IsNullOrWhiteSpace(airport.Code))
        {
            return "Debe ingresar el código del aeropuerto.";
        }

        if (airport.Code.Length != AirportCodeLength)
        {
            return "El código del aeropuerto debe tener exactamente 3 caracteres.";
        }

        if (airport.AirportName.Length > AirportNameMaxLength)
        {
            return $"El nombre del aeropuerto no puede superar los {AirportNameMaxLength} caracteres.";
        }

        if (!AirportNameRegex.IsMatch(airport.AirportName))
        {
            return "El nombre del aeropuerto no debe contener números ni caracteres especiales como #, !, %, $.";
        }

        return string.Empty;
    }

    public List<AirportSuggestionDto> GetSuggestions(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<AirportSuggestionDto>();
        return _airportRepository.GetSuggestions(query);
    }

    private static void NormalizeAirport(AirportModel airport)
    {
        airport.Code = airport.Code?.Trim().ToUpper() ?? string.Empty;
        airport.AirportName = airport.AirportName?.Trim() ?? string.Empty;
        airport.Country = airport.Country?.Trim() ?? string.Empty;
        airport.City = airport.City?.Trim() ?? string.Empty;
    }
}
