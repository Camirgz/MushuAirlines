using backend.Exceptions;
using backend.Interfaces;
using backend.Model;
using backend.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace backend.Tests;

[TestFixture]
public class PurchaseServiceTests
{
    private Mock<IPassengerRepository>       _mockPassengerRepo = null!;
    private Mock<IPurchaseRepository>        _mockPurchaseRepo  = null!;
    private Mock<ICodeGenerator>             _mockCodeGenerator = null!;
    private Mock<IPricingCalculator>         _mockPricing       = null!;
    private Mock<IRouteCreationService>      _mockRouteService  = null!;
    private Mock<ILogger<PurchaseService>>   _mockLogger        = null!;
    private PurchaseService                  _service           = null!;

    private static readonly DateOnly TestDate = new DateOnly(2026, 8, 15);

    [SetUp]
    public void Setup()
    {
        _mockPassengerRepo = new Mock<IPassengerRepository>();
        _mockPurchaseRepo  = new Mock<IPurchaseRepository>();
        _mockCodeGenerator = new Mock<ICodeGenerator>();
        _mockPricing       = new Mock<IPricingCalculator>();
        _mockRouteService  = new Mock<IRouteCreationService>();
        _mockLogger        = new Mock<ILogger<PurchaseService>>();

        _service = new PurchaseService(
            _mockPassengerRepo.Object,
            _mockPurchaseRepo.Object,
            _mockCodeGenerator.Object,
            _mockPricing.Object,
            _mockRouteService.Object,
            _mockLogger.Object);
    }

    private static RouteCreationModel Route(int economyCap, int firstClassCap, string type = "B737") =>
        new() { EconomyClassCapacity = economyCap, FirstClassCapacity = firstClassCap, AircraftTypeId = type };

    private static RouteCreationModel PricedRoute() => new()
    {
        EconomyClassCapacity = 100,
        FirstClassCapacity   = 20,
        AircraftTypeId       = "B737",
        PriceEconomy         = 100m,
        PriceFirstClass      = 200m,
        HandBagPrice         = 10m,
        BagPrice             = 20m,
        BagMultiplier        = 1m
    };

    private static PurchaseTotals MinimalTotals(int count) => new()
    {
        TotalPaid        = 100m * count,
        TotalSeats       = count,
        DetailByClass    = [new SeatClassSubtotal { SeatClass = SeatClass.Economy, SeatCount = count, Subtotal = 100m * count }],
        BaggageDetails   = [],
        PassengerBaggageDetails = Enumerable.Range(0, count)
            .Select(i => new PassengerBaggageSubtotal { PassengerIndex = i })
            .ToList()
    };

    private static PurchaseRequestModel BuildRequest(int passengerCount = 2, bool stopover = false)
    {
        var passengers = Enumerable.Range(0, passengerCount)
            .Select(i => new PassengerInfo
            {
                FirstName       = $"Pasajero{i}",
                LastName        = "Test",
                PassportCountry = "Costa Rica",
                BirthDate       = new DateOnly(1990, 1, 1),
                Gender          = Gender.Male
            })
            .ToList();

        var seats = Enumerable.Range(0, passengerCount)
            .Select(i => new SeatSelection { PassengerIndex = i, SeatClass = SeatClass.Economy })
            .ToList();

        return new PurchaseRequestModel
        {
            Passengers     = passengers,
            SeatSelections = seats,
            Payment        = new PaymentInfo { Method = PaymentMethod.Visa, ContactEmail = "test@test.com" },
            Flight         = new FlightSelection { RouteCode = "R1", FlightDate = TestDate },
            Flight2        = stopover ? new FlightSelection { RouteCode = "R2", FlightDate = TestDate } : null
        };
    }

    private void SetupHappyPath(int passengerCount = 2)
    {
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(PricedRoute());
        _mockRouteService.Setup(s => s.GetRouteByCode("R2")).Returns(PricedRoute());
        _mockRouteService
            .Setup(s => s.GetOrCreateScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(42);
        _mockRouteService
            .Setup(s => s.GetOrCreateScheduledFlight("R2", It.IsAny<DateTime>()))
            .Returns(43);
        _mockPurchaseRepo
            .Setup(r => r.HasAvailableSeatsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        _mockPurchaseRepo
            .Setup(r => r.GetNextAvailableSeatNumbersAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int _, int count) => Enumerable.Range(1, count).ToList());
        _mockPurchaseRepo
            .Setup(r => r.ReservationCodeExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockPurchaseRepo
            .Setup(r => r.InvoiceNumberExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _mockPurchaseRepo
            .Setup(r => r.ExecutePurchaseTransactionAsync(It.IsAny<PurchaseTransactionData>()))
            .ReturnsAsync(100);
        _mockCodeGenerator.Setup(c => c.GenerateReservationCode()).Returns("RES-001");
        _mockCodeGenerator.Setup(c => c.GenerateInvoiceNumber()).Returns("INV-001");
        _mockPassengerRepo
            .Setup(r => r.CreatePassengerAsync(It.IsAny<PassengerInfo>()))
            .ReturnsAsync(1);
        _mockPricing
            .Setup(p => p.Calculate(
                It.IsAny<List<SeatSelection>>(), It.IsAny<List<PassengerInfo>>(),
                It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<decimal>(), It.IsAny<IEnumerable<BagPricing>>()))
            .Returns(MinimalTotals(passengerCount));
    }


    [Test]
    public async Task IsFlightAvailable_RouteNotFound_ShouldReturnTrue()
    {
        // Arrange
        _mockRouteService
            .Setup(s => s.GetRouteByCode(It.IsAny<string>()))
            .Throws<Exception>();

        // Act
        var result = await _service.IsFlightAvailableAsync("UNKNOWN", TestDate, 0, 1);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFlightAvailable_RouteAndAircraftCapacityBothZero_ShouldReturnTrue()
    {
        // Arrange
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(0, 0, "B737"));
        _mockPurchaseRepo.Setup(r => r.GetAircraftCapacityByClassAsync("B737")).ReturnsAsync((0, 0));

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 0, 5);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFlightAvailable_RouteCapacityZero_FallsBackToAircraftCapacity_CountFits_ShouldReturnTrue()
    {
        // Arrange
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(0, 0, "B737"));
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns((int?)null);
        _mockPurchaseRepo.Setup(r => r.GetAircraftCapacityByClassAsync("B737")).ReturnsAsync((0, 10));

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 0, 5);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFlightAvailable_RouteCapacityZero_FallsBackToAircraftCapacity_CountExceeds_ShouldReturnFalse()
    {
        // Arrange
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(0, 0, "B737"));
        _mockPurchaseRepo.Setup(r => r.GetAircraftCapacityByClassAsync("B737")).ReturnsAsync((0, 10));

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 0, 12);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsFlightAvailable_RequestedCountExceedsTotalCapacity_ShouldReturnFalse()
    {
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(6, 2));

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 0, 9);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsFlightAvailable_NoScheduledFlight_CountFitsCapacity_ShouldReturnTrue()
    {
        // Arrange
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(6, 2));
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns((int?)null);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 0, 4);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFlightAvailable_BookedSeatsAndRequestedExactlyFillCapacity_ShouldReturnTrue()
    {
        // Arrange
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(6, 2));
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(42);
        _mockPurchaseRepo.Setup(r => r.GetBookedSeatsByClassAsync(42, "Economy")).ReturnsAsync(2);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 0, 4);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsFlightAvailable_BookedPlusRequestedExceedsCapacity_ShouldReturnFalse()
    {
        // Arrange
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(6, 2));
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(42);
        _mockPurchaseRepo.Setup(r => r.GetBookedSeatsByClassAsync(42, "Economy")).ReturnsAsync(5);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 0, 4);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task IsFlightAvailable_FlightFullyBooked_SingleSeatRequest_ShouldReturnFalse()
    {
        // Arrange
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(Route(6, 0));
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(10);
        _mockPurchaseRepo.Setup(r => r.GetBookedSeatsByClassAsync(10, "Economy")).ReturnsAsync(6);

        // Act
        var result = await _service.IsFlightAvailableAsync("R1", TestDate, 0, 1);

        // Assert
        Assert.That(result, Is.False);
    }


    [Test]
    public async Task CheckDuplicates_NoScheduledFlight_ShouldReturnEmptyList()
    {
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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


    [Test]
    public void CreatePurchase_NoSeatsAvailable_ShouldThrowSeatUnavailableException()
    {
        // Arrange
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(PricedRoute());
        _mockRouteService
            .Setup(s => s.GetOrCreateScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(42);
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(42);
        _mockPurchaseRepo
            .Setup(r => r.GetBookedSeatsByClassAsync(42, "Economy"))
            .ReturnsAsync(99);

        // Act & Assert
        Assert.That(
            () => _service.CreatePurchaseAsync(BuildRequest()),
            Throws.InstanceOf<SeatUnavailableException>());
    }

    [Test]
    public async Task CreatePurchase_HappyPath_ShouldCallExecuteTransactionExactlyOnce()
    {
        // Arrange
        SetupHappyPath(passengerCount: 2);

        // Act
        await _service.CreatePurchaseAsync(BuildRequest(passengerCount: 2));

        // Assert
        _mockPurchaseRepo.Verify(
            r => r.ExecutePurchaseTransactionAsync(It.IsAny<PurchaseTransactionData>()),
            Times.Once);
    }

    [Test]
    public async Task CreatePurchase_HappyPath_ShouldCallCreatePassengerForEachPassenger()
    {
        // Arrange
        const int passengerCount = 3;
        SetupHappyPath(passengerCount: passengerCount);

        // Act
        await _service.CreatePurchaseAsync(BuildRequest(passengerCount: passengerCount));

        // Assert
        _mockPassengerRepo.Verify(
            r => r.CreatePassengerAsync(It.IsAny<PassengerInfo>()),
            Times.Exactly(passengerCount));
    }

    [Test]
    public async Task CreatePurchase_HappyPath_TransactionDataHasTicketPerSeat()
    {
        // Arrange
        const int passengerCount = 3;
        SetupHappyPath(passengerCount: passengerCount);

        PurchaseTransactionData? captured = null;
        _mockPurchaseRepo
            .Setup(r => r.ExecutePurchaseTransactionAsync(It.IsAny<PurchaseTransactionData>()))
            .Callback<PurchaseTransactionData>(d => captured = d)
            .ReturnsAsync(100);

        // Act
        await _service.CreatePurchaseAsync(BuildRequest(passengerCount: passengerCount));

        // Assert
        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Tickets1, Has.Count.EqualTo(passengerCount));
        Assert.That(captured.Tickets2, Is.Null);
    }

    [Test]
    public void CreatePurchase_Stopover_SecondFlightFull_ShouldThrowSeatUnavailableException()
    {
        // Arrange
        _mockRouteService.Setup(s => s.GetRouteByCode("R1")).Returns(PricedRoute());
        _mockRouteService.Setup(s => s.GetRouteByCode("R2")).Returns(PricedRoute());
        _mockRouteService
            .Setup(s => s.GetOrCreateScheduledFlight("R1", It.IsAny<DateTime>()))
            .Returns(42);
        _mockRouteService
            .Setup(s => s.GetOrCreateScheduledFlight("R2", It.IsAny<DateTime>()))
            .Returns(43);
        _mockRouteService
            .Setup(s => s.FindExistingScheduledFlight("R2", It.IsAny<DateTime>()))
            .Returns(43);
        _mockPurchaseRepo
            .Setup(r => r.GetBookedSeatsByClassAsync(43, "Economy"))
            .ReturnsAsync(99);

        // Act & Assert
        Assert.That(
            () => _service.CreatePurchaseAsync(BuildRequest(stopover: true)),
            Throws.InstanceOf<SeatUnavailableException>());
    }

    [Test]
    public async Task CreatePurchase_Stopover_TransactionDataHasTicketsForBothFlights()
    {
        // Arrange
        const int passengerCount = 2;
        SetupHappyPath(passengerCount: passengerCount);

        PurchaseTransactionData? captured = null;
        _mockPurchaseRepo
            .Setup(r => r.ExecutePurchaseTransactionAsync(It.IsAny<PurchaseTransactionData>()))
            .Callback<PurchaseTransactionData>(d => captured = d)
            .ReturnsAsync(100);

        // Act
        await _service.CreatePurchaseAsync(BuildRequest(passengerCount: passengerCount, stopover: true));

        // Assert
        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Tickets1, Has.Count.EqualTo(passengerCount));
        Assert.That(captured.Tickets2, Has.Count.EqualTo(passengerCount));
    }
}
