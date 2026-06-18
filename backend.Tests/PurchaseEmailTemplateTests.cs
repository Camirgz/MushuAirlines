using backend.Model;
using backend.Templates;

namespace backend.Tests;

[TestFixture]
public class PurchaseEmailTemplateTests
{
    private PurchaseConfirmationModel model = null!;

    [SetUp]
    public void Setup()
    {
        model = new PurchaseConfirmationModel
        {
            FullName = "Juan Perez",
            Email = "juan@test.com",
            PassportNumber = 123456,
            ReservationCode = "ABC123",
            InvoiceNumber = "FAC-001",
            PaymentMethod = PaymentMethod.Visa,
            TotalPaid = 250,
            TotalSeats = 2,
            FlightNumber = "FL001",
            AircraftType = "Boeing 737",
            OriginAirport = "SJO",
            DestinationAirport = "MEX",
            DepartureDate = DateTime.Now,
            ArrivalDate = DateTime.Now.AddHours(3),
            Layover = "Ninguna"
        };
    }

    [Test]
    public void Build_ShouldReturnHtml()
    {
        // Act
        string html = PurchaseEmailTemplate.Build(model);

        // Assert
        Assert.That(html, Is.Not.Null.And.Not.Empty);
        Assert.That(html, Does.Contain("<html>"));
    }

    [Test]
    public void Build_ShouldContainReservationCode()
    {
        // Act
        string html = PurchaseEmailTemplate.Build(model);

        // Assert
        Assert.That(html, Does.Contain(model.ReservationCode));
    }

    [Test]
    public void Build_ShouldContainInvoiceNumber()
    {
        // Act
        string html = PurchaseEmailTemplate.Build(model);

        // Assert
        Assert.That(html, Does.Contain(model.InvoiceNumber));
    }

    [Test]
    public void Build_ShouldContainCustomerName()
    {
        // Act
        string html = PurchaseEmailTemplate.Build(model);

        // Assert
        Assert.That(html, Does.Contain(model.FullName));
    }

    [Test]
    public void Build_ShouldContainFlightInformation()
    {
        // Act
        string html = PurchaseEmailTemplate.Build(model);

        // Assert
        Assert.That(html, Does.Contain(model.FlightNumber));
        Assert.That(html, Does.Contain(model.OriginAirport));
        Assert.That(html, Does.Contain(model.DestinationAirport));
    }

    [Test]
    public void Build_ShouldContainPaymentInformation()
    {
        // Act
        string html = PurchaseEmailTemplate.Build(model);

        // Assert
        Assert.That(html, Does.Contain(model.PaymentMethod.ToString()));
        Assert.That(html, Does.Contain(model.TotalPaid.ToString("N2")));
    }
}