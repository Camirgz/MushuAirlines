import axios from "axios";
import API_BASE_URL from "@/config/api";

const baseUrl = `${API_BASE_URL}/api/aircraft`;
const typeBaseUrl = `${API_BASE_URL}/api/aircraft-type`;

export function getAircraftTypes() {
  return axios.get(baseUrl);
}

export function getAircraftTypeById(id) {
  return axios.get(`${baseUrl}/${id}`);
}

export function createAircraftType(payload) {
  return axios.post(baseUrl, payload);
}

export function updateAircraftType(id, payload) {
  return axios.put(`${baseUrl}/${id}`, payload);
}

export function deleteAircraftType(id) {
  return axios.delete(`${baseUrl}/${id}`);
}

export function getAircraftTypeOptions() {
  return axios.get(typeBaseUrl);
}
