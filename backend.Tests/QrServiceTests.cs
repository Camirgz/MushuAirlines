using backend.Services;

namespace backend.Tests;

[TestFixture]
public class QrServiceTests
{
    private QrService qrService = null!;

    [SetUp]
    public void Setup()
    {
        qrService = new QrService();
    }

   [Test]
    public void GenerateQr_ShouldReturnByteArray()
    {
        // Act
        byte[] qr = qrService.GenerateQr("ABC123");

        // Assert
        Assert.That(qr, Is.Not.Null);
        Assert.That(qr, Is.InstanceOf<byte[]>());
    }

    [Test]
    public void GenerateQr_ShouldGenerateDifferentImagesForDifferentTexts()
    {
        // Act
        byte[] qr1 = qrService.GenerateQr("ABC123");
        byte[] qr2 = qrService.GenerateQr("XYZ999");

        // Assert
        Assert.That(qr1.SequenceEqual(qr2), Is.False);
    }

    [Test]
    public void GenerateQr_ShouldNotReturnEmptyArray()
    {
        // Act
        byte[] qr = qrService.GenerateQr("ABC123");

        // Assert
        Assert.That(qr.Length, Is.GreaterThan(0));
    }

    [Test]
    public void GenerateQr_SameTextShouldGenerateSameQr()
    {
        // Act
        byte[] qr1 = qrService.GenerateQr("ABC123");
        byte[] qr2 = qrService.GenerateQr("ABC123");

        // Assert
        Assert.That(qr1.SequenceEqual(qr2), Is.True);
    }
}