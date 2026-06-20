using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;

namespace selenium.Tests;

[TestFixture]
[Category("UI")]
public class AddBaggageUITests
{
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;

    private const string BaseUrl            = "http://localhost:8080";
    private const string ReservationCode    = "9CLRLD";
    private const string PassengerFirstName = "Daniel";
    private const string PassengerLastName  = "Rojas";

    [SetUp]
    public void SetUp()
    {
        var options = new EdgeOptions();
        var driverDir = Path.GetDirectoryName(typeof(AddBaggageUITests).Assembly.Location);
        var service   = EdgeDriverService.CreateDefaultService(driverDir);
        _driver = new EdgeDriver(service, options);
        _driver.Manage().Window.Maximize();
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
    }

    [TearDown]
    public void TearDown()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    private void JsClick(IWebElement element) =>
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);

    private void TriggerVueInput(IWebElement element, string value)
    {
        ((IJavaScriptExecutor)_driver).ExecuteScript(
            """
            var setter = Object.getOwnPropertyDescriptor(
                window.HTMLInputElement.prototype, 'value').set;
            setter.call(arguments[0], arguments[1]);
            arguments[0].dispatchEvent(new Event('input', { bubbles: true }));
            """,
            element, value);
    }

    [Test]
    public void AgregarMaleta_PrimeraMaleta_DebeMostrarResumenYHabilitarPago()
    {
        _driver.Navigate().GoToUrl($"{BaseUrl}/my-reservation");

        _wait.Until(d => d.FindElements(
            By.CssSelector("input[placeholder='Ingrese el código de reserva']")).Count > 0);

        _driver.FindElement(By.CssSelector("input[placeholder='Ingrese el código de reserva']"))
               .SendKeys(ReservationCode);
        _driver.FindElement(By.CssSelector("input[placeholder='Ingrese su nombre']"))
               .SendKeys(PassengerFirstName);
        _driver.FindElement(By.CssSelector("input[placeholder='Ingrese su apellido']"))
               .SendKeys(PassengerLastName);
        _driver.FindElement(By.CssSelector("button.login-btn")).Click();

        _wait.Until(d =>
        {
            try   { d.SwitchTo().Alert(); return true; }
            catch (NoAlertPresentException) { return false; }
        });
        _driver.SwitchTo().Alert().Accept();

        _wait.Until(d => d.Url.Contains("/my-reservation/report"));

        var baggageBtn = _wait.Until(d =>
        {
            var btns = d.FindElements(By.XPath(
                "//button[contains(@class,'action') and " +
                ".//div[normalize-space(text())='Maletas']]"));
            return btns.Count > 0 ? btns[0] : null;
        });
        JsClick(baggageBtn!);

        _wait.Until(d => d.Url.Contains("/add-baggage"));

        _wait.Until(d => d.FindElements(By.CssSelector(".route-airport")).Count >= 2);
        var airports = _driver.FindElements(By.CssSelector(".route-airport"));
        Assert.That(airports, Has.Count.GreaterThanOrEqualTo(2),
            "El banner del vuelo debe mostrar al menos dos aeropuertos (origen y destino).");

        var addBtn = _wait.Until(d =>
            d.FindElements(By.CssSelector("button[aria-label='Agregar maleta']"))
             .FirstOrDefault(e => e.Enabled && e.Displayed));
        JsClick(addBtn!);

        var counter = _wait.Until(d =>
        {
            var el = d.FindElements(By.CssSelector(".counter-extra")).FirstOrDefault();
            return el?.Text == "+1" ? el : null;
        });
        Assert.That(counter!.Text, Is.EqualTo("+1"),
            "El contador debe mostrar +1 tras agregar una maleta.");

        var summaryName = _wait.Until(d =>
        {
            var el = d.FindElements(By.CssSelector(".summary-name")).FirstOrDefault();
            return el is { Displayed: true } && !string.IsNullOrEmpty(el.Text) ? el : null;
        });
        Assert.That(summaryName!.Text, Is.Not.Empty,
            "El resumen de cargo debe mostrar el nombre del pasajero.");

        var payBtn = _driver.FindElement(By.CssSelector("button.pay-btn"));
        Assert.That(payBtn.Enabled, Is.True,
            "El botón 'Pagar maletas adicionales' debe estar habilitado al seleccionar una maleta.");

        JsClick(payBtn);
        _wait.Until(d =>
            d.FindElements(By.CssSelector(".card-payment-form")).Any(e => e.Displayed));

        _driver.FindElement(By.XPath(
            "//button[contains(@class,'payment-method-btn') and normalize-space(text())='Visa']"))
               .Click();

        _driver.FindElement(By.CssSelector("input[placeholder='Nombre del titular']"))
               .SendKeys("Daniel Rojas");

        TriggerVueInput(
            _driver.FindElement(By.CssSelector("input[placeholder='1234 5678 9012 3456']")),
            "4111111111111111");

        TriggerVueInput(
            _driver.FindElement(By.CssSelector("input[placeholder='MM/AA']")),
            "1230");

        _driver.FindElement(By.CssSelector("input[placeholder='123']"))
               .SendKeys("123");

        var confirmBtn = _wait.Until(d =>
            d.FindElements(By.CssSelector("button.btn-pay"))
             .FirstOrDefault(e => e.Enabled && e.Displayed));
        JsClick(confirmBtn!);

        _wait.Until(d => d.Url.Contains("/my-reservation/report"));
        Assert.That(_driver.Url, Does.Contain("/my-reservation/report"),
            "Tras el pago exitoso, debe redirigir de vuelta al reporte de reserva.");
    }
}
