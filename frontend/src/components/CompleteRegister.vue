<template>
  <div class="page">
    <h2>Completar Registro</h2>

    <form @submit.prevent="completeRegister">
      <input
        type="password"
        v-model="password"
        placeholder="Contraseña"
        required
      />

      <input
        type="password"
        v-model="confirmPassword"
        placeholder="Confirmar contraseña"
        required
      />

      <button type="submit">Crear cuenta</button>
    </form>

    <p>{{ message }}</p>
  </div>
</template>

<script>
import axios from "axios";

export default {
  data() {
    return {
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
      if (this.password !== this.confirmPassword) {
        this.message = "Las contraseñas no coinciden";
        return;
      }

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
      }
    },
    validatePassword(password) {
      const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W]).{8,}$/;
      return regex.test(password);
    }
  }
};
</script>