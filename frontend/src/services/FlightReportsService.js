import axios from "axios";
import API_BASE_URL from "@/config/api";

const BASE = `${API_BASE_URL}/api/reports/flights`;

function authHeaders() {
    const token = localStorage.getItem("token");
    return { Authorization: `Bearer ${token}` };
}

async function readBlobError(err) {
    try {
        const blob = err.response?.data;
        if (blob instanceof Blob) {
            const text = await blob.text();
            const parsed = JSON.parse(text);
            return parsed.message ?? parsed.title ?? text;
        }
    } catch (_) { /* ignore */ }
    return err.response?.data?.message ?? err.response?.data ?? err.message ?? "Error desconocido.";
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
        if (err.response?.status === 401) throw { status: 401, message: "Sesión expirada." };
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
        if (err.response?.status === 401) throw { status: 401, message: "Sesión expirada." };
        throw { message: await readBlobError(err) };
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
        if (err.response?.status === 401) throw { status: 401, message: "Sesión expirada." };
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
        if (err.response?.status === 401) throw { status: 401, message: "Sesión expirada." };
        throw { message: await readBlobError(err) };
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
        if (err.response?.status === 401) throw { status: 401, message: "Sesión expirada." };
        throw { message: await readBlobError(err) };
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
        if (err.response?.status === 401) throw { status: 401, message: "Sesión expirada." };
        throw { message: await readBlobError(err) };
    }
}
