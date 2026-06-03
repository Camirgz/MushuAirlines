using backend.Services;

namespace backend.Tests;

[TestFixture]
public class CodeGeneratorTests
{
    private CodeGenerator _generator = null!;

    [SetUp]
    public void Setup()
    {
        _generator = new CodeGenerator();
    }


    [Test]
    public void GenerateReservationCode_ShouldReturnExactly6Characters()
    {
        // Act
        string code = _generator.GenerateReservationCode();

        // Assert
        Assert.That(code, Has.Length.EqualTo(6));
    }

    [Test]
    public void GenerateReservationCode_ShouldContainOnlyUppercaseAlphanumericCharacters()
    {
        // Arrange
        string expectedPattern = "^[A-Z0-9]{6}$";

        // Act
        string code = _generator.GenerateReservationCode();

        // Assert
        Assert.That(code, Does.Match(expectedPattern));
    }

    [Test]
    public void GenerateReservationCode_ShouldNotReturnNullOrEmpty()
    {
        // Act
        string code = _generator.GenerateReservationCode();

        // Assert
        Assert.That(code, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void GenerateReservationCode_MultipleCallsShouldProduceDistinctCodes()
    {
        // Arrange
        int sampleSize = 50;

        // Act
        var codes = Enumerable.Range(0, sampleSize)
            .Select(_ => _generator.GenerateReservationCode())
            .ToList();
            
        // Assert
        Assert.That(codes.Distinct().Count(), Is.EqualTo(sampleSize));
    }

    [Test]
    public void GenerateReservationCode_ShouldFitVarchar6ColumnConstraint()
    {
        // Arrange
        int maxLength = 6;

        // Act
        string code = _generator.GenerateReservationCode();

        // Assert
        Assert.That(code.Length, Is.LessThanOrEqualTo(maxLength));
    }


    [Test]
    public void GenerateInvoiceNumber_ShouldStartWithMAPrefix()
    {
        // Arrange
        string expectedPrefix = "MA-";

        // Act
        string invoice = _generator.GenerateInvoiceNumber();

        // Assert
        Assert.That(invoice, Does.StartWith(expectedPrefix));
    }

    [Test]
    public void GenerateInvoiceNumber_ShouldContainTodaysDatePart()
    {
        // Arrange
        string expectedDate = DateTime.UtcNow.ToString("yyyyMMdd");

        // Act
        string invoice = _generator.GenerateInvoiceNumber();

        // Assert
        Assert.That(invoice, Does.Contain(expectedDate));
    }

    [Test]
    public void GenerateInvoiceNumber_ShouldMatchExpectedFormat()
    {
        // Arrange
        string expectedPattern = @"^MA-\d{8}-[A-Z0-9]{8}$";

        // Act
        string invoice = _generator.GenerateInvoiceNumber();

        // Assert
        Assert.That(invoice, Does.Match(expectedPattern));
    }

    [Test]
    public void GenerateInvoiceNumber_ShouldNotExceedVarchar255ColumnConstraint()
    {
        // Arrange
        int maxLength = 255;

        // Act
        string invoice = _generator.GenerateInvoiceNumber();

        // Assert
        Assert.That(invoice.Length, Is.LessThanOrEqualTo(maxLength));
    }

    [Test]
    public void GenerateInvoiceNumber_ShouldNotReturnNullOrEmpty()
    {
        // Act
        string invoice = _generator.GenerateInvoiceNumber();

        // Assert
        Assert.That(invoice, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void GenerateInvoiceNumber_MultipleCallsShouldProduceDistinctValues()
    {
        // Arrange
        int sampleSize = 50;

        // Act
        var invoices = Enumerable.Range(0, sampleSize)
            .Select(_ => _generator.GenerateInvoiceNumber())
            .ToList();

        // Assert
        Assert.That(invoices.Distinct().Count(), Is.EqualTo(sampleSize));
    }
}
