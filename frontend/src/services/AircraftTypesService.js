import axios from "axios";

const baseUrl = "http://localhost:5103/api/aircraft";
const typeBaseUrl = "http://localhost:5103/api/aircraft-type";

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

export function getAircraftTypeOptions() {
  return axios.get(typeBaseUrl);
}
