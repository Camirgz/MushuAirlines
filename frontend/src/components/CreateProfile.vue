<template>
  <div class="login-page">
    <div class="login-card">

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

          <div class="input-group-custom full">
            <label>URL</label>
            <div class="input-box">
              <i class="bi bi-link"></i>
              <input v-model="form.url" />
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

        <button type="submit" class="login-btn">
          Crear Usuario
        </button>

      </form>

      <p style="text-align:center; margin-top:10px;">{{ message }}</p>

    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  data() {
    return {
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
      try {
        const token = localStorage.getItem("token");
        if (!this.validateEmail(this.form.email)) {
          this.message = "Correo inválido";
          return;
        }
        const response = await axios.post(
          "http://localhost:5103/api/PendingAccount",
          this.form,
          {
            headers: {
              Authorization: `Bearer ${token}`
            }
          }
        );

        this.message = response.data;
      } catch (error) {
        this.message = error.response?.data || "Error";
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
</style>