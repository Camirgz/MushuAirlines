<template>
  <div class="create-airport-page">
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

    <main class="create-airport-main">
      <!-- Banner -->
      <section class="create-airport-hero">
        <RouterLink to="/admin/airports" class="hero-back-link">
          <i class="bi bi-arrow-left"></i>
          Volver a la lista
        </RouterLink>

        <div class="hero-content">
          <i class="bi bi-airplane-engines hero-icon"></i>

          <div>
            <h1>Gestión de aeropuertos</h1>
            <p>Panel de administración para operadores de Mushu Airlines</p>
          </div>
        </div>
      </section>

      <!-- Form card -->
      <section class="create-airport-card">
        <h2>Crear Aeropuerto</h2>

        <div
          v-if="alertMessage"
          class="form-alert"
          :class="alertType === 'success' ? 'success-alert' : 'warning-alert'"
        >
          <i
            class="bi"
            :class="alertType === 'success' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill'"
          ></i>

          <div>
            <strong>{{ alertTitle }}</strong>
            <p>{{ alertMessage }}</p>
          </div>
        </div>

        <form class="airport-form" novalidate @submit.prevent="createAirport">
          <div class="form-group" :class="{ 'has-error': submitted && errors.country }">
            <label for="country">País <span>*</span></label>
            <select
            v-model="form.country"
            @change="loadCities"
            >
            <option value="">Selecciona un país</option>

            <option
                v-for="country in countries"
                :key="country"
                :value="country"
            >
                {{ country }}
            </option>
            </select>
            <small v-if="submitted && errors.country" class="error-text">
              Debe seleccionar un país.
            </small>
          </div>

          <div class="form-group" :class="{ 'has-error': submitted && errors.city }">
            <label for="city">Ciudad <span>*</span></label>
            <select
            v-model="form.city"
            :disabled="!form.country"
            >
            <option value="">Selecciona una ciudad</option>

            <option
                v-for="city in cities"
                :key="city"
                :value="city"
            >
                {{ city }}
            </option>
            </select>
            <small v-if="submitted && errors.city" class="error-text">
              Debe seleccionar una ciudad.
            </small>
          </div>

          <div class="form-group" :class="{ 'has-error': submitted && errors.name }">
            <label for="airportName">Nombre del Aeropuerto <span>*</span></label>
            <input
              id="airportName"
              v-model.trim="form.airportName"
              type="text"
              maxlength="200"
              placeholder="Ej: Aeropuerto Internacional Benito Juárez"
            />

            <div class="helper-row">
              <small>{{ airportNameLength }}/200 caracteres</small>
              <small v-if="submitted && errors.name" class="error-text">
                Debe ingresar el nombre del aeropuerto.
              </small>
            </div>
          </div>

          <div class="form-group" :class="{ 'has-error': submitted && errors.code }">
            <label for="airportCode">Código del Aeropuerto <span>*</span></label>
            <input
              id="airportCode"
              v-model="form.code"
              type="text"
              maxlength="3"
              placeholder="EJ: MEX"
              @input="formatCode"
            />

            <div class="helper-row">
              <small>3 caracteres en mayúsculas (Ej: MEX, MAD, JFK)</small>
              <small v-if="submitted && errors.code" class="error-text">
                El código debe tener exactamente 3 letras.
              </small>
            </div>
          </div>

          <button class="submit-btn" type="submit" :disabled="isSubmitting">
            <span v-if="!isSubmitting">Crear Aeropuerto</span>
            <span v-else>
              <i class="bi bi-hourglass-split me-2"></i>
              Creando...
            </span>
          </button>
        </form>
      </section>
    </main>
  </div>
</template>

<script>
export default {
  name: "AirportCreationForm",

  data() {
    return {
      isDropdownOpen: false,

      countries: [],
      cities: [],

      form: {
        country: "",
        city: "",
        airportName: "",
        code: "",
      },

      errors: {},
      submitted: false,

      alertTitle: "",
      alertMessage: "",
      alertType: "",

      isSubmitting: false,
    };
  },

  computed: {
    airportNameLength() {
      return this.form.airportName.length;
    },
  },

  mounted() {
    this.loadCountries();
  },

  methods: {
    toggleDropdown() {
      this.isDropdownOpen = !this.isDropdownOpen;
    },

    closeDropdown() {
      this.isDropdownOpen = false;
    },

    async loadCountries() {
      try {
        const response = await fetch("http://localhost:5103/api/AirportCreation/countries");

        if (!response.ok) {
          throw new Error("No se pudieron cargar los países.");
        }

        this.countries = await response.json();
      } catch (error) {
        this.alertType = "warning";
        this.alertTitle = "Error de conexión";
        this.alertMessage = "No se pudieron cargar los países. Revise si el backend está ejecutándose.";
      }
    },

    async loadCities() {
      this.form.city = "";
      this.cities = [];

      if (!this.form.country) {
        return;
      }

      try {
        const response = await fetch(
          `http://localhost:5103/api/AirportCreation/cities?country=${encodeURIComponent(this.form.country)}`
        );

        if (!response.ok) {
          throw new Error("No se pudieron cargar las ciudades.");
        }

        this.cities = await response.json();
      } catch (error) {
        this.alertType = "warning";
        this.alertTitle = "Error de conexión";
        this.alertMessage = "No se pudieron cargar las ciudades.";
      }
    },

    formatCode() {
      this.form.code = this.form.code
        .toUpperCase()
        .replace(/[^A-Z]/g, "")
        .slice(0, 3);
    },

    validateForm() {
      this.errors = {};

      if (!this.form.country) {
        this.errors.country = true;
      }

      if (!this.form.city) {
        this.errors.city = true;
      }

      if (!this.form.airportName.trim()) {
        this.errors.name = true;
      }

      if (!this.form.code || this.form.code.length !== 3) {
        this.errors.code = true;
      }

      return Object.keys(this.errors).length === 0;
    },

    async createAirport() {
      this.submitted = true;
      this.alertTitle = "";
      this.alertMessage = "";
      this.alertType = "";

      if (!this.validateForm()) {
        this.alertType = "warning";
        this.alertTitle = "Formulario incompleto";
        this.alertMessage = "Debe completar los campos requeridos antes de crear el aeropuerto.";
        return;
      }

      this.isSubmitting = true;

      try {
        const response = await fetch("http://localhost:5103/api/AirportCreation", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            code: this.form.code,
            airportName: this.form.airportName,
            country: this.form.country,
            city: this.form.city,
          }),
        });

        const message = await response.text();

        if (!response.ok) {
          this.alertType = "warning";
          this.alertTitle = "No se pudo crear el aeropuerto";
          this.alertMessage = message;
          return;
        }

        this.alertType = "success";
        this.alertTitle = "Aeropuerto creado";
        this.alertMessage = "El aeropuerto fue creado correctamente.";

        setTimeout(() => {
          this.$router.push("/admin/airports");
        }, 1500);
      } catch (error) {
        this.alertType = "warning";
        this.alertTitle = "Error de conexión";
        this.alertMessage = "No se pudo conectar con el backend.";
      } finally {
        this.isSubmitting = false;
      }
    },
  },
};
</script>

<style scoped>
/* General page layout */
.create-airport-page {
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
.create-airport-main {
  max-width: 1120px;
  margin: 0 auto;
  padding: 34px 24px 80px;
}

.create-airport-hero {
  max-width: 672px;
  margin: 0 auto 32px;
  background: linear-gradient(135deg, #f00000 0%, #ff4b00 45%, #ffc400 100%);
  color: #ffffff;
  border-radius: 16px;
  padding: 32px 38px;
  box-shadow: 0 18px 32px rgba(15, 23, 42, 0.16);
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

.hero-content {
  display: flex;
  align-items: center;
  gap: 22px;
}

.hero-icon {
  font-size: 2.6rem;
}

.create-airport-hero h1 {
  font-size: 1.95rem;
  font-weight: 900;
  margin: 0 0 6px;
}

.create-airport-hero p {
  margin: 0;
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.95);
}

/* Form card */
.create-airport-card {
  max-width: 672px;
  margin: 0 auto;
  background: #ffffff;
  border-radius: 16px;
  padding: 32px;
  box-shadow: 0 16px 36px rgba(15, 23, 42, 0.08);
}

.create-airport-card h2 {
  font-size: 1.45rem;
  font-weight: 900;
  color: #07172c;
  margin: 0 0 22px;
}

.airport-form {
  display: flex;
  flex-direction: column;
  gap: 22px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 0.88rem;
  font-weight: 800;
  color: #1f2937;
}

.form-group label span {
  color: #f01818;
}

.form-group input,
.form-group select {
  width: 100%;
  height: 48px;
  border: 1px solid #cbd5e1;
  border-radius: 10px;
  padding: 0 16px;
  font-size: 0.95rem;
  color: #111827;
  background: #ffffff;
  outline: none;
  transition: 0.2s ease;
}

.form-group input::placeholder {
  color: #9ca3af;
}

.form-group input:focus,
.form-group select:focus {
  border-color: #ff5a00;
  box-shadow: 0 0 0 3px rgba(255, 90, 0, 0.12);
}

.form-group select:disabled {
  background: #f3f4f6;
  cursor: not-allowed;
  color: #9ca3af;
}

.form-group.has-error input,
.form-group.has-error select {
  border-color: #f01818;
  background: #fff7f7;
}

.helper-row {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 4px;
  min-height: 18px;
}

.helper-row small,
.form-group small {
  color: #64748b;
  font-size: 0.78rem;
}

.error-text {
  color: #f01818 !important;
  font-weight: 700;
  text-align: left;
}

.form-alert {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  border-radius: 12px;
  padding: 14px 16px;
  margin-bottom: 22px;
  border: 1px solid transparent;
}

.form-alert i {
  font-size: 1.25rem;
  margin-top: 1px;
}

.form-alert strong {
  display: block;
  font-size: 0.9rem;
  font-weight: 900;
  margin-bottom: 3px;
}

.form-alert p {
  margin: 0;
  font-size: 0.86rem;
  line-height: 1.45;
}

.warning-alert {
  background: #fff7ed;
  border-color: #fed7aa;
  color: #9a3412;
}

.success-alert {
  background: #ecfdf5;
  border-color: #bbf7d0;
  color: #166534;
}

.submit-btn {
  width: 100%;
  min-height: 48px;
  border: none;
  border-radius: 10px;
  background: #ff5a00;
  color: #ffffff;
  font-weight: 900;
  cursor: pointer;
  transition: 0.2s ease;
}

.submit-btn:hover:not(:disabled) {
  background: #f01818;
  transform: translateY(-1px);
  box-shadow: 0 8px 18px rgba(240, 24, 24, 0.18);
}

.submit-btn:disabled {
  opacity: 0.75;
  cursor: not-allowed;
}

/* Responsive design */
@media (max-width: 900px) {
  .navbar {
    align-items: flex-start;
    gap: 16px;
    flex-direction: column;
  }

  .nav-actions {
    width: 100%;
    flex-wrap: wrap;
    gap: 12px;
  }

  .management-dropdown {
    left: 0;
    right: auto;
  }
}

@media (max-width: 640px) {
  .create-airport-main {
    padding: 24px 16px 60px;
  }

  .create-airport-hero,
  .create-airport-card {
    padding: 24px;
  }

  .hero-content {
    align-items: flex-start;
  }

  .create-airport-hero h1 {
    font-size: 1.55rem;
  }

  .helper-row {
    flex-direction: column;
    gap: 4px;
  }
}
</style>
