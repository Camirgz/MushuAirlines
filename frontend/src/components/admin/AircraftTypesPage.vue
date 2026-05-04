<template>
  <div class="admin-page">
    <!-- Navbar -->
    <nav class="navbar bg-white shadow-sm px-4 py-2">
      <RouterLink to="/" class="navbar-brand d-flex align-items-center gap-2">
        <img src="@/assets/logo.png" alt="Logo Mushu Airlines" class="logo-img" />
        <div>
          <div class="brand-name">Mushu Airlines</div>
          <div class="brand-tagline">Vuela con el dragón</div>
        </div>
      </RouterLink>

      <div class="nav-actions">
        <RouterLink to="/" class="nav-link-item">
          <i class="bi bi-search me-2"></i>
          Buscar vuelos
        </RouterLink>
        <a href="#" class="nav-link-item">
          <i class="bi bi-briefcase me-2"></i>
          Mis vuelos
        </a>
        <a href="#" class="nav-link-item">
          <i class="bi bi-calendar-check me-2"></i>
          Check-in
        </a>

        <div class="management-wrapper">
          <button class="management-btn" @click="ToggleDropdown">
            <i class="bi bi-gear me-2"></i>
            Gestión
            <i class="bi ms-2" :class="IsDropdownOpen ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
          </button>

          <div v-if="IsDropdownOpen" class="management-dropdown">
            <RouterLink to="/admin" class="dropdown-item-custom" @click="CloseDropdown">
              <i class="bi bi-grid"></i>
              <span>Página principal interna</span>
            </RouterLink>
            <RouterLink to="/admin/aircraft-types" class="dropdown-item-custom" @click="CloseDropdown">
              <i class="bi bi-airplane"></i>
              <span>Tipos de aeronaves</span>
            </RouterLink>
            <RouterLink to="/admin/routes" class="dropdown-item-custom" @click="CloseDropdown">
              <i class="bi bi-geo-alt"></i>
              <span>Rutas</span>
            </RouterLink>
            <RouterLink to="/admin/airports" class="dropdown-item-custom" @click="CloseDropdown">
              <i class="bi bi-airplane-engines"></i>
              <span>Aeropuertos</span>
            </RouterLink>
            <RouterLink to="/admin/users" class="dropdown-item-custom" @click="CloseDropdown">
              <i class="bi bi-people"></i>
              <span>Usuarios administradores y operarios</span>
            </RouterLink>
          </div>
        </div>

        <RouterLink to="/" class="logout-btn">
          <i class="bi bi-box-arrow-right me-2"></i>
          Logout
        </RouterLink>
      </div>
    </nav>

    <!-- Main content -->
    <main class="admin-main">
      <!-- Hero banner -->
      <section class="admin-hero">
        <i class="bi bi-airplane hero-icon"></i>
        <div>
          <h1>Lista de Aeronaves</h1>
          <p>Panel de administración para operadores de Mushu Airlines</p>
        </div>
      </section>

      <!-- Content card -->
      <section class="content-card">
        <div class="card-header">
          <h2>Aeronaves ({{ AircraftTypes.length }})</h2>
          <RouterLink to="/admin/aircraft-types/create" class="create-btn">
            <i class="bi bi-plus-lg me-2"></i>
            Crear Aeronave
          </RouterLink>
        </div>

        <div v-if="IsLoading" class="status-msg">
          <i class="bi bi-arrow-repeat spin me-2"></i>
          Cargando aeronaves...
        </div>

        <div v-else-if="ErrorMessage" class="error-msg">
          <i class="bi bi-exclamation-circle me-2"></i>
          {{ ErrorMessage }}
        </div>

        <table v-else class="aircraft-table">
          <thead>
            <tr>
              <th>MODELO</th>
              <th>TIPO</th>
              <th>PESO (KG)</th>
              <th>CAPACIDAD</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="Aircraft in AircraftTypes" :key="Aircraft.Id">
              <td>{{ Aircraft.Model }}</td>
              <td class="type-cell">{{ Aircraft.Type }}</td>
              <td>{{ Aircraft.WeightKg.toLocaleString() }}</td>
              <td>
                <span class="capacity-badge">{{ Aircraft.Capacity }} asientos</span>
              </td>
            </tr>
          </tbody>
        </table>
      </section>
    </main>
  </div>
</template>

<script>
import { GetAircraftTypes } from "../../services/AircraftTypesService";

export default {
  name: "AircraftTypesPage",

  data() {
    return {
      IsDropdownOpen: false,
      AircraftTypes: [],
      IsLoading: false,
      ErrorMessage: "",
    };
  },

  mounted() {
    this.LoadAircraftTypes();
  },

  methods: {
    ToggleDropdown() {
      this.IsDropdownOpen = !this.IsDropdownOpen;
    },
    CloseDropdown() {
      this.IsDropdownOpen = false;
    },
    LoadAircraftTypes() {
      this.IsLoading = true;
      this.ErrorMessage = "";

      GetAircraftTypes()
        .then((Response) => {
          this.AircraftTypes = Response.data;
        })
        .catch(() => {
          this.ErrorMessage = "No se pudieron cargar las aeronaves. Intente de nuevo.";
        })
        .finally(() => {
          this.IsLoading = false;
        });
    },
  },
};
</script>

<style scoped>
.admin-page {
  min-height: 100vh;
  background: #f8f9fa;
  color: #111827;
}

/* Navbar */
.navbar {
  position: sticky;
  top: 0;
  z-index: 100;
  min-height: 72px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #ffffff;
  border-bottom: 1px solid #e5e7eb;
}

.navbar-brand {
  text-decoration: none;
  color: inherit;
}

.logo-img {
  width: 46px;
  height: 46px;
  border-radius: 12px;
  object-fit: contain;
}

.brand-name {
  font-weight: 800;
  font-size: 1.25rem;
  color: #111827;
  line-height: 1.1;
}

.brand-tagline {
  font-size: 0.78rem;
  color: #6b7280;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 18px;
}

.nav-link-item {
  text-decoration: none;
  color: #111827;
  font-size: 0.95rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  transition: 0.2s ease;
}

.nav-link-item:hover {
  color: #f01818;
}

/* Management dropdown */
.management-wrapper {
  position: relative;
}

.management-btn {
  padding: 11px 18px;
  background: linear-gradient(135deg, #f01818 0%, #ff5a00 45%, #ffc400 100%);
  color: #ffffff;
  border: none;
  border-radius: 10px;
  font-weight: 800;
  cursor: pointer;
  display: flex;
  align-items: center;
  box-shadow: 0 8px 18px rgba(240, 24, 24, 0.22);
  transition: 0.2s ease;
}

.management-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 10px 22px rgba(240, 24, 24, 0.28);
}

.management-dropdown {
  position: absolute;
  top: 56px;
  right: 0;
  width: 340px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  box-shadow: 0 18px 38px rgba(15, 23, 42, 0.16);
  padding: 8px;
  z-index: 200;
}

.dropdown-item-custom {
  display: flex;
  align-items: center;
  gap: 12px;
  text-decoration: none;
  color: #111827;
  padding: 12px 14px;
  border-radius: 9px;
  font-size: 0.9rem;
  font-weight: 700;
  transition: 0.2s ease;
}

.dropdown-item-custom i {
  color: #ff3b00;
  font-size: 1rem;
}

.dropdown-item-custom:hover {
  background: #fff4ed;
  color: #ff3b00;
}

.dropdown-item-custom.router-link-exact-active {
  background: linear-gradient(135deg, #f01818 0%, #ff5a00 45%, #ffc400 100%);
  color: #ffffff;
}

.dropdown-item-custom.router-link-exact-active i {
  color: #ffffff;
}

/* Logout */
.logout-btn {
  text-decoration: none;
  padding: 10px 18px;
  border: 1px solid #ff4b4b;
  color: #f01818;
  border-radius: 10px;
  font-weight: 800;
  display: flex;
  align-items: center;
  background: #ffffff;
  transition: 0.2s ease;
}

.logout-btn:hover {
  background: #f01818;
  color: #ffffff;
  box-shadow: 0 8px 18px rgba(240, 24, 24, 0.18);
}

/* Main */
.admin-main {
  max-width: 1120px;
  margin: 0 auto;
  padding: 34px 24px 80px;
}

/* Hero */
.admin-hero {
  background: linear-gradient(135deg, #f00000 0%, #ff4b00 45%, #ffc400 100%);
  color: #ffffff;
  border-radius: 16px;
  padding: 34px 38px;
  display: flex;
  align-items: center;
  gap: 22px;
  box-shadow: 0 18px 32px rgba(15, 23, 42, 0.16);
  margin-bottom: 32px;
}

.hero-icon {
  font-size: 2.6rem;
}

.admin-hero h1 {
  font-size: 1.95rem;
  font-weight: 900;
  margin: 0 0 6px;
}

.admin-hero p {
  margin: 0;
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.95);
}

/* Content card */
.content-card {
  background: #ffffff;
  border-radius: 16px;
  padding: 32px;
  box-shadow: 0 16px 36px rgba(15, 23, 42, 0.08);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 28px;
}

.card-header h2 {
  font-size: 1.35rem;
  font-weight: 900;
  color: #07172c;
  margin: 0;
}

.create-btn {
  text-decoration: none;
  padding: 11px 22px;
  background: linear-gradient(135deg, #f01818 0%, #ff5a00 45%, #ffc400 100%);
  color: #ffffff;
  border-radius: 10px;
  font-weight: 800;
  display: flex;
  align-items: center;
  box-shadow: 0 8px 18px rgba(240, 24, 24, 0.22);
  transition: 0.2s ease;
}

.create-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 10px 22px rgba(240, 24, 24, 0.3);
}

/* Table */
.aircraft-table {
  width: 100%;
  border-collapse: collapse;
}

.aircraft-table thead tr {
  border-bottom: 2px solid #e5e7eb;
}

.aircraft-table th {
  text-align: left;
  font-size: 0.78rem;
  font-weight: 700;
  color: #6b7280;
  letter-spacing: 0.05em;
  padding: 0 16px 14px 0;
}

.aircraft-table tbody tr {
  border-bottom: 1px solid #f3f4f6;
  transition: background 0.15s ease;
}

.aircraft-table tbody tr:last-child {
  border-bottom: none;
}

.aircraft-table tbody tr:hover {
  background: #fffaf5;
}

.aircraft-table td {
  padding: 18px 16px 18px 0;
  font-size: 0.95rem;
  color: #111827;
}

.type-cell {
  color: #ff5a00;
  font-weight: 600;
}

.capacity-badge {
  display: inline-block;
  padding: 5px 12px;
  background: #fff3e0;
  color: #e65c00;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 700;
}

.status-msg {
  padding: 24px 0;
  color: #6b7280;
  font-size: 0.95rem;
  display: flex;
  align-items: center;
}

.error-msg {
  padding: 16px 20px;
  background: #fff1f1;
  color: #b91c1c;
  border: 1px solid #fecaca;
  border-radius: 10px;
  font-size: 0.95rem;
  font-weight: 600;
  display: flex;
  align-items: center;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to   { transform: rotate(360deg); }
}

.spin {
  display: inline-block;
  animation: spin 0.8s linear infinite;
}
</style>
