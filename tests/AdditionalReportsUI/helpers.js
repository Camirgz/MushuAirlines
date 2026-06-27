import { Builder, By, until } from 'selenium-webdriver';
import chrome from 'selenium-webdriver/chrome.js';

export const BASE_URL    = 'http://localhost:8080';
export const BACKEND_URL = 'http://localhost:5103';
export const TIMEOUT     = 10_000;

export function buildDriver() {
  const options = new chrome.Options();
  options.addArguments(
    '--headless=new',
    '--no-sandbox',
    '--disable-dev-shm-usage',
    '--disable-gpu',
    '--window-size=1280,900',
  );
  return new Builder()
    .forBrowser('chrome')
    .setChromeOptions(options)
    .build();
}

/** Hace login real contra el backend local e inyecta el token en localStorage */
export async function injectToken(driver) {
  // Obtiene el token del backend local
  const res = await fetch(`${BACKEND_URL}/api/login`, {
    method:  'POST',
    headers: { 'Content-Type': 'application/json' },
    body:    JSON.stringify({ username: 'admin', password: '1234' }),
  });
  if (!res.ok) throw new Error(`Login fallido: ${res.status} ${await res.text()}`);
  const token = await res.text();

  // Navega a la app e inyecta el token antes de ir a la ruta protegida
  await driver.get(BASE_URL);
  await driver.executeScript(`localStorage.setItem('token', ${JSON.stringify(token)})`);
}

export async function waitForEl(driver, locator) {
  return driver.wait(until.elementLocated(locator), TIMEOUT);
}

export async function waitForText(driver, locator, text) {
  const el = await waitForEl(driver, locator);
  await driver.wait(until.elementTextContains(el, text), TIMEOUT);
  return el;
}
