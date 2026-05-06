<template>
  <div class="login-page">
    <div class="login-card">
      
      <div class="login-header">
        <h2>Completar Registro Mushu</h2>
        <p>Establece tu contraseña para activar tu cuenta</p>
      </div>

      <form @submit.prevent="completeRegister">

        <div class="input-group-custom">
          <label>Contraseña *</label>
          <div class="input-box">
            <i class="bi bi-lock"></i>
            <input
              type="password"
              v-model="password"
              placeholder="Crea tu contraseña"
              required
            />
          </div>
        </div>

        <div class="input-group-custom">
          <label>Confirmar Contraseña *</label>
          <div class="input-box">
            <i class="bi bi-lock"></i>
            <input
              type="password"
              v-model="confirmPassword"
              placeholder="Confirma tu contraseña"
              required
            />
          </div>
        </div>

        <div class="requirements">
          <strong>Requisitos de contraseña:</strong>
          <ul>
            <li>Mínimo 8 caracteres</li>
            <li>Al menos una letra minúscula</li>
            <li>Al menos una letra mayúscula</li>
            <li>Al menos un número</li>
            <li>Al menos un carácter especial</li>
            <li>Deben coincidir ambas contraseñas</li>
          </ul>
        </div>

        <div class="actions">
          <button type="button" class="cancel-btn" @click="goBack" :disabled="loading">
            Cancelar
          </button>

          <button type="submit" class="login-btn" :disabled="loading">
            {{ loading ? "Activando..." : "Activar Cuenta" }}
          </button>
      </div>

      </form>

      <p class="message">{{ message }}</p>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  data() {
    return {
      loading: false,
      token: "",
      password: "",
      confirmPassword: "",
      message: ""
    };
  },
  mounted() {
    const params = new URLSearchParams(window.location.search);
    this.token = params.get("token");
  },
  methods: {
    async completeRegister() {
      if (this.loading) return;
      if (this.password !== this.confirmPassword) {
        this.message = "Las contraseñas no coinciden";
        return;
      }
      this.loading = true;
      try {
        if (!this.validatePassword(this.password)) {
          this.message = "Contraseña débil (8 chars, mayúscula, minúscula, número y símbolo)";
          return;
        }
        const response = await axios.post(
          "http://localhost:5103/api/PendingAccount/completeRegister",
          {
            token: this.token,
            password: this.password
          }
        );

        this.message = response.data;
      } catch (error) {
        this.message = error.response?.data || "Error";
      } finally {
        this.loading = false;
      }
    },
    validatePassword(password) {
      const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W]).{8,}$/;
      return regex.test(password);
    },
    goBack() {
      window.location.href = "/login";
    }
  }
};
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  background: linear-gradient(#d63031, #f39c12);
  display: flex;
  justify-content: center;
  align-items: center;
}

.login-card {
  background: white;
  width: 420px;
  padding: 30px;
  border-radius: 15px;
}

.login-header {
  text-align: center;
  margin-bottom: 20px;
}

.login-header h2 {
  margin-bottom: 5px;
}

.login-header p {
  color: gray;
  font-size: 14px;
}

.input-group-custom {
  margin-bottom: 18px;
}

.input-group-custom label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

.input-box {
  border: 1px solid lightgray;
  border-radius: 8px;
  padding: 10px;
  display: flex;
  gap: 8px;
}

.input-box input {
  border: none;
  outline: none;
  width: 100%;
}

.requirements {
  background: #eef3f9;
  padding: 15px;
  border-radius: 10px;
  margin-bottom: 20px;
  font-size: 13px;
}

.requirements ul {
  margin: 8px 0 0 15px;
  padding: 0;
}

.actions {
  display: flex;
  gap: 10px;
}

.cancel-btn {
  flex: 1;
  padding: 12px;
  border: none;
  border-radius: 8px;
  background: lightgray;
}

.login-btn {
  flex: 1;
  padding: 12px;
  border: none;
  border-radius: 8px;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: white;
  font-weight: bold;
}

.message {
  margin-top: 15px;
  text-align: center;
  font-weight: bold;
}
</style>