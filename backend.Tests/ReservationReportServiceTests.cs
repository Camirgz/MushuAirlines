using backend.Interfaces;
using backend.Model;
using backend.Services;
using Moq;

namespace backend.Tests;

[TestFixture]
public class ReservationReportServiceTests
{
    private Mock<IPurchaseConfirmationRepository> _repository = null!;
    private ReservationReportService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new Mock<IPurchaseConfirmationRepository>();

        _service = new ReservationReportService(
            _repository.Object);
    }

    [Test]
    public void GetReservation_ExistingReservation_ShouldReturnReservation()
    {
        // Arrange
        var reservation = new PurchaseConfirmationModel
        {
            PurchaseId = 10,
            ReservationCode = "AQG2B5",
            FullName = "Leo Sibaja"
        };

        _repository
            .Setup(r => r.GetPurchaseIdByReservationCode("AQG2B5"))
            .Returns(10);

        _repository
            .Setup(r => r.GetPurchase(10))
            .Returns(reservation);

        // Act
        var result = _service.GetReservation("AQG2B5");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ReservationCode, Is.EqualTo("AQG2B5"));
        Assert.That(result.FullName, Is.EqualTo("Leo Sibaja"));
    }

    [Test]
    public void GetReservation_ReservationNotFound_ShouldThrowException()
    {
        // Arrange
        _repository
            .Setup(r => r.GetPurchaseIdByReservationCode("AQG2B5"))
            .Returns(10);

        _repository
            .Setup(r => r.GetPurchase(10))
            .Returns((PurchaseConfirmationModel)null!);

        // Act & Assert
        var ex = Assert.Throws<Exception>(() =>
            _service.GetReservation("AQG2B5"))!;

        Assert.That(ex.Message, Is.EqualTo("Reserva no encontrada."));
    }

    private void SetupAddBaggage(
        int purchaseId,
        List<BagPricing> legs,
        List<PassengerBaggageDetail> currentBags)
    {
        _repository.Setup(r => r.GetPurchaseIdByReservationCode("AQG2B5")).Returns(purchaseId);
        _repository.Setup(r => r.GetBagPricingByPurchaseId(purchaseId)).Returns(legs);
        _repository.Setup(r => r.GetPassengerBaggageDetails(purchaseId)).Returns(currentBags);
    }

    [Test]
    public void AddBaggage_NoBagsRequested_ShouldThrowArgumentException()
    {
        _repository.Setup(r => r.GetPurchaseIdByReservationCode("AQG2B5")).Returns(10);

        var request = new AddBaggageRequest
        {
            Passengers = [new PassengerBaggageAddition { PassengerFullName = "Leo Sibaja", ExtraBags = 0 }]
        };

        var ex = Assert.Throws<ArgumentException>(() => _service.AddBaggage("AQG2B5", request));
        Assert.That(ex!.Message, Is.EqualTo("Debe seleccionar al menos una maleta adicional."));
    }

    [Test]
    public void AddBaggage_FirstBag_ShouldChargeBaseBagPrice()
    {
        SetupAddBaggage(10,
            legs:        [new() { BagPrice = 35m, BagMultiplier = 1.5m }],
            currentBags: [new() { PassengerFullName = "Leo Sibaja", CheckedBagCount = 0 }]);

        var result = _service.AddBaggage("AQG2B5", new AddBaggageRequest
        {
            Passengers = [new PassengerBaggageAddition { PassengerFullName = "Leo Sibaja", ExtraBags = 1 }]
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCharged,   Is.EqualTo(35m));
            Assert.That(result.TotalBagsAdded, Is.EqualTo(1));
        });
    }

    [Test]
    public void AddBaggage_SecondBag_ShouldApplyMultiplier()
    {
        SetupAddBaggage(10,
            legs:        [new() { BagPrice = 35m, BagMultiplier = 1.5m }],
            currentBags: [new() { PassengerFullName = "Leo Sibaja", CheckedBagCount = 1 }]);

        var result = _service.AddBaggage("AQG2B5", new AddBaggageRequest
        {
            Passengers = [new PassengerBaggageAddition { PassengerFullName = "Leo Sibaja", ExtraBags = 1 }]
        });

        Assert.That(result.TotalCharged, Is.EqualTo(52.5m));
    }

    [Test]
    public void AddBaggage_MultipleExtraBags_ShouldApplyMultiplierFromSecond()
    {
        SetupAddBaggage(10,
            legs:        [new() { BagPrice = 35m, BagMultiplier = 1.5m }],
            currentBags: [new() { PassengerFullName = "Leo Sibaja", CheckedBagCount = 0 }]);

        var result = _service.AddBaggage("AQG2B5", new AddBaggageRequest
        {
            Passengers = [new PassengerBaggageAddition { PassengerFullName = "Leo Sibaja", ExtraBags = 3 }]
        });

        Assert.That(result.TotalCharged, Is.EqualTo(140m));
    }

    [Test]
    public void AddBaggage_Stopover_FirstBag_ShouldSumBothLegPrices()
    {
        SetupAddBaggage(10,
            legs: [
                new() { BagPrice = 30m, BagMultiplier = 1.5m },
                new() { BagPrice = 25m, BagMultiplier = 1.2m }
            ],
            currentBags: [new() { PassengerFullName = "Leo Sibaja", CheckedBagCount = 0 }]);

        var result = _service.AddBaggage("AQG2B5", new AddBaggageRequest
        {
            Passengers = [new PassengerBaggageAddition { PassengerFullName = "Leo Sibaja", ExtraBags = 1 }]
        });

        Assert.That(result.TotalCharged, Is.EqualTo(55m));
    }

    [Test]
    public void AddBaggage_Stopover_SecondBag_UsesEachLegOwnMultiplier()
    {
        SetupAddBaggage(10,
            legs: [
                new() { BagPrice = 30m, BagMultiplier = 1.5m },
                new() { BagPrice = 25m, BagMultiplier = 1.2m }
            ],
            currentBags: [new() { PassengerFullName = "Leo Sibaja", CheckedBagCount = 1 }]);

        var result = _service.AddBaggage("AQG2B5", new AddBaggageRequest
        {
            Passengers = [new PassengerBaggageAddition { PassengerFullName = "Leo Sibaja", ExtraBags = 1 }]
        });

        Assert.That(result.TotalCharged, Is.EqualTo(75m));
    }

    [Test]
    public void AddBaggage_MultiplePassengers_ChargesEachIndependently()
    {
        SetupAddBaggage(10,
            legs:        [new() { BagPrice = 35m, BagMultiplier = 1.5m }],
            currentBags: [
                new() { PassengerFullName = "Ana Mora",  CheckedBagCount = 0 },
                new() { PassengerFullName = "Luis Vega", CheckedBagCount = 1 }
            ]);

        var result = _service.AddBaggage("AQG2B5", new AddBaggageRequest
        {
            Passengers = [
                new PassengerBaggageAddition { PassengerFullName = "Ana Mora",  ExtraBags = 1 },
                new PassengerBaggageAddition { PassengerFullName = "Luis Vega", ExtraBags = 1 }
            ]
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCharged,   Is.EqualTo(87.5m));
            Assert.That(result.TotalBagsAdded, Is.EqualTo(2));
        });
    }

    [Test]
    public void AddBaggage_PassengerWithZeroExtraBags_ShouldBeIgnored()
    {
        SetupAddBaggage(10,
            legs:        [new() { BagPrice = 35m, BagMultiplier = 1.5m }],
            currentBags: [
                new() { PassengerFullName = "Leo Sibaja", CheckedBagCount = 0 },
                new() { PassengerFullName = "Ana Mora",   CheckedBagCount = 0 }
            ]);

        var result = _service.AddBaggage("AQG2B5", new AddBaggageRequest
        {
            Passengers = [
                new PassengerBaggageAddition { PassengerFullName = "Leo Sibaja", ExtraBags = 1 },
                new PassengerBaggageAddition { PassengerFullName = "Ana Mora",   ExtraBags = 0 }
            ]
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCharged,   Is.EqualTo(35m));
            Assert.That(result.TotalBagsAdded, Is.EqualTo(1));
        });
    }


    [Test]
    public void GetReservation_WithTickets_ShouldReturnPassengerTickets()
    {
        // Arrange
        var reservation = new PurchaseConfirmationModel
        {
            PurchaseId = 10,
            ReservationCode = "AQG2B5",
            FullName = "Leo Sibaja",
            Tickets = new List<TicketSummary>
            {
                new()
                {
                    PassengerFullName = "Leo Sibaja",
                    SeatNumber = "5",
                    SeatClass = SeatClass.Economy,
                    FlightNumber = "15"
                }
            }
        };

        _repository
            .Setup(r => r.GetPurchaseIdByReservationCode("AQG2B5"))
            .Returns(10);

        _repository
            .Setup(r => r.GetPurchase(10))
            .Returns(reservation);

        // Act
        var result = _service.GetReservation("AQG2B5");

        // Assert
        Assert.That(result.Tickets, Is.Not.Null);
        Assert.That(result.Tickets.Count, Is.EqualTo(1));

        Assert.That(
            result.Tickets[0].PassengerFullName,
            Is.EqualTo("Leo Sibaja"));

        Assert.That(
            result.Tickets[0].FlightNumber,
            Is.EqualTo("15"));
    }
}