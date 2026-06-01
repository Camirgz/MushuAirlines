using backend.DTOs;
using backend.Interfaces;
using backend.Model;
using backend.Services;

namespace backend.Tests;

[TestFixture]
public class AirportServiceTests
{
    private FakeAirportRepository _repository = null!;
    private AirportService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new FakeAirportRepository();
        _service = new AirportService(_repository);
    }

    private AirportModel CreateSanJoseAirport() => new()
    {
        Code = "SJO",
        AirportName = "Juan Santamaría",
        Country = "Costa Rica",
        City = "San José"
    };

    private AirportModel CreateMexicoCityAirport() => new()
    {
        Code = "MEX",
        AirportName = "Aeropuerto Internacional de México",
        Country = "México",
        City = "Ciudad de México"
    };

    private AirportModel CreatePanamaAirport() => new()
    {
        Code = "PTY",
        AirportName = "Tocumen",
        Country = "Panamá",
        City = "Ciudad de Panamá"
    };

    [Test]
    public void GetAirports_ReturnsAirports()
    {
        // Arrange
        var airport = CreateSanJoseAirport();
        _repository.Airports.Add(airport);

        // Act
        var result = _service.GetAirports();

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Code, Is.EqualTo("SJO"));
    }

    [Test]
    public void GetCountries_ReturnsCountries()
    {
        // Act
        var result = _service.GetCountries();

        // Assert
        Assert.That(result.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(result.Any(c => c.Country == "Costa Rica"), Is.True);
    }

    [Test]
    public void GetCitiesByCountry_ReturnsEmpty_WhenCountryIsEmpty()
    {
        var result = _service.GetCitiesByCountry("");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void CreateAirport_ReturnsError_WhenAirportIsNull()
    {
        string result = _service.CreateAirport(null!);

        Assert.That(result, Is.EqualTo("Debe ingresar los datos del aeropuerto."));
    }

    [Test]
    public void CreateAirport_ReturnsError_WhenCodeLengthIsInvalid()
    {
        // Arrange
        var airport = CreateSanJoseAirport();
        airport.Code = "SJ"; // código inválido

        // Act
        string result = _service.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("El código del aeropuerto debe tener exactamente 3 caracteres."));
    }

    [Test]
    public void CreateAirport_ReturnsError_WhenCityDoesNotBelongToCountry()
    {
        // Arrange
        var airport = CreateSanJoseAirport();
        airport.City = "Ciudad de Panamá";

        // Act
        string result = _service.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("La ciudad seleccionada no pertenece al país seleccionado."));
    }

    [Test]
    public void CreateAirport_ReturnsError_WhenCodeAlreadyExists()
    {
        // Arrange
        var existingAirport = CreateSanJoseAirport();
        _repository.Airports.Add(existingAirport);

        var newAirport = CreateSanJoseAirport();
        newAirport.AirportName = "Aeropuerto de México";

        // Act
        string result = _service.CreateAirport(newAirport);

        // Assert
        Assert.That(result, Is.EqualTo("Ya existe un aeropuerto con ese código."));
    }

    [Test]
    public void CreateAirport_InsertsAirport_WhenDataIsValid()
    {
        // Arrange
        var airport = CreateMexicoCityAirport();

        // Act
        string result = _service.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
        Assert.That(_repository.InsertWasCalled, Is.True);
        Assert.That(_repository.Airports.Count, Is.EqualTo(1));
        Assert.That(_repository.Airports[0].Code, Is.EqualTo("MEX"));
    }

    [Test]
    public void UpdateAirportName_ReturnsError_WhenAirportDoesNotExist()
    {
        string result = _service.UpdateAirportName("ABC", "Aeropuerto Nuevo Bombre");

        Assert.That(result, Is.EqualTo("No existe un aeropuerto con ese código."));
    }

    [Test]
    public void UpdateAirportName_ReturnsError_WhenNewNameIsSameAsCurrent()
    {
        // Arrange
        var airport = CreateSanJoseAirport();
        _repository.Airports.Add(airport);

        // Act
        string result = _service.UpdateAirportName("SJO", "Juan Santamaría");

        // Assert
        Assert.That(result, Is.EqualTo("El nuevo nombre del aeropuerto debe ser diferente al nombre actual."));
    }

    [Test]
    public void UpdateAirportName_UpdatesAirport_WhenDataIsValid()
    {
        // Arrange
        var airport = CreateSanJoseAirport();
        _repository.Airports.Add(airport);
        var newName = "Aeropuerto Internacional Juan Santamaría";

        // Act
        string result = _service.UpdateAirportName("SJO", newName);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
        Assert.That(_repository.UpdateWasCalled, Is.True);
        Assert.That(_repository.Airports[0].AirportName, Is.EqualTo(newName));
    }
}

public class FakeAirportRepository : IAirportRepository
{
    public List<AirportModel> Airports { get; } = new();
    public bool InsertWasCalled { get; private set; }
    public bool UpdateWasCalled { get; private set; }

    private readonly Dictionary<string, List<string>> _countriesToCities = new()
    {
        { "Costa Rica", new List<string> { "San José", "Alajuela", "Cartago" } },
        { "Panamá", new List<string> { "Ciudad de Panamá", "Colón" } },
        { "México", new List<string> { "Ciudad de México", "Cancún", "Monterrey" } }
    };

    public List<AirportModel> GetAirports() => Airports;

    public List<AirportCatalogDto> GetCountries() =>
        _countriesToCities.Keys.Select(country => new AirportCatalogDto { Country = country }).ToList();

    public List<AirportCatalogDto> GetCitiesByCountry(string country) =>
        string.IsNullOrEmpty(country) || !_countriesToCities.ContainsKey(country)
            ? new List<AirportCatalogDto>()
            : _countriesToCities[country]
                .Select(city => new AirportCatalogDto { City = city })
                .ToList();

    public AirportModel? GetAirportByCode(string code) => Airports.FirstOrDefault(a => a.Code == code);

    public bool CityBelongsToCountry(string country, string city) =>
        _countriesToCities.ContainsKey(country) && _countriesToCities[country].Contains(city);

    public bool AirportCodeExists(string code) => Airports.Any(a => a.Code == code);

    public bool AirportNameExists(string airportName, string country, string city) =>
        Airports.Any(a => a.AirportName == airportName && a.Country == country && a.City == city);

    public bool AirportNameExistsInLocationExceptCode(string airportName, string country, string city, string excludedCode) =>
        Airports.Any(a =>
            a.AirportName == airportName &&
            a.Country == country &&
            a.City == city &&
            a.Code != excludedCode
        );

    public void InsertAirport(AirportModel airport)
    {
        InsertWasCalled = true;
        Airports.Add(airport);
    }

    public bool UpdateAirportName(string code, string airportName)
    {
        var airport = GetAirportByCode(code);
        if (airport == null) return false;

        airport.AirportName = airportName;
        UpdateWasCalled = true;
        return true;
    }
}
