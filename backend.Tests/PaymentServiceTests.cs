using backend.Interfaces;
using backend.Model;
using backend.Services;
using Moq;

namespace backend.Tests;

[TestFixture]
public class PaymentServiceTests
{
    private PaymentService paymentService = null!;
    private Mock<IPaymentRepository> repositoryMock = null!;

    [SetUp]
    public void Setup()
    {
        repositoryMock = new Mock<IPaymentRepository>();

        paymentService = new PaymentService(
            repositoryMock.Object
        );
    }

    [Test]
    public void ApprovePayment_ValidPayment_ShouldReturnPurchaseId()
    {
        // Arrange
        PaymentModel payment = new()
        {
            PaymentMethod = "Visa",
            Holder = "Leo Sibaja",
            CardNumber = "1111 1111 1111 1111",
            Expiry = "12/30",
            Cvv = "123"
        };

        // Act
        int result = paymentService.ApprovePayment(payment);

        // Assert
        Assert.That(result, Is.GreaterThan(0));
    }

    [Test]
    public void ApprovePayment_WithoutPaymentMethod_ShouldThrowException()
    {
        // Arrange
        PaymentModel payment = new()
        {
            PaymentMethod = "",
            Holder = "Leo Sibaja",
            CardNumber = "1111 1111 1111 1111",
            Expiry = "12/30",
            Cvv = "123"
        };

        // Act & Assert
        Exception ex = Assert.Throws<Exception>(
            () => paymentService.ApprovePayment(payment)
        )!;

        Assert.That(ex.Message,Is.EqualTo("Debe seleccionar un método de pago."));
    }

    [Test]
    public void ApprovePayment_InsufficientFundsCard_ShouldThrowException()
    {
        // Arrange
        PaymentModel payment = new()
        {
            PaymentMethod = "Visa",
            Holder = "Leo Sibaja",
            CardNumber = "9999 9999 9999 9999",
            Expiry = "12/30",
            Cvv = "123"
        };
        // Act & Assert
        Exception ex = Assert.Throws<Exception>(() => paymentService.ApprovePayment(payment))!;
        Assert.That(ex.Message,Is.EqualTo("Pago rechazado: fondos insuficientes."));
    }

    [Test]
    public void ApprovePayment_ExpiredCard_ShouldThrowException()
    {
        // Arrange
        PaymentModel payment = new()
        {
            PaymentMethod = "Visa",
            Holder = "Leo Sibaja",
            CardNumber = "1111 1111 1111 1111",
            Expiry = "01/20",
            Cvv = "123"
        };

        // Act & Assert
        Exception ex = Assert.Throws<Exception>(
            () => paymentService.ApprovePayment(payment)
        )!;

        Assert.That(
            ex.Message,
            Is.EqualTo("Pago rechazado: tarjeta expirada.")
        );
    }
}