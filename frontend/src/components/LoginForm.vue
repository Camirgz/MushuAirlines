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
export default {
    data() {
        return {
            form: { username: "", password: "", },
        };
    },
    methods: {
        saveLogin() {
            console.log("Datos a guardar:", this.form);
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
                console.log("ERROR COMPLETO:", error);
                console.log("RESPUESTA BACKEND:", error.response.data);
                alert(error.response.data);
            });
        },
    },
};
</script>

<style scoped>
.login-page{
  min-height: 100vh;
  background: linear-gradient(#d63031, #f39c12);
  display: flex;
  justify-content: center;
  align-items: center;
}

.login-card{
  background: white;
  width: 400px;
  padding: 30px;
  border-radius: 15px;
}

.login-icon{
  text-align: center;
  margin-bottom: 15px;
}

.login-title{
  text-align: center;
  font-weight: bold;
}

.login-subtitle{
  text-align: center;
  color: gray;
  margin-bottom: 25px;
}

.input-group-custom{
  margin-bottom: 18px;
}

.input-group-custom label{
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

.input-box{
  border: 1px solid lightgray;
  border-radius: 8px;
  padding: 10px;
  display: flex;
  gap: 8px;
}

.input-box input{
  border: none;
  outline: none;
  width: 100%;
}

.login-btn{
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: 8px;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: white;
  font-weight: bold;
}

</style>