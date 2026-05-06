<template>
  <div class="login-page">
    <div class="login-card">

      <RouterLink to="/admin/users" class="back-arrow">&#8592; Volver</RouterLink>

      <h2 class="login-title">Crear Nuevo Usuario</h2>
      <p class="login-subtitle">Registro de empleados Mushu Airlines</p>

      <form @submit.prevent="createEmployee">

        <div class="grid">

          <div class="input-group-custom">
            <label>Nombre</label>
            <div class="input-box">
              <i class="bi bi-person"></i>
              <input v-model="form.firstName" required />
            </div>
          </div>

          <div class="input-group-custom">
            <label>Apellido</label>
            <div class="input-box">
              <i class="bi bi-person"></i>
              <input v-model="form.lastName" required />
            </div>
          </div>

          <div class="input-group-custom full">
            <label>Correo</label>
            <div class="input-box">
              <i class="bi bi-envelope"></i>
              <input v-model="form.email" type="email" required />
            </div>
          </div>

          <div class="input-group-custom">
            <label>SSN</label>
            <div class="input-box">
              <i class="bi bi-card-text"></i>
              <input v-model="form.ssn" required />
            </div>
          </div>

          <div class="input-group-custom">
            <label>Nacionalidad</label>
            <div class="input-box">
              <i class="bi bi-globe"></i>
              <input v-model="form.nationality" required />
            </div>
          </div>

          <div class="input-group-custom">
            <label>Salario</label>
            <div class="input-box">
              <i class="bi bi-currency-dollar"></i>
              <input v-model="form.salary" type="number" />
            </div>
          </div>

          <div class="input-group-custom">
            <label>Horario</label>
            <div class="input-box">
              <i class="bi bi-clock"></i>
              <input v-model="form.workSchedule" />
            </div>
          </div>

          <div class="input-group-custom full">
            <label>Permisos</label>
            <div class="input-box">
              <i class="bi bi-shield"></i>
              <input v-model="form.permissions" />
            </div>
          </div>

        </div>

        <div class="input-group-custom">
          <label>Rol</label>
          <div class="input-box">
            <select v-model="form.role">
              <option>Administrator</option>
              <option>Operator</option>
            </select>
          </div>
        </div>

        <button type="submit" class="login-btn" :disabled="loading">
          {{ loading ? "Creando..." : "Crear Usuario" }}
        </button>

      </form>

      <div v-if="message" :class="['alert-box', isError ? 'alert-error' : 'alert-success']">
        <i :class="['bi', isError ? 'bi-exclamation-circle-fill' : 'bi-check-circle-fill']"></i>
        {{ message }}
      </div>

    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  data() {
    return {
      loading: false,
      isError: false,
      form: {
        firstName: "",
        lastName: "",
        ssn: "",
        nationality: "",
        url: "",
        salary: 0,
        workSchedule: "",
        permissions: "",
        email: "",
        role: "Operator"
      },
      message: ""
    };
  },
  methods: {
    async createEmployee() {
      if (this.loading) return; //don't allow multiple submissions

      this.loading = true;
      try {
        const token = localStorage.getItem("token");
        if (!this.validateEmail(this.form.email)) {
          this.message = "El correo electrónico ingresado no es válido.";
          this.isError = true;
          return;
        }
        await axios.post(
          "http://localhost:5103/api/PendingAccount",
          this.form,
          {
            headers: {
              Authorization: `Bearer ${token}`
            }
          }
        );

        this.message = "Invitación enviada con éxito. El usuario recibirá un correo para completar su registro.";
        this.isError = false;
      } catch (error) {
        const raw = error.response?.data || "";
        this.message = (typeof raw === "string" && raw.startsWith("ERROR REAL:"))
          ? "Ocurrió un error inesperado. Por favor intente de nuevo."
          : raw || "Ocurrió un error. Por favor intente de nuevo.";
        this.isError = true;
      } finally {
        this.loading = false; // allow new submissions after response
      }
    },
    validateEmail(email) {
      const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      return regex.test(email);
    }
  }
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
  width: 700px;
  padding: 30px;
  border-radius: 15px;
}

.back-arrow {
  display: inline-block;
  margin-bottom: 16px;
  color: #e74c3c;
  font-weight: bold;
  text-decoration: none;
  font-size: 14px;
  padding: 8px 16px;
  border: 1px solid #f1948a;
  border-radius: 10px;
  background: #fdf2f2;
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

.input-box input,
.input-box select{
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

.grid{
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 15px;
}

.full{
  grid-column: span 2;
}

.alert-box {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 16px;
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
}

.alert-success {
  background: #eafaf1;
  color: #1e8449;
  border: 1px solid #a9dfbf;
}

.alert-error {
  background: #fdf2f2;
  color: #c0392b;
  border: 1px solid #f1948a;
}
</style>