using backend.Model;
using backend.Services;

namespace backend.Tests;

[TestFixture]
public class PurchasePricingCalculatorTests
{
    private PurchasePricingCalculator _calculator = null!;

    [SetUp]
    public void Setup()
    {
        _calculator = new PurchasePricingCalculator();
    }


    [Test]
    public void Calculate_OnlyEconomySeats_ShouldReturnCorrectTotal()
    {
        // Arrange
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 },
            new() { PassengerIndex = 1, SeatClass = "Economy", SeatNumber = 2 },
            new() { PassengerIndex = 2, SeatClass = "Economy", SeatNumber = 3 },
        };
        decimal economyPrice    = 100m;
        decimal expectedTotal   = 300m;
        int     expectedSeats   = 3;

        // Act
        var result = _calculator.Calculate(seats, economyPrice, firstClassPrice: 300m);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalPaid,  Is.EqualTo(expectedTotal));
            Assert.That(result.TotalSeats, Is.EqualTo(expectedSeats));
        });
    }

    [Test]
    public void Calculate_OnlyFirstClassSeats_ShouldReturnCorrectTotal()
    {
        // Arrange
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "FirstClass", SeatNumber = 1 },
            new() { PassengerIndex = 1, SeatClass = "FirstClass", SeatNumber = 2 },
        };
        decimal firstClassPrice = 400m;
        decimal expectedTotal   = 800m;
        int     expectedSeats   = 2;

        // Act
        var result = _calculator.Calculate(seats, economyPrice: 100m, firstClassPrice);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalPaid,  Is.EqualTo(expectedTotal));
            Assert.That(result.TotalSeats, Is.EqualTo(expectedSeats));
        });
    }

    [Test]
    public void Calculate_MixedClasses_ShouldSumBothSubtotals()
    {
        // Arrange
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy",    SeatNumber = 5 },
            new() { PassengerIndex = 1, SeatClass = "Economy",    SeatNumber = 6 },
            new() { PassengerIndex = 2, SeatClass = "FirstClass", SeatNumber = 1 },
        };
        decimal economyPrice    = 150m;
        decimal firstClassPrice = 500m;
        decimal expectedTotal   = 800m; // 2 × 150 + 1 × 500
        int     expectedSeats   = 3;

        // Act
        var result = _calculator.Calculate(seats, economyPrice, firstClassPrice);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalPaid,  Is.EqualTo(expectedTotal));
            Assert.That(result.TotalSeats, Is.EqualTo(expectedSeats));
        });
    }

    // ── DetailByClass ─────────────────────────────────────────────────────────

    [Test]
    public void Calculate_MixedClasses_ShouldProduceTwoDetailEntries()
    {
        // Arrange
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy",    SeatNumber = 10 },
            new() { PassengerIndex = 1, SeatClass = "FirstClass", SeatNumber = 1  },
        };

        // Act
        var result = _calculator.Calculate(seats, economyPrice: 200m, firstClassPrice: 600m);

        // Assert
        Assert.That(result.DetailByClass, Has.Count.EqualTo(2));
    }

    [Test]
    public void Calculate_EconomyDetail_ShouldHaveCorrectCountAndSubtotal()
    {
        // Arrange
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 3 },
            new() { PassengerIndex = 1, SeatClass = "Economy", SeatNumber = 4 },
        };
        decimal economyPrice     = 250m;
        int     expectedCount    = 2;
        decimal expectedSubtotal = 500m;

        // Act
        var result = _calculator.Calculate(seats, economyPrice, firstClassPrice: 999m);

        // Assert
        var detail = result.DetailByClass.Single(d => d.SeatClass == "Economy");
        Assert.Multiple(() =>
        {
            Assert.That(detail.SeatCount,    Is.EqualTo(expectedCount));
            Assert.That(detail.Subtotal, Is.EqualTo(expectedSubtotal));
        });
    }

    [Test]
    public void Calculate_FirstClassDetail_ShouldHaveCorrectCountAndSubtotal()
    {
        // Arrange
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "FirstClass", SeatNumber = 1 },
            new() { PassengerIndex = 1, SeatClass = "FirstClass", SeatNumber = 2 },
            new() { PassengerIndex = 2, SeatClass = "FirstClass", SeatNumber = 3 },
        };
        decimal firstClassPrice  = 700m;
        int     expectedCount    = 3;
        decimal expectedSubtotal = 2100m;

        // Act
        var result = _calculator.Calculate(seats, economyPrice: 50m, firstClassPrice);

        // Assert
        var detail = result.DetailByClass.Single(d => d.SeatClass == "FirstClass");
        Assert.Multiple(() =>
        {
            Assert.That(detail.SeatCount,    Is.EqualTo(expectedCount));
            Assert.That(detail.Subtotal, Is.EqualTo(expectedSubtotal));
        });
    }


    [Test]
    public void Calculate_EmptySeatList_ShouldReturnZeroTotals()
    {
        // Arrange
        var seats = new List<SeatSelection>();

        // Act
        var result = _calculator.Calculate(seats, economyPrice: 100m, firstClassPrice: 200m);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalPaid,     Is.EqualTo(0m));
            Assert.That(result.TotalSeats,    Is.EqualTo(0));
            Assert.That(result.DetailByClass, Is.Empty);
        });
    }

    [Test]
    public void Calculate_SingleSeat_ShouldReturnUnitPrice()
    {
        // Arrange
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 }
        };
        decimal economyPrice  = 99.99m;
        int     expectedSeats = 1;

        // Act
        var result = _calculator.Calculate(seats, economyPrice, firstClassPrice: 0m);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.TotalPaid,  Is.EqualTo(economyPrice));
            Assert.That(result.TotalSeats, Is.EqualTo(expectedSeats));
        });
    }

    [Test]
    public void Calculate_UnknownSeatClass_ShouldThrowArgumentException()
    {
        // Arrange
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Business", SeatNumber = 1 }
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _calculator.Calculate(seats, economyPrice: 100m, firstClassPrice: 200m));
    }

    [Test]
    public void Calculate_TotalPaid_ShouldEqualSumOfAllSubtotals()
    {
        // Arrange
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy",    SeatNumber = 5 },
            new() { PassengerIndex = 1, SeatClass = "Economy",    SeatNumber = 6 },
            new() { PassengerIndex = 2, SeatClass = "FirstClass", SeatNumber = 1 },
            new() { PassengerIndex = 3, SeatClass = "FirstClass", SeatNumber = 2 },
        };

        // Act
        var result = _calculator.Calculate(seats, economyPrice: 175m, firstClassPrice: 450m);

        // Assert
        decimal expectedTotal = result.DetailByClass.Sum(d => d.Subtotal);
        Assert.That(result.TotalPaid, Is.EqualTo(expectedTotal));
    }
}
