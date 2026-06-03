using backend.Services;
using backend.Interfaces;
using backend.Model;
using backend.DTOs;
using Moq;

namespace backend.Tests;

[TestFixture]
public class AirportServiceTests
{
    private Mock<IAirportRepository> _mockAirportRepository = null!;
    private AirportService _airportService = null!;

    [SetUp]
    public void Setup()
    {
        _mockAirportRepository = new Mock<IAirportRepository>();
        _airportService = new AirportService(_mockAirportRepository.Object);
    }

    [Test]
    public void GetAirports_WhenAirportsExist_ShouldReturnAirportList()
    {
        // Arrange
        var airports = new List<AirportModel>
        {
            new AirportModel
            {
                Code = "MAD",
                AirportName = "Adolfo Suárez Madrid-Barajas",
                Country = "España",
                City = "Madrid"
            },
            new AirportModel
            {
                Code = "BCN",
                AirportName = "Josep Tarradellas Barcelona-El Prat",
                Country = "España",
                City = "Barcelona"
            }
        };

        _mockAirportRepository
            .Setup(repository => repository.GetAirports())
            .Returns(airports);

        // Act
        var result = _airportService.GetAirports();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Code, Is.EqualTo("MAD"));
            Assert.That(result[1].Code, Is.EqualTo("BCN"));
        });
    }

    [Test]
    public void GetAirports_WhenNoAirportsExist_ShouldReturnEmptyList()
    {
        // Arrange
        _mockAirportRepository
            .Setup(repository => repository.GetAirports())
            .Returns(new List<AirportModel>());

        // Act
        var result = _airportService.GetAirports();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public void GetCountries_WhenCountriesExist_ShouldReturnCountryList()
    {
        // Arrange
        var countries = new List<AirportCatalogDto>
        {
            new AirportCatalogDto { Country = "Costa Rica", City = string.Empty },
            new AirportCatalogDto { Country = "España", City = string.Empty }
        };

        _mockAirportRepository
            .Setup(repository => repository.GetCountries())
            .Returns(countries);

        // Act
        var result = _airportService.GetCountries();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Country, Is.EqualTo("Costa Rica"));
            Assert.That(result[1].Country, Is.EqualTo("España"));
        });
    }

    [Test]
    public void GetCountries_WhenNoCountriesExist_ShouldReturnEmptyList()
    {
        // Arrange
        _mockAirportRepository
            .Setup(repository => repository.GetCountries())
            .Returns(new List<AirportCatalogDto>());

        // Act
        var result = _airportService.GetCountries();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        });
    }

    [Test]
    public void GetCitiesByCountry_WhenCountryIsValid_ShouldReturnCityList()
    {
        // Arrange
        string country = "España";

        var cities = new List<AirportCatalogDto>
        {
            new AirportCatalogDto { Country = "España", City = "Madrid" },
            new AirportCatalogDto { Country = "España", City = "Barcelona" }
        };

        _mockAirportRepository
            .Setup(repository => repository.GetCitiesByCountry(country))
            .Returns(cities);

        // Act
        var result = _airportService.GetCitiesByCountry(country);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].City, Is.EqualTo("Madrid"));
            Assert.That(result[1].City, Is.EqualTo("Barcelona"));
        });
    }

    [Test]
    public void CreateAirport_WhenAirportIsNull_ShouldReturnErrorMessage()
    {
        // Act
        var result = _airportService.CreateAirport(null!);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar los datos del aeropuerto."));
    }

    [Test]
    public void CreateAirport_WhenCountryIsNull_ShouldReturnErrorMessage()
    {
        // Arrange
        var airport = new AirportModel
        {
            Country = null!,
            City = "Madrid",
            AirportName = "Barajas",
            Code = "MAD"
        };

        // Act
        var result = _airportService.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("Debe seleccionar un país."));
    }

    [Test]
    public void CreateAirport_WhenCityIsNull_ShouldReturnErrorMessage()
    {
        // Arrange
        var airport = new AirportModel
        {
            Country = "España",
            City = null!,
            AirportName = "Barajas",
            Code = "MAD"
        };

        // Act
        var result = _airportService.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("Debe seleccionar una ciudad."));
    }

    [Test]
    public void CreateAirport_WhenAirportNameIsNull_ShouldReturnErrorMessage()
    {
        // Arrange
        var airport = new AirportModel
        {
            Country = "España",
            City = "Madrid",
            AirportName = null!,
            Code = "MAD"
        };

        // Act
        var result = _airportService.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar el nombre del aeropuerto."));
    }

    [Test]
    public void CreateAirport_WhenCodeIsNull_ShouldReturnErrorMessage()
    {
        // Arrange
        var airport = new AirportModel
        {
            Country = "España",
            City = "Madrid",
            AirportName = "Barajas",
            Code = null!
        };

        // Act
        var result = _airportService.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar el código del aeropuerto."));
    }

    [Test]
    public void CreateAirport_WhenCodeLengthIsInvalid_ShouldReturnErrorMessage()
    {
        // Arrange
        var airport = new AirportModel
        {
            Country = "España",
            City = "Madrid",
            AirportName = "Barajas",
            Code = "MA"
        };

        // Act
        var result = _airportService.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("El código del aeropuerto debe tener exactamente 3 caracteres."));
    }

    [Test]
    public void CreateAirport_WhenAirportNameIsTooLong_ShouldReturnErrorMessage()
    {
        // Arrange
        var airport = new AirportModel
        {
            Country = "España",
            City = "Madrid",
            AirportName = new string('A', 201),
            Code = "MAD"
        };

        // Act
        var result = _airportService.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("El nombre del aeropuerto no puede superar los 200 caracteres."));
    }

    [Test]
    public void CreateAirport_WhenAirportNameContainsInvalidCharacters_ShouldReturnErrorMessage()
    {
        // Arrange
        var airport = new AirportModel
        {
            Country = "España",
            City = "Madrid",
            AirportName = "Airport123#",
            Code = "MAD"
        };

        // Act
        var result = _airportService.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("El nombre del aeropuerto no debe contener números ni caracteres especiales como #, !, %, $."));
    }

    [Test]
    public void CreateAirport_WhenCityDoesNotBelongToCountry_ShouldReturnErrorMessage()
    {
        // Arrange
        var airport = new AirportModel
        {
            Country = "España",
            City = "Madrid",
            AirportName = "Barajas",
            Code = "MAD"
        };

        _mockAirportRepository
            .Setup(repository => repository.CityBelongsToCountry(airport.Country, airport.City))
            .Returns(false);

        // Act
        var result = _airportService.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("La ciudad seleccionada no pertenece al país seleccionado."));
    }

    [Test]
    public void CreateAirport_WhenAirportCodeAlreadyExists_ShouldReturnErrorMessage()
    {
        // Arrange
        var airport = new AirportModel
        {
            Country = "España",
            City = "Madrid",
            AirportName = "Barajas",
            Code = "MAD"
        };

        _mockAirportRepository
            .Setup(repository => repository.CityBelongsToCountry(airport.Country, airport.City))
            .Returns(true);

        _mockAirportRepository
            .Setup(repository => repository.AirportCodeExists(airport.Code))
            .Returns(true);

        // Act
        var result = _airportService.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.EqualTo("Ya existe un aeropuerto con ese código."));
    }

    [Test]
    public void CreateAirport_WhenDataIsValid_ShouldCreateAirport()
    {
        // Arrange
        var airport = new AirportModel
        {
            Country = "España",
            City = "Madrid",
            AirportName = "Barajas",
            Code = "MAD"
        };

        _mockAirportRepository
            .Setup(repository => repository.CityBelongsToCountry(airport.Country, airport.City))
            .Returns(true);

        _mockAirportRepository
            .Setup(repository => repository.AirportCodeExists(airport.Code))
            .Returns(false);

        _mockAirportRepository
            .Setup(repository => repository.AirportNameExists(airport.AirportName, airport.Country, airport.City))
            .Returns(false);

        // Act
        var result = _airportService.CreateAirport(airport);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void UpdateAirportName_WhenAirportNameIsWhiteSpace_ShouldReturnErrorMessage()
    {
        // Act
        var result = _airportService.UpdateAirportName("MAD", "   ");

        // Assert
        Assert.That(result, Is.EqualTo("Debe ingresar el nuevo nombre del aeropuerto."));
    }

    [Test]
    public void UpdateAirportName_WhenAirportNameContainsInvalidCharacters_ShouldReturnErrorMessage()
    {
        // Act
        var result = _airportService.UpdateAirportName("MAD", "Airport123#");

        // Assert
        Assert.That(result, Is.EqualTo("El nombre del aeropuerto no debe contener caracteres especiales como #, !, %, $."));
    }

    [Test]
    public void UpdateAirportName_WhenNewNameIsSameAsCurrentName_ShouldReturnErrorMessage()
    {
        // Arrange
        string code = "MAD";
        string currentName = "Barajas";
        string newName = "Barajas";

        var currentAirport = new AirportModel
        {
            Code = code,
            AirportName = currentName,
            Country = "España",
            City = "Madrid"
        };

        _mockAirportRepository
            .Setup(repository => repository.GetAirportByCode(code))
            .Returns(currentAirport);

        // Act
        var result = _airportService.UpdateAirportName(code, newName);

        // Assert
        Assert.That(result, Is.EqualTo("El nuevo nombre del aeropuerto debe ser diferente al nombre actual."));
    }

    [Test]
    public void UpdateAirportName_WhenDataIsValid_ShouldUpdateAirportName()
    {
        // Arrange
        string code = "MAD";
        string newName = "Barajas Nueva";

        var currentAirport = new AirportModel
        {
            Code = code,
            AirportName = "Barajas Vieja",
            Country = "España",
            City = "Madrid"
        };

        _mockAirportRepository
            .Setup(repository => repository.GetAirportByCode(code))
            .Returns(currentAirport);

        _mockAirportRepository
            .Setup(repository => repository.AirportNameExistsInLocationExceptCode(
                newName,
                currentAirport.Country,
                currentAirport.City,
                code
            ))
            .Returns(false);

        _mockAirportRepository
            .Setup(repository => repository.UpdateAirportName(code, newName))
            .Returns(true);

        // Act
        var result = _airportService.UpdateAirportName(code, newName);

        // Assert
        Assert.That(result, Is.Empty);
    }
}
