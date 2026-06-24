using System.Text.RegularExpressions;
using Microsoft.AspNetCore.StaticAssets;
namespace backend.Model;

public class AirportModel
{
    public const int AirportCodeLength = 3;
    public const int AirportNameMaxLength = 200;

    private static readonly Regex AirportNameRegex = new(
        @"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s.'-]+$",
        RegexOptions.Compiled
    );

    public string Code { get; set; } = string.Empty;
    public string AirportName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public void Normalize()
    {
        Code = Code?.Trim().ToUpper() ?? string.Empty;
        AirportName = NormalizeAirportName(AirportName);
        Country = Country?.Trim() ?? string.Empty;
        City = City?.Trim() ?? string.Empty;
    }

    public string ValidateForCreation()
    {
        if (string.IsNullOrWhiteSpace(Country))
        {
            return "Debe seleccionar un país";
        }

        if (string.IsNullOrWhiteSpace(City))
        {
            return "Debe seleccionar una ciudad";
        }

        string airportNameValidationMessage = ValidateAirportName(
            AirportName,
            useCreationMessage: true
        );

        if (!string.IsNullOrEmpty(airportNameValidationMessage))
        {
            return airportNameValidationMessage;
        }

        return ValidateCode(Code);
    }

    public static string ValidateCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return "Debe ingresar el código del aeropuerto.";
        }

        if (code.Trim().Length != AirportCodeLength)
        {
            return "El código del aeropuerto debe tener exactamente 3 caracteres.";
        }

        return string.Empty;
    }

    public static string ValidateAirportName(
        string? airportName,
        bool useCreationMessage
    )
    {
        if (string.IsNullOrWhiteSpace(airportName))
        {
            return useCreationMessage
                ? "Debe ingresar el nombre del aeropuerto."
                : "Debe ingresar el nuevo nombre del aeropuerto.";
        }

        string normalizedAirportName = NormalizeAirportName(airportName);

        if (normalizedAirportName.Length > AirportNameMaxLength)
        {
            return $"El nombre del aeropuerto no puede superar los {AirportNameMaxLength} caracteres.";
        }

        if (!AirportNameRegex.IsMatch(normalizedAirportName))
        {
            return useCreationMessage
                ? "El nombre del aeropuerto no debe contener números ni caracteres especiales como #, !, %, $."
                : "El nombre del aeropuerto no debe contener caracteres especiales como #, !, %, $.";
        }

        return string.Empty;
    }

    public static string NormalizeAirportName(string? airportName)
    {
        return airportName?.Trim() ?? string.Empty;
    }
}
