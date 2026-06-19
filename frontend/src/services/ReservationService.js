import axios from "axios";
import API_BASE_URL from "@/config/api";
const BASE = `${API_BASE_URL}/api`;
export async function getReservationReport() {
    const token = localStorage.getItem("reservationToken");

    try {
        const response = await axios.get(
            `${BASE}/reservationreport`,
            {
                headers: {
                    Authorization: `Bearer ${token}`
                }
            }
        );

        return response.data;
    }
    catch (err) {
        const data = err.response?.data ?? {};

        throw {
            message:
                data.message ??
                "No se pudo cargar la información del vuelo."
        };
    }
}