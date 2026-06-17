import axios from "axios";

const BASE = "http://localhost:5103/api";

export async function getReservationReport() {
    const token = localStorage.getItem("token");

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