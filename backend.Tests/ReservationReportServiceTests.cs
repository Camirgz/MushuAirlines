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
}