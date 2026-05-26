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
        <i class="bi bi-search me-1"></i>
        Buscar vuelos
      </RouterLink>

      <a href="#" class="nav-link-item">
        <i class="bi bi-briefcase me-1"></i>
        Mis vuelos
      </a>

      <a href="#" class="nav-link-item">
        <i class="bi bi-calendar-check me-1"></i>
        Check-in
      </a>

      <div v-if="isInternalUser" class="management-wrapper">
        <button class="management-btn" type="button" @click="toggleDropdown">
          <i class="bi bi-gear me-1"></i>
          Gestión

          <i
            class="bi ms-1"
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
            v-if="isAdmin"
            to="/admin/aircraft-types"
            class="dropdown-item-custom"
            @click="closeDropdown"
          >
            <i class="bi bi-airplane"></i>
            <span>Tipos de aeronaves</span>
          </RouterLink>

          <RouterLink
            v-if="isAdmin"
            to="/admin/routes"
            class="dropdown-item-custom"
            @click="closeDropdown"
          >
            <i class="bi bi-geo-alt"></i>
            <span>Rutas</span>
          </RouterLink>

          <RouterLink
            v-if="isAdmin"
            to="/admin/airports"
            class="dropdown-item-custom"
            @click="closeDropdown"
          >
            <i class="bi bi-airplane-engines"></i>
            <span>Aeropuertos</span>
          </RouterLink>

          <RouterLink
            v-if="isAdmin"
            to="/admin/users"
            class="dropdown-item-custom"
            @click="closeDropdown"
          >
            <i class="bi bi-people"></i>
            <span>Usuarios administradores y operarios</span>
          </RouterLink>
        </div>
      </div>

      <button class="logout-btn" type="button" @click="logout">
        <i class="bi bi-box-arrow-right me-1"></i>
        Logout
      </button>
    </div>
  </nav>
</template>

<script>
export default {
  name: "AdminNavbar",

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

    isOperator() {
        return this.userrole === "Operator";
    },

    isInternalUser() {
      return this.isAdmin || this.isOperator;
    },
  },

  mounted() {
    this.userRole = this.getRoleFromToken();
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
.navbar {
  position: sticky;
  top: 0;
  z-index: 100;
  min-height: 66px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #ffffff;
}

.navbar-brand {
  text-decoration: none;
  color: inherit;
}

.logo-img {
  width: 42px;
  height: 42px;
  border-radius: 8px;
  object-fit: contain;
}

.brand-name {
  font-weight: 700;
  font-size: 1.05rem;
  color: #1a1a1a;
  line-height: 1.2;
}

.brand-tagline {
  font-size: 0.7rem;
  color: #888;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 24px;
}

.nav-link-item {
  text-decoration: none;
  color: #333;
  font-size: 0.92rem;
  display: flex;
  align-items: center;
  font-weight: 500;
  transition: color 0.2s ease;
}

.nav-link-item i {
  color: #555;
  transition: color 0.2s ease;
}

.nav-link-item:hover {
  color: #e74c3c;
}

.nav-link-item:hover i {
  color: #e74c3c;
}

.management-wrapper {
  position: relative;
}

.management-btn {
  padding: 8px 16px;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
  border: none;
  border-radius: 999px;
  font-size: 0.88rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.management-btn:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

.management-dropdown {
  position: absolute;
  top: 48px;
  right: 0;
  width: 320px;
  background: #ffffff;
  border: 1.5px solid #ddd;
  border-radius: 0 0 10px 10px;
  box-shadow: 0 8px 28px rgba(0, 0, 0, 0.15);
  padding: 8px;
  z-index: 200;
}

.dropdown-item-custom {
  display: flex;
  align-items: center;
  gap: 12px;
  text-decoration: none;
  color: #1a1a1a;
  padding: 12px 14px;
  border-radius: 8px;
  font-size: 0.88rem;
  font-weight: 600;
  transition: background 0.15s ease, color 0.15s ease;
}

.dropdown-item-custom i {
  color: #e74c3c;
  font-size: 0.95rem;
}

.dropdown-item-custom:hover {
  background: #fff5f5;
  color: #e74c3c;
}

.dropdown-item-custom.router-link-active,
.dropdown-item-custom.router-link-exact-active {
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
}

.dropdown-item-custom.router-link-active i,
.dropdown-item-custom.router-link-exact-active i {
  color: #ffffff;
}

.logout-btn {
  padding: 7px 16px;
  border: 1px solid #e74c3c;
  color: #e74c3c;
  border-radius: 999px;
  font-size: 0.88rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  background: #ffffff;
  cursor: pointer;
  transition: background 0.2s ease, color 0.2s ease;
}

.logout-btn:hover {
  background: #e74c3c;
  color: #ffffff;
}
</style>
