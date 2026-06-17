<template>
    <div class="login-page">
        <div class="login-card">
            <div class="login-icon">
                <img src="@/assets/logo.png" alt="logo" width="55" height="55" />
            </div>
            <h2 class="login-title">Login del Dragón</h2>
            <p class="login-subtitle">Acceso administrativo de Mushu Airlines</p>
            <form @submit.prevent="saveLogin">
                <div class="input-group-custom">
                    <label>Usuario</label>
                    <div class="input-box">
                        <i class="bi bi-person"></i>
                        <input
                        v-model="form.username"
                        type="text"
                        placeholder="Ingrese su usuario"
                        required
                        />
                    </div>
                </div>
                <div class="input-group-custom">
                    <label>Contraseña</label>
                    <div class="input-box">
                        <i class="bi bi-lock"></i>
                        <input
                        v-model="form.password"
                        type="password"
                        placeholder="Ingrese su contraseña"
                        required
                        />
                    </div>
                </div>
                <button type="submit" class="login-btn">
                    Iniciar Sesión
                </button>
            </form>
        </div>
    </div>
</template>

<script>
import axios from "axios";
import "@/assets/styles/login.css";
export default {
    data() {
        return {
            form: { username: "", password: "", },
        };
    },
    methods: {
        saveLogin() {
            axios.post("http://localhost:5103/api/login", {
                username: this.form.username,
                password: this.form.password,
            })
            .then(function(response) {
                console.log("TOKEN RECIBIDO:", response.data);
                // we save the token in localStorage and redirect to the admin page
                localStorage.setItem("token", response.data);
                alert("Login correcto");
                window.location.href = "/admin";
            })
            .catch(function(error) {
                const msg = error.response?.data || "Error de conexión con el servidor";
                alert(msg);
            });
        },
    },
};
</script>
