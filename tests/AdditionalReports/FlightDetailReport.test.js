import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { shallowMount } from '@vue/test-utils'
import FlightDetailReport from '@/components/reports/FlightDetailReport.vue'

vi.mock('@/services/FlightReportsService', () => ({
  getFlightDetailReport:    vi.fn(),
  downloadFlightDetailExcel: vi.fn(),
  downloadFlightDetailPdf:   vi.fn(),
}))

import { getFlightDetailReport } from '@/services/FlightReportsService'

afterEach(() => { vi.clearAllMocks() })

function makeRow(overrides = {}) {
  return {
    fecha:                 '2026-06-01',
    origen:                'MAD',
    destino:               'CDG',
    codigoVuelo:           'MA-001',
    pasajerosPrimeraClase: 1,
    pasajerosEconomia:     2,
    aerolinea:             'Mushu Airlines',
    ventaPasajeros:        2000,
    ventaEquipajes:        700,
    totalVenta:            2700,
    ...overrides,
  }
}

const totalsRow = {
  fecha:                 null,
  origen:                null,
  destino:               null,
  codigoVuelo:           null,
  pasajerosPrimeraClase: 2,
  pasajerosEconomia:     5,
  aerolinea:             'TOTALES',
  ventaPasajeros:        4000,
  ventaEquipajes:        1500,
  totalVenta:            5500,
}

async function mount(data = {}) {
  const wrapper = shallowMount(FlightDetailReport, {
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

describe('FlightDetailReport – computed: dataRows y totalsRow', () => {
  it('separa las filas de datos de la fila de totales usando fecha === null', async () => {
    const wrapper = await mount({ rows: [makeRow(), makeRow({ codigoVuelo: 'MA-002' }), totalsRow] })
    expect(wrapper.vm.dataRows).toHaveLength(2)
    expect(wrapper.vm.totalsRow).not.toBeNull()
    expect(wrapper.vm.totalsRow.aerolinea).toBe('TOTALES')
  })

  it('devuelve null en totalsRow cuando no hay fila de totales', async () => {
    const wrapper = await mount({ rows: [makeRow()] })
    expect(wrapper.vm.totalsRow).toBeNull()
  })

  it('devuelve dataRows vacío cuando solo viene la fila de totales', async () => {
    const wrapper = await mount({ rows: [totalsRow] })
    expect(wrapper.vm.dataRows).toHaveLength(0)
  })
})

describe('FlightDetailReport – computed: buildFilters', () => {
  it('incluye el origen seleccionado del autocomplete en los filtros', async () => {
    const wrapper = await mount({ selectedOrigen: 'MAD', origenQuery: 'Adolfo Suárez' })
    expect(wrapper.vm.buildFilters().origen).toBe('MAD')
  })

  it('usa el texto escrito si no se seleccionó ninguna sugerencia de origen', async () => {
    const wrapper = await mount({ selectedOrigen: null, origenQuery: 'CDG' })
    expect(wrapper.vm.buildFilters().origen).toBe('CDG')
  })

  it('omite origen del filtro cuando el campo está vacío', async () => {
    const wrapper = await mount({ selectedOrigen: null, origenQuery: '' })
    expect(wrapper.vm.buildFilters().origen).toBeUndefined()
  })

  it('incluye la clase cuando se seleccionó una', async () => {
    const wrapper = await mount({ filtros: { clase: 'FirstClass', fechaDesde: '', fechaHasta: '' } })
    expect(wrapper.vm.buildFilters().clase).toBe('FirstClass')
  })

  it('omite la clase del filtro cuando está en Todos', async () => {
    const wrapper = await mount({ filtros: { clase: '', fechaDesde: '', fechaHasta: '' } })
    expect(wrapper.vm.buildFilters().clase).toBeUndefined()
  })
})

describe('FlightDetailReport – cargarReporte', () => {
  it('muestra los vuelos devueltos por el servicio', async () => {
    getFlightDetailReport.mockResolvedValue([makeRow(), totalsRow])
    const wrapper = await mount()
    await wrapper.vm.cargarReporte()
    expect(wrapper.vm.rows).toHaveLength(2)
    expect(wrapper.vm.loadError).toBeNull()
  })

  it('muestra el mensaje de error cuando el servicio falla', async () => {
    getFlightDetailReport.mockRejectedValue({ message: 'Error de conexión' })
    const wrapper = await mount()
    await wrapper.vm.cargarReporte()
    expect(wrapper.vm.loadError).toBe('Error de conexión')
    expect(wrapper.vm.rows).toHaveLength(0)
  })

  it('activa el estado loading mientras se carga y lo desactiva al terminar', async () => {
    let resolve
    getFlightDetailReport.mockReturnValue(new Promise(r => { resolve = r }))
    const wrapper = await mount()
    const promise = wrapper.vm.cargarReporte()
    expect(wrapper.vm.loading).toBe(true)
    resolve([])
    await promise
    expect(wrapper.vm.loading).toBe(false)
  })

  it('marca searched en true tras ejecutar la búsqueda', async () => {
    getFlightDetailReport.mockResolvedValue([])
    const wrapper = await mount()
    expect(wrapper.vm.searched).toBe(false)
    await wrapper.vm.cargarReporte()
    expect(wrapper.vm.searched).toBe(true)
  })
})

describe('FlightDetailReport – limpiarFiltros', () => {
  it('resetea todos los filtros y los resultados al limpiar', async () => {
    getFlightDetailReport.mockResolvedValue([makeRow(), totalsRow])
    const wrapper = await mount()
    await wrapper.vm.cargarReporte()
    await wrapper.vm.limpiarFiltros()
    expect(wrapper.vm.rows).toHaveLength(0)
    expect(wrapper.vm.filtros.clase).toBe('')
    expect(wrapper.vm.origenQuery).toBe('')
    expect(wrapper.vm.destinoQuery).toBe('')
    expect(wrapper.vm.searched).toBe(false)
  })
})

describe('FlightDetailReport – selectOrigen / selectDestino', () => {
  it('guarda el valor IATA y el label al seleccionar un aeropuerto de origen', async () => {
    const wrapper = await mount()
    wrapper.vm.selectOrigen({ value: 'MAD', label: 'Madrid-Barajas (MAD)', type: 'airport' })
    expect(wrapper.vm.selectedOrigen).toBe('MAD')
    expect(wrapper.vm.origenQuery).toBe('Madrid-Barajas (MAD)')
    expect(wrapper.vm.showOrigenDropdown).toBe(false)
  })

  it('guarda el valor IATA y el label al seleccionar un aeropuerto de destino', async () => {
    const wrapper = await mount()
    wrapper.vm.selectDestino({ value: 'CDG', label: 'Paris-Charles de Gaulle (CDG)', type: 'airport' })
    expect(wrapper.vm.selectedDestino).toBe('CDG')
    expect(wrapper.vm.destinoQuery).toBe('Paris-Charles de Gaulle (CDG)')
    expect(wrapper.vm.showDestinoDropdown).toBe(false)
  })
})
