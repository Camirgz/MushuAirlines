<template>
  <div class="airports-page">
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

        <div class="management-wrapper">
          <button class="management-btn" type="button" @click="toggleDropdown">
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

    <main class="airports-main">
      <!-- Airport list view -->
      <template v-if="!selectedAirport">
        <section class="airports-hero">
          <RouterLink to="/admin" class="hero-back-link">
            <i class="bi bi-arrow-left"></i>
            Volver al panel
          </RouterLink>

          <div class="hero-content">
            <i class="bi bi-airplane-engines hero-icon"></i>

            <div>
              <h1>Lista de Aeropuertos</h1>
              <p>Panel de administración para operadores de Mushu Airlines</p>
            </div>
          </div>
        </section>

        <div v-if="successMessage" class="success-message">
          <i class="bi bi-check-circle-fill"></i>
          <span>{{ successMessage }}</span>
        </div>

        <section class="airports-card">
          <div class="card-header-row">
            <h2>Aeropuertos ({{ filteredAirports.length }})</h2>

            <RouterLink
              to="/admin/airports/create-airport"
              class="create-airport-btn"
            >
              <i class="bi bi-plus-lg me-2"></i>
              Crear Aeropuerto
            </RouterLink>
          </div>

          <div class="search-wrapper">
            <i class="bi bi-search"></i>
            <input
              v-model="searchText"
              type="text"
              placeholder="Buscar por nombre o código..."
              aria-label="Buscar aeropuerto"
            />
          </div>

          <div v-if="filteredAirports.length > 0" class="table-wrapper">
            <table class="airports-table">
              <thead>
                <tr>
                  <th>Nombre</th>
                  <th>Código</th>
                  <th>Ciudad</th>
                  <th>Acciones</th>
                </tr>
              </thead>

              <tbody>
                <tr v-for="airport in filteredAirports" :key="airport.code">
                  <td>{{ airport.airportName }}</td>
                  <td>
                    <span class="airport-code">{{ airport.code }}</span>
                  </td>
                  <td>{{ airport.city }}</td>
                  <td>
                    <button
                      type="button"
                      class="view-btn"
                      @click="openAirportDetails(airport)"
                    >
                      <i class="bi bi-eye me-1"></i>
                      Ver
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <div v-else class="empty-state">
            <div class="empty-icon">
              <i class="bi bi-airplane-engines"></i>
            </div>

            <h3>{{ emptyTitle }}</h3>
            <p>{{ emptyDescription }}</p>

            <RouterLink
              to="/admin/airports/create-airport"
              class="empty-create-btn"
            >
              <i class="bi bi-plus-lg me-2"></i>
              Crear primer aeropuerto
            </RouterLink>
          </div>
        </section>
      </template>

      <!-- Airport details view -->
      <template v-else>
        <section class="details-hero">
          <button type="button" class="hero-back-link hero-back-button" @click="closeDetails">
            <i class="bi bi-arrow-left"></i>
            Volver a la lista
          </button>

          <div class="hero-content">
            <i class="bi bi-airplane-engines hero-icon"></i>

            <div>
              <h1>Detalles del Aeropuerto</h1>
              <p>Información completa del aeropuerto</p>
            </div>
          </div>
        </section>

        <section class="details-card">
          <div class="detail-group detail-full">
            <span class="detail-label">Nombre del Aeropuerto</span>
            <p>{{ selectedAirport.airportName }}</p>
          </div>

          <div class="detail-grid">
            <div class="detail-group">
              <span class="detail-label">País</span>
              <p>{{ selectedAirport.country }}</p>
            </div>

            <div class="detail-group">
              <span class="detail-label">Ciudad</span>
              <p>{{ selectedAirport.city }}</p>
            </div>
          </div>

          <div class="detail-group detail-full">
            <span class="detail-label">Código del Aeropuerto</span>
            <span class="airport-code airport-code-large">{{ selectedAirport.code }}</span>
          </div>

          <hr class="details-line" />

          <button type="button" class="close-details-btn" @click="closeDetails">
            Cerrar
          </button>
        </section>
      </template>
    </main>
  </div>
</template>

<script>
export default {
  name: "AirportsPage",

  data() {
    return {
      isDropdownOpen: false,
      searchText: "",
      selectedAirport: null,
      airports: [],
      loading: false,
    };
  },

  computed: {
    filteredAirports() {
      const text = this.searchText.trim().toLowerCase();

      if (!text) {
        return this.airports;
      }

      return this.airports.filter((airport) => {
        return (
          airport.airportName.toLowerCase().includes(text) ||
          airport.code.toLowerCase().includes(text) ||
          airport.city.toLowerCase().includes(text) ||
          airport.country.toLowerCase().includes(text)
        );
      });
    },

    emptyTitle() {
      return this.airports.length === 0
        ? "No hay aeropuertos creados"
        : "No se encontraron aeropuertos";
    },

    emptyDescription() {
      return this.airports.length === 0
        ? "Cuando registre aeropuertos, aparecerán en esta lista."
        : "Intente buscar por otro nombre o código.";
    },
  },

  mounted() {
    this.loadAirports();
  },

  methods: {
    toggleDropdown() {
      this.isDropdownOpen = !this.isDropdownOpen;
    },

    closeDropdown() {
      this.isDropdownOpen = false;
    },

    async loadAirports() {
      this.loading = true;

      try {
        const response = await fetch("http://localhost:5103/api/AirportCreation");

        if (!response.ok) {
          throw new Error("No se pudieron cargar los aeropuertos.");
        }

        this.airports = await response.json();
      } catch (error) {
        console.error("Error loading airports:", error);
      } finally {
        this.loading = false;
      }
    },

    openAirportDetails(airport) {
      this.selectedAirport = airport;
      this.searchText = "";
      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    closeDetails() {
      this.selectedAirport = null;
      window.scrollTo({ top: 0, behavior: "smooth" });
    },
  },
};
</script>

<style scoped>
/* General page layout */
.airports-page {
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

.nav-link-item:hover,
.nav-link-item:hover i {
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
  transition: 0.2s ease;
}

.dropdown-item-custom:hover {
  background: #fff4ed;
  color: #ff3b00;
}

.dropdown-item-custom.router-link-active,
.dropdown-item-custom.router-link-exact-active {
  background: linear-gradient(135deg, #f01818 0%, #ff5a00 45%, #ffc400 100%);
  color: #ffffff;
}

.dropdown-item-custom.router-link-active i,
.dropdown-item-custom.router-link-exact-active i {
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

/* Main content */
.airports-main {
  max-width: 1120px;
  margin: 0 auto;
  padding: 34px 24px 80px;
}

.airports-hero,
.details-hero {
  background: linear-gradient(135deg, #f00000 0%, #ff4b00 45%, #ffc400 100%);
  color: #ffffff;
  border-radius: 16px;
  padding: 32px 38px;
  box-shadow: 0 18px 32px rgba(15, 23, 42, 0.16);
  margin-bottom: 32px;
}

.details-hero {
  max-width: 672px;
  margin-left: auto;
  margin-right: auto;
}

.hero-content {
  display: flex;
  align-items: center;
  gap: 22px;
}

.hero-icon {
  font-size: 2.6rem;
}

.airports-hero h1,
.details-hero h1 {
  font-size: 1.95rem;
  font-weight: 900;
  margin: 0 0 6px;
}

.airports-hero p,
.details-hero p {
  margin: 0;
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.95);
}

.hero-back-link {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 18px;
  color: #ffffff;
  text-decoration: none;
  font-weight: 800;
  font-size: 0.95rem;
}

.hero-back-link:hover {
  color: #ffffff;
  opacity: 0.88;
}

.hero-back-button {
  background: transparent;
  border: none;
  padding: 0;
  cursor: pointer;
}

/* Success message */
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
  font-weight: 800;
}

.success-message i {
  font-size: 1.1rem;
}

/* List card */
.airports-card {
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 16px 36px rgba(15, 23, 42, 0.08);
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
  font-size: 1.35rem;
  font-weight: 900;
  color: #07172c;
}

.create-airport-btn,
.empty-create-btn {
  text-decoration: none;
  background: #ff5a00;
  color: #ffffff;
  border: none;
  border-radius: 10px;
  padding: 11px 18px;
  font-weight: 800;
  display: inline-flex;
  align-items: center;
  transition: 0.2s ease;
}

.create-airport-btn:hover,
.empty-create-btn:hover {
  color: #ffffff;
  background: #f01818;
  transform: translateY(-1px);
  box-shadow: 0 8px 18px rgba(240, 24, 24, 0.18);
}

.search-wrapper {
  margin: 0 24px 24px;
  height: 42px;
  border: 1px solid #cbd5e1;
  border-radius: 9px;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 13px;
  background: #ffffff;
}

.search-wrapper i {
  color: #94a3b8;
  font-size: 1rem;
}

.search-wrapper input {
  width: 100%;
  height: 100%;
  border: none;
  outline: none;
  color: #111827;
  font-size: 0.95rem;
}

.search-wrapper input::placeholder {
  color: #7b8290;
}

.table-wrapper {
  overflow-x: auto;
}

.airports-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}

.airports-table thead {
  background: #f8fafc;
}

.airports-table th {
  padding: 14px 24px;
  color: #64748b;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  text-align: left;
}

.airports-table td {
  padding: 18px 24px;
  border-top: 1px solid #e5e7eb;
  color: #061126;
}

.airport-code {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: #ffedd5;
  color: #c2410c;
  border-radius: 5px;
  padding: 4px 9px;
  font-size: 0.78rem;
  font-weight: 700;
}

.airport-code-large {
  font-size: 1rem;
  padding: 8px 13px;
  margin-top: 4px;
}

.view-btn {
  border: none;
  background: transparent;
  color: #ff3b00;
  font-weight: 800;
  padding: 0;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
}

.view-btn:hover {
  color: #f01818;
  text-decoration: underline;
}

/* Empty state */
.empty-state {
  margin: 0 24px 24px;
  border: 1px dashed #cbd5e1;
  background: #f8fafc;
  border-radius: 12px;
  padding: 34px 24px;
  text-align: center;
}

.empty-icon {
  width: 56px;
  height: 56px;
  margin: 0 auto 14px;
  border-radius: 14px;
  background: #fff4ed;
  color: #ff3b00;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.6rem;
}

.empty-state h3 {
  margin: 0 0 8px;
  font-size: 1.1rem;
  font-weight: 900;
  color: #111827;
}

.empty-state p {
  margin: 0 auto 18px;
  max-width: 440px;
  color: #6b7280;
  line-height: 1.5;
}

/* Details card */
.details-card {
  max-width: 672px;
  margin: 0 auto;
  background: #ffffff;
  border-radius: 16px;
  padding: 32px;
  box-shadow: 0 16px 36px rgba(15, 23, 42, 0.08);
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 34px;
  margin: 26px 0;
}

.detail-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-label {
  color: #64748b;
  font-size: 0.88rem;
  font-weight: 800;
}

.detail-group p {
  margin: 0;
  color: #061126;
  font-size: 1.05rem;
}

.details-line {
  border: none;
  border-top: 1px solid #e5e7eb;
  margin: 24px 0 16px;
}

.close-details-btn {
  width: 100%;
  border: none;
  border-radius: 10px;
  background: #f1f3f6;
  color: #111827;
  font-weight: 800;
  padding: 14px 18px;
  cursor: pointer;
  transition: 0.2s ease;
}

.close-details-btn:hover {
  background: #e5e7eb;
}

@media (max-width: 900px) {
  .navbar {
    align-items: flex-start;
    gap: 16px;
  }

  .nav-actions {
    flex-wrap: wrap;
    justify-content: flex-end;
    gap: 12px;
  }
}

@media (max-width: 768px) {
  .navbar {
    padding: 14px 18px !important;
    flex-direction: column;
  }

  .nav-actions {
    width: 100%;
    justify-content: flex-start;
  }

  .management-dropdown {
    left: 0;
    right: auto;
    width: min(340px, 90vw);
  }

  .airports-main {
    padding: 24px 16px 60px;
  }

  .airports-hero,
  .details-hero {
    padding: 26px;
  }

  .hero-content {
    align-items: flex-start;
  }

  .hero-icon {
    font-size: 2.1rem;
  }

  .airports-hero h1,
  .details-hero h1 {
    font-size: 1.55rem;
  }

  .card-header-row {
    align-items: stretch;
    flex-direction: column;
  }

  .create-airport-btn {
    justify-content: center;
  }

  .detail-grid {
    grid-template-columns: 1fr;
    gap: 22px;
  }
}
</style>
