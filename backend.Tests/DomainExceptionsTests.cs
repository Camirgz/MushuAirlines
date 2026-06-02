using backend.Exceptions;

namespace backend.Tests;

[TestFixture]
public class DomainExceptionsTests
{

    [Test]
    public void SeatUnavailableException_ShouldStoreProperties()
    {
        // Arrange
        string expectedSeat      = "12A";
        int    expectedFlightId  = 99;

        // Act
        var ex = new SeatUnavailableException(expectedSeat, expectedFlightId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.SeatNumber,        Is.EqualTo(expectedSeat));
            Assert.That(ex.ScheduledFlightId, Is.EqualTo(expectedFlightId));
        });
    }

    [Test]
    public void SeatUnavailableException_MessageShouldContainSeatAndFlightId()
    {
        // Arrange
        string seatNumber        = "7B";
        int    scheduledFlightId = 42;

        // Act
        var ex = new SeatUnavailableException(seatNumber, scheduledFlightId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Does.Contain(seatNumber));
            Assert.That(ex.Message, Does.Contain(scheduledFlightId.ToString()));
        });
    }

    [Test]
    public void SeatUnavailableException_ShouldBeAnException()
    {
        // Arrange
        string seatNumber        = "1A";
        int    scheduledFlightId = 1;

        // Act
        var ex = new SeatUnavailableException(seatNumber, scheduledFlightId);

        // Assert
        Assert.That(ex, Is.InstanceOf<Exception>());
    }


    [Test]
    public void InvalidFlightDateException_ShouldStoreProperties()
    {
        // Arrange
        var    date      = new DateOnly(2026, 6, 15);
        string routeCode = "SJO-MIA";
        string reason    = "no opera ese día";

        // Act
        var ex = new InvalidFlightDateException(date, routeCode, reason);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.RequestedDate, Is.EqualTo(date));
            Assert.That(ex.RouteCode,     Is.EqualTo(routeCode));
        });
    }

    [Test]
    public void InvalidFlightDateException_MessageShouldContainDateRouteAndReason()
    {
        // Arrange
        var    date      = new DateOnly(2026, 6, 15);
        string routeCode = "SJO-MIA";
        string reason    = "no opera ese día";

        // Act
        var ex = new InvalidFlightDateException(date, routeCode, reason);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Does.Contain("2026-06-15"));
            Assert.That(ex.Message, Does.Contain(routeCode));
            Assert.That(ex.Message, Does.Contain(reason));
        });
    }

    [Test]
    public void InvalidFlightDateException_ShouldBeAnException()
    {
        // Arrange
        var    date      = DateOnly.MinValue;
        string routeCode = "X";
        string reason    = "y";

        // Act
        var ex = new InvalidFlightDateException(date, routeCode, reason);

        // Assert
        Assert.That(ex, Is.InstanceOf<Exception>());
    }

    [Test]
    public void PassengerDataException_ShouldStoreField()
    {
        // Arrange
        string field  = "Email";
        string reason = "formato inválido";

        // Act
        var ex = new PassengerDataException(field, reason);

        // Assert
        Assert.That(ex.Field, Is.EqualTo(field));
    }

    [Test]
    public void PassengerDataException_MessageShouldContainFieldAndReason()
    {
        // Arrange
        string field  = "DocumentNumber";
        string reason = "está vacío";

        // Act
        var ex = new PassengerDataException(field, reason);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Does.Contain(field));
            Assert.That(ex.Message, Does.Contain(reason));
        });
    }

    [Test]
    public void PassengerDataException_ShouldBeAnException()
    {
        // Arrange
        string field  = "X";
        string reason = "Y";

        // Act
        var ex = new PassengerDataException(field, reason);

        // Assert
        Assert.That(ex, Is.InstanceOf<Exception>());
    }
}
