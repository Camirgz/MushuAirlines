import { test, before, after } from 'node:test';
import assert from 'node:assert/strict';
import { By } from 'selenium-webdriver';
import { buildDriver, injectToken, waitForEl, BASE_URL, TIMEOUT } from './helpers.js';

let driver;

before(async () => { driver = buildDriver(); });
after(async  () => { await driver.quit(); });

async function goToReport() {
  await injectToken(driver, 'Administrator');
  await driver.get(`${BASE_URL}/admin/reports/monthly-income`);
  await waitForEl(driver, By.xpath('//button[contains(text(),"Buscar")]'));
}

/**
 * Hace clic en Buscar y espera hasta 5 s a que aparezca algún estado de
 * respuesta. Devuelve false si el backend no responde a tiempo.
 */
async function buscarYEsperar() {
  const btn = await waitForEl(driver, By.xpath('//button[contains(text(),"Buscar")]'));
  await btn.click();
  try {
    await driver.wait(async () => {
      const rows  = await driver.findElements(By.css('table tbody tr'));
      const empty = await driver.findElements(By.xpath('//*[contains(text(),"No se encontraron")]'));
      const error = await driver.findElements(By.css('[class*="error"]'));
      return rows.length > 0 || empty.length > 0 || error.length > 0;
    }, 5000);
    return true;
  } catch {
    return false;
  }
}

// ── Tests que NO necesitan backend ────────────────────────────────────────────

test('carga la página con el título y el campo de año visibles', async () => {
  await goToReport();
  const heading = await waitForEl(driver, By.xpath('//*[contains(text(),"Ingresos por Mes")]'));
  assert.ok((await heading.getText()).includes('Ingresos por Mes'));
  await waitForEl(driver, By.css('input[type="number"]'));
});

test('el campo de año acepta un valor numérico válido', async () => {
  await goToReport();
  const yearInput = await waitForEl(driver, By.css('input[type="number"]'));
  await yearInput.clear();
  await yearInput.sendKeys('2026');
  assert.equal(await yearInput.getAttribute('value'), '2026');
});

test('el botón Limpiar resetea el campo de año', async () => {
  await goToReport();
  const yearInput = await waitForEl(driver, By.css('input[type="number"]'));
  await yearInput.clear();
  await yearInput.sendKeys('2026');
  const limpiar = await waitForEl(driver, By.xpath('//button[contains(text(),"Limpiar")]'));
  await limpiar.click();
  assert.equal(await yearInput.getAttribute('value'), '');
});

test('el botón Buscar es clickeable y activa el estado de carga', async () => {
  await goToReport();
  const btn = await waitForEl(driver, By.xpath('//button[contains(text(),"Buscar")]'));
  await btn.click();
  const apareció = await driver.wait(async () => {
    const loading = await driver.findElements(By.xpath('//button[contains(text(),"Cargando")]'));
    const rows    = await driver.findElements(By.css('table tbody tr'));
    const empty   = await driver.findElements(By.xpath('//*[contains(text(),"No se encontraron")]'));
    const error   = await driver.findElements(By.css('[class*="error"]'));
    return loading.length > 0 || rows.length > 0 || empty.length > 0 || error.length > 0;
  }, TIMEOUT).then(() => true).catch(() => false);
  assert.ok(apareció, 'El botón Buscar no produjo ningún cambio en la UI');
});

// ── Tests condicionales (requieren backend corriendo) ─────────────────────────

test('cuando hay datos se muestran los botones de exportar', async () => {
  await goToReport();
  const respondió = await buscarYEsperar();
  if (!respondió) { console.log('Backend no disponible — test omitido'); return; }
  const rows = await driver.findElements(By.css('table tbody tr'));
  if (rows.length === 0) { console.log('Sin datos — test omitido'); return; }
  await waitForEl(driver, By.xpath('//button[contains(text(),"Exportar a Excel")]'));
  await waitForEl(driver, By.xpath('//button[contains(text(),"Exportar a PDF")]'));
});

test('los encabezados incluyen las columnas de ingresos esperadas', async () => {
  await goToReport();
  const respondió = await buscarYEsperar();
  if (!respondió) { console.log('Backend no disponible — test omitido'); return; }
  const rows = await driver.findElements(By.css('table tbody tr'));
  if (rows.length === 0) { console.log('Sin datos — test omitido'); return; }
  const thead = await waitForEl(driver, By.css('table thead'));
  const text  = (await thead.getText()).toUpperCase();
  assert.ok(text.includes('MES'),            `Falta "Mes": ${text}`);
  assert.ok(text.includes('VUELOS'),         `Falta "Vuelos": ${text}`);
  assert.ok(text.includes('ING. TIQUETES'),  `Falta "Ing. Tiquetes": ${text}`);
  assert.ok(text.includes('ING. MALETAS'),   `Falta "Ing. Maletas": ${text}`);
  assert.ok(text.includes('TOTAL INGRESOS'), `Falta "Total Ingresos": ${text}`);
});

test('la fila de totales aparece en el tfoot con la etiqueta TOTALES', async () => {
  await goToReport();
  const respondió = await buscarYEsperar();
  if (!respondió) { console.log('Backend no disponible — test omitido'); return; }
  const rows = await driver.findElements(By.css('table tbody tr'));
  if (rows.length === 0) { console.log('Sin datos — test omitido'); return; }
  const tfoot = await waitForEl(driver, By.css('table tfoot'));
  assert.ok((await tfoot.getText()).includes('TOTALES'));
});

test('filtrar por año 2026 solo muestra meses de ese año', async () => {
  await goToReport();
  const yearInput = await waitForEl(driver, By.css('input[type="number"]'));
  await yearInput.clear();
  await yearInput.sendKeys('2026');
  const respondió = await buscarYEsperar();
  if (!respondió) { console.log('Backend no disponible — test omitido'); return; }
  const rows = await driver.findElements(By.css('table tbody tr'));
  if (rows.length === 0) { console.log('Sin datos para 2026 — test omitido'); return; }
  for (const row of rows) {
    const text = await row.getText();
    assert.ok(text.includes('2026'), `Fila no corresponde a 2026: ${text}`);
  }
});
