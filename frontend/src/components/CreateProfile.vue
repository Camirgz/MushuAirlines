<template>
  <div class="page-container">
    <nav class="navbar bg-white shadow-sm px-4 py-2">
      <RouterLink
        to="/"
        class="navbar-brand d-flex align-items-center gap-2"
      >
        <img
          src="@/assets/logo.png"
          width="42"
          height="42"
          class="rounded-2"
        />
        <div>
          <div class="brand-name">Mushu Airlines</div>
          <div class="brand-tagline">Vuela con el dragón</div>
        </div>
      </RouterLink>

      <div class="d-flex align-items-center gap-3">

        <RouterLink to="/admin" class="btn btn-gradient">
          Gestión
        </RouterLink>

        <RouterLink
          to="/"
          class="btn btn-outline-danger rounded-pill px-3 py-2"
        >
          Logout
        </RouterLink>

      </div>
    </nav>

    <div class="container mt-5">
      <div class="admin-banner">
        <RouterLink
          to="/admin/users"
          class="text-white text-decoration-none fw-bold"
        >
          ← Volver
        </RouterLink>

        <h1 class="mt-3">
          Gestión de Usuarios
        </h1>

        <p>
          Registro de empleados Mushu Airlines
        </p>

      </div>
    </div>

    <div class="container mt-4 mb-5">

      <div class="flight-card">

        <h2 class="mb-4 fw-bold">
          Crear Nuevo Usuario
        </h2>

        <!-- ALERT -->

        <div
          v-if="message"
          :class="isError
            ? 'alert-error-custom'
            : 'alert-success-custom'"
        >
          {{ message }}
        </div>
        <form @submit.prevent="createEmployee">

          <div class="row">

            <div class="col-md-6 form-group mb-3">
              <label>Nombre *</label>

              <input
                v-model="form.firstName"
                type="text"
                placeholder="Ej: Juan"
                required
              />
            </div>

            <div class="col-md-6 form-group mb-3">
              <label>Apellido *</label>

              <input
                v-model="form.lastName"
                type="text"
                placeholder="Ej: Pérez"
                required
              />
            </div>

            <div class="col-md-12 form-group mb-3">
              <label>Correo *</label>

              <input
                v-model="form.email"
                type="email"
                placeholder="correo@ejemplo.com"
                required
              />
            </div>

            <div class="col-md-6 form-group mb-3">
              <label>SSN *</label>

              <input
                v-model="form.ssn"
                type="text"
                required
              />
            </div>

            <div class="col-md-6 form-group mb-3">
              <label>Nacionalidad *</label>

              <input
                v-model="form.nationality"
                type="text"
                required
              />
            </div>

            <div class="col-md-6 form-group mb-3">
              <label>Salario *</label>

              <input
                type="number"
                min="0"
                v-model.number="form.salary"
                placeholder="₡ 0"
              />
            </div>

            <div class="col-md-6 form-group mb-3">
              <label>Horario *</label>

              <input
                v-model="form.workSchedule"
                type="text"
                placeholder="Ej: L-V 8AM a 5PM"
              />
            </div>

            <div class="col-md-12 form-group mb-3">
              <label>Permisos *</label>

              <input
                v-model="form.permissions"
                type="text"
                placeholder="Ej: Gestión de vuelos"
              />
            </div>

            <div class="col-md-12 form-group mb-4">
              <label>Rol *</label>

              <select v-model="form.role">

                <option value="Administrator">
                  Administrator
                </option>

                <option value="Operator">
                  Operator
                </option>

              </select>
            </div>

          </div>

          <button
            type="submit"
            class="search-btn"
            :disabled="loading"
          >

            {{ loading
              ? "Creando..."
              : "Crear Usuario"
            }}

          </button>

        </form>

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
        salary: null,
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

.page-container {
  min-height: 100vh;
  background: #f8f9fa;
}

.navbar {
  border-bottom: 1px solid #e5e7eb;
}

.brand-name {
  font-weight: 800;
  font-size: 1.1rem;
}

.brand-tagline {
  font-size: 0.75rem;
  color: #6b7280;
}

.btn-gradient {
  background: linear-gradient(135deg, #f01818, #ff5a00, #ffc400);
  color: white;
  border: none;
  border-radius: 10px;
  padding: 10px 18px;
  font-weight: 700;
}

.btn-gradient:hover {
  color: white;
  opacity: 0.95;
}

.admin-banner {
  background: linear-gradient(135deg, #f01818, #ff5a00, #ffc400);
  color: white;
  border-radius: 16px;
  padding: 32px;
  box-shadow: 0 10px 24px rgba(0,0,0,0.12);
}

.admin-banner h1 {
  margin-bottom: 8px;
  font-weight: 900;
}

.admin-banner p {
  margin: 0;
  opacity: 0.95;
}

.flight-card {
  background: white;
  border-radius: 16px;
  padding: 32px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.08);
}

.form-group label {
  font-weight: 700;
  margin-bottom: 8px;
  display: block;
  color: #1f2937;
}

.form-group input,
.form-group select {
  width: 100%;
  height: 48px;
  border: 1px solid #d1d5db;
  border-radius: 10px;
  padding: 0 14px;
  outline: none;
  transition: 0.2s ease;
  background: white;
}

.form-group input:focus,
.form-group select:focus {
  border-color: #ff5a00;
  box-shadow: 0 0 0 3px rgba(255,90,0,0.12);
}

.search-btn {
  width: 100%;
  height: 50px;
  border: none;
  border-radius: 10px;
  background: #ff5a00;
  color: white;
  font-weight: 800;
  transition: 0.2s ease;
}

.search-btn:hover:not(:disabled) {
  background: #f01818;
}

.search-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}
.alert-success-custom {
  background: #ecfdf5;
  color: #166534;
  border: 1px solid #bbf7d0;
  border-radius: 10px;
  padding: 14px;
  margin-bottom: 20px;
}

.alert-error-custom {
  background: #fff7ed;
  color: #9a3412;
  border: 1px solid #fed7aa;
  border-radius: 10px;
  padding: 14px;
  margin-bottom: 20px;
}

</style>