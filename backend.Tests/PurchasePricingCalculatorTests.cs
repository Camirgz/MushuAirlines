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

    private static List<PassengerInfo> NoBaggage(int count) =>
        Enumerable.Range(0, count).Select(_ => new PassengerInfo()).ToList();

    private static List<PassengerInfo> WithBags(params (int hand, int checkedBag)[] bags) =>
        bags.Select(b => new PassengerInfo { HandBagCount = b.hand, CheckedBagCount = b.checkedBag }).ToList();

    // ── Seat totals ───────────────────────────────────────────────────────────

    [Test]
    public void Calculate_OnlyEconomySeats_NoBaggage_ShouldReturnCorrectTotal()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 },
            new() { PassengerIndex = 1, SeatClass = "Economy", SeatNumber = 2 },
            new() { PassengerIndex = 2, SeatClass = "Economy", SeatNumber = 3 },
        };

        var result = _calculator.Calculate(seats, NoBaggage(3), economyPrice: 100m, firstClassPrice: 300m, 0m, 0m, 1m);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalPaid,  Is.EqualTo(300m));
            Assert.That(result.TotalSeats, Is.EqualTo(3));
        });
    }

    [Test]
    public void Calculate_OnlyFirstClassSeats_NoBaggage_ShouldReturnCorrectTotal()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "FirstClass", SeatNumber = 1 },
            new() { PassengerIndex = 1, SeatClass = "FirstClass", SeatNumber = 2 },
        };

        var result = _calculator.Calculate(seats, NoBaggage(2), economyPrice: 100m, firstClassPrice: 400m, 0m, 0m, 1m);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalPaid,  Is.EqualTo(800m));
            Assert.That(result.TotalSeats, Is.EqualTo(2));
        });
    }

    [Test]
    public void Calculate_MixedClasses_NoBaggage_ShouldSumBothSubtotals()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy",    SeatNumber = 5 },
            new() { PassengerIndex = 1, SeatClass = "Economy",    SeatNumber = 6 },
            new() { PassengerIndex = 2, SeatClass = "FirstClass", SeatNumber = 1 },
        };

        var result = _calculator.Calculate(seats, NoBaggage(3), economyPrice: 150m, firstClassPrice: 500m, 0m, 0m, 1m);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalPaid,  Is.EqualTo(800m)); // 2×150 + 1×500
            Assert.That(result.TotalSeats, Is.EqualTo(3));
        });
    }

    [Test]
    public void Calculate_EmptySeatList_ShouldReturnZeroTotals()
    {
        var result = _calculator.Calculate([], NoBaggage(0), economyPrice: 100m, firstClassPrice: 200m, 0m, 0m, 1m);

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
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 }
        };

        var result = _calculator.Calculate(seats, NoBaggage(1), economyPrice: 99.99m, firstClassPrice: 0m, 0m, 0m, 1m);

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalPaid,  Is.EqualTo(99.99m));
            Assert.That(result.TotalSeats, Is.EqualTo(1));
        });
    }

    [Test]
    public void Calculate_UnknownSeatClass_ShouldThrowArgumentException()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Business", SeatNumber = 1 }
        };

        Assert.Throws<ArgumentException>(() =>
            _calculator.Calculate(seats, NoBaggage(1), economyPrice: 100m, firstClassPrice: 200m, 0m, 0m, 1m));
    }

    [Test]
    public void Calculate_TotalPaid_ShouldEqualSumOfAllSubtotals()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy",    SeatNumber = 5 },
            new() { PassengerIndex = 1, SeatClass = "Economy",    SeatNumber = 6 },
            new() { PassengerIndex = 2, SeatClass = "FirstClass", SeatNumber = 1 },
            new() { PassengerIndex = 3, SeatClass = "FirstClass", SeatNumber = 2 },
        };

        var result = _calculator.Calculate(seats, NoBaggage(4), economyPrice: 175m, firstClassPrice: 450m, 0m, 0m, 1m);

        decimal expectedTotal = result.DetailByClass.Sum(d => d.Subtotal);
        Assert.That(result.TotalPaid, Is.EqualTo(expectedTotal));
    }

    // ── DetailByClass ─────────────────────────────────────────────────────────

    [Test]
    public void Calculate_MixedClasses_ShouldProduceTwoDetailEntries()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy",    SeatNumber = 10 },
            new() { PassengerIndex = 1, SeatClass = "FirstClass", SeatNumber = 1  },
        };

        var result = _calculator.Calculate(seats, NoBaggage(2), economyPrice: 200m, firstClassPrice: 600m, 0m, 0m, 1m);

        Assert.That(result.DetailByClass, Has.Count.EqualTo(2));
    }

    [Test]
    public void Calculate_EconomyDetail_ShouldHaveCorrectCountAndSubtotal()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 3 },
            new() { PassengerIndex = 1, SeatClass = "Economy", SeatNumber = 4 },
        };

        var result = _calculator.Calculate(seats, NoBaggage(2), economyPrice: 250m, firstClassPrice: 999m, 0m, 0m, 1m);

        var detail = result.DetailByClass.Single(d => d.SeatClass == "Economy");
        Assert.Multiple(() =>
        {
            Assert.That(detail.SeatCount, Is.EqualTo(2));
            Assert.That(detail.Subtotal,  Is.EqualTo(500m));
        });
    }

    [Test]
    public void Calculate_FirstClassDetail_ShouldHaveCorrectCountAndSubtotal()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "FirstClass", SeatNumber = 1 },
            new() { PassengerIndex = 1, SeatClass = "FirstClass", SeatNumber = 2 },
            new() { PassengerIndex = 2, SeatClass = "FirstClass", SeatNumber = 3 },
        };

        var result = _calculator.Calculate(seats, NoBaggage(3), economyPrice: 50m, firstClassPrice: 700m, 0m, 0m, 1m);

        var detail = result.DetailByClass.Single(d => d.SeatClass == "FirstClass");
        Assert.Multiple(() =>
        {
            Assert.That(detail.SeatCount, Is.EqualTo(3));
            Assert.That(detail.Subtotal,  Is.EqualTo(2100m));
        });
    }

    // ── Baggage pricing: first unit at base price, second+ with multiplier ────

    [Test]
    public void Calculate_OneHandBag_ShouldChargeBasePrice()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 },
        };

        var result = _calculator.Calculate(seats, WithBags((hand: 1, checkedBag: 0)),
            economyPrice: 100m, firstClassPrice: 0m, handBagPrice: 30m, bagPrice: 0m, bagMultiplier: 2m);

        Assert.Multiple(() =>
        {
            Assert.That(result.HandBaggageSubtotal, Is.EqualTo(30m));
            Assert.That(result.TotalPaid,           Is.EqualTo(130m));
        });
    }

    [Test]
    public void Calculate_TwoHandBags_SecondShouldUseMultiplier()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 },
        };

        // 1st bag: 30, 2nd bag: 30 × 2 = 60 → total hand = 90
        var result = _calculator.Calculate(seats, WithBags((hand: 2, checkedBag: 0)),
            economyPrice: 100m, firstClassPrice: 0m, handBagPrice: 30m, bagPrice: 0m, bagMultiplier: 2m);

        Assert.Multiple(() =>
        {
            Assert.That(result.HandBaggageSubtotal, Is.EqualTo(90m));
            Assert.That(result.TotalPaid,           Is.EqualTo(190m));
        });
    }

    [Test]
    public void Calculate_ThreeHandBags_SecondAndThirdShouldUseMultiplier()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 },
        };

        // 1st: 30, 2nd: 30×2=60, 3rd: 30×2=60 → total hand = 150
        var result = _calculator.Calculate(seats, WithBags((hand: 3, checkedBag: 0)),
            economyPrice: 0m, firstClassPrice: 0m, handBagPrice: 30m, bagPrice: 0m, bagMultiplier: 2m);

        Assert.That(result.HandBaggageSubtotal, Is.EqualTo(150m));
    }

    [Test]
    public void Calculate_OneCheckedBag_ShouldChargeBasePriceWithoutMultiplier()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 },
        };

        var result = _calculator.Calculate(seats, WithBags((hand: 0, checkedBag: 1)),
            economyPrice: 100m, firstClassPrice: 0m, handBagPrice: 0m, bagPrice: 50m, bagMultiplier: 1.5m);

        Assert.Multiple(() =>
        {
            Assert.That(result.CheckedBaggageSubtotal, Is.EqualTo(50m)); // no multiplier on first bag
            Assert.That(result.TotalPaid,              Is.EqualTo(150m));
        });
    }

    [Test]
    public void Calculate_TwoCheckedBags_SecondShouldUseMultiplier()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 },
        };

        // 1st: 50, 2nd: 50×1.5=75 → total checked = 125
        var result = _calculator.Calculate(seats, WithBags((hand: 0, checkedBag: 2)),
            economyPrice: 100m, firstClassPrice: 0m, handBagPrice: 0m, bagPrice: 50m, bagMultiplier: 1.5m);

        Assert.Multiple(() =>
        {
            Assert.That(result.CheckedBaggageSubtotal, Is.EqualTo(125m));
            Assert.That(result.TotalPaid,              Is.EqualTo(225m));
        });
    }

    [Test]
    public void Calculate_BothBaggageTypes_ShouldApplyRuleIndependently()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 },
        };

        // hand: 1×20=20 (first bag, no multiplier)
        // checked: 50 + 50×1.0=100 (second bag, multiplier=1.0 so same price)
        var result = _calculator.Calculate(seats, WithBags((hand: 1, checkedBag: 2)),
            economyPrice: 100m, firstClassPrice: 0m, handBagPrice: 20m, bagPrice: 50m, bagMultiplier: 1.0m);

        Assert.Multiple(() =>
        {
            Assert.That(result.HandBaggageSubtotal,    Is.EqualTo(20m));
            Assert.That(result.CheckedBaggageSubtotal, Is.EqualTo(100m));
            Assert.That(result.TotalPaid,              Is.EqualTo(220m));
        });
    }

    [Test]
    public void Calculate_MultiplePassengers_BaggageCalculatedPerPassenger()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 },
            new() { PassengerIndex = 1, SeatClass = "Economy", SeatNumber = 2 },
        };

        // Passenger 0: 2 checked → 50 + 50×2 = 150
        // Passenger 1: 1 checked → 50 (no multiplier)
        // Total checked = 200
        var result = _calculator.Calculate(seats,
            WithBags((hand: 0, checkedBag: 2), (hand: 0, checkedBag: 1)),
            economyPrice: 100m, firstClassPrice: 0m, handBagPrice: 0m, bagPrice: 50m, bagMultiplier: 2m);

        Assert.Multiple(() =>
        {
            Assert.That(result.CheckedBaggageCount,    Is.EqualTo(3));
            Assert.That(result.CheckedBaggageSubtotal, Is.EqualTo(200m));
            Assert.That(result.TotalPaid,              Is.EqualTo(400m)); // 2×100 seats + 200 bags
        });
    }

    [Test]
    public void Calculate_PassengerBaggageDetails_ShouldContainEntryPerPassenger()
    {
        var seats = new List<SeatSelection>
        {
            new() { PassengerIndex = 0, SeatClass = "Economy", SeatNumber = 1 },
            new() { PassengerIndex = 1, SeatClass = "Economy", SeatNumber = 2 },
        };

        var result = _calculator.Calculate(seats,
            WithBags((hand: 1, checkedBag: 0), (hand: 0, checkedBag: 2)),
            economyPrice: 0m, firstClassPrice: 0m, handBagPrice: 20m, bagPrice: 50m, bagMultiplier: 1.5m);

        Assert.That(result.PassengerBaggageDetails, Has.Count.EqualTo(2));

        var p0 = result.PassengerBaggageDetails[0];
        var p1 = result.PassengerBaggageDetails[1];

        Assert.Multiple(() =>
        {
            Assert.That(p0.HandSubtotal,    Is.EqualTo(20m));   // 1 hand bag, no multiplier
            Assert.That(p0.CheckedSubtotal, Is.EqualTo(0m));
            Assert.That(p1.HandSubtotal,    Is.EqualTo(0m));
            Assert.That(p1.CheckedSubtotal, Is.EqualTo(125m));  // 50 + 50×1.5
        });
    }
}
