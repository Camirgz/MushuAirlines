<template>
  <AdminPageLayout>
    <AdminHero
      title="Gestión de Usuarios"
      subtitle="Registro de empleados de Mushu Airlines"
      icon="bi bi-person-plus"
      back-to="/admin/users"
      back-text="Volver a usuarios"
    />

    <AdminCard class="create-user-card">
      <h2>Crear Nuevo Usuario</h2>

      <div
        v-if="message"
        class="form-alert"
        :class="isError ? 'warning-alert' : 'success-alert'"
      >
        <i
          class="bi"
          :class="isError ? 'bi-exclamation-triangle-fill' : 'bi-check-circle-fill'"
        ></i>

        <div>
          <strong>{{ isError ? "No se pudo crear el usuario" : "Usuario creado" }}</strong>
          <p>{{ message }}</p>
        </div>
      </div>

      <form class="user-form" @submit.prevent="createEmployee">
        <div class="form-grid">
          <div class="form-group">
            <label for="firstName">Nombre <span>*</span></label>

            <input
              id="firstName"
              v-model.trim="form.firstName"
              type="text"
              placeholder="Ej: Juan"
              required
            />
          </div>

          <div class="form-group">
            <label for="lastName">Apellido <span>*</span></label>

            <input
              id="lastName"
              v-model.trim="form.lastName"
              type="text"
              placeholder="Ej: Pérez"
              required
            />
          </div>

          <div class="form-group full-width">
            <label for="email">Correo <span>*</span></label>

            <input
              id="email"
              v-model.trim="form.email"
              type="email"
              placeholder="correo@ejemplo.com"
              required
            />
          </div>

          <div class="form-group">
            <label for="ssn">SSN <span>*</span></label>

            <input
              id="ssn"
              v-model.trim="form.ssn"
              type="text"
              placeholder="Ej: 123456789"
              required
            />
          </div>

          <div class="form-group">
            <label for="nationality">Nacionalidad <span>*</span></label>

            <input
              id="nationality"
              v-model.trim="form.nationality"
              type="text"
              placeholder="Ej: Costarricense"
              required
            />
          </div>

          <div class="form-group">
            <label for="salary">Salario <span>*</span></label>

            <input
              id="salary"
              v-model.number="form.salary"
              type="number"
              min="0"
              placeholder="$ 0"
              required
            />
          </div>

          <div class="form-group">
            <label for="workSchedule">Horario <span>*</span></label>

            <input
              id="workSchedule"
              v-model.trim="form.workSchedule"
              type="text"
              placeholder="Ej: L-V 8am a 5pm"
              required
            />
          </div>

          <div class="form-group full-width">
            <label for="permissions">Permisos <span>*</span></label>

            <input
              id="permissions"
              v-model.trim="form.permissions"
              type="text"
              placeholder="Ej: Gestión de vuelos"
              required
            />
          </div>

          <div class="form-group full-width">
            <label for="role">Rol <span>*</span></label>

            <select id="role" v-model="form.role">
              <option value="Administrator">Administrator</option>
              <option value="Operator">Operator</option>
            </select>
          </div>
        </div>

        <button type="submit" class="submit-btn" :disabled="loading">
          <span v-if="!loading">Crear Usuario</span>

          <span v-else>
            <i class="bi bi-hourglass-split me-2"></i>
            Creando...
          </span>
        </button>
      </form>
    </AdminCard>
  </AdminPageLayout>
</template>

<script>
import axios from "axios";
import API_BASE_URL from "@/config/api";

import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";

const BaseURL = `${API_BASE_URL}/api/PendingAccount`;

export default {
  name: "CreateProfile",

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
      loading: false,
      isError: false,
      message: "",

      form: {
        firstName: "",
        lastName: "",
        ssn: "",
        nationality: "",
        salary: null,
        workSchedule: "",
        permissions: "",
        email: "",
        role: "Operator",
      },
    };
  },

  methods: {
    async createEmployee() {
      if (this.loading) {
        return;
      }

      this.message = "";
      this.isError = false;

      if (!this.validateEmail(this.form.email)) {
        this.message = "El correo electrónico ingresado no es válido.";
        this.isError = true;
        return;
      }

      this.loading = true;

      try {
        const token = localStorage.getItem("token");

        await axios.post(BaseURL, this.form, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        this.message = "Invitación enviada con éxito. El usuario recibirá un correo para completar su registro.";
        this.isError = false;

        setTimeout(() => {
          this.$router.push("/admin/users");
        }, 1800);
      } catch (error) {
        const raw = error.response?.data || "";

        this.message =
          typeof raw === "string" && raw.startsWith("ERROR REAL:")
            ? "Ocurrió un error inesperado. Por favor intente de nuevo."
            : raw || "Ocurrió un error. Por favor intente de nuevo.";

        this.isError = true;
      } finally {
        this.loading = false;
      }
    },

    validateEmail(email) {
      const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

      return regex.test(email);
    },
  },
};
</script>

<style scoped>
.create-user-card {
  width: 100%;
}

.create-user-card h2 {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0 0 22px;
}

.form-alert {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  border-radius: 12px;
  padding: 14px 16px;
  margin-bottom: 22px;
  border: 1px solid transparent;
}

.form-alert i {
  font-size: 1.25rem;
  margin-top: 1px;
}

.form-alert strong {
  display: block;
  font-size: 0.9rem;
  font-weight: 700;
  margin-bottom: 3px;
}

.form-alert p {
  margin: 0;
  font-size: 0.86rem;
  line-height: 1.45;
}

.warning-alert {
  background: #fff7ed;
  border-color: #fed7aa;
  color: #9a3412;
}

.success-alert {
  background: #ecfdf5;
  border-color: #bbf7d0;
  color: #166534;
}

.user-form {
  display: flex;
  flex-direction: column;
  gap: 22px;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 22px;
}

.full-width {
  grid-column: 1 / -1;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 0.88rem;
  font-weight: 700;
  color: #333;
}

.form-group label span {
  color: #e74c3c;
}

.form-group input,
.form-group select {
  width: 100%;
  height: 48px;
  border: 1.5px solid #e0e0e0;
  border-radius: 8px;
  padding: 0 16px;
  font-size: 0.9rem;
  color: #333;
  background: #ffffff;
  outline: none;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.form-group input::placeholder {
  color: #bbb;
}

.form-group input:focus,
.form-group select:focus {
  border-color: #e74c3c;
  box-shadow: 0 0 0 3px rgba(231, 76, 60, 0.12);
}

.submit-btn {
  width: 100%;
  min-height: 48px;
  border: none;
  border-radius: 8px;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.submit-btn:hover:not(:disabled) {
  opacity: 0.9;
  transform: translateY(-1px);
}

.submit-btn:disabled {
  opacity: 0.75;
  cursor: not-allowed;
}

@media (max-width: 768px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>
