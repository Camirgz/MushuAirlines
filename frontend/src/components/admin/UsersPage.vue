<template>
  <AdminPageLayout>
    <AdminHero
      title="Lista de Usuarios"
      subtitle="Panel de administración de Mushu Airlines"
      icon="bi bi-people"
      back-to="/admin"
      back-text="Volver al panel"
    />

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
            </tr>
          </thead>

          <tbody>
            <tr v-for="user in users" :key="user.ssn + user.email">
              <td>{{ user.fullName }}</td>
              <td>{{ user.ssn }}</td>
              <td>{{ user.email }}</td>
              <td>
                <span :class="['role-badge', roleBadgeClass(user.role)]">
                  {{ displayRole(user.role) }}
                </span>
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
      loading: false,
      errorMsg: "",
      searchInput: "",
      activeSearch: "",
      page: 1,
      totalCount: 0,
      pageSize: 10,
    };
  },

  computed: {
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

        this.users = response.data.users ?? [];
        this.totalCount = response.data.totalCount ?? 0;
      } catch (error) {
        this.errorMsg = error.response?.data || "Error al cargar usuarios.";
      } finally {
        this.loading = false;
      }
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

      return role;
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

@media (max-width: 768px) {
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
