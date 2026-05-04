<template>
  <div class="page">
    <h2>Crear nuevo empleado</h2>

    <form @submit.prevent="createEmployee">
      <input v-model="form.firstName" placeholder="Nombre" required />
      <input v-model="form.lastName" placeholder="Apellido" required />
      <input v-model="form.ssn" placeholder="Cédula" required />
      <input v-model="form.nationality" placeholder="Nacionalidad" required />

      <input v-model="form.url" placeholder="Foto URL" />
      <input v-model="form.salary" type="number" placeholder="Salario" />
      <input v-model="form.workSchedule" placeholder="Horario" />
      <input v-model="form.permissions" placeholder="Permisos" />

      <input v-model="form.email" type="email" placeholder="Correo" required />
      <select v-model="form.role">
        <option>Administrator</option>
        <option>Operator</option>
      </select>

      <button type="submit">Enviar invitación</button>
    </form>

    <p>{{ message }}</p>
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