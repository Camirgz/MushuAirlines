import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { shallowMount } from '@vue/test-utils'
import LandingPage from '@/components/LandingPage.vue'

const fetchMock = vi.fn().mockResolvedValue({ ok: true, json: () => Promise.resolve([]) })
beforeEach(() => { global.fetch = fetchMock; fetchMock.mockClear() })
afterEach(() => { delete global.fetch; localStorage.clear() })

function makeToken(role) {
  const payload = btoa(JSON.stringify({ 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': role }))
  return `${btoa('{}')}.${payload}.sig`
}

function makeFlight(id, priceEconomy, durationHours) {
  return { id, origin: 'SJO', destination: 'MIA', durationHours, priceEconomy, priceFirstClass: priceEconomy * 2, frequency: [] }
}

async function mount(data = {}) {
  const wrapper = shallowMount(LandingPage, {
    global: { stubs: { RouterLink: { template: '<a><slot /></a>' }, FlightResultCard: true } },
  })
  if (Object.keys(data).length) await wrapper.setData(data)
  return wrapper
}

describe('Navbar – guest user', () => {
  it('shows the Admin Login button when nobody is logged in', async () => {
    const wrapper = await mount({ userRole: null })
    expect(wrapper.text()).toContain('Admin Login')
    expect(wrapper.text()).not.toContain('Logout')
  })
})

describe('Navbar – logged in user', () => {
  it('shows the Logout button instead of Admin Login when a user is logged in', async () => {
    const wrapper = await mount({ userRole: 'Operator' })
    expect(wrapper.text()).toContain('Logout')
    expect(wrapper.text()).not.toContain('Admin Login')
  })

  it('shows the Gestión menu only for administrators', async () => {
    const admin    = await mount({ userRole: 'Administrator' })
    const operator = await mount({ userRole: 'Operator' })
    expect(admin.text()).toContain('Gestión')
    expect(operator.text()).not.toContain('Gestión')
  })

  it('logs the user out and brings back the Admin Login button', async () => {
    localStorage.setItem('token', makeToken('Administrator'))
    const wrapper = await mount({ userRole: 'Administrator' })
    wrapper.vm.logout()
    await wrapper.vm.$nextTick()
    expect(wrapper.text()).toContain('Admin Login')
    expect(wrapper.text()).not.toContain('Logout')
  })
})

describe('Sort bar – direct flights', () => {
  const flights = [
    makeFlight(1, 80000, 6),
    makeFlight(2, 30000, 2),
    makeFlight(3, 55000, 4),
  ]
  const base = { hasSearched: true, directFlightResults: flights, filterPriceMin: 0, filterPriceMax: 999999, priceSliderRange: 999999, filterDurationMin: 0, filterDurationMax: 24, filterPriceClass: 'economy' }

  it('orders flights cheapest first by default', async () => {
    const wrapper = await mount({ ...base, sortBy: 'price-economy' })
    expect(wrapper.vm.filteredDirectFlights.map(f => f.id)).toEqual([2, 3, 1])
  })

  it('orders flights by shortest duration when the user selects Duración', async () => {
    const wrapper = await mount({ ...base, sortBy: 'duration-asc' })
    expect(wrapper.vm.filteredDirectFlights.map(f => f.id)).toEqual([2, 3, 1])
  })

  it('hides flights that exceed the price filter', async () => {
    const wrapper = await mount({ ...base, filterPriceMax: 50000 })
    expect(wrapper.vm.filteredDirectFlights.every(f => f.priceEconomy <= 50000)).toBe(true)
  })
})

describe('Sort bar – stopover flights', () => {
  function makeConn(layoverMinutes) {
    return {
      leg1: makeFlight(10, 20000, 2),
      leg2: makeFlight(11, 20000, 2),
      layoverMinutes, connectionCity: 'PTY',
    }
  }

  const connections = [makeConn(180), makeConn(45), makeConn(90)]
  const base = { stopoverResults: connections, filterPriceMin: 0, filterPriceMax: 9999999, priceSliderRange: 9999999, filterDurationMin: 0, filterDurationMax: 24, filterPriceClass: 'economy' }

  it('shows the shortest layover first when the user picks Escala más corta', async () => {
    const wrapper = await mount({ ...base, sortBy: 'layover-asc' })
    expect(wrapper.vm.filteredStopoverResults.map(c => c.layoverMinutes)).toEqual([45, 90, 180])
  })

  it('shows the longest layover first when the user picks Escala más larga', async () => {
    const wrapper = await mount({ ...base, sortBy: 'layover-desc' })
    expect(wrapper.vm.filteredStopoverResults.map(c => c.layoverMinutes)).toEqual([180, 90, 45])
  })
})
