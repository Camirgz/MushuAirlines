import { describe, it, expect, afterEach, vi } from 'vitest'
import { shallowMount } from '@vue/test-utils'
import MonthlyIncomeReport from '@/components/reports/MonthlyIncomeReport.vue'

vi.mock('@/services/FlightReportsService', () => ({
  getMonthlyIncomeReport:     vi.fn(),
  downloadMonthlyIncomeExcel: vi.fn(),
  downloadMonthlyIncomePdf:   vi.fn(),
}))

import { getMonthlyIncomeReport } from '@/services/FlightReportsService'

afterEach(() => { vi.clearAllMocks() })

function makeRow(mes, overrides = {}) {
  return {
    mes,
    cantidadVuelos:               3,
    totalPasajerosPrimeraClase:   2,
    totalPasajerosEconomia:       5,
    totalPasajeros:               7,
    ingresosTiquetes:             4000,
    ingresosMaletas:              1500,
    totalIngresos:                5500,
    ...overrides,
  }
}

const totalsRow = {
  mes:                          'TOTALES',
  cantidadVuelos:               6,
  totalPasajerosPrimeraClase:   4,
  totalPasajerosEconomia:       10,
  totalPasajeros:               14,
  ingresosTiquetes:             8000,
  ingresosMaletas:              3000,
  totalIngresos:                11000,
}

async function mount(data = {}) {
  const wrapper = shallowMount(MonthlyIncomeReport, {
    global: {
      stubs: {
        AdminPageLayout: { template: '<div><slot /></div>' },
        AdminHero:       true,
        AdminCard:       { template: '<div><slot /></div>' },
        RouterLink:      { template: '<a><slot /></a>' },
      },
    },
  })
  if (Object.keys(data).length) await wrapper.setData(data)
  return wrapper
}

describe('MonthlyIncomeReport – computed: dataRows y totalsRow', () => {
  it('separa las filas de meses de la fila TOTALES', async () => {
    const wrapper = await mount({ rows: [makeRow('Junio 2026'), makeRow('Mayo 2026'), totalsRow] })
    expect(wrapper.vm.dataRows).toHaveLength(2)
    expect(wrapper.vm.totalsRow).not.toBeNull()
    expect(wrapper.vm.totalsRow.mes).toBe('TOTALES')
  })

  it('devuelve null en totalsRow cuando no viene la fila de totales', async () => {
    const wrapper = await mount({ rows: [makeRow('Junio 2026')] })
    expect(wrapper.vm.totalsRow).toBeNull()
  })

  it('devuelve dataRows vacío si solo viene la fila TOTALES', async () => {
    const wrapper = await mount({ rows: [totalsRow] })
    expect(wrapper.vm.dataRows).toHaveLength(0)
  })

  it('currentYear es el año actual', async () => {
    const wrapper = await mount()
    expect(wrapper.vm.currentYear).toBe(new Date().getFullYear())
  })
})

describe('MonthlyIncomeReport – computed: buildFilters', () => {
  it('incluye el año cuando está definido', async () => {
    const wrapper = await mount({ filtros: { anio: 2026, aerolinea: '' } })
    expect(wrapper.vm.buildFilters().anio).toBe(2026)
  })

  it('omite el año del filtro cuando no se ingresó ninguno', async () => {
    const wrapper = await mount({ filtros: { anio: null, aerolinea: '' } })
    expect(wrapper.vm.buildFilters().anio).toBeUndefined()
  })

  it('incluye el origen seleccionado del autocomplete', async () => {
    const wrapper = await mount({ selectedOrigen: 'MAD', origenQuery: 'Madrid' })
    expect(wrapper.vm.buildFilters().origen).toBe('MAD')
  })

  it('usa el texto escrito cuando no se seleccionó sugerencia de origen', async () => {
    const wrapper = await mount({ selectedOrigen: null, origenQuery: 'CDG' })
    expect(wrapper.vm.buildFilters().origen).toBe('CDG')
  })

  it('omite origen cuando el campo está vacío', async () => {
    const wrapper = await mount({ selectedOrigen: null, origenQuery: '' })
    expect(wrapper.vm.buildFilters().origen).toBeUndefined()
  })
})

describe('MonthlyIncomeReport – cargarReporte', () => {
  it('muestra los meses devueltos por el servicio', async () => {
    getMonthlyIncomeReport.mockResolvedValue([makeRow('Junio 2026'), totalsRow])
    const wrapper = await mount()
    await wrapper.vm.cargarReporte()
    expect(wrapper.vm.rows).toHaveLength(2)
    expect(wrapper.vm.loadError).toBeNull()
  })

  it('muestra el mensaje de error cuando el servicio falla', async () => {
    getMonthlyIncomeReport.mockRejectedValue({ message: 'Sin conexión' })
    const wrapper = await mount()
    await wrapper.vm.cargarReporte()
    expect(wrapper.vm.loadError).toBe('Sin conexión')
    expect(wrapper.vm.rows).toHaveLength(0)
  })

  it('activa loading mientras carga y lo desactiva al terminar', async () => {
    let resolve
    getMonthlyIncomeReport.mockReturnValue(new Promise(r => { resolve = r }))
    const wrapper = await mount()
    const promise = wrapper.vm.cargarReporte()
    expect(wrapper.vm.loading).toBe(true)
    resolve([])
    await promise
    expect(wrapper.vm.loading).toBe(false)
  })

  it('marca searched en true tras ejecutar la búsqueda', async () => {
    getMonthlyIncomeReport.mockResolvedValue([])
    const wrapper = await mount()
    expect(wrapper.vm.searched).toBe(false)
    await wrapper.vm.cargarReporte()
    expect(wrapper.vm.searched).toBe(true)
  })
})

describe('MonthlyIncomeReport – limpiarFiltros', () => {
  it('resetea filtros y resultados al limpiar', async () => {
    getMonthlyIncomeReport.mockResolvedValue([makeRow('Junio 2026'), totalsRow])
    const wrapper = await mount()
    await wrapper.vm.cargarReporte()
    await wrapper.vm.limpiarFiltros()
    expect(wrapper.vm.rows).toHaveLength(0)
    expect(wrapper.vm.filtros.anio).toBeNull()
    expect(wrapper.vm.origenQuery).toBe('')
    expect(wrapper.vm.destinoQuery).toBe('')
    expect(wrapper.vm.searched).toBe(false)
  })
})

describe('MonthlyIncomeReport – selectOrigen / selectDestino', () => {
  it('guarda el IATA y el label al seleccionar un aeropuerto de origen', async () => {
    const wrapper = await mount()
    wrapper.vm.selectOrigen({ value: 'MAD', label: 'Madrid-Barajas (MAD)', type: 'airport' })
    expect(wrapper.vm.selectedOrigen).toBe('MAD')
    expect(wrapper.vm.origenQuery).toBe('Madrid-Barajas (MAD)')
    expect(wrapper.vm.showOrigenDropdown).toBe(false)
  })

  it('guarda el IATA y el label al seleccionar un aeropuerto de destino', async () => {
    const wrapper = await mount()
    wrapper.vm.selectDestino({ value: 'CDG', label: 'Paris-Charles de Gaulle (CDG)', type: 'airport' })
    expect(wrapper.vm.selectedDestino).toBe('CDG')
    expect(wrapper.vm.destinoQuery).toBe('Paris-Charles de Gaulle (CDG)')
    expect(wrapper.vm.showDestinoDropdown).toBe(false)
  })
})
