<template>
    <div class="login-page">
        <div class="login-card">
            <div class="login-icon">
                <img src="@/assets/logo.png" alt="logo" width="55" height="55" />
            </div>

            <h2 class="login-title">Mis Reservas</h2>
            <p class="login-subtitle">
                Consulte su itinerario ingresando los datos de su reserva
            </p>

            <form @submit.prevent="saveLogin">

                <div class="input-group-custom">
                    <label>Código de reserva</label>
                    <div class="input-box">
                        <i class="bi bi-ticket-perforated"></i>
                        <input
                            v-model="form.reservationCode"
                            type="text"
                            placeholder="Ingrese el código de reserva"
                            required
                        />
                    </div>
                </div>

                <div class="input-group-custom">
                    <label>Nombre</label>
                    <div class="input-box">
                        <i class="bi bi-person"></i>
                        <input
                            v-model="form.firstName"
                            type="text"
                            placeholder="Ingrese su nombre"
                            required
                        />
                    </div>
                </div>

                <div class="input-group-custom">
                    <label>Apellido</label>
                    <div class="input-box">
                        <i class="bi bi-person"></i>
                        <input
                            v-model="form.lastName"
                            type="text"
                            placeholder="Ingrese su apellido"
                            required
                        />
                    </div>
                </div>

                <button type="submit" class="login-btn">
                    Consultar Reserva
                </button>

            </form>

            <button class="back-home-btn" @click="$router.push('/')">
                <i class="bi bi-arrow-left"></i> Volver al inicio
            </button>
        </div>
    </div>
</template>

<script>
import axios from "axios";
import API_BASE_URL from "@/config/api";
const BASE = `${API_BASE_URL}/api`;
import "@/assets/styles/login.css";
export default {
    data() {
        return {
            form: {
                reservationCode: "",
                firstName: "",
                lastName: ""
            }
        };
    },

    methods: {
        saveLogin() {
            console.log("Datos enviados:", this.form);
            axios.post(`${BASE}/ReservationLogin`, {
                reservationCode: this.form.reservationCode,
                firstName: this.form.firstName,
                lastName: this.form.lastName
            })
            .then(function(response){
                localStorage.setItem("reservationToken", response.data);
                alert("Reserva encontrada");
                window.location.href="/my-reservation/report";
            })
            .catch(function(error){
                const msg =
                    error.response?.data ||
                    "Error de conexión con el servidor";
                alert(msg);
            });
        }
    }
};
</script>