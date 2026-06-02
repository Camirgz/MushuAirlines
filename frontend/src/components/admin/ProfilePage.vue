<template>
  <AdminPageLayout>
    <AdminHero
      title="Mi Perfil"
      subtitle="Información personal de mi cuenta personal en Mushu Airlines"
      icon="bi bi-person-circle"
    />

    <div v-if="successMessage" class="success-message">
      <i class="bi bi-check-circle-fill"></i>
      <span>{{ successMessage }}</span>
    </div>

    <div v-if="errorMessage" class="error-message">
      <i class="bi bi-exclamation-circle-fill"></i>
      <span>{{ errorMessage }}</span>
    </div>

    <AdminCard>
      <div v-if="!isEditing" class="profile-details">
        <div class="detail-grid">
          <div class="detail-group">
            <span>Nombre</span>
            <p>{{ profile.firstName }}</p>
          </div>

          <div class="detail-group">
            <span>Apellido</span>
            <p>{{ profile.lastName }}</p>
          </div>

          <div class="detail-group detail-full">
            <span>Correo</span>
            <p>{{ profile.email }}</p>
          </div>

          <div class="detail-group">
            <span>SSN</span>
            <p>{{ profile.ssn }}</p>
          </div>

          <div class="detail-group">
            <span>Nacionalidad</span>
            <p>{{ profile.nationality }}</p>
          </div>

          <div class="detail-group">
            <span>Salario</span>
            <p>{{ formatSalary(profile.salary) }}</p>
          </div>

          <div class="detail-group">
            <span>Horario</span>
            <p>{{ profile.workSchedule }}</p>
          </div>

          <div class="detail-group">
            <span>Permisos</span>
            <p>{{ profile.permissions }}</p>
          </div>

          <div class="detail-group">
            <span>Rol</span>
            <p>{{ displayRole(profile.role) }}</p>
          </div>
        </div>

        <hr class="section-line" />

        <button type="button" class="edit-btn-main" @click="startEdit">
          Editar perfil
        </button>
      </div>

      <form v-else class="profile-form" @submit.prevent="saveProfile">
        <div class="form-grid">
          <div class="form-group">
            <label>Nombre <span>*</span></label>
            <input v-model.trim="form.firstName" type="text" required />
          </div>

          <div class="form-group">
            <label>Apellido <span>*</span></label>
            <input v-model.trim="form.lastName" type="text" required />
          </div>

          <div class="form-group detail-full">
            <label>Correo</label>
            <input v-model="form.email" type="email" disabled />
            <small>El correo no se puede modificar.</small>
          </div>

          <div class="form-group">
            <label>SSN <span>*</span></label>
            <input v-model.trim="form.ssn" type="text" required />
          </div>

          <div class="form-group">
            <label>Nacionalidad <span>*</span></label>
            <input v-model.trim="form.nationality" type="text" required />
          </div>

          <template v-if="isAdmin">
            <div class="form-group">
              <label>Salario <span>*</span></label>
              <input v-model.number="form.salary" type="number" min="0" required />
            </div>

            <div class="form-group">
              <label>Horario <span>*</span></label>
              <input v-model.trim="form.workSchedule" type="text" required />
            </div>

            <div class="form-group detail-full">
              <label>Permisos <span>*</span></label>
              <input v-model.trim="form.permissions" type="text" required />
            </div>

            <div class="form-group detail-full">
              <label>Rol <span>*</span></label>
              <select v-model="form.role">
                <option value="Administrator">Administrator</option>
                <option value="Operator">Operator</option>
              </select>
            </div>
          </template>
        </div>

        <div class="edit-actions">
          <button type="submit" class="save-btn" :disabled="saving">
            {{ saving ? "Guardando..." : "Guardar cambios" }}
          </button>

          <button type="button" class="cancel-btn" @click="cancelEdit">
            Cancelar
          </button>
        </div>
      </form>
    </AdminCard>
  </AdminPageLayout>
</template>

<script>
import axios from "axios";

import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";

const BaseURL = "http://localhost:5103/api/profile";

export default {
  name: "ProfilePage",

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
      profile: {},
      form: {},
      isEditing: false,
      saving: false,
      successMessage: "",
      errorMessage: "",
      userRole: null,
    };
  },

  computed: {
    isAdmin() {
      return this.userRole === "Administrator";
    },

    isOperator() {
      return this.userRole === "Operator";
    },
  },

  mounted() {
    this.userRole = this.getRoleFromToken();
    this.loadProfile();
  },

  methods: {
    async loadProfile() {
      this.errorMessage = "";

      try {
        const token = localStorage.getItem("token");

        const response = await axios.get(`${BaseURL}/me`, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        this.profile = response.data;
      } catch (error) {
        this.errorMessage =
          error.response?.data ||
          "No se pudo cargar el perfil.";
      }
    },

    startEdit() {
      this.form = { ...this.profile };
      this.isEditing = true;
      this.successMessage = "";
      this.errorMessage = "";

      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    cancelEdit() {
      this.isEditing = false;
      this.form = {};
      this.errorMessage = "";

      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    async saveProfile() {
      if (this.saving) {
        return;
      }

      this.successMessage = "";
      this.errorMessage = "";

      const payload = this.buildProfilePayload();

      const validationMessage = this.validateProfileForm(payload);

      if (validationMessage) {
        this.errorMessage = validationMessage;
        return;
      }

      if (this.isSameProfileData(payload)) {
        this.errorMessage = "No se realizó ningún cambio porque los datos son iguales.";
        return;
      }

      this.saving = true;

      try {
        const token = localStorage.getItem("token");

        await this.delay(1200);

        await axios.put(`${BaseURL}/me`, payload, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        this.successMessage = "Perfil actualizado correctamente.";

        await this.delay(1800);

        await this.loadProfile();

        this.isEditing = false;
        this.form = {};

        window.scrollTo({ top: 0, behavior: "smooth" });
      } catch (error) {
        this.errorMessage = error.response?.data || "No se pudo actualizar el perfil.";

        await this.delay(1800);
      } finally {
        this.saving = false;
      }
    },

    getRoleFromToken() {
      const token = localStorage.getItem("token");

      if (!token) return null;

      try {
        const payload = JSON.parse(atob(token.split(".")[1]));

        return (
          payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
          payload.role ||
          payload.Role ||
          null
        );
      } catch {
        return null;
      }
    },

    displayRole(role) {
      if (role === "Administrator") return "Administrador";
      if (role === "Operator") return "Operador";
      return role || "-";
    },

    delay(milliseconds) {
      return new Promise((resolve) => {
        setTimeout(resolve, milliseconds);
      });
    },

    validateProfileForm(payload) {
      if (!payload.firstName?.trim()) {
        return "Debe ingresar el nombre.";
      }

      if (!payload.lastName?.trim()) {
        return "Debe ingresar el apellido.";
      }

      if (!payload.ssn?.trim()) {
        return "Debe ingresar el SSN.";
      }

      if (!payload.nationality?.trim()) {
        return "Debe ingresar la nacionalidad.";
      }

      if (this.isAdmin) {
        if (payload.salary === null || payload.salary === undefined || payload.salary < 0) {
          return "El salario no puede ser negativo.";
        }

        if (!payload.workSchedule?.trim()) {
          return "Debe ingresar el horario.";
        }

        if (!payload.permissions?.trim()) {
          return "Debe ingresar los permisos.";
        }

        if (payload.role !== "Administrator" && payload.role !== "Operator") {
          return "El rol seleccionado no es válido.";
        }
      }

      return "";
    },

    isSameProfileData(payload) {
      const sameBasicData =
        this.normalizeText(payload.firstName) === this.normalizeText(this.profile.firstName) &&
        this.normalizeText(payload.lastName) === this.normalizeText(this.profile.lastName) &&
        this.normalizeText(payload.ssn) === this.normalizeText(this.profile.ssn) &&
        this.normalizeText(payload.nationality) === this.normalizeText(this.profile.nationality);

      if (!this.isAdmin) {
        return sameBasicData;
      }

      return (
        sameBasicData &&
        Number(payload.salary) === Number(this.profile.salary) &&
        this.normalizeText(payload.workSchedule) === this.normalizeText(this.profile.workSchedule) &&
        this.normalizeText(payload.permissions) === this.normalizeText(this.profile.permissions) &&
        this.normalizeText(payload.role) === this.normalizeText(this.profile.role)
      );
    },

    normalizeText(value) {
      return String(value ?? "").trim().toLowerCase();
    },

    buildProfilePayload() {
      if (this.isAdmin) {
        return {
          firstName: this.form.firstName,
          lastName: this.form.lastName,
          ssn: this.form.ssn,
          nationality: this.form.nationality,
          salary: this.form.salary,
          workSchedule: this.form.workSchedule,
          permissions: this.form.permissions,
          role: this.form.role,
        };
      }

      return {
        firstName: this.form.firstName,
        lastName: this.form.lastName,
        ssn: this.form.ssn,
        nationality: this.form.nationality,

        salary: this.profile.salary ?? 0,
        workSchedule: this.profile.workSchedule || "N/A",
        permissions: this.profile.permissions || "N/A",
        role: this.profile.role || this.userRole || "Operator",
      };
    },

    formatSalary(salary) {
      if (salary === null || salary === undefined || salary === "") {
        return "-";
      }

      return `₡ ${Number(salary).toLocaleString("es-CR")}`;
    },
  },
};
</script>

<style scoped>
.success-message,
.error-message {
  display: flex;
  align-items: center;
  gap: 10px;
  border-radius: 12px;
  padding: 14px 16px;
  margin-bottom: 20px;
  font-size: 0.92rem;
  font-weight: 700;
}

.success-message {
  background: #ecfdf5;
  color: #166534;
  border: 1px solid #bbf7d0;
}

.error-message {
  background: #fef2f2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.detail-grid,
.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 24px 34px;
}

.detail-full {
  grid-column: 1 / -1;
}

.detail-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-group span,
.form-group label {
  color: #666f83;
  font-size: 0.82rem;
  font-weight: 700;
}

.detail-group p {
  margin: 0;
  color: #001233;
  font-size: 1rem;
}

.profile-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
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
}

.form-group input:disabled {
  background: #f3f4f6;
  color: #888;
  cursor: not-allowed;
}

.form-group small {
  color: #888;
  font-size: 0.78rem;
}

.section-line {
  border: none;
  border-top: 1px solid #e5e7eb;
  margin: 26px 0 18px;
}

.edit-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.edit-btn-main,
.save-btn,
.cancel-btn {
  width: 100%;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 700;
  padding: 14px 18px;
  cursor: pointer;
  transition: 0.2s ease;
}

.edit-btn-main,
.save-btn {
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
}

.cancel-btn {
  background: #f3f4f6;
  color: #1f2937;
}

.cancel-btn:hover {
  background: #e5e7eb;
}

@media (max-width: 768px) {
  .detail-grid,
  .form-grid,
  .edit-actions {
    grid-template-columns: 1fr;
  }
}
</style>
