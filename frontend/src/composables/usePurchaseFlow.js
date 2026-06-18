import { reactive, readonly, computed } from 'vue'

const STORAGE_KEY = 'mushu_purchase_flow'

function defaultState() {
  return {
    flight:     null,
    flight2:    null,
    seats:      [],
    passengers: [],
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


export function usePurchaseFlow() {

  /** Save flight + seat selections after the user confirms from FlightResultCard.
   *  For stopovers pass flight2 = { code, flightDate } for the second leg. */
  function setFlight(flight, seats, flight2 = null) {
    _state.flight  = flight
    _state.flight2 = flight2
    _state.seats   = seats
    persist()
  }

  /** Save passenger info after PassengerInfoPage is submitted. */
  function setPassengers(passengers) {
    _state.passengers = passengers
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
      flight2: _state.flight2 ? {
        routeCode:  _state.flight2.code,
        flightDate: _state.flight2.flightDate,
      } : null,
      passengers: _state.passengers.map(p => ({
        firstName:       p.firstName,
        lastName:        p.lastName,
        gender:          p.gender,
        passportCountry: p.passportCountry,
        birthDate:       p.birthDate,
        email:           p.email          ?? '',
        phone:           p.phone          ?? '',
        handBagCount:    p.handBagCount   ?? 0,
        checkedBagCount: p.checkedBagCount ?? 0,
      })),
      seatSelections: _state.seats.map(s => ({
        passengerIndex: s.passengerIndex,
        seatClass:      s.seatClass,
        seatNumber:     s.seatNumber,
      })),
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
