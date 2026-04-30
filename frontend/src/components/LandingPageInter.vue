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
      <section class="admin-card">
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

</style>
