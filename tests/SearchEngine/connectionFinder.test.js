import { describe, it, expect } from 'vitest'
import { findStopoverConnections, isOvernightFlight, addDaysToDateString } from '@/services/connectionFinder'

function makeFlight(overrides) {
  return {
    id: 1,
    origin: 'SJO',
    destination: 'PTY',
    departureTime: '08:00',
    arrivalTime: '10:00',
    frequency: ['Lunes', 'Martes', 'Miércoles'],
    destinationCity: 'Panamá',
    priceEconomy: 30000,
    priceFirstClass: 70000,
    durationHours: 2,
    ...overrides,
  }
}

const leg1 = makeFlight({ id: 1, origin: 'SJO', destination: 'PTY' })
const leg2 = makeFlight({ id: 2, origin: 'PTY', destination: 'MIA', departureTime: '11:00', arrivalTime: '15:00' })

describe('Stopover connection finder', () => {
  it('offers a connection when two flights have enough time between them', () => {
    const results = findStopoverConnections([leg1, leg2], 'SJO', 'MIA', '2026-06-01')
    expect(results).toHaveLength(1)
    expect(results[0].connectionCity).toBe('Panamá')
  })

  it('does not offer a connection when the wait between flights is too short', () => {
    const tooClose = { ...leg2, departureTime: '10:20' }
    expect(findStopoverConnections([leg1, tooClose], 'SJO', 'MIA', '2026-06-01')).toHaveLength(0)
  })

  it('does not offer a connection when the wait between flights is too long', () => {
    const tooLate = { ...leg2, departureTime: '23:30' }
    expect(findStopoverConnections([leg1, tooLate], 'SJO', 'MIA', '2026-06-01')).toHaveLength(0)
  })

  it('shows no options when there are no available flights', () => {
    expect(findStopoverConnections([], 'SJO', 'MIA', '2026-06-01')).toHaveLength(0)
  })
})

describe('Overnight flight detection', () => {
  it('a flight that lands the next day is detected as overnight', () => {
    expect(isOvernightFlight(makeFlight({ departureTime: '22:00', arrivalTime: '02:00' }))).toBe(true)
  })

  it('a regular daytime flight is not overnight', () => {
    expect(isOvernightFlight(makeFlight({ departureTime: '08:00', arrivalTime: '12:00' }))).toBe(false)
  })
})

describe('Midnight-crossing layover', () => {
  it('forms a connection when leg1 arrives late at night and leg2 departs after midnight', () => {
    // leg1: SJO→PTY arriving 23:00 Monday, leg2: PTY→MIA departing 03:00 Tuesday (4h gap)
    const leg1Night = makeFlight({ id: 1, origin: 'SJO', destination: 'PTY', departureTime: '20:00', arrivalTime: '23:00', frequency: ['Lunes'] })
    const leg2NextDay = makeFlight({ id: 2, origin: 'PTY', destination: 'MIA', departureTime: '03:00', arrivalTime: '07:00', frequency: ['Martes'] })
    const results = findStopoverConnections([leg1Night, leg2NextDay], ['SJO'], ['MIA'], '2026-06-22')
    expect(results).toHaveLength(1)
    expect(results[0].layoverMinutes).toBe(240)
  })

  it('does not form a connection when the midnight-crossing gap exceeds 12 hours', () => {
    // leg1 arrives 23:00, leg2 departs 13:00 next day (14h gap)
    const leg1Night = makeFlight({ id: 1, origin: 'SJO', destination: 'PTY', departureTime: '20:00', arrivalTime: '23:00', frequency: ['Lunes'] })
    const leg2TooLate = makeFlight({ id: 2, origin: 'PTY', destination: 'MIA', departureTime: '13:00', arrivalTime: '17:00', frequency: ['Martes'] })
    expect(findStopoverConnections([leg1Night, leg2TooLate], ['SJO'], ['MIA'], '2026-06-22')).toHaveLength(0)
  })
})

describe('Date calculation', () => {
  it('correctly advances the departure date by one day', () => {
    expect(addDaysToDateString('2026-05-31', 1)).toBe('2026-06-01')
  })
})
