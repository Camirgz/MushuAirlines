import axios from "axios";

const BaseUrl = "http://localhost:5103/api/aircraft";

export function GetAircraftTypes() {
  return axios.get(BaseUrl);
}

export function CreateAircraftType(Payload) {
  return axios.post(BaseUrl, Payload);
}

