import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import FlightResultCard from '@/components/FlightResultCard.vue'

const directFlight = {
  id: 1,
  origin: 'SJO', destination: 'MIA',
  departureTime: '08:00', arrivalTime: '12:00',
  date: '2026-06-01', arrivalDate: '2026-06-01',
  durationLabel: '4h',
  priceEconomy: 50000, priceFirstClass: 120000,
}

const leg1 = { id: 2, origin: 'SJO', destination: 'PTY', departureTime: '07:00', arrivalTime: '09:00', date: '2026-06-01', arrivalDate: '2026-06-01', durationLabel: '2h', priceEconomy: 30000, priceFirstClass: 70000 }
const leg2 = { id: 3, origin: 'PTY', destination: 'MIA', departureTime: '11:00', arrivalTime: '15:00', date: '2026-06-01', arrivalDate: '2026-06-01', durationLabel: '4h', priceEconomy: 40000, priceFirstClass: 90000 }

describe('Flight result card – direct flight', () => {
  it('shows the route and both class prices so the user can compare', () => {
    const wrapper = mount(FlightResultCard, { props: { resultType: 'direct', directFlight } })
    expect(wrapper.text()).toContain('SJO')
    expect(wrapper.text()).toContain('MIA')
    expect(wrapper.text()).toContain('50,000')
    expect(wrapper.text()).toContain('120,000')
  })

  it('shows the total cost when travelling with more than one passenger', () => {
    const wrapper = mount(FlightResultCard, { props: { resultType: 'direct', directFlight, passengerCount: 2 } })
    expect(wrapper.text()).toContain('100,000')
  })

  it('lets the user select the flight by clicking Seleccionar', async () => {
    const wrapper = mount(FlightResultCard, { props: { resultType: 'direct', directFlight } })
    await wrapper.find('.select-btn').trigger('click')
    expect(wrapper.emitted('select')).toBeTruthy()
  })
})

describe('Flight result card – stopover flight', () => {
  it('shows the layover city so the user knows where they stop', () => {
    const wrapper = mount(FlightResultCard, {
      props: { resultType: 'stopover', leg1, leg2, layoverMinutes: 120, connectionCity: 'Panamá' },
    })
    expect(wrapper.find('.layover-badge').exists()).toBe(true)
    expect(wrapper.text()).toContain('Panamá')
  })

  it('shows the combined price for both legs', () => {
    const wrapper = mount(FlightResultCard, {
      props: { resultType: 'stopover', leg1, leg2, layoverMinutes: 120, connectionCity: 'Panamá' },
    })
    expect(wrapper.text()).toContain('70,000') // 30000 + 40000
  })

  it('lets the user select the itinerary by clicking Seleccionar', async () => {
    const wrapper = mount(FlightResultCard, {
      props: { resultType: 'stopover', leg1, leg2, layoverMinutes: 120, connectionCity: 'Panamá' },
    })
    await wrapper.find('.select-btn').trigger('click')
    expect(wrapper.emitted('select')[0][0].type).toBe('stopover')
  })
})
