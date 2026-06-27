import axios from 'axios'
import API_BASE_URL from '@/config/api'

const BASE = `${API_BASE_URL}/api`

/**
 * Validate card details via the payment simulation endpoint.
 *
 * POST /api/payment/approve
 *
 * Throws { type: 'payment', message } when the card is rejected.
 *
 * @param {{ holder, cardNumber, expiry, cvv, paymentMethod }} cardData
 */
export async function validatePayment(cardData) {
  try {
    await axios.post(`${BASE}/payment/approve`, cardData)
  } catch (err) {
    const data = err.response?.data ?? {}
    throw { type: 'payment', message: data.message ?? 'Error al procesar el pago.' }
  }
}

/**
 * Create the full purchase (passengers, tickets, itinerary).
 *
 * POST /api/purchase
 *
 * On success returns the full PurchaseResponseModel (includes purchaseId,
 * reservationCode, invoiceNumber, totalPaid, tickets, etc.).
 *
 * Throws a structured error keyed by type so callers can show targeted messages:
 *   { type: 'passenger', message, field }       — 400 PassengerDataException
 *   { type: 'seat',      message, seatNumber }  — 409 SeatUnavailableException
 *   { type: 'flight',    message, routeCode }   — 422 InvalidFlightDateException
 *   { type: 'unknown',   message }              — any other error
 *
 * @param {Object} requestBody — output of usePurchaseFlow().buildPurchaseRequest()
 * @returns {Promise<Object>} PurchaseResponseModel
 */
export async function createPurchase(requestBody) {
  try {
    const response = await axios.post(`${BASE}/purchase`, requestBody)
    return response.data
  } catch (err) {
    const status = err.response?.status
    const data   = err.response?.data ?? {}

    if (status === 400) {
      throw { type: 'passenger', message: data.message ?? 'Datos de pasajero inválidos.', field: data.field }
    }
    if (status === 409) {
      throw { type: 'seat', message: data.message ?? 'Asiento no disponible.', seatNumber: data.seatNumber }
    }
    if (status === 422) {
      throw { type: 'flight', message: data.message ?? 'Vuelo o fecha no válidos.', routeCode: data.routeCode }
    }
    throw { type: 'unknown', message: data.message ?? 'Ocurrió un error al procesar la compra.' }
  }
}

/**
 * Check if any of the provided passengers already have a ticket on the same flight.
 * All three fields (name, birthDate, passportCountry) must match for a duplicate to be detected.
 *
 * POST /api/purchase/check-passenger-duplicates
 *
 * @param {string} routeCode
 * @param {string} flightDate  — YYYY-MM-DD
 * @param {{ firstName, lastName, birthDate, passportCountry }[]} passengers
 * @returns {Promise<{ hasDuplicates: boolean, duplicates: string[] }>}
 */
export async function checkPassengerDuplicates(routeCode, flightDate, passengers) {
  try {
    const response = await axios.post(`${BASE}/purchase/check-passenger-duplicates`, {
      routeCode,
      flightDate,
      passengers: passengers.map(p => ({
        firstName:       p.firstName.trim(),
        lastName:        p.lastName.trim(),
        birthDate:       p.birthDate,
        passportCountry: p.passportCountry,
      })),
    })
    return response.data
  } catch {
    return { hasDuplicates: false, duplicates: [] } // fail-open
  }
}

/**
 * Check if a flight has enough available seats for the requested passenger count.
 *
 * GET /api/purchase/check-availability?routeCode=...&flightDate=...&count=N
 *
 * Returns true if seats are available, false if the flight is full.
 * Gracefully returns true on network error to avoid blocking the flow unnecessarily.
 *
 * @param {string} routeCode
 * @param {string} flightDate  — YYYY-MM-DD
 * @param {number} count       — number of seats needed
 * @returns {Promise<boolean>}
 */
export async function checkAvailability(routeCode, flightDate, firstClassCount, economyCount) {
  try {
    const response = await axios.get(`${BASE}/purchase/check-availability`, {
      params: { routeCode, flightDate, firstClassCount, economyCount }
    })
    return response.data.available === true
  } catch {
    return true // fail-open: let the purchase attempt handle it
  }
}

/**
 * Fetch purchase confirmation data without triggering another email.
 *
 * GET /api/purchaseconfirmation/{purchaseId}
 *
 * Returns PurchaseConfirmationModel on success.
 * Throws { type: 'notFound', message } when the purchase does not exist.
 *
 * @param {number} purchaseId
 * @returns {Promise<Object>} PurchaseConfirmationModel
 */
export async function getPurchaseData(purchaseId) {
  try {
    const response = await axios.get(`${BASE}/purchaseconfirmation/${purchaseId}`)
    return response.data
  } catch (err) {
    const data = err.response?.data ?? {}
    throw { type: 'notFound', message: data.message ?? 'Compra no encontrada.' }
  }
}

/**
 * Trigger the confirmation + invoice email for a completed purchase.
 *
 * POST /api/purchaseconfirmation/send/{purchaseId}
 *
 * This call is intentionally non-blocking: the caller should fire-and-forget
 * and never fail the navigation to the confirmation page because of an email error.
 *
 * @param {number} purchaseId
 */
export async function sendConfirmation(purchaseId) {
  await axios.post(`${BASE}/purchaseconfirmation/send/${purchaseId}`)
}
export async function getPurchaseDataForBaggage(purchaseId) {
  try {
    const response = await axios.get(
      `${BASE}/purchaseconfirmation/${purchaseId}/baggage`
    );
    return response.data;
  } catch (err) {
    const data = err.response?.data ?? {};
    throw {
      type: "notFound",
      message: data.message ?? "Compra no encontrada."
    };
  }
}