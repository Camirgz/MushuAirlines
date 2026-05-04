<template>

  <nav class="navbar bg-white shadow-sm px-4 py-2">
    <RouterLink to="/" class="navbar-brand d-flex align-items-center gap-2">
      <img 
        src="@/assets/logo.png" 
        alt="Logo Mushu Airlines" 
        class="logo-img" 
      />
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

      <!-- Management button with dropdown -->
      <div class="management-wrapper">
        <button class="management-btn" @click="toggleDropdown">
          <i class="bi bi-gear me-2"></i>
          Gestión
          <i 
            class="bi ms-2"
            :class="isDropdownOpen ? 'bi-chevron-up' : 'bi-chevron-down'"
          ></i>
        </button>

        <div v-if="isDropdownOpen" class="management-dropdown">
          <RouterLink 
            to="/admin" 
            class="dropdown-item-custom"
            @click="closeDropdown"
          >
            <i class="bi bi-grid"></i>
            <span>Página principal interna</span>
          </RouterLink>

          <RouterLink 
            to="/admin/aircraft-types" 
            class="dropdown-item-custom"
            @click="closeDropdown"
          >
            <i class="bi bi-airplane"></i>
            <span>Tipos de aeronaves</span>
          </RouterLink>

          <RouterLink 
            to="/admin/routes" 
            class="dropdown-item-custom"
            @click="closeDropdown"
          >
            <i class="bi bi-geo-alt"></i>
            <span>Rutas</span>
          </RouterLink>

          <RouterLink 
            to="/admin/airports" 
            class="dropdown-item-custom"
            @click="closeDropdown"
          >
            <i class="bi bi-airplane-engines"></i>
            <span>Aeropuertos</span>
          </RouterLink>

          <RouterLink 
            to="/admin/users" 
            class="dropdown-item-custom"
            @click="closeDropdown"
          >
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

  <div class="page">
    <h1>Lista de aeropuertos</h1>
    <p>Aquí se administrarán los aeropuertos de Mushu Airlines.</p>

    <RouterLink to="/admin" class="back-btn">
      Volver al panel
    </RouterLink>
  </div>

  <div class="create-airport">
    <button type="button" class="create-btn">
      <RouterLink to="/admin/airports/create-airports">
        Crear
      </RouterLink>
    </button>
  </div>
</template>

<script>
  export default {
    name: "AirportsPage",

    data() {
      return {
        isDropdownOpen: false,
      };
    },

    methods: {
      toggleDropdown() {
        this.isDropdownOpen = !this.isDropdownOpen;
      },

      closeDropdown() {
        this.isDropdownOpen = false;
      },
    },
  };
</script>

<style scoped>

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
  border: none;
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

.nav-link-item i {
  font-size: 1.05rem;
  color: #374151;
  transition: 0.2s ease;
}

.nav-link-item:hover {
  color: #f01818;
}

.nav-link-item:hover i {
  color: #f01818;
}

/* Management dropdown wrapper */
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

.management-btn:active {
  transform: translateY(0);
}

/* Dropdown menu */
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
  transition: 0.2s ease;
}

.dropdown-item-custom:hover {
  background: #fff4ed;
  color: #ff3b00;
}

.dropdown-item-custom.router-link-active,
.dropdown-item-custom.router-link-exact-active,
.dropdown-item-custom.active {
  background: linear-gradient(135deg, #f01818 0%, #ff5a00 45%, #ffc400 100%);
  color: #ffffff;
}

.dropdown-item-custom.router-link-active i,
.dropdown-item-custom.router-link-exact-active i,
.dropdown-item-custom.active i {
  color: #ffffff;
}

/* Logout button */
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

.page {
  padding: 40px;
}

.back-btn {
  color: #f01818;
  font-weight: 700;
}
</style>
