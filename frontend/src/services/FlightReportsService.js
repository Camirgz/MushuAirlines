import axios from "axios";
import API_BASE_URL from "@/config/api";

const BASE = `${API_BASE_URL}/api/reports/flights`;

function authHeaders() {
    const token = localStorage.getItem("token");
    return { Authorization: `Bearer ${token}` };
}

function buildQuery(params) {
    const query = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== "") {
            query.append(key, value);
        }
    });
    const str = query.toString();
    return str ? `?${str}` : "";
}

export async function getFlightDetailReport(filters = {}) {
    try {
        const response = await axios.get(
            `${BASE}/detail${buildQuery(filters)}`,
            { headers: authHeaders() }
        );
        return response.data;
    } catch (err) {
        const msg = err.response?.data?.message ?? err.response?.data ?? err.message ?? "No se pudo cargar el reporte de vuelo detallado.";
        throw { message: typeof msg === 'string' ? msg : JSON.stringify(msg) };
    }
}

export async function downloadFlightDetailExcel(filters = {}) {
    try {
        const response = await axios.get(
            `${BASE}/detail/excel${buildQuery(filters)}`,
            { responseType: "blob", headers: authHeaders() }
        );
        return response.data;
    } catch (err) {
        throw { message: "No se pudo descargar el archivo Excel." };
    }
}

export async function getMonthlyIncomeReport(filters = {}) {
    try {
        const response = await axios.get(
            `${BASE}/monthly${buildQuery(filters)}`,
            { headers: authHeaders() }
        );
        return response.data;
    } catch (err) {
        const msg = err.response?.data?.message ?? err.response?.data ?? err.message ?? "No se pudo cargar el reporte de ingresos mensuales.";
        throw { message: typeof msg === 'string' ? msg : JSON.stringify(msg) };
    }
}

export async function downloadMonthlyIncomeExcel(filters = {}) {
    try {
        const response = await axios.get(
            `${BASE}/monthly/excel${buildQuery(filters)}`,
            { responseType: "blob", headers: authHeaders() }
        );
        return response.data;
    } catch (err) {
        throw { message: "No se pudo descargar el archivo Excel." };
    }
}

export async function downloadFlightDetailPdf(filters = {}) {
    try {
        const response = await axios.get(
            `${BASE}/detail/pdf${buildQuery(filters)}`,
            { responseType: "blob", headers: authHeaders() }
        );
        return response.data;
    } catch (err) {
        throw { message: "No se pudo descargar el PDF." };
    }
}

export async function downloadMonthlyIncomePdf(filters = {}) {
    try {
        const response = await axios.get(
            `${BASE}/monthly/pdf${buildQuery(filters)}`,
            { responseType: "blob", headers: authHeaders() }
        );
        return response.data;
    } catch (err) {
        throw { message: "No se pudo descargar el PDF." };
    }
}
