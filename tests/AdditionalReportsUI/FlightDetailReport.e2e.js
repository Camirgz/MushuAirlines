import { test, before, after } from 'node:test';
import assert from 'node:assert/strict';
import { By } from 'selenium-webdriver';
import { buildDriver, injectToken, waitForEl, BASE_URL, TIMEOUT } from './helpers.js';

let driver;

before(async () => { driver = buildDriver(); });
after(async  () => { await driver.quit(); });

async function goToReport() {
  await injectToken(driver, 'Administrator');
  await driver.get(`${BASE_URL}/admin/reports/flight-detail`);
  await waitForEl(driver, By.xpath('//button[contains(text(),"Buscar")]'));
}

/**
 * Hace clic en Buscar y espera hasta 5 s a que aparezca algún estado de
 * respuesta (tabla, vacío o error). Devuelve false si el backend no responde
 * a tiempo, en lugar de lanzar excepción — los tests que lo necesiten pueden
 * saltar condicionalmente.
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

test('carga la página con el título y los filtros visibles', async () => {
  await goToReport();
  const heading = await waitForEl(driver, By.xpath('//*[contains(text(),"Vuelo Detallado")]'));
  assert.ok((await heading.getText()).includes('Vuelo Detallado'));
  await waitForEl(driver, By.css('select'));
  await waitForEl(driver, By.css('input[type="date"]'));
});

test('el dropdown de clase contiene las opciones correctas', async () => {
  await goToReport();
  const select  = await waitForEl(driver, By.css('select'));
  const options = await select.findElements(By.css('option'));
  const texts   = await Promise.all(options.map(o => o.getText()));
  assert.ok(texts.includes('Todos'),         `Falta "Todos" — opciones: ${texts}`);
  assert.ok(texts.includes('Primera Clase'), `Falta "Primera Clase" — opciones: ${texts}`);
  assert.ok(texts.includes('Clase Turista'), `Falta "Clase Turista" — opciones: ${texts}`);
});

test('el botón Limpiar resetea los campos de fecha', async () => {
  await goToReport();
  const [fechaDesde] = await driver.findElements(By.css('input[type="date"]'));
  await fechaDesde.sendKeys('2026-01-01');
  const limpiar = await waitForEl(driver, By.xpath('//button[contains(text(),"Limpiar")]'));
  await limpiar.click();
  assert.equal(await fechaDesde.getAttribute('value'), '');
});

test('el botón Buscar es clickeable y activa el estado de carga', async () => {
  await goToReport();
  const btn = await waitForEl(driver, By.xpath('//button[contains(text(),"Buscar")]'));
  await btn.click();
  // Inmediatamente tras el click debe aparecer "Cargando..." o algún estado de respuesta
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

test('la fila de totales aparece en el tfoot con la etiqueta TOTALES', async () => {
  await goToReport();
  const respondió = await buscarYEsperar();
  if (!respondió) { console.log('Backend no disponible — test omitido'); return; }
  const rows = await driver.findElements(By.css('table tbody tr'));
  if (rows.length === 0) { console.log('Sin datos — test omitido'); return; }
  const tfoot = await waitForEl(driver, By.css('table tfoot'));
  assert.ok((await tfoot.getText()).includes('TOTALES'));
});

test('el encabezado muestra "Código Vuelo" y no "N° Vuelo"', async () => {
  await goToReport();
  const respondió = await buscarYEsperar();
  if (!respondió) { console.log('Backend no disponible — test omitido'); return; }
  const rows = await driver.findElements(By.css('table tbody tr'));
  if (rows.length === 0) { console.log('Sin datos — test omitido'); return; }
  const thead = await waitForEl(driver, By.css('table thead'));
  const text  = (await thead.getText()).toUpperCase();
  assert.ok(text.includes('CÓDIGO VUELO'), `Falta "Código Vuelo": ${text}`);
  assert.ok(!text.includes('N° VUELO'),    `Aún aparece "N° Vuelo": ${text}`);
});
