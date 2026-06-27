using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace backend.Tests;

[TestFixture]
[Category("E2E")]
public class DeleteUserSeleniumTests
{
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;

    private const string BaseUrl = "http://localhost:8080";
    private const string Username = "admin";
    private const string Password = "1234";

    [SetUp]
    public void Setup()
    {
        var options = new ChromeOptions();
        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        Login();
    }

    private void Login()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/login");

        _wait.Until(d => d.FindElement(By.CssSelector("input[placeholder='Ingrese su usuario']")));

        _driver
            .FindElement(By.CssSelector("input[placeholder='Ingrese su usuario']"))
            .SendKeys(Username);

        _driver
            .FindElement(By.CssSelector("input[placeholder='Ingrese su contraseña']"))
            .SendKeys(Password);

        _driver
            .FindElement(By.CssSelector("button[type='submit']"))
            .Click();

        try
        {
            var alertWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            alertWait.Until(d =>
            {
                try
                {
                    d.SwitchTo().Alert();
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

        var redirectWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
        redirectWait.Until(d =>
            d.Url.Contains("/admin") || d.Url == $"{BaseUrl}/" || d.Url == $"{BaseUrl}"
        );
    }

    private void JsClick(IWebElement element)
    {
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
    }

    [Test]
    public void ClickEliminar_ShouldOpenModalWithCorrectUserName()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/admin/users");

        _wait.Until(d => d.FindElement(By.CssSelector(".users-table tbody tr")));

        string userName = _driver
            .FindElement(By.CssSelector(".users-table tbody tr:first-child td:first-child"))
            .Text;

        var deleteBtn = _driver
            .FindElement(By.CssSelector(".users-table tbody tr:first-child .delete-btn"));

        JsClick(deleteBtn);

        _wait.Until(d => d.FindElement(By.CssSelector(".delete-modal")).Displayed);

        string modalText = _driver
            .FindElement(By.CssSelector(".delete-modal-text"))
            .Text;

        Assert.That(modalText, Does.Contain(userName));
    }

    [Test]
    public void ClickCancelar_ShouldCloseModalWithoutDeleting()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/admin/users");

        _wait.Until(d => d.FindElement(By.CssSelector(".users-table tbody tr")));

        int userCountBefore = _driver
            .FindElements(By.CssSelector(".users-table tbody tr"))
            .Count;

        var deleteBtn = _driver
            .FindElement(By.CssSelector(".users-table tbody tr:first-child .delete-btn"));

        JsClick(deleteBtn);

        _wait.Until(d => d.FindElement(By.CssSelector(".delete-modal")).Displayed);

        var cancelBtn = _driver.FindElement(By.CssSelector(".modal-secondary-btn"));
        JsClick(cancelBtn);

        _wait.Until(d =>
        {
            var modals = d.FindElements(By.CssSelector(".delete-modal"));
            return modals.Count == 0 || !modals[0].Displayed;
        });

        int userCountAfter = _driver
            .FindElements(By.CssSelector(".users-table tbody tr"))
            .Count;

        Assert.That(userCountAfter, Is.EqualTo(userCountBefore));
    }

    [TearDown]
    public void TearDown()
    {
        _driver?.Quit();
        _driver?.Dispose();
        _driver = null!;
    }
}