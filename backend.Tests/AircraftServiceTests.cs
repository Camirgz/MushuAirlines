using backend.Interfaces;
using backend.Model;
using backend.Services;
using Moq;

namespace backend.Tests;

[TestFixture]
public class AircraftServiceTests
{
    private Mock<IAircraftRepository> _mockAircraftRepository = null!;
    private AircraftService _aircraftService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockAircraftRepository = new Mock<IAircraftRepository>();
        _aircraftService = new AircraftService(_mockAircraftRepository.Object);
    }

    [Test]
    public void Update_WhenOnlyWeightIncrease_ShouldReturnEmptyStringAndUpdatesAircraft()
    {
        // Arrange
        int aircraftId = 1;

        var currentAircraft = new AircraftResponseModel
        {
            Id = aircraftId,
            Model = "Boeing 778",
            Type = "Avión comercial",
            WeightKg = 8100,
            EconomyRows = 4,
            EconomySeatsPerRow = 6,
            FirstClassRows = 3,
            FirstClassSeatsPerRow = 2,
            Capacity = 30
        };

        var updateModel = new UpdateAircraftRequestModel
        {
            WeightKg = 8101,
            EconomyRows = 4,
            EconomySeatsPerRow = 6,
            FirstClassRows = 3,
            FirstClassSeatsPerRow = 2
        };

        _mockAircraftRepository
            .Setup(repository => repository.GetById(aircraftId))
            .Returns(currentAircraft);

        // Act
        string result = _aircraftService.Update(aircraftId, updateModel);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Update_WhenOnlyWeightDecrease_ShouldReturnsWeightCannotDecrease()
    {
        // Arrange
        int aircraftId = 1;

        var currentAircraft = new AircraftResponseModel
        {
            Id = aircraftId,
            Model = "Boeing 778",
            Type = "Avión comercial",
            WeightKg = 8100,
            EconomyRows = 4,
            EconomySeatsPerRow = 6,
            FirstClassRows = 3,
            FirstClassSeatsPerRow = 2,
            Capacity = 30
        };

        var updateModel = new UpdateAircraftRequestModel
        {
            WeightKg = 8099,
            EconomyRows = 4,
            EconomySeatsPerRow = 6,
            FirstClassRows = 3,
            FirstClassSeatsPerRow = 2
        };

        _mockAircraftRepository
            .Setup(repository => repository.GetById(aircraftId))
            .Returns(currentAircraft);

        // Act
        string result = _aircraftService.Update(aircraftId, updateModel);

        // Assert
        Assert.That(result, Is.EqualTo("El peso soportado no puede disminuir."));
    }

    [Test]
    public void Update_WhenOnlyEconomyRowsDecrease_ShouldReturnEconomyRowsCannotDecrease()
    {
        // Arrange
        int aircraftId = 1;

        var currentAircraft = new AircraftResponseModel
        {
           Id = aircraftId,
           Model = "R43",
           Type = "Helicóptero",
           WeightKg = 300,
           EconomyRows = 2,
           EconomySeatsPerRow = 1,
           FirstClassRows = 2,
           FirstClassSeatsPerRow = 1,
           Capacity = 4
        };

        var updateModel = new UpdateAircraftRequestModel
        {
            WeightKg = 300,
            EconomyRows = 1,
            EconomySeatsPerRow = 1,
            FirstClassRows = 2,
            FirstClassSeatsPerRow = 1
        };

        _mockAircraftRepository
            .Setup(repository => repository.GetById(aircraftId))
            .Returns(currentAircraft);

        // Act
        string result = _aircraftService.Update(aircraftId, updateModel);

        // Assert
        Assert.That(result, Is.EqualTo("La cantidad de filas de clase turista no puede disminuir."));
    }

    [Test]
    public void Update_WhenTotalSeatsIsGreaterThanOrEqualToOneThousand_ReturnsCapacityErrorMessage()
    {
        // Arrange
        int aircraftId = 1;

        var currentAircraft = new AircraftResponseModel
        {
            Id = aircraftId,
            Model = "Boeing 777",
            Type = "Avión comercial",
            WeightKg = 80000,
            EconomyRows = 4,
            EconomySeatsPerRow = 3,
            FirstClassRows = 6,
            FirstClassSeatsPerRow = 5,
            Capacity = 42
        };

        var updateModel = new UpdateAircraftRequestModel
        {
            WeightKg = 80001,
            EconomyRows = 50,
            EconomySeatsPerRow = 10,
            FirstClassRows = 50,
            FirstClassSeatsPerRow = 10
        };

        _mockAircraftRepository
            .Setup(repository => repository.GetById(aircraftId))
            .Returns(currentAircraft);

        // Act
        string result = _aircraftService.Update(aircraftId, updateModel);

        // Assert
        Assert.That(result, Is.EqualTo("La capacidad total de asientos (1000) no puede ser igual o mayor a 1000."));
    }

    [Test]
    public void Update_WhenAllFieldsAreSame_ShouldReturnNeedIncreaseAtLeastOneField()
    {
        // Arrange
        int aircraftId = 1;

        var currentAircraft = new AircraftResponseModel
        {
            Id = aircraftId,
            Model = "Boeing 779",
            Type = "Avión comercial",
            WeightKg = 80000,
            EconomyRows = 20,
            EconomySeatsPerRow = 6,
            FirstClassRows = 8,
            FirstClassSeatsPerRow = 5,
            Capacity = 160
        };

        var updateModel = new UpdateAircraftRequestModel
        {
            WeightKg = 80000,
            EconomyRows = 20,
            EconomySeatsPerRow = 6,
            FirstClassRows = 8,
            FirstClassSeatsPerRow = 5
        };

        _mockAircraftRepository
            .Setup(repository => repository.GetById(aircraftId))
            .Returns(currentAircraft);
        
        // Act
        string result = _aircraftService.Update(aircraftId, updateModel);

        // Assert
        Assert.That(result, Is.EqualTo("Debe aumentar al menos un valor para actualizar la aeronave."));
    }

    [Test]
    public void Delete_WhenIdIsInvalid_ShouldReturnInvalidAircraftMessage()
    {
        // Arrange
        int aircraftId = 0;

        // Act
        string result = _aircraftService.Delete(aircraftId);

        // Assert
        Assert.That(result, Is.EqualTo("La aeronave seleccionada no es válida."));
    }

    [Test]
    public void Delete_WhenAircraftDoesNotExist_ShouldReturnNotFoundMessage()
    {
        // Arrange
        int aircraftId = 99;

        _mockAircraftRepository
            .Setup(repository => repository.GetById(aircraftId))
            .Returns((AircraftResponseModel?)null);

        // Act
        string result = _aircraftService.Delete(aircraftId);

        // Assert
        Assert.That(result, Is.EqualTo("No se encontró la aeronave que desea eliminar."));
    }

    [Test]
    public void Delete_WhenRepositoryReturnsFalse_ShouldReturnNotFoundMessage()
    {
        // Arrange
        int aircraftId = 1;

        var aircraft = new AircraftResponseModel
        {
            Id = aircraftId,
            Model = "Boeing 777",
            Type = "Avión comercial",
            WeightKg = 80000,
            EconomyRows = 20,
            EconomySeatsPerRow = 7,
            FirstClassRows = 4,
            FirstClassSeatsPerRow = 4,
            Capacity = 156
        };

        _mockAircraftRepository
            .Setup(repository => repository.GetById(aircraftId))
            .Returns(aircraft);

        _mockAircraftRepository
            .Setup(repository => repository.Delete(aircraftId))
            .Returns(false);

        // Act
        string result = _aircraftService.Delete(aircraftId);

        // Assert
        Assert.That(result, Is.EqualTo("No se encontró la aeronave que desea eliminar."));
    }

    [Test]
    public void Delete_WhenAircraftExists_ShouldReturnEmptyStringAndDeletesAircraft()
    {
        // Arrange
        int aircraftId = 1;

        var aircraft = new AircraftResponseModel
        {
            Id = aircraftId,
            Model = "Boeing 777",
            Type = "Avión comercial",
            WeightKg = 80000,
            EconomyRows = 20,
            EconomySeatsPerRow = 7,
            FirstClassRows = 4,
            FirstClassSeatsPerRow = 4,
            Capacity = 156
        };

        _mockAircraftRepository
            .Setup(repository => repository.GetById(aircraftId))
            .Returns(aircraft);

        _mockAircraftRepository
            .Setup(repository => repository.Delete(aircraftId))
            .Returns(true);

        // Act
        string result = _aircraftService.Delete(aircraftId);

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }
}
