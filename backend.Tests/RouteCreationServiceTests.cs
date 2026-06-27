using backend.Interfaces;
using backend.Model;
using backend.Services;
using Moq;
using NUnit.Framework;

namespace backend.Tests;

[TestFixture]
public class RouteCreationServiceTests
{
    private Mock<IRouteCreationRepository> _mockRouteCreationRepository = null!;
    private RouteCreationService _routeCreationService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRouteCreationRepository = new Mock<IRouteCreationRepository>();
        _routeCreationService = new RouteCreationService(_mockRouteCreationRepository.Object);
    }

    [Test]
    public void CreateRoute_WhenRepositoryInsertSucceeds_ShouldReturnEmptyString()
    {
        // Arrange
        RouteCreationModel route = BuildRouteCreationModel();

        _mockRouteCreationRepository
            .Setup(repository => repository.InsertRoute(route));

        // Act
        string result = _routeCreationService.CreateRoute(route);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void CreateRoute_WhenRepositoryThrowsException_ShouldReturnErrorMessage()
    {
        // Arrange
        RouteCreationModel route = BuildRouteCreationModel();

        _mockRouteCreationRepository
            .Setup(repository => repository.InsertRoute(route))
            .Throws(new Exception("No se encontró la aeronave seleccionada."));

        // Act
        string result = _routeCreationService.CreateRoute(route);

        // Assert
        Assert.That(
            result,
            Is.EqualTo("ERROR: No se encontró la aeronave seleccionada.")
        );
    }

    [Test]
    public void GetRoutes_WhenRepositoryReturnsRoutes_ShouldMapRoutesCorrectly()
    {
        // Arrange
        List<RouteDbModel> dbRoutes = new()
        {
            BuildRouteDbModel("R001"),
            BuildRouteDbModel("R002")
        };

        _mockRouteCreationRepository
            .Setup(repository => repository.GetRoutes())
            .Returns(dbRoutes);

        // Act
        List<RouteCreationModel> result = _routeCreationService.GetRoutes();

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Code, Is.EqualTo("R001"));
        Assert.That(result[0].OriginAirport, Is.EqualTo("SJO"));
        Assert.That(result[0].DestinationAirport, Is.EqualTo("MAD"));
        Assert.That(result[0].AircraftTypeId, Is.EqualTo("Boeing 777"));
        Assert.That(result[0].AircraftCode, Is.EqualTo(1));
        Assert.That(result[0].Frequency, Is.EquivalentTo(new List<string> { "Lunes", "Martes" }));
    }

    [Test]
    public void GetRouteByCode_WhenRouteExists_ShouldReturnMappedRoute()
    {
        // Arrange
        string code = "R001";

        _mockRouteCreationRepository
            .Setup(repository => repository.GetRouteByCode(code))
            .Returns(BuildRouteDbModel(code));

        // Act
        RouteCreationModel? result = _routeCreationService.GetRouteByCode(code);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Code, Is.EqualTo(code));
        Assert.That(result.OriginAirport, Is.EqualTo("SJO"));
        Assert.That(result.DestinationAirport, Is.EqualTo("MAD"));
        Assert.That(result.OriginCity, Is.EqualTo("San José"));
        Assert.That(result.DestinationCity, Is.EqualTo("Madrid"));
        Assert.That(result.Frequency, Is.EquivalentTo(new List<string> { "Lunes", "Martes" }));
    }

    [Test]
    public void GetRouteByCode_WhenRouteDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        string code = "NOEXISTE";

        _mockRouteCreationRepository
            .Setup(repository => repository.GetRouteByCode(code))
            .Returns((RouteDbModel?)null);

        // Act
        RouteCreationModel? result = _routeCreationService.GetRouteByCode(code);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindExistingScheduledFlight_WhenRepositoryReturnsFlightId_ShouldReturnFlightId()
    {
        // Arrange
        string routeCode = "R001";
        DateTime date = new(2026, 6, 27);

        _mockRouteCreationRepository
            .Setup(repository => repository.FindExistingScheduledFlight(routeCode, date))
            .Returns(15);

        // Act
        int? result = _routeCreationService.FindExistingScheduledFlight(routeCode, date);

        // Assert
        Assert.That(result, Is.EqualTo(15));
    }

    [Test]
    public void GetOrCreateScheduledFlight_WhenRepositoryReturnsFlightId_ShouldReturnFlightId()
    {
        // Arrange
        string routeCode = "R001";
        DateTime date = new(2026, 6, 27);

        _mockRouteCreationRepository
            .Setup(repository => repository.GetOrCreateScheduledFlight(routeCode, date))
            .Returns(25);

        // Act
        int result = _routeCreationService.GetOrCreateScheduledFlight(routeCode, date);

        // Assert
        Assert.That(result, Is.EqualTo(25));
    }

    [Test]
    public void DeleteRoute_WhenCodeIsEmpty_ShouldReturnInvalidRouteMessage()
    {
        // Act
        string result = _routeCreationService.DeleteRoute("");

        // Assert
        Assert.That(result, Is.EqualTo("La ruta seleccionada no es válida."));
    }

    [Test]
    public void DeleteRoute_WhenRepositoryReturnsFalse_ShouldReturnNotFoundMessage()
    {
        // Arrange
        string code = "R001";

        _mockRouteCreationRepository
            .Setup(repository => repository.DeleteRoute(code))
            .Returns(false);

        // Act
        string result = _routeCreationService.DeleteRoute(code);

        // Assert
        Assert.That(result, Is.EqualTo("No se encontró la ruta que desea eliminar."));
    }

    [Test]
    public void DeleteRoute_WhenRepositoryReturnsTrue_ShouldReturnEmptyString()
    {
        // Arrange
        string code = "R001";

        _mockRouteCreationRepository
            .Setup(repository => repository.DeleteRoute(code))
            .Returns(true);

        // Act
        string result = _routeCreationService.DeleteRoute(code);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    private static RouteCreationModel BuildRouteCreationModel()
    {
        return new RouteCreationModel
        {
            Code = "R001",
            OriginAirport = "SJO",
            DestinationAirport = "MAD",
            DepartureTime = "08:00",
            ArrivalTime = "18:00",
            Duration = "10:00",
            AircraftTypeId = "Boeing 777",
            AircraftCode = 1,
            Frequency = new List<string> { "Lunes", "Martes" },
            PriceFirstClass = 500,
            PriceEconomy = 250,
            HandBagPrice = 20,
            HandBagWeight = 10,
            BagPrice = 40,
            BagWeight = 23,
            BagMultiplier = 1.5m,
            StartDate = "2026-06-01",
            FinalizationDate = "2026-12-31",
            EconomyClassCapacity = 120,
            FirstClassCapacity = 20,
            OriginCity = "San José",
            DestinationCity = "Madrid"
        };
    }

    private static RouteDbModel BuildRouteDbModel(string code)
    {
        return new RouteDbModel
        {
            Code = code,
            OriginAirport = "SJO",
            DestinationAirport = "MAD",
            DepartureTime = "08:00",
            ArrivalTime = "18:00",
            Duration = "10:00",
            AircraftTypeId = "Boeing 777",
            AircraftCode = 1,
            Frequency = "Lunes,Martes",
            PriceFirstClass = 500,
            PriceEconomy = 250,
            HandBagPrice = 20,
            HandBagWeight = 10,
            BagPrice = 40,
            BagWeight = 23,
            BagMultiplier = 1.5m,
            StartDate = "2026-06-01",
            FinalizationDate = "2026-12-31",
            EconomyClassCapacity = 120,
            FirstClassCapacity = 20,
            OriginCity = "San José",
            DestinationCity = "Madrid"
        };
    }
}
