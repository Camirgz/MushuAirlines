import axios from "axios";
import API_BASE_URL from "@/config/api";
const BASE = `${API_BASE_URL}/api`;
export async function sendCancellationEmail() {
  try {
    const token = localStorage.getItem("reservationToken");
    const response = await axios.post(
      `${BASE}/ReservationCancellation/sendEmailCancellation`,
      {},
      {
        headers: {
          Authorization: `Bearer ${token}`, 
        },
      }
    );
    return response.data;
  } catch (err) {
    const data = err.response?.data ?? {};
    throw {
      type: "error",
      message: data.message ?? "No se pudo enviar el correo.",
    };
  }
}

export async function cancelReservation(token) {
  try {
    const response = await axios.post(`${BASE}/ReservationCancellation/cancel/${token}`);
    return response.data;
  } catch (err) {
    const data = err.response?.data ?? {};

    throw {
      type: "error",
      message: data.message ?? "No se pudo cancelar la reserva.",
    };
  }
}