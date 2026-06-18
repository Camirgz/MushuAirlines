<template>
  <AdminPageLayout>
    <template v-if="isListMode">
      <AdminHero
        title="Lista de Usuarios"
        subtitle="Panel de administración de Mushu Airlines"
        icon="bi bi-people"
        back-to="/admin"
        back-text="Volver al panel"
      />

      <div v-if="successMessage" class="success-message">
        <i class="bi bi-check-circle-fill"></i>
        <span>{{ successMessage }}</span>
      </div>

      <div v-if="errorMsg" class="error-message">
        <i class="bi bi-exclamation-circle-fill"></i>
        <span>{{ errorMsg }}</span>
      </div>

      <AdminCard class="users-card">
        <div class="card-header-row">
          <h2>Usuarios ({{ totalCount }})</h2>

          <RouterLink to="/create-profile" class="create-user-btn">
            <i class="bi bi-plus-lg me-2"></i>
            Crear Usuario
          </RouterLink>
        </div>

        <div class="search-wrapper">
          <i class="bi bi-search"></i>

          <input
            v-model="searchInput"
            @keyup.enter="doSearch"
            type="text"
            placeholder="Buscar por nombre, SSN o correo..."
            aria-label="Buscar usuario"
          />

          <button type="button" class="search-btn-small" @click="doSearch">
            Buscar
          </button>
        </div>

        <div v-if="loading" class="loading-state">
          <i class="bi bi-arrow-repeat"></i>
          <p>Cargando usuarios...</p>
        </div>

        <div v-else-if="users.length > 0" class="table-wrapper">
          <table class="users-table">
            <thead>
              <tr>
                <th>Nombre Completo</th>
                <th>SSN</th>
                <th>Correo</th>
                <th>Rol</th>
                <th>Acciones</th>
              </tr>
            </thead>

            <tbody>
              <tr v-for="user in users" :key="user.id">
                <td>{{ user.fullName }}</td>
                <td>{{ user.ssn }}</td>
                <td>{{ user.email }}</td>
                <td>
                  <span :class="['role-badge', roleBadgeClass(user.role)]">
                    {{ displayRole(user.role) }}
                  </span>
                </td>
                <td>
                  <div class="actions-wrapper">
                    <button
                      type="button"
                      class="view-btn"
                      @click="openUserDetails(user)"
                    >
                      <i class="bi bi-eye me-1"></i>
                      Ver
                    </button>

                    <button
                      type="button"
                      class="edit-btn"
                      @click="openUserEdit(user)"
                    >
                      <i class="bi bi-pencil me-1"></i>
                      Editar
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="empty-state">
          <div class="empty-icon">
            <i class="bi bi-people"></i>
          </div>

          <h3>No se encontraron usuarios</h3>

          <p>
            Intente buscar por otro nombre, SSN o correo electrónico.
          </p>

          <RouterLink to="/create-profile" class="empty-create-btn">
            <i class="bi bi-plus-lg me-2"></i>
            Crear primer usuario
          </RouterLink>
        </div>

        <div class="pagination" v-if="!loading && totalPages > 0">
          <button
            type="button"
            @click="changePage(page - 1)"
            :disabled="page === 1"
            class="page-btn"
          >
            Anterior
          </button>

          <span class="page-info">Página {{ page }} de {{ totalPages }}</span>

          <button
            type="button"
            @click="changePage(page + 1)"
            :disabled="page >= totalPages"
            class="page-btn"
          >
            Siguiente
          </button>
        </div>
      </AdminCard>
    </template>

    <template v-else-if="selectedUser">
      <AdminHero
        title="Detalles del Usuario"
        subtitle="Información completa del usuario registrado"
        icon="bi bi-person-lines-fill"
      />

      <AdminCard class="details-card">
        <div class="detail-grid">
          <div class="detail-group">
            <span class="detail-label">Nombre</span>
            <p>{{ selectedUser.firstName || "-" }}</p>
          </div>

          <div class="detail-group">
            <span class="detail-label">Apellido</span>
            <p>{{ selectedUser.lastName || "-" }}</p>
          </div>
          
          <div class="detail-group detail-full">
            <span class="detail-label">Correo</span>
            <p>{{ selectedUser.email || "-" }}</p>
          </div>

          <div class="detail-group">
            <span class="detail-label">Nombre completo</span>
            <p>{{ selectedUser.fullName || "-" }}</p>
          </div>

          <div class="detail-group">
            <span class="detail-label">SSN</span>
            <p>{{ selectedUser.ssn || "-" }}</p>
          </div>


          <div class="detail-group">
            <span class="detail-label">Nacionalidad</span>
            <p>{{ selectedUser.nationality || "-" }}</p>
          </div>

          <div class="detail-group">
            <span class="detail-label">Salario</span>
            <p>{{ formatSalary(selectedUser.salary) }}</p>
          </div>

          <div class="detail-group">
            <span class="detail-label">Horario</span>
            <p>{{ selectedUser.workSchedule || "-" }}</p>
          </div>

          <div class="detail-group">
            <span class="detail-label">Rol</span>
            <span :class="['role-badge', roleBadgeClass(selectedUser.role)]">
              {{ displayRole(selectedUser.role) }}
            </span>
          </div>

          <div class="detail-group detail-full">
            <span class="detail-label">Permisos</span>
            <p>{{ selectedUser.permissions || "-" }}</p>
          </div>
        </div>

        <hr class="details-line" />

        <div class="details-actions">
          <button type="button" class="edit-details-btn" @click="openUserEdit(selectedUser)">
            Editar usuario
          </button>

          <button type="button" class="close-details-btn" @click="closeDetails">
            Cerrar
          </button>
        </div>
      </AdminCard>
    </template>

    <template v-else-if="editingUser">
      <AdminHero
        title="Editar Usuario"
        subtitle="Modificación de datos del usuario"
        icon="bi bi-pencil-square"
      />

      <div v-if="successMessage" class="success-message">
        <i class="bi bi-check-circle-fill"></i>
        <span>{{ successMessage }}</span>
      </div>

      <div v-if="errorMsg" class="error-message">
        <i class="bi bi-exclamation-circle-fill"></i>
        <span>{{ errorMsg }}</span>
      </div>

      <AdminCard class="edit-card">
        <form class="user-form" @submit.prevent="saveUserChanges">
          <div class="form-grid">
            <div class="form-group">
              <label for="firstName">Nombre <span>*</span></label>
              <input id="firstName" v-model.trim="editForm.firstName" type="text" required />
            </div>

            <div class="form-group">
              <label for="lastName">Apellido <span>*</span></label>
              <input id="lastName" v-model.trim="editForm.lastName" type="text" required />
            </div>

            <div class="form-group full-width">
              <label for="email">Correo</label>
              <input id="email" v-model="editForm.email" type="email" disabled />
              <small>El correo no se puede modificar.</small>
            </div>

            <div class="form-group">
              <label for="ssn">SSN <span>*</span></label>
              <input id="ssn" v-model.trim="editForm.ssn" type="text" required />
            </div>

            <div class="form-group">
              <label for="nationality">Nacionalidad <span>*</span></label>
              <input id="nationality" v-model.trim="editForm.nationality" type="text" required />
            </div>

            <div class="form-group">
              <label for="salary">Salario <span>*</span></label>
              <input id="salary" v-model.number="editForm.salary" type="number" min="0" required />
            </div>

            <div class="form-group">
              <label for="workSchedule">Horario <span>*</span></label>
              <input id="workSchedule" v-model.trim="editForm.workSchedule" type="text" required />
            </div>

            <div class="form-group full-width">
              <label for="permissions">Permisos <span>*</span></label>
              <input id="permissions" v-model.trim="editForm.permissions" type="text" required />
            </div>

            <div class="form-group full-width">
              <label for="role">Rol <span>*</span></label>
              <select id="role" v-model="editForm.role">
                <option value="Administrator">Administrator</option>
                <option value="Operator">Operator</option>
              </select>
            </div>
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
    </template>
  </AdminPageLayout>
</template>

<script>
import axios from "axios";

import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";

const BaseURL = "http://localhost:5103/api/UserList";

export default {
  name: "UsersPage",

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
      users: [],
      selectedUser: null,
      editingUser: null,
      editingUserId: null,
      editForm: {
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
      loading: false,
      saving: false,
      successMessage: "",
      errorMsg: "",
      searchInput: "",
      activeSearch: "",
      page: 1,
      totalCount: 0,
      pageSize: 10,
    };
  },

  computed: {
    isListMode() {
      return !this.selectedUser && !this.editingUser;
    },

    totalPages() {
      return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
    },
  },

  mounted() {
    this.fetchUsers();
  },

  methods: {
    async fetchUsers() {
      this.loading = true;
      this.errorMsg = "";

      try {
        const token = localStorage.getItem("token");

        const response = await axios.get(BaseURL, {
          params: {
            page: this.page,
            search: this.activeSearch || undefined,
          },
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });

        this.users = (response.data.users ?? []).map(this.normalizeUser);
        this.totalCount = response.data.totalCount ?? 0;
      } catch (error) {
        this.errorMsg = error.response?.data || "Error al cargar usuarios.";
      } finally {
        this.loading = false;
      }
    },

    normalizeUser(user) {
      const fullName = user.fullName ?? user.FullName ?? "";
      const firstName = user.firstName ?? user.FirstName ?? this.getFirstName(fullName);
      const lastName = user.lastName ?? user.LastName ?? this.getLastName(fullName);
      const rawRole = user.role ?? user.Role ?? "Operator";

      return {
        id: user.id ?? user.Id,
        firstName,
        lastName,
        fullName: fullName || `${firstName} ${lastName}`.trim(),
        ssn: user.ssn ?? user.Ssn ?? user.SSN ?? "",
        nationality: user.nationality ?? user.Nationality ?? "",
        salary: user.salary ?? user.Salary ?? null,
        workSchedule: user.workSchedule ?? user.WorkSchedule ?? "",
        permissions: user.permissions ?? user.Permissions ?? "",
        email: user.email ?? user.Email ?? "",
        role: this.normalizeRole(rawRole),
      };
    },

    normalizeRole(role) {
      if (role === "Administrador") {
        return "Administrator";
      }

      if (role === "Operador") {
        return "Operator";
      }

      return role || "Operator";
    },

    getFirstName(fullName) {
      if (!fullName) return "";
      return fullName.split(" ")[0] ?? "";
    },

    getLastName(fullName) {
      if (!fullName) return "";
      const parts = fullName.split(" ");
      return parts.slice(1).join(" ");
    },

    doSearch() {
      this.activeSearch = this.searchInput.trim();
      this.page = 1;
      this.fetchUsers();
    },

    changePage(newPage) {
      if (newPage < 1 || newPage > this.totalPages) {
        return;
      }

      this.page = newPage;
      this.fetchUsers();
    },

    openUserDetails(user) {
      this.selectedUser = { ...user };
      this.editingUser = null;
      this.successMessage = "";
      this.errorMsg = "";
      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    closeDetails() {
      this.selectedUser = null;
      this.errorMsg = "";
      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    openUserEdit(user) {
      this.editingUser = { ...user };
      this.selectedUser = null;
      this.successMessage = "";
      this.errorMsg = "";
      this.editingUserId = user.id;

      this.editForm = {
        firstName: user.firstName || "",
        lastName: user.lastName || "",
        ssn: user.ssn || "",
        nationality: user.nationality || "",
        salary: user.salary ?? null,
        workSchedule: user.workSchedule || "",
        permissions: user.permissions || "",
        email: user.email || "",
        role: this.normalizeRole(user.role),
      };

      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    cancelEdit() {
      this.editingUser = null;
      this.editingUserId = null;
      this.errorMsg = "";
      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    async saveUserChanges() {
      if (this.saving) {
        return;
      }

      this.successMessage = "";
      this.errorMsg = "";

      if (!this.validateEditForm()) {
        this.errorMsg = "Debe completar todos los campos requeridos antes de guardar.";
        return;
      }

      this.saving = true;

      try {
        const token = localStorage.getItem("token");

        const payload = {
          firstName: this.editForm.firstName,
          lastName: this.editForm.lastName,
          ssn: this.editForm.ssn,
          nationality: this.editForm.nationality,
          salary: this.editForm.salary,
          workSchedule: this.editForm.workSchedule,
          permissions: this.editForm.permissions,
          role: this.editForm.role,
        };

        await axios.put(
          `${BaseURL}/${encodeURIComponent(this.editingUserId)}`,
          payload,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );

        this.successMessage = "El usuario fue actualizado correctamente.";

        await this.fetchUsers();

        setTimeout(() => {
          this.editingUser = null;
          this.editingUserId = null;
          this.successMessage = "";
          window.scrollTo({ top: 0, behavior: "smooth" });
        }, 1500);
      } catch (error) {
        this.errorMsg = error.response?.data || "No se pudo actualizar el usuario.";
      } finally {
        this.saving = false;
      }
    },

    validateEditForm() {
      return (
        this.editForm.firstName.trim() &&
        this.editForm.lastName.trim() &&
        this.editForm.ssn.trim() &&
        this.editForm.nationality.trim() &&
        this.editForm.salary !== null &&
        this.editForm.salary >= 0 &&
        this.editForm.workSchedule.trim() &&
        this.editForm.permissions.trim() &&
        this.editForm.role
      );
    },

    roleBadgeClass(role) {
      if (role === "Administrator" || role === "Administrador") {
        return "badge-admin";
      }

      if (role === "Operator" || role === "Operador") {
        return "badge-operator";
      }

      return "badge-unknown";
    },

    displayRole(role) {
      if (role === "Administrator") {
        return "Administrador";
      }

      if (role === "Operator") {
        return "Operador";
      }

      return role || "-";
    },

    formatSalary(salary) {
      if (salary === null || salary === undefined || salary === "") {
        return "-";
      }

      return `$ ${Number(salary).toLocaleString("es-CR")}`;
    },
  },
};
</script>

<style scoped>
.error-message {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #fef2f2;
  color: #991b1b;
  border: 1px solid #fecaca;
  border-radius: 12px;
  padding: 14px 16px;
  margin-bottom: 20px;
  font-size: 0.92rem;
  font-weight: 700;
}

.error-message i {
  font-size: 1.1rem;
}

.users-card {
  padding: 0;
  overflow: hidden;
}

.card-header-row {
  padding: 24px 24px 14px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.card-header-row h2 {
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
}

.create-user-btn,
.empty-create-btn {
  text-decoration: none;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
  border: none;
  border-radius: 8px;
  padding: 11px 18px;
  font-size: 0.9rem;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.create-user-btn:hover,
.empty-create-btn:hover {
  color: #ffffff;
  opacity: 0.9;
  transform: translateY(-1px);
}

.search-wrapper {
  margin: 0 24px 24px;
  min-height: 44px;
  border: 1.5px solid #e0e0e0;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 13px;
  background: #ffffff;
  transition: border-color 0.2s ease;
}

.search-wrapper:focus-within {
  border-color: #e74c3c;
}

.search-wrapper i {
  color: #bbb;
  font-size: 1rem;
}

.search-wrapper input {
  width: 100%;
  height: 42px;
  border: none;
  outline: none;
  color: #333;
  font-size: 0.88rem;
}

.search-wrapper input::placeholder {
  color: #bbb;
}

.search-btn-small {
  border: none;
  border-radius: 7px;
  background: #fff0ee;
  color: #e74c3c;
  font-size: 0.82rem;
  font-weight: 700;
  padding: 8px 14px;
  cursor: pointer;
  transition: background 0.2s ease;
}

.search-btn-small:hover {
  background: #ffe1dd;
}

.loading-state {
  margin: 0 24px 24px;
  border: 1.5px dashed #e0e0e0;
  background: #f8f9fa;
  border-radius: 12px;
  padding: 34px 24px;
  text-align: center;
  color: #888;
}

.loading-state i {
  font-size: 1.8rem;
  color: #e74c3c;
  display: block;
  margin-bottom: 10px;
}

.loading-state p {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 700;
}

.table-wrapper {
  overflow-x: auto;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}

.users-table thead {
  background: #f8f9fa;
}

.users-table th {
  padding: 14px 24px;
  color: #888;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  text-align: left;
  font-weight: 700;
}

.users-table td {
  padding: 18px 24px;
  border-top: 1px solid #f0f0f0;
  color: #333;
}

.users-table tbody tr:hover {
  background: #fffafa;
}

.role-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 999px;
  padding: 5px 13px;
  font-size: 0.78rem;
  font-weight: 700;
}

.badge-admin {
  background: #fff0ee;
  color: #e74c3c;
}

.badge-operator {
  background: #fff7ed;
  color: #d97706;
}

.badge-unknown {
  background: #f3f4f6;
  color: #6b7280;
}

.empty-state {
  margin: 0 24px 24px;
  border: 1.5px dashed #e0e0e0;
  background: #f8f9fa;
  border-radius: 12px;
  padding: 34px 24px;
  text-align: center;
}

.empty-icon {
  width: 56px;
  height: 56px;
  margin: 0 auto 14px;
  border-radius: 14px;
  background: #fff0ee;
  color: #e74c3c;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.6rem;
}

.empty-state h3 {
  margin: 0 0 8px;
  font-size: 1rem;
  font-weight: 700;
  color: #333;
}

.empty-state p {
  margin: 0 auto 18px;
  max-width: 440px;
  color: #888;
  font-size: 0.92rem;
  line-height: 1.5;
}

.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 14px;
  padding: 0 24px 24px;
}

.page-btn {
  padding: 8px 18px;
  border: 1.5px solid #e0e0e0;
  border-radius: 8px;
  background: white;
  color: #555;
  font-size: 0.85rem;
  font-weight: 700;
  cursor: pointer;
  transition: border-color 0.2s ease, color 0.2s ease;
}

.page-btn:hover:not(:disabled) {
  border-color: #e74c3c;
  color: #e74c3c;
}

.page-btn:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.page-info {
  font-size: 0.85rem;
  color: #888;
}

.actions-wrapper {
  display: flex;
  align-items: center;
  gap: 14px;
}

.view-btn,
.edit-btn {
  border: none;
  background: transparent;
  font-size: 0.88rem;
  font-weight: 700;
  padding: 0;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
}

.view-btn {
  color: #e74c3c;
}

.edit-btn {
  color: #2563eb;
}

.view-btn:hover,
.edit-btn:hover {
  text-decoration: underline;
}

.view-btn:hover {
  color: #c0392b;
}

.edit-btn:hover {
  color: #1d4ed8;
}

.success-message {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #ecfdf5;
  color: #166534;
  border: 1px solid #bbf7d0;
  border-radius: 12px;
  padding: 14px 16px;
  margin-bottom: 20px;
  font-size: 0.92rem;
  font-weight: 700;
}

.success-message i {
  font-size: 1.1rem;
}

.details-card,
.edit-card {
  width: 100%;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 26px 34px;
}

.detail-full {
  grid-column: 1 / -1;
}

.detail-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-label {
  color: #666f83;
  font-size: 0.82rem;
  font-weight: 700;
}

.detail-group p {
  margin: 0;
  color: #001233;
  font-size: 1rem;
}

.details-line {
  border: none;
  border-top: 1px solid #e5e7eb;
  margin: 26px 0 18px;
}

.details-actions,
.edit-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.edit-details-btn,
.close-details-btn,
.save-btn,
.cancel-btn {
  width: 100%;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 700;
  padding: 14px 18px;
  cursor: pointer;
  transition: opacity 0.2s ease, transform 0.2s ease, background 0.2s ease;
}

.edit-details-btn,
.save-btn {
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
}

.close-details-btn,
.cancel-btn {
  background: #f3f4f6;
  color: #1f2937;
}

.edit-details-btn:hover,
.save-btn:hover:not(:disabled),
.close-details-btn:hover,
.cancel-btn:hover {
  transform: translateY(-1px);
}

.close-details-btn:hover,
.cancel-btn:hover {
  background: #e5e7eb;
}

.save-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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

.form-group input:disabled {
  background: #f3f4f6;
  color: #888;
  cursor: not-allowed;
}

.form-group small {
  color: #888;
  font-size: 0.78rem;
}

.form-group input:focus,
.form-group select:focus {
  border-color: #e74c3c;
  box-shadow: 0 0 0 3px rgba(231, 76, 60, 0.12);
}

@media (max-width: 768px) {
  .actions-wrapper {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .detail-grid,
  .form-grid {
    grid-template-columns: 1fr;
  }

  .details-actions,
  .edit-actions {
    grid-template-columns: 1fr;
  }

  .card-header-row {
    align-items: stretch;
    flex-direction: column;
  }

  .create-user-btn {
    justify-content: center;
  }

  .search-wrapper {
    flex-wrap: wrap;
    padding: 10px 13px;
  }

  .search-wrapper input {
    min-width: 100%;
  }

  .search-btn-small {
    width: 100%;
  }

  .pagination {
    flex-direction: column;
  }
}
</style>
