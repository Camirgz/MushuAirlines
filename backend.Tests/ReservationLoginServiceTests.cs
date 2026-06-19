using backend.Interfaces;
using backend.Model;
using backend.Repositories;
using backend.Services;
using Moq;
using Microsoft.Extensions.Configuration;

namespace backend.Tests;

[TestFixture]
public class ReservationLoginServiceTests
{
    private Mock<IReservationLoginRepository> _repository = null!;
    private ReservationLoginService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new Mock<IReservationLoginRepository>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:Key", "EstaEsUnaLlaveSuperSeguraDePrueba123456789" }
            })
            .Build();

        _service = new ReservationLoginService(
            _repository.Object,
            configuration);
    }

    [Test]
    public void ReservationLogin_ValidReservation_ShouldReturnToken()
    {
        // Arrange
        var model = new ReservationLoginModel
        {
            ReservationCode = "AQG2B5",
            FirstName = "Leo",
            LastName = "Sibaja"
        };

        _repository
            .Setup(r => r.ReservationExists(model))
            .Returns(true);

        // Act
        var token = _service.ReservationLogin(model);

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.Not.Empty);
    }

    [Test]
    public void ReservationLogin_InvalidReservation_ShouldReturnMessage()
    {
        // Arrange
        var model = new ReservationLoginModel
        {
            ReservationCode = "XXXXXX",
            FirstName = "Leo",
            LastName = "Sibaja"
        };

        _repository
            .Setup(r => r.ReservationExists(model))
            .Returns(false);

        // Act
        var result = _service.ReservationLogin(model);

        // Assert
        Assert.That(result, Is.EqualTo("Reserva no encontrada"));
    }
}