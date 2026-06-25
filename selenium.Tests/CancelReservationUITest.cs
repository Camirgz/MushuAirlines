using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace selenium.Tests;
[TestFixture]
[Category("UIcancel")]
public class CancelReservationUITests
{
    private IWebDriver _driver = null!;
    private WebDriverWait _wait = null!;

    private const string BaseUrl            = "http://localhost:8080";
    private const string ReservationCode    = "0M529W";
    private const string PassengerFirstName = "g";
    private const string PassengerLastName  = "g";

    [SetUp]
    public void SetUp()
    {
        var options = new ChromeOptions();
        _driver = new ChromeDriver(options);
        _driver.Manage().Window.Maximize();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(200); 
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(200)); 

    }

    [TearDown]
    public void TearDown()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    private void JsClick(IWebElement element) =>
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);

    [Test]
    public void CancelarReserva_FlujoCompleto_DebeRedirigirAConfirmacion()
    {
        // 1. Login
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

        // 2. Aceptar alerta "Consultar reserva"
        _wait.Until(d =>
        {
            try   { d.SwitchTo().Alert(); return true; }
            catch (NoAlertPresentException) { return false; }
        });
        _driver.SwitchTo().Alert().Accept();

        _wait.Until(d => d.Url.Contains("/my-reservation/report"));


        // Click "Cancelar reserva" 
        var cancelBtn = _wait.Until(d =>
        {
            var btns = d.FindElements(By.XPath(
                "//button[contains(@class,'action') and " +
                ".//div[normalize-space(text())='Cancelar reserva']]"));
            return btns.Count > 0 ? btns[0] : null;
        });
        JsClick(cancelBtn!);

        // Click "Enviar correo"
        var enviarBtn = _wait.Until(d =>
            d.FindElements(By.XPath(
                "//button[contains(@class,'primary') and normalize-space(text())='Enviar correo']"))
             .FirstOrDefault(e => e.Displayed));
        JsClick(enviarBtn!);

        // aceptar
        var aceptarBtn = _wait.Until(d =>
            d.FindElements(By.XPath(
                "//button[contains(@class,'primary') and normalize-space(text())='Aceptar']"))
             .FirstOrDefault(e => e.Displayed));
        Assert.That(aceptarBtn, Is.Not.Null,
            "Debe aparecer el popup de confirmación con botón Aceptar tras enviar el correo.");
        JsClick(aceptarBtn!);


        _driver.Navigate().GoToUrl($"{BaseUrl}/cancel-reservation/{ReservationCode}");
        Thread.Sleep(1500);
        var confirmarCancelBtn = _wait.Until(d =>
            d.FindElements(By.XPath(
                "//button[normalize-space(text())='Cancelar reserva']"))
             .FirstOrDefault(e => e.Displayed && e.Enabled));
        Assert.That(confirmarCancelBtn, Is.Not.Null,
            "Debe existir el botón Cancelar reserva en la página de confirmación.");
        JsClick(confirmarCancelBtn!);

        _wait.Until(d =>
            d.FindElements(By.XPath("//*[contains(text(),'cancelada') or contains(text(),'Cancelada')]"))
             .Any(e => e.Displayed));

        Assert.Pass("Flujo de cancelación completado correctamente.");
    }
}