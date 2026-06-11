using backend.Interfaces;
using backend.Model;
using backend.Services;
using Moq;

namespace backend.Tests;

[TestFixture]
public class PurchaseServiceTests
{
    private Mock<IPassengerRepository>  _mockPassengerRepo = null!;
    private Mock<IItineraryRepository>  _mockItineraryRepo = null!;
    private Mock<IPurchaseRepository>   _mockPurchaseRepo  = null!;
    private Mock<ICodeGenerator>        _mockCodeGenerator = null!;
    private Mock<IPricingCalculator>    _mockPricing       = null!;
    private Mock<IRouteCreationService> _mockRouteService  = null!;
    private PurchaseService             _service           = null!;

    private static readonly DateOnly TestDate = new DateOnly(2026, 8, 15);

    [SetUp]
    public void Setup()
    {
        _mockPassengerRepo = new Mock<IPassengerRepository>();
        _mockItineraryRepo = new Mock<IItineraryRepository>();
        _mockPurchaseRepo  = new Mock<IPurchaseRepository>();
        _mockCodeGenerator = new Mock<ICodeGenerator>();
        _mockPricing       = new Mock<IPricingCalculator>();
        _mockRouteService  = new Mock<IRouteCreationService>();

        _service = new PurchaseService(
            _mockPassengerRepo.Object,
            _mockItineraryRepo.Object,
            _mockPurchaseRepo.Object,
            _mockCodeGenerator.Object,
            _mockPricing.Object,
            _mockRouteService.Object);
    }

    private static RouteCreationModel Route(int economyCap, int firstClassCap, string type = "B737") =>
        new() { EconomyClassCapacity = economyCap, FirstClassCapacity = firstClassCap, AircraftTypeId = type };

    // ── IsFlightAvailableAsync ─────────────────────────────────────────────────

    [Test]
    public async Task IsFlightAvailable_RouteNotFound_ShouldReturnTrue()
    {
        // Arrange
        _mockRouteService
            .Setup(s => s.GetRouteByCode(It.IsAny<string>()))
            .Throws<Exception>();

        // Act
        var result = await _service.IsFlightAvailableAsync("UNKNOWN", TestDate, 1);

        // Assert — fail-open: unknown route never blocks a user
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFlightAvailable_RouteAndAircraftCapacityBothZero_ShouldReturnTrue()
    {
        // Arrange
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(0, 0, "B737"));
        _mockPurchaseRepo.Setup(r => r.GetAircraftCapacityByTypeAsync("B737")).ReturnsAsync(0);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 5);

        // Assert — fail-open: cannot determine capacity
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFlightAvailable_RouteCapacityZero_FallsBackToAircraftCapacity_CountFits_ShouldReturnTrue()
    {
        // Arrange — route has 0 (old migration), aircraft has 10 seats
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(0, 0, "B737"));
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns((int?)null);
        _mockPurchaseRepo.Setup(r => r.GetAircraftCapacityByTypeAsync("B737")).ReturnsAsync(10);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 5);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFlightAvailable_RouteCapacityZero_FallsBackToAircraftCapacity_CountExceeds_ShouldReturnFalse()
    {
        // Arrange — aircraft has 10 seats, but 12 requested
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(0, 0, "B737"));
        _mockPurchaseRepo.Setup(r => r.GetAircraftCapacityByTypeAsync("B737")).ReturnsAsync(10);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 12);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsFlightAvailable_RequestedCountExceedsTotalCapacity_ShouldReturnFalse()
    {
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(6, 2));

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 9);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsFlightAvailable_NoScheduledFlight_CountFitsCapacity_ShouldReturnTrue()
    {
        // Arrange — capacity = 8, no prior purchases, requesting 4
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(6, 2));
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns((int?)null);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 4);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFlightAvailable_BookedSeatsAndRequestedExactlyFillCapacity_ShouldReturnTrue()
    {
        // Arrange — capacity = 8, booked = 4, requested = 4 → 4 + 4 = 8 ≤ 8
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(6, 2));
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(42);
        _mockPurchaseRepo.Setup(r => r.GetBookedSeatsAsync(42)).ReturnsAsync(4);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 4);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFlightAvailable_BookedPlusRequestedExceedsCapacity_ShouldReturnFalse()
    {
        // Arrange — capacity = 8, booked = 5, requested = 4 → 4 + 5 = 9 > 8
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(6, 2));
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(42);
        _mockPurchaseRepo.Setup(r => r.GetBookedSeatsAsync(42)).ReturnsAsync(5);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 4);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsFlightAvailable_FlightFullyBooked_SingleSeatRequest_ShouldReturnFalse()
    {
        // Arrange — capacity = 6, already booked = 6, requesting 1 more
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(6, 0));
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(10);
        _mockPurchaseRepo.Setup(r => r.GetBookedSeatsAsync(10)).ReturnsAsync(6);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 1);

        // Assert
        Assert.That(result, Is.False);
    }

    // ── CheckPassengerDuplicatesAsync ──────────────────────────────────────────

    [Test]
    public async Task CheckDuplicates_NoScheduledFlight_ShouldReturnEmptyList()
    {
        // Arrange — no prior purchases means no duplicates possible
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns((int?)null);

        var passengers = new[]
        {
            new PassengerCheckInfo
            {
                FirstName = "Ana", LastName = "García",
                BirthDate = new DateOnly(1990, 1, 1), PassportCountry = "Costa Rica"
            }
        };

        // Act
        var result = await _service.CheckPassengerDuplicatesAsync("R1", TestDate, passengers);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task CheckDuplicates_AllThreeFieldsMatch_ShouldDetectDuplicate()
    {
        // Arrange
        var birthDate = new DateOnly(1990, 5, 15);
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(1);
        _mockPurchaseRepo
            .Setup(r => r.GetPassengerIdentitiesOnFlightAsync(1))
            .ReturnsAsync(new List<PassengerIdentityRecord>
            {
                new() { FullName = "Ana García", BirthDate = birthDate.ToDateTime(TimeOnly.MinValue), PassportCountry = "Costa Rica" }
            });

        var passengers = new[]
        {
            new PassengerCheckInfo
            {
                FirstName = "Ana", LastName = "García",
                BirthDate = birthDate, PassportCountry = "Costa Rica"
            }
        };

        // Act
        var result = await _service.CheckPassengerDuplicatesAsync("R1", TestDate, passengers);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo("Ana García"));
        });
    }

    [Test]
    public async Task CheckDuplicates_SameNameDifferentBirthDate_ShouldNotDetectDuplicate()
    {
        // Arrange — same name and country but different birth date → allowed
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(1);
        _mockPurchaseRepo
            .Setup(r => r.GetPassengerIdentitiesOnFlightAsync(1))
            .ReturnsAsync(new List<PassengerIdentityRecord>
            {
                new() { FullName = "Ana García", BirthDate = new DateTime(1990, 5, 15), PassportCountry = "Costa Rica" }
            });

        var passengers = new[]
        {
            new PassengerCheckInfo
            {
                FirstName = "Ana", LastName = "García",
                BirthDate = new DateOnly(1991, 5, 15), PassportCountry = "Costa Rica"
            }
        };

        // Act
        var result = await _service.CheckPassengerDuplicatesAsync("R1", TestDate, passengers);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task CheckDuplicates_SameNameDifferentPassportCountry_ShouldNotDetectDuplicate()
    {
        // Arrange — same name and birth date but different passport country → allowed
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(1);
        _mockPurchaseRepo
            .Setup(r => r.GetPassengerIdentitiesOnFlightAsync(1))
            .ReturnsAsync(new List<PassengerIdentityRecord>
            {
                new() { FullName = "Ana García", BirthDate = new DateTime(1990, 5, 15), PassportCountry = "Costa Rica" }
            });

        var passengers = new[]
        {
            new PassengerCheckInfo
            {
                FirstName = "Ana", LastName = "García",
                BirthDate = new DateOnly(1990, 5, 15), PassportCountry = "México"
            }
        };

        // Act
        var result = await _service.CheckPassengerDuplicatesAsync("R1", TestDate, passengers);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task CheckDuplicates_NoMatchingPassenger_ShouldReturnEmptyList()
    {
        // Arrange — completely different passenger on the flight
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(1);
        _mockPurchaseRepo
            .Setup(r => r.GetPassengerIdentitiesOnFlightAsync(1))
            .ReturnsAsync(new List<PassengerIdentityRecord>
            {
                new() { FullName = "Carlos López", BirthDate = new DateTime(1985, 3, 10), PassportCountry = "Costa Rica" }
            });

        var passengers = new[]
        {
            new PassengerCheckInfo
            {
                FirstName = "Ana", LastName = "García",
                BirthDate = new DateOnly(1990, 5, 15), PassportCountry = "Costa Rica"
            }
        };

        // Act
        var result = await _service.CheckPassengerDuplicatesAsync("R1", TestDate, passengers);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task CheckDuplicates_MultiplePassengers_OnlyDuplicateIsReturned()
    {
        // Arrange — two passengers in the request, only one matches the existing booking
        var birthDate = new DateOnly(1990, 5, 15);
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(1);
        _mockPurchaseRepo
            .Setup(r => r.GetPassengerIdentitiesOnFlightAsync(1))
            .ReturnsAsync(new List<PassengerIdentityRecord>
            {
                new() { FullName = "Ana García", BirthDate = birthDate.ToDateTime(TimeOnly.MinValue), PassportCountry = "Costa Rica" }
            });

        var passengers = new[]
        {
            new PassengerCheckInfo { FirstName = "Ana",    LastName = "García", BirthDate = birthDate,                PassportCountry = "Costa Rica" },
            new PassengerCheckInfo { FirstName = "Carlos", LastName = "López",  BirthDate = new DateOnly(1985, 3, 10), PassportCountry = "México" },
        };

        // Act
        var result = await _service.CheckPassengerDuplicatesAsync("R1", TestDate, passengers);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0], Is.EqualTo("Ana García"));
        });
    }

    [Test]
    public async Task CheckDuplicates_ComparisonIsCaseInsensitive()
    {
        // Arrange — name and country in DB are lowercase; request sends uppercase
        var birthDate = new DateOnly(1990, 5, 15);
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(1);
        _mockPurchaseRepo
            .Setup(r => r.GetPassengerIdentitiesOnFlightAsync(1))
            .ReturnsAsync(new List<PassengerIdentityRecord>
            {
                new() { FullName = "Ana García", BirthDate = birthDate.ToDateTime(TimeOnly.MinValue), PassportCountry = "costa rica" }
            });

        var passengers = new[]
        {
            new PassengerCheckInfo
            {
                FirstName = "ANA", LastName = "GARCÍA",
                BirthDate = birthDate, PassportCountry = "COSTA RICA"
            }
        };

        // Act
        var result = await _service.CheckPassengerDuplicatesAsync("R1", TestDate, passengers);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1));
    }
}
