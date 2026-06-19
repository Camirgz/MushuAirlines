using NUnit.Framework;
using QuestPDF.Infrastructure;

namespace backend.Tests;

[SetUpFixture]
public class GlobalTestSetup
{
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }
}