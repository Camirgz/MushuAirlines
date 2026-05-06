import axios from "axios";

const BaseUrl     = "http://localhost:5103/api/aircraft";
const TypeBaseUrl = "http://localhost:5103/api/aircraft-type";

export function GetAircraftTypes() {
  return axios.get(BaseUrl);
}

export function CreateAircraftType(Payload) {
  return axios.post(BaseUrl, Payload);
}

export function GetAircraftTypeOptions() {
  return axios.get(TypeBaseUrl);
}

