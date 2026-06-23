// Spanish weekday names as stored in the DB frequency field.
const WEEKDAY_NAMES = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado']

const MIN_LAYOVER_MINUTES = 30
const MAX_LAYOVER_MINUTES = 12 * 60 // 12 hours

// Parse "HH:MM" or "HH:MM:SS" into total minutes since midnight.
function timeToMinutes(timeString) {
  const parts = timeString.split(':')
  return parseInt(parts[0], 10) * 60 + parseInt(parts[1], 10)
}

// A flight is overnight when its arrival clock time is earlier than its departure.
function isOvernightFlight(flight) {
  return timeToMinutes(flight.departureTime) > timeToMinutes(flight.arrivalTime)
}

// Gap in minutes between leg1 arrival and leg2 departure.
// When leg2 departs after midnight relative to leg1 arrival, the gap wraps around.
function calculateLayoverMinutes(leg1ArrivalTime, leg2DepartureTime) {
  const arrivalMinutes = timeToMinutes(leg1ArrivalTime)
  const departureMinutes = timeToMinutes(leg2DepartureTime)
  if (departureMinutes >= arrivalMinutes) {
    return departureMinutes - arrivalMinutes
  }
  return (24 * 60 - arrivalMinutes) + departureMinutes
}

function isValidLayover(layoverMinutes) {
  return layoverMinutes >= MIN_LAYOVER_MINUTES && layoverMinutes <= MAX_LAYOVER_MINUTES
}

// Return the Spanish name for the day-of-week of a "YYYY-MM-DD" string.
// Returns null when no date is provided.
function getWeekdayName(dateString) {
  if (!dateString) return null
  const [year, month, day] = dateString.split('-').map(Number)
  return WEEKDAY_NAMES[new Date(year, month - 1, day).getDay()]
}

// Advance one day in the WEEKDAY_NAMES cycle.
function advanceWeekday(weekdayName) {
  const index = WEEKDAY_NAMES.indexOf(weekdayName)
  return WEEKDAY_NAMES[(index + 1) % 7]
}

// Return true when either no weekday filter is active or the flight runs on that day.
function flightOperatesOnWeekday(flight, weekdayName) {
  if (!weekdayName || !flight.frequency || flight.frequency.length === 0) return true
  return flight.frequency.includes(weekdayName)
}

// Return true when the flight has not yet passed its finalization date on the given date string.
function flightIsActiveOnDate(flight, dateString) {
  if (!dateString || !flight.finalizationDate) return true
  const [y, m, d] = dateString.split('-').map(Number)
  const searchDate = new Date(y, m - 1, d)
  const [ey, em, ed] = flight.finalizationDate.split('-').map(Number)
  return new Date(ey, em - 1, ed) >= searchDate
}

// Add `days` to a "YYYY-MM-DD" string and return the resulting date string.
// Returns an empty string when the input is empty.
function addDaysToDateString(dateString, days) {
  if (!dateString) return ''
  const [year, month, day] = dateString.split('-').map(Number)
  const date = new Date(year, month - 1, day)
  date.setDate(date.getDate() + days)
  return [
    date.getFullYear(),
    String(date.getMonth() + 1).padStart(2, '0'),
    String(date.getDate()).padStart(2, '0'),
  ].join('-')
}

// Find all valid two-leg itineraries.
// originCodes and destinationCodes are arrays of airport codes (e.g. ['JFK', 'LGA']).
// A connection is valid when:
//   - leg1 departs from one of originCodes toward an intermediate airport X
//   - leg2 departs from X toward one of destinationCodes
//   - the layover gap is between MIN_LAYOVER_MINUTES and MAX_LAYOVER_MINUTES
//   - both legs operate on the correct weekday for the given date
//
// allFlights must be the normalized flight objects produced by routeToFlight.
function findStopoverConnections(allFlights, originCodes, destinationCodes, dateString) {
  const selectedWeekday = getWeekdayName(dateString)
  const nextDayWeekday = selectedWeekday ? advanceWeekday(selectedWeekday) : null

  const firstLegs = allFlights.filter(flight =>
    originCodes.includes(flight.origin) &&
    !destinationCodes.includes(flight.destination) &&
    flightOperatesOnWeekday(flight, selectedWeekday) &&
    flightIsActiveOnDate(flight, dateString)
  )

  const secondLegs = allFlights.filter(flight =>
    destinationCodes.includes(flight.destination) &&
    flightIsActiveOnDate(flight, dateString)
  )

  const connections = []

  for (const leg1 of firstLegs) {
    const leg1IsOvernight = isOvernightFlight(leg1)
    const leg1ArrivalDay = leg1IsOvernight ? nextDayWeekday : selectedWeekday

    for (const leg2 of secondLegs) {
      if (leg2.origin !== leg1.destination) continue

      const layoverMinutes = calculateLayoverMinutes(leg1.arrivalTime, leg2.departureTime)
      if (!isValidLayover(layoverMinutes)) continue

      // leg2 departs after midnight relative to leg1 arrival when its clock time is earlier
      const layoverCrossesMidnight = timeToMinutes(leg2.departureTime) < timeToMinutes(leg1.arrivalTime)
      const weekdayRequiredForLeg2 = layoverCrossesMidnight ? advanceWeekday(leg1ArrivalDay) : leg1ArrivalDay
      if (!flightOperatesOnWeekday(leg2, weekdayRequiredForLeg2)) continue

      connections.push({
        leg1,
        leg2,
        layoverMinutes,
        connectionAirportCode: leg1.destination,
        connectionCity: leg1.destinationCity || leg1.destination,
      })
    }
  }

  return connections
}

export {
  findStopoverConnections,
  isOvernightFlight,
  addDaysToDateString,
}
