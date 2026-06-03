using backend.Model;
using backend.Services;

namespace backend.Tests;

[TestFixture]
public class PaymentServiceTests
{
    private PaymentService _paymentService = null!;

    [SetUp]
    public void Setup()
    {
        _paymentService = new PaymentService();
    }

    [Test]
    public void ValidatePayment_ValidPayment_ShouldNotThrow()
    {
        // Arrange
        var payment = new PaymentModel
        {
            PaymentMethod = "Visa",
            Holder        = "Leo Sibaja",
            CardNumber    = "1111 1111 1111 1111",
            Expiry        = "12/30",
            Cvv           = "123"
        };

        // Act & Assert — a valid payment must pass without throwing
        Assert.DoesNotThrow(() => _paymentService.ValidatePayment(payment));
    }

    [Test]
    public void ValidatePayment_WithoutPaymentMethod_ShouldThrowException()
    {
        // Arrange
        var payment = new PaymentModel
        {
            PaymentMethod = "",
            Holder        = "Leo Sibaja",
            CardNumber    = "1111 1111 1111 1111",
            Expiry        = "12/30",
            Cvv           = "123"
        };

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => _paymentService.ValidatePayment(payment))!;
        Assert.That(ex.Message, Is.EqualTo("Debe seleccionar un método de pago."));
    }

    [Test]
    public void ValidatePayment_InsufficientFundsCard_ShouldThrowException()
    {
        // Arrange
        var payment = new PaymentModel
        {
            PaymentMethod = "Visa",
            Holder        = "Leo Sibaja",
            CardNumber    = "9999 9999 9999 9999",
            Expiry        = "12/30",
            Cvv           = "123"
        };

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => _paymentService.ValidatePayment(payment))!;
        Assert.That(ex.Message, Is.EqualTo("Pago rechazado: fondos insuficientes."));
    }

    [Test]
    public void ValidatePayment_ExpiredCard_ShouldThrowException()
    {
        // Arrange
        var payment = new PaymentModel
        {
            PaymentMethod = "Visa",
            Holder        = "Leo Sibaja",
            CardNumber    = "1111 1111 1111 1111",
            Expiry        = "01/20",
            Cvv           = "123"
        };

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => _paymentService.ValidatePayment(payment))!;
        Assert.That(ex.Message, Is.EqualTo("Pago rechazado: tarjeta expirada."));
    }

    [Test]
    public void ValidatePayment_NullPaymentMethod_ShouldThrowException()
    {
        // Arrange
        var payment = new PaymentModel
        {
            PaymentMethod = null!,
            Holder        = "Leo Sibaja",
            CardNumber    = "1111 1111 1111 1111",
            Expiry        = "12/30",
            Cvv           = "123"
        };

        // Act & Assert
        Assert.Throws<Exception>(() => _paymentService.ValidatePayment(payment));
    }
}
