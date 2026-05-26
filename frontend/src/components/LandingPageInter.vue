<template>
  <div class="admin-page">
    <!-- Navbar -->
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
        <div v-if="isAdmin" class="management-wrapper">
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
        <button class="logout-btn" @click="logout">
          <i class="bi bi-box-arrow-right me-2"></i>
          Logout
        </button>
      </div>
    </nav>

    <!-- Main content -->
    <main class="admin-main">
      <!-- Banner -->
      <section class="admin-hero">
        <i class="bi bi-airplane hero-icon"></i>
        <div>
          <h1>Panel de administración</h1>
          <p>Panel de administración para operadores de Mushu Airlines</p>
        </div>
      </section>

      <!-- Panel -->
      <section v-if="isAdmin" class="admin-card">
        <h2>Administración de vuelos de la aerolínea</h2>

        <div class="option-list">
          <RouterLink to="/admin/aircraft-types" class="admin-option">
            <div class="option-left">
              <div class="option-icon">
                <i class="bi bi-airplane"></i>
              </div>
              <span>Tipos de aeronaves</span>
            </div>

            <i class="bi bi-chevron-right option-arrow"></i>
          </RouterLink>

          <RouterLink to="/admin/routes" class="admin-option">
            <div class="option-left">
              <div class="option-icon">
                <i class="bi bi-geo-alt"></i>
              </div>
              <span>Rutas</span>
            </div>

            <i class="bi bi-chevron-right option-arrow"></i>
          </RouterLink>

          <RouterLink to="/admin/airports" class="admin-option">
            <div class="option-left">
              <div class="option-icon">
                <i class="bi bi-airplane-engines"></i>
              </div>
              <span>Aeropuertos</span>
            </div>

            <i class="bi bi-chevron-right option-arrow"></i>
          </RouterLink>
        </div>

        <hr class="section-line" />

        <h2>Administración de usuarios de la aerolínea</h2>

        <div class="option-list">
          <RouterLink to="/admin/users" class="admin-option">
            <div class="option-left">
              <div class="option-icon">
                <i class="bi bi-people"></i>
              </div>
              <span>Usuarios administradores y operarios</span>
            </div>

            <i class="bi bi-chevron-right option-arrow"></i>
          </RouterLink>
        </div>

        <hr class="section-line" />

        <h2>Sistema de reportes</h2>

        <div class="empty-reports">
          <div>
            <h3>Reportes no disponibles por el momento</h3>
            <p>
              Esta sección estará disponible en una futura actualización del sistema.
            </p>
          </div>
        </div>
      </section>
    </main>
  </div>
</template>

<script>
export default {
  name: "LandingPageInter",

  data() {
    return {
      isDropdownOpen: false,
      userRole: null,
    };
  },

  computed: {
    isAdmin() {
      return this.userRole === "Administrator";
    },
  },

  mounted() {
    this.userRole = this.getRoleFromToken();

    console.log("Rol actual:", this.userRole);
  },

  methods: {
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
      } catch (error) {
        console.error("Error leyendo el token:", error);
        return null;
      }
    },

    toggleDropdown() {
      this.isDropdownOpen = !this.isDropdownOpen;
    },

    closeDropdown() {
      this.isDropdownOpen = false;
    },

    logout() {
      localStorage.removeItem("token");
      this.$router.push("/");
    },
  },
};
</script>

<style scoped>
/* General page layout */
.admin-page {
  min-height: 100vh;
  background: var(--bg-page);
  color: var(--text-dark);
}

/* Navbar */
.navbar {
  position: sticky;
  top: 0;
  z-index: 100;
  min-height: var(--navbar-min-height);
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: var(--navbar-bg);
  border-bottom: var(--navbar-border);
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
  color: var(--text-dark);
  line-height: 1.1;
}

.brand-tagline {
  font-size: 0.78rem;
  color: var(--text-medium);
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 18px;
}

.nav-link-item {
  text-decoration: none;
  color: var(--text-dark);
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

.nav-link-item:hover,
.nav-link-item:hover i {
  color: var(--color-primary-hover);
}

/* Management dropdown wrapper */
.management-wrapper {
  position: relative;
}

.management-btn {
  padding: 11px 18px;
  background: var(--gradient-brand-diagonal);
  color: #ffffff;
  border: none;
  border-radius: var(--radius-btn);
  font-weight: 800;
  cursor: pointer;
  display: flex;
  align-items: center;
  box-shadow: var(--shadow-btn-primary);
  transition: 0.2s ease;
}

.management-btn:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-btn-primary-hover);
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
  background: var(--bg-card);
  border: 1px solid var(--border-color);
  border-radius: var(--radius-panel);
  box-shadow: var(--shadow-dropdown);
  padding: 8px;
  z-index: 200;
}

.dropdown-item-custom {
  display: flex;
  align-items: center;
  gap: 12px;
  text-decoration: none;
  color: var(--text-dark);
  padding: 12px 14px;
  border-radius: 9px;
  font-size: 0.9rem;
  font-weight: 700;
  transition: 0.2s ease;
}

.dropdown-item-custom i {
  color: var(--color-accent-soft);
  font-size: 1rem;
  transition: 0.2s ease;
}

.dropdown-item-custom:hover {
  background: var(--bg-dropdown-hover);
  color: var(--color-accent-soft);
}

.dropdown-item-custom.router-link-active,
.dropdown-item-custom.router-link-exact-active,
.dropdown-item-custom.active {
  background: var(--gradient-brand-diagonal);
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
  color: var(--color-primary-hover);
  border-radius: var(--radius-btn);
  font-weight: 800;
  display: flex;
  align-items: center;
  background: var(--bg-card);
  transition: 0.2s ease;
}

.logout-btn:hover {
  background: var(--color-primary-hover);
  color: #ffffff;
  box-shadow: 0 8px 18px rgba(240, 24, 24, 0.18);
}

/* Main content container */
.admin-main {
  max-width: var(--content-max-width);
  margin: 0 auto;
  padding: var(--content-padding);
}

/* Hero banner */
.admin-hero {
  background: var(--gradient-hero);
  color: #ffffff;
  border-radius: var(--radius-card);
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

/* Main admin card */
.admin-card {
  background: var(--bg-card);
  border-radius: var(--radius-card);
  padding: 32px;
  box-shadow: var(--shadow-card);
}

.admin-card h2 {
  font-size: 1.35rem;
  font-weight: 900;
  color: #07172c;
  margin-bottom: 18px;
}

.option-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.admin-option {
  text-decoration: none;
  color: var(--text-dark);
  border: 1px solid #e2e8f0;
  border-radius: var(--radius-btn);
  padding: 16px 18px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--bg-card);
  transition: 0.2s ease;
}

.admin-option:hover {
  border-color: var(--color-accent-warm);
  background: #fffaf0;
  box-shadow: 0 8px 18px rgba(255, 122, 0, 0.12);
  transform: translateY(-1px);
}

.admin-option.router-link-active,
.admin-option.router-link-exact-active {
  border-color: var(--color-accent-warm);
  background: #fffaf0;
}

.option-left {
  display: flex;
  align-items: center;
  gap: 13px;
  font-weight: 800;
}

.option-icon {
  width: 38px;
  height: 38px;
  background: var(--gradient-brand-diagonal);
  color: #ffffff;
  border-radius: 9px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1rem;
  flex-shrink: 0;
}

.option-arrow {
  color: #94a3b8;
  transition: 0.2s ease;
}

.admin-option:hover .option-arrow {
  color: var(--color-accent-warm);
  transform: translateX(3px);
}

.section-line {
  border: none;
  border-top: 1px solid var(--border-color);
  margin: 26px 0;
}

/* Empty reports section */
.empty-reports {
  border: 1px dashed #cbd5e1;
  background: #f8fafc;
  border-radius: var(--radius-panel);
  padding: 22px 24px;
}

.empty-reports h3 {
  margin: 0 0 6px;
  font-size: 1rem;
  font-weight: 800;
  color: #374151;
}

.empty-reports p {
  margin: 0;
  color: var(--text-medium);
  font-size: 0.92rem;
  line-height: 1.5;
}
</style>
