<template>
  <div class="page">

    <!-- Header banner -->
    <div class="header-banner">
      <i class="bi bi-people header-icon"></i>
      <div>
        <h1 class="header-title">Lista de Usuarios</h1>
        <p class="header-subtitle">Panel de administración de Mushu Airlines</p>
      </div>
      <RouterLink to="/admin" class="back-btn">&#8592; Volver al panel</RouterLink>
    </div>

    <!-- Main content -->
    <div class="content">
      <div class="content-panel">

        <!-- Panel top row -->
        <div class="panel-top">
          <h2 class="panel-title">Usuarios ({{ totalCount }})</h2>
          <RouterLink to="/create-profile" class="create-btn">+ Crear Usuario</RouterLink>
        </div>

        <!-- Search -->
        <div class="search-wrapper">
          <i class="bi bi-search search-icon"></i>
          <input
            v-model="searchInput"
            @keyup.enter="doSearch"
            placeholder="Buscar por nombre, SSN o correo..."
            class="search-input"
          />
        </div>

        <!-- Loading / Error -->
        <div v-if="loading" class="status-msg">Cargando...</div>
        <div v-else-if="errorMsg" class="status-msg error">{{ errorMsg }}</div>

        <!-- Table -->
        <table v-else class="users-table">
          <thead>
            <tr>
              <th>Nombre Completo</th>
              <th>SSN</th>
              <th>Correo</th>
              <th>Rol</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="users.length === 0">
              <td colspan="4" class="empty-msg">No se encontraron usuarios</td>
            </tr>
            <tr v-for="user in users" :key="user.ssn + user.email">
              <td>{{ user.fullName }}</td>
              <td>{{ user.ssn }}</td>
              <td>{{ user.email }}</td>
              <td>
                <span :class="['badge', roleBadgeClass(user.role)]">{{ user.role }}</span>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Pagination -->
        <div class="pagination" v-if="!loading && totalPages > 0">
          <button @click="changePage(page - 1)" :disabled="page === 1" class="page-btn">Anterior</button>
          <span class="page-info">Página {{ page }} de {{ totalPages }}</span>
          <button @click="changePage(page + 1)" :disabled="page >= totalPages" class="page-btn">Siguiente</button>
        </div>

      </div>
    </div>

  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "UsersPage",
  data() {
    return {
      users: [],
      loading: false,
      errorMsg: "",
      searchInput: "",
      activeSearch: "",
      page: 1,
      totalCount: 0,
      pageSize: 10
    };
  },
  computed: {
    totalPages() {
      return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
    }
  },
  methods: {
    async fetchUsers() {
      this.loading = true;
      this.errorMsg = "";
      try {
        const token = localStorage.getItem("token");
        const response = await axios.get("http://localhost:5103/api/UserList", {
          params: {
            page: this.page,
            search: this.activeSearch || undefined
          },
          headers: { Authorization: `Bearer ${token}` }
        });
        this.users = response.data.users;
        this.totalCount = response.data.totalCount;
      } catch (error) {
        this.errorMsg = error.response?.data || "Error al cargar usuarios";
      } finally {
        this.loading = false;
      }
    },
    doSearch() {
      this.activeSearch = this.searchInput;
      this.page = 1;
      this.fetchUsers();
    },
    changePage(newPage) {
      if (newPage < 1 || newPage > this.totalPages) return;
      this.page = newPage;
      this.fetchUsers();
    },
    roleBadgeClass(role) {
      if (role === "Administrador") return "badge-admin";
      if (role === "Operador") return "badge-operator";
      return "badge-unknown";
    }
  },
  mounted() {
    this.fetchUsers();
  }
};
</script>

<style scoped>
.page {
  min-height: 100vh;
  background: #f4f5f7;
}

/* Header */
.header-banner {
  background: linear-gradient(to right, #d63031, #f39c12);
  padding: 28px 40px;
  display: flex;
  align-items: center;
  gap: 20px;
}

.header-icon {
  font-size: 48px;
  color: white;
}

.header-title {
  color: white;
  font-size: 26px;
  font-weight: bold;
  margin: 0;
}

.header-subtitle {
  color: rgba(255, 255, 255, 0.85);
  font-size: 14px;
  margin: 4px 0 0;
}

/* Content */
.content {
  padding: 30px 40px;
}

.content-panel {
  background: white;
  border-radius: 12px;
  padding: 28px 32px;
  box-shadow: 0 1px 6px rgba(0, 0, 0, 0.07);
}

/* Panel top row */
.panel-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.panel-title {
  font-size: 20px;
  font-weight: bold;
  margin: 0;
  color: #1a1a1a;
}

.create-btn {
  padding: 10px 22px;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: white;
  font-weight: bold;
  text-decoration: none;
  border-radius: 8px;
  font-size: 14px;
}

/* Search */
.search-wrapper {
  display: flex;
  align-items: center;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 10px 16px;
  gap: 10px;
  margin-bottom: 24px;
}

.search-icon {
  color: #aaa;
  font-size: 15px;
}

.search-input {
  border: none;
  outline: none;
  flex: 1;
  font-size: 14px;
  color: #333;
  background: transparent;
}

/* Table */
.users-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 24px;
}

.users-table th {
  text-transform: uppercase;
  font-size: 11px;
  color: #999;
  font-weight: 600;
  letter-spacing: 0.6px;
  padding: 0 0 10px;
  border-bottom: 2px solid #eee;
  text-align: left;
}

.users-table td {
  padding: 14px 0;
  border-bottom: 1px solid #f0f0f0;
  font-size: 14px;
  color: #333;
}

.users-table tbody tr:last-child td {
  border-bottom: none;
}

.users-table tbody tr:hover td {
  background: #fafafa;
}

.empty-msg {
  text-align: center;
  color: #aaa;
  padding: 36px 0 !important;
  font-size: 14px;
}

/* Badges */
.badge {
  display: inline-block;
  padding: 4px 14px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
}

.badge-admin    { background: #fde8d8; color: #c0392b; }
.badge-operator { background: #fef3cd; color: #d68910; }
.badge-unknown  { background: #f0f0f0; color: #7f8c8d; }

/* Pagination */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  margin-bottom: 28px;
}

.page-btn {
  padding: 8px 20px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: white;
  color: #555;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
}

.page-btn:hover:not(:disabled) {
  border-color: #e74c3c;
  color: #e74c3c;
}

.page-btn:disabled {
  opacity: 0.35;
  cursor: default;
}

.page-info {
  font-size: 13px;
  color: #777;
}

/* States */
.status-msg {
  text-align: center;
  padding: 36px;
  color: #aaa;
  font-size: 14px;
  margin-bottom: 20px;
}

.status-msg.error {
  color: #e74c3c;
}

/* Back button */
.back-btn {
  margin-left: auto;
  background: white;
  color: #e74c3c;
  font-weight: bold;
  text-decoration: none;
  font-size: 14px;
  padding: 10px 20px;
  border-radius: 10px;
  white-space: nowrap;
}
</style>
