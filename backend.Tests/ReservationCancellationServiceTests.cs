using backend.Interfaces;
using backend.Model;
using backend.Services;
using backend.Templates;
using Moq;

namespace backend.Tests;
[TestFixture]
public class ReservationCancellationServiceTests
{
    private Mock<IReservationCancellationRepository> _repository = null!;
    private Mock<IEmailCancellationService> _emailService = null!;
    private ReservationCancellationService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new Mock<IReservationCancellationRepository>();
        _emailService = new Mock<IEmailCancellationService>();

        _service = new ReservationCancellationService(
            _repository.Object,
            _emailService.Object);
    }

    [Test]
    public void SendCancellationEmail_ValidCode_ShouldCallEmailService()
    {
        // Arrange
        var reservation = new CancellationReservationModel
        {
            ReservationCode = "AQG2B5",
            Email = "juan@test.com",
            FullName = "Juan Perez"
        };

        _repository
            .Setup(r => r.GetReservation("AQG2B5"))
            .Returns(reservation);

        // Act
        _service.SendCancellationEmail("AQG2B5");

        // Assert
        _emailService.Verify(
            e => e.SendCancellationEmail(reservation),
            Times.Once);
    }

    [Test]
    public void SendCancellationEmail_ReservationNotFound_ShouldThrow()
    {
        // Arrange
        _repository
            .Setup(r => r.GetReservation("XXXXXX"))
            .Returns((CancellationReservationModel?)null);

        var ex = Assert.Throws<Exception>(
            () => _service.SendCancellationEmail("XXXXXX"));

        Assert.That(ex.Message, Is.EqualTo("Reserva no encontrada."));
    }

    [Test]
    public void SendCancellationEmail_ValidCode_ShouldNotThrow()
    {
        var reservation = new CancellationReservationModel
        {
            ReservationCode = "AQG2B5",
            Email = "juan@test.com",
            FullName = "Juan Perez"
        };

        _repository
            .Setup(r => r.GetReservation("AQG2B5"))
            .Returns(reservation);

        Assert.DoesNotThrow(() => _service.SendCancellationEmail("AQG2B5"));
    }
}