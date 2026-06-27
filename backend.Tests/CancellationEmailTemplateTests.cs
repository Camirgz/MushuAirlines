using backend.Interfaces;
using backend.Model;
using backend.Services;
using backend.Templates;
using Moq;

namespace backend.Tests;

[TestFixture]
public class CancellationEmailTemplateTests
{
    private CancellationReservationModel model = null!;

    [SetUp]
    public void Setup()
    {
        model = new CancellationReservationModel
        {
            ReservationCode = "AQG2B5",
            Email = "juan@test.com",
            FullName = "Juan Perez"
        };
    }

    [Test]
    public void Build_ShouldReturnHtml()
    {
        string html = CancellationEmailTemplate.Build(model);

        Assert.That(html, Is.Not.Null.And.Not.Empty);
        Assert.That(html, Does.Contain("<html>"));
    }

    [Test]
    public void Build_ShouldContainReservationCode()
    {
        string html = CancellationEmailTemplate.Build(model);

        Assert.That(html, Does.Contain(model.ReservationCode));
    }

    [Test]
    public void Build_ShouldContainCancellationLink()
    {
        string html = CancellationEmailTemplate.Build(model);

        Assert.That(html, Does.Contain($"cancel-reservation/{model.ReservationCode}"));
    }

    [Test]
    public void Build_ShouldContainCancellationButton()
    {
        string html = CancellationEmailTemplate.Build(model);

        Assert.That(html, Does.Contain("<a"));
        Assert.That(html, Does.Contain("Cancelar reserva"));
    }
}
