using backend.Model;
using backend.Templates;

namespace backend.Tests;

[TestFixture]
public class EmailSectionsTests
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

            InvoiceNumber = "MA-20250530-ABCDEFGH",
            PaymentMethod = "Visa",
            TotalPaid = 500,

            TotalSeats = 2,

            Details =
            [
                new SeatClassSubtotal
                {
                    SeatClass = "Economy",
                    SeatCount = 2,
                    Subtotal = 500
                }
            ]
        };
    }

    [Test]
    public void BuildTicketsSection_ShouldShowCorrectPricePerSeat()
    {
        // Act
        string html = EmailSections.BuildTicketsSection(model);

        // 500 / 2 = 250
        Assert.That(html, Does.Contain("250"));
    }

    [Test]
    public void BuildTicketsSection_ShouldContainSeatClassAndSeatCount()
    {
        // Act
        string html = EmailSections.BuildTicketsSection(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Contain("Economy"));
            Assert.That(html, Does.Contain("2"));
        });
    }

    [Test]
    public void BuildPaymentSection_ShouldContainInvoiceAndPaymentData()
    {
        // Act
        string html = EmailSections.BuildPaymentSection(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(html, Does.Contain(model.InvoiceNumber));
            Assert.That(html, Does.Contain(model.PaymentMethod));
            Assert.That(html, Does.Contain(model.TotalPaid.ToString("N2")));
        });
    }
}