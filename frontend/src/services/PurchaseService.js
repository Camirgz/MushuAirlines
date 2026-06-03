import axios from 'axios'

const BASE = 'http://localhost:5103/api'

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
