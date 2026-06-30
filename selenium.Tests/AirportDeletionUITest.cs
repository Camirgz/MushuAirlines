using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace backend.Tests;

[TestFixture]
[Category("UIAirportDelete")]
public class AirportDeletionUITest
{
    private ChromeDriver _driver = null!;
    private WebDriverWait _wait = null!;

    private const string BaseUrl = "http://localhost:8080";
    private const string AirportCode = "ZZZ";
    private const int Delay = 1500;

    [SetUp]
    public void Setup()
    {
        _driver = new ChromeDriver();
        _driver.Manage().Window.Maximize();
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
    }

    [Test]
    public void Delete_Airport_Test()
    {
        _driver.Navigate().GoToUrl(BaseUrl);
        Thread.Sleep(Delay);

        Assert.That(_driver.Url, Does.Contain(BaseUrl));

        _driver.Navigate().GoToUrl($"{BaseUrl}/login");
        Thread.Sleep(Delay);

        Assert.That(_driver.Url, Does.Contain("/login"));

        _wait.Until(d => d.FindElement(By.CssSelector("input[type='text']")))
            .SendKeys("admin");

        _driver.FindElement(By.CssSelector("input[type='password']"))
            .SendKeys("1234");

        _driver.FindElement(By.CssSelector("button[type='submit']"))
            .Click();

        Thread.Sleep(Delay);

        AcceptAlertIfExists();

        Thread.Sleep(Delay);

        _driver.Navigate().GoToUrl($"{BaseUrl}/admin/airports");
        Thread.Sleep(Delay);

        Assert.That(_driver.Url, Does.Contain("/admin/airports"));

        var searchInput = _wait.Until(d =>
            d.FindElement(By.XPath("//input[contains(@placeholder,'Buscar')]"))
        );

        searchInput.Clear();
        searchInput.SendKeys(AirportCode);

        Thread.Sleep(Delay);

        var airportRow = _wait.Until(d =>
            d.FindElement(By.XPath($"//tr[contains(.,'{AirportCode}')]"))
        );

        Assert.That(airportRow.Text, Does.Contain(AirportCode));
        Assert.That(airportRow.Text, Does.Contain("Aeropuerto Selenium Test"));

        airportRow.FindElement(By.CssSelector(".delete-btn")).Click();
        Thread.Sleep(Delay);

        var modalTitle = _wait.Until(d =>
            d.FindElement(By.CssSelector(".delete-modal-title"))
        );

        Assert.That(modalTitle.Text, Is.EqualTo("Eliminar aeropuerto"));

        var modalText = _driver.FindElement(By.CssSelector(".delete-modal-text")).Text;
        Assert.That(modalText, Does.Contain("Aeropuerto Selenium Test"));

        _driver.FindElement(By.CssSelector(".delete-confirm-btn")).Click();
        Thread.Sleep(Delay);

        Assert.That(
            _driver.PageSource,
            Does.Contain("eliminado correctamente")
        );

        bool airportStillExists = _driver
            .FindElements(By.XPath($"//tbody//tr[contains(.,'{AirportCode}')]"))
            .Any();

        Assert.That(airportStillExists, Is.False);
    }

    [TearDown]
    public void TearDown()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    private void AcceptAlertIfExists()
    {
        try
        {
            var alertWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));

            alertWait.Until(driver =>
            {
                try
                {
                    driver.SwitchTo().Alert();
                    return true;
                }
                catch (NoAlertPresentException)
                {
                    return false;
                }
            });

            _driver.SwitchTo().Alert().Accept();
        }
        catch (WebDriverTimeoutException)
        {
            // continue
        }
    }
}
