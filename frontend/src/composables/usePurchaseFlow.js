import { reactive, readonly, computed } from 'vue'

const STORAGE_KEY = 'mushu_purchase_flow'

function defaultState() {
  return {
    flight:     null,
    seats:      [],
    passengers: [],
    baggage: {
      handCount:     0,
      handWeight:    0,
      checkedCount:  0,
      checkedWeight: 0,
    },
    payment: {
      method:       '',
      contactEmail: '',
    },
  }
}

function loadFromStorage() {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

// Module-level singleton — one shared reactive object for the whole app.
const _state = reactive(loadFromStorage() ?? defaultState())

function persist() {
  sessionStorage.setItem(STORAGE_KEY, JSON.stringify(_state))
}

/**
 * usePurchaseFlow — shared state for the end-to-end purchase flow.
 *
 * Expected shapes:
 *
 * flight  — FlightDto from GET /api/flights, extended with { flightDate: 'YYYY-MM-DD' }
 * seats   — [{ passengerIndex, seatClass, seatNumber }]
 * passengers — [{ firstName, lastName, gender, passportCountry, passportNumber,
 *                 birthDate, email, phone }]
 * baggage — { handCount, handWeight, checkedCount, checkedWeight }
 * payment — { method, contactEmail }
 */
export function usePurchaseFlow() {

  /** Save flight + seat selections after the user confirms from FlightResultCard. */
  function setFlight(flight, seats) {
    _state.flight = flight
    _state.seats  = seats
    persist()
  }

  /** Save passenger info and baggage after PassengerInfoPage is submitted. */
  function setPassengers(passengers, baggage) {
    _state.passengers = passengers
    _state.baggage    = baggage
    persist()
  }

  /** Save payment method and contact email after PaymentForm is submitted. */
  function setPayment(payment) {
    _state.payment = payment
    persist()
  }

  /** Reset the full flow and remove the sessionStorage entry. */
  function clear() {
    Object.assign(_state, defaultState())
    sessionStorage.removeItem(STORAGE_KEY)
  }

  /**
   * Build the PurchaseRequestModel expected by POST /api/purchase.
   * Call this inside PaymentForm once both payment and all prior steps are set.
   */
  function buildPurchaseRequest() {
    return {
      flight: {
        routeCode:  _state.flight?.code ?? '',
        flightDate: _state.flight?.flightDate ?? '',
      },
      passengers: _state.passengers.map(p => ({
        firstName:       p.firstName,
        lastName:        p.lastName,
        gender:          p.gender,
        passportCountry: p.passportCountry,
        birthDate:       p.birthDate,
        email:           p.email  ?? '',
        phone:           p.phone  ?? '',
      })),
      seatSelections: _state.seats.map(s => ({
        passengerIndex: s.passengerIndex,
        seatClass:      s.seatClass,
        seatNumber:     s.seatNumber,
      })),
      baggage: {
        handCount:     _state.baggage.handCount,
        handWeight:    _state.baggage.handWeight,
        checkedCount:  _state.baggage.checkedCount,
        checkedWeight: _state.baggage.checkedWeight,
      },
      payment: {
        method:       _state.payment.method,
        contactEmail: _state.payment.contactEmail,
      },
    }
  }

  const hasFlight     = computed(() => _state.flight !== null)
  const hasPassengers = computed(() => _state.passengers.length > 0)

  return {
    state:          readonly(_state),
    hasFlight,
    hasPassengers,
    setFlight,
    setPassengers,
    setPayment,
    buildPurchaseRequest,
    clear,
  }
}
