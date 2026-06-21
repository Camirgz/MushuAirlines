using backend.Interfaces;
using backend.Model;
using backend.Services;
using Moq;

namespace backend.Tests;

[TestFixture]
public class PdfItineraryServiceTests
{
    private Mock<IQrService> _qrService = null!;
    private PdfItineraryService _service = null!;
    private byte[] _fakeQrImage = null!;
    [SetUp]
    public void Setup()
    {
        _qrService = new Mock<IQrService>();

        _fakeQrImage = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");

        _qrService
            .Setup(q => q.GenerateQr(It.IsAny<string>()))
            .Returns(_fakeQrImage);

        _service = new PdfItineraryService(
            _qrService.Object);
    }


    private static PurchaseConfirmationModel BuildBaseModel()
    {
        return new PurchaseConfirmationModel
        {
            ReservationCode = "AQG2B5",
            FullName = "Leo Sibaja",
            Email = "leo@example.com",
            FlightNumber = "13",
            AircraftModel = "777",
            OriginAirport = "SJO",
            DestinationAirport = "TVV",
            DepartureDate = new DateTime(2026, 6, 20, 19, 0, 0),
            ArrivalDate = new DateTime(2026, 6, 21, 7, 0, 0),
            PassengerBaggageDetails = new List<PassengerBaggageDetail>
            {
                new()
                {
                    PassengerFullName = "Leo Sibaja",
                    HandBagCount = 1,
                    CheckedBagCount = 1
                }
            }
        };
    }

    [Test]
    public void GeneratePdf_DirectFlight_ShouldReturnNonEmptyPdf()
    {
        // Arrange
        var model = BuildBaseModel();

        // Act
        byte[] pdf = _service.GeneratePdf(model);

        // Assert
        Assert.That(pdf, Is.Not.Null);
        Assert.That(pdf.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GeneratePdf_StopoverFlight_ShouldReturnNonEmptyPdf()
    {
        // Arrange
        var model = BuildBaseModel();
        model.FlightNumber2 = "27";
        model.AircraftModel2 = "Boeing 737";
        model.OriginAirport2 = "TVV";
        model.DestinationAirport2 = "MIA";
        model.DepartureDate2 = new DateTime(2026, 6, 21, 10, 0, 0);
        model.ArrivalDate2 = new DateTime(2026, 6, 21, 14, 0, 0);

        // Act
        byte[] pdf = _service.GeneratePdf(model);

        // Assert
        Assert.That(pdf.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GeneratePdf_NoPassengers_ShouldNotThrow()
    {
        // Arrange
        var model = BuildBaseModel();
        model.PassengerBaggageDetails = new List<PassengerBaggageDetail>();

        // Act & Assert
        Assert.DoesNotThrow(() => _service.GeneratePdf(model));
    }

    [Test]
    public void GeneratePdf_NullOptionalFields_ShouldNotThrow()
    {
        var model = BuildBaseModel();
        model.AircraftModel = null!;

        // Act & Assert
        Assert.DoesNotThrow(() => _service.GeneratePdf(model));
    }
}