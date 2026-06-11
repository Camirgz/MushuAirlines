using backend.Model;
using backend.Templates;

namespace backend.Tests;

[TestFixture]
public class InvoiceEmailTemplateTests
{
    private PurchaseConfirmationModel model = null!;

    [SetUp]
    public void Setup()
    {
        model = new PurchaseConfirmationModel
        {
            InvoiceNumber = "FAC-001",
            ReservationCode = "ABC123",
            TotalPaid = 250.50m
        };
    }

    [Test]
    public void Build_IncludeInvoiceAndReservationNumber()
    {
        string html = InvoiceEmailTemplate.Build(model);
        Assert.That(model.InvoiceNumber, Is.EqualTo("FAC-001"));
        Assert.That(model.ReservationCode, Is.EqualTo("ABC123"));
        Assert.That(html, Does.Contain(model.ReservationCode));
        Assert.That(html, Does.Contain(model.InvoiceNumber));
    }

    [Test]
    public void Build_ShouldFormatTotalPaidWithTwoDecimals()
    {
        string html = InvoiceEmailTemplate.Build(model);
        Assert.That(html, Does.Contain($"${model.TotalPaid:N2}"));
    }

    [Test]
    public void Build_ShouldGenerateHtmlDocument()
    {
        string html = InvoiceEmailTemplate.Build(model);

        Assert.Multiple(() =>
        {
            Assert.That(html, Is.Not.Null.And.Not.Empty);
            Assert.That(html, Does.Contain("<html>"));
            Assert.That(html, Does.Contain("</html>"));
        });
    }
}