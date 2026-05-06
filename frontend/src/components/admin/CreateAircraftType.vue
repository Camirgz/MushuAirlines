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
          <h1>Crear Aeronave</h1>
          <p>Panel de administración para operadores de Mushu Airlines</p>
        </div>
      </section>

      <!-- Form card -->
      <section class="content-card">
        <h2>Crear Aeronave</h2>

        <form @submit.prevent="Submit">
          <!-- Model -->
          <div class="form-group">
            <label class="form-label">Modelo <span class="required">*</span></label>
            <input
              v-model="Form.Model"
              type="text"
              class="form-input"
              placeholder="Ej: 737-800"
              required
            />
          </div>

          <!-- Type: free text with autocomplete suggestions from existing types -->
          <div class="form-group">
            <label class="form-label">Tipo de aeronave <span class="required">*</span></label>
            <input
              v-model="Form.Type"
              list="AircraftTypesList"
              type="text"
              class="form-input"
              placeholder="Ej: Avión comercial, Helicóptero..."
              @input="OnTypeNameChanged"
              required
            />
            <datalist id="AircraftTypesList">
              <option
                v-for="Option in AircraftTypeOptions"
                :key="Option.Id"
                :value="Option.Name"
              />
            </datalist>
          </div>

          <!-- Weight -->
          <div class="form-group">
            <label class="form-label">Peso soportado (kg) <span class="required">*</span></label>
            <input
              v-model.number="Form.WeightKg"
              type="number"
              class="form-input"
              placeholder="Ej: 75000"
              min="1"
              required
            />
          </div>

          <hr class="section-divider" />

          <!-- Economy class -->
          <h3 class="section-title">Clase Turista</h3>
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Cantidad de filas <span class="required">*</span></label>
              <input
                v-model.number="Form.EconomyClass.RowCount"
                type="number"
                class="form-input"
                placeholder="Ej: 20"
                min="1"
                required
              />
            </div>
            <div class="form-group">
              <label class="form-label">Asientos por fila <span class="required">*</span></label>
              <input
                v-model.number="Form.EconomyClass.SeatsPerRow"
                type="number"
                class="form-input"
                placeholder="Ej: 6"
                min="1"
                required
              />
            </div>
          </div>

          <hr class="section-divider" />

          <!-- First class -->
          <h3 class="section-title">Primera Clase</h3>
          <div class="form-row">
            <div class="form-group">
              <label class="form-label">Cantidad de filas</label>
              <input
                v-model.number="Form.FirstClass.RowCount"
                type="number"
                class="form-input"
                placeholder="Ej: 4"
                min="1"
              />
            </div>
            <div class="form-group">
              <label class="form-label">Asientos por fila</label>
              <input
                v-model.number="Form.FirstClass.SeatsPerRow"
                type="number"
                class="form-input"
                placeholder="Ej: 4"
                min="1"
              />
            </div>
          </div>

          <!-- Seat counter -->
          <div class="seat-counter" :class="{ 'seat-counter--danger': TotalSeats >= 1000 }">
            <i class="bi bi-person-fill me-2"></i>
            Capacidad total: <strong>{{ TotalSeats }} asientos</strong>
            <span v-if="TotalSeats >= 1000" class="seat-limit-msg">
              — máximo permitido: 999
            </span>
          </div>

          <!-- Error message -->
          <div v-if="ErrorMessage" class="error-msg">
            <i class="bi bi-exclamation-circle me-2"></i>
            {{ ErrorMessage }}
          </div>

          <!-- Actions -->
          <div class="form-actions">
            <button type="button" class="cancel-btn" @click="Cancel" :disabled="IsSubmitting">
              <i class="bi bi-x-lg me-2"></i>
              Cancelar
            </button>
            <button type="submit" class="submit-btn" :disabled="IsSubmitting || TotalSeats >= 1000">
              <i class="bi bi-floppy me-2"></i>
              {{ IsSubmitting ? "Guardando..." : "Guardar" }}
            </button>
          </div>
        </form>
      </section>
    </main>
  </div>
</template>

<script>
import { CreateAircraftType, GetAircraftTypeOptions } from "../../services/AircraftTypesService";

export default {
  name: "CreateAircraftType",

  computed: {
    TotalSeats() {
      const Economy    = (this.Form.EconomyClass.RowCount  || 0) * (this.Form.EconomyClass.SeatsPerRow || 0);
      const FirstClass = (this.Form.FirstClass.RowCount    || 0) * (this.Form.FirstClass.SeatsPerRow   || 0);
      return Economy + FirstClass;
    },
  },

  data() {
    return {
      IsDropdownOpen: false,
      IsSubmitting: false,
      ErrorMessage: "",
      AircraftTypeOptions: [],
      Form: {
        Model: "",
        Type: "",
        WeightKg: null,
        EconomyClass: {
          RowCount: null,
          SeatsPerRow: null,
        },
        FirstClass: {
          RowCount: null,
          SeatsPerRow: null,
        },
      },
    };
  },

  mounted() {
    this.LoadAircraftTypeOptions();
  },

  methods: {
    ToggleDropdown() {
      this.IsDropdownOpen = !this.IsDropdownOpen;
    },
    CloseDropdown() {
      this.IsDropdownOpen = false;
    },
    LoadAircraftTypeOptions() {
      GetAircraftTypeOptions()
        .then((Response) => {
          this.AircraftTypeOptions = Response.data;
        })
        .catch(() => {});
    },
    OnTypeNameChanged() {
      const Option = this.AircraftTypeOptions.find(
        (O) => O.Name === this.Form.Type
      );
      if (!Option) return;

      if (Option.DefaultModel)                 this.Form.Model                    = Option.DefaultModel;
      if (Option.DefaultWeightKg)              this.Form.WeightKg                 = Option.DefaultWeightKg;
      if (Option.DefaultEconomyRows)           this.Form.EconomyClass.RowCount    = Option.DefaultEconomyRows;
      if (Option.DefaultEconomySeatsPerRow)    this.Form.EconomyClass.SeatsPerRow = Option.DefaultEconomySeatsPerRow;
      if (Option.DefaultFirstClassRows)        this.Form.FirstClass.RowCount      = Option.DefaultFirstClassRows;
      if (Option.DefaultFirstClassSeatsPerRow) this.Form.FirstClass.SeatsPerRow   = Option.DefaultFirstClassSeatsPerRow;
    },
    Submit() {
      this.IsSubmitting = true;
      this.ErrorMessage = "";

      const Payload = {
        Model: this.Form.Model,
        Type: this.Form.Type,
        WeightKg: this.Form.WeightKg,
        EconomyRows: this.Form.EconomyClass.RowCount,
        EconomySeatsPerRow: this.Form.EconomyClass.SeatsPerRow,
        FirstClassRows: this.Form.FirstClass.RowCount,
        FirstClassSeatsPerRow: this.Form.FirstClass.SeatsPerRow,
      };

      CreateAircraftType(Payload)
        .then(() => {
          this.$router.push("/admin/aircraft-types");
        })
        .catch((Error) => {
          const ServerMessage = Error.response?.data;
          this.ErrorMessage = ServerMessage
            ? `Error del servidor: ${ServerMessage}`
            : `Error de red: ${Error.message}`;
        })
        .finally(() => {
          this.IsSubmitting = false;
        });
    },
    Cancel() {
      this.$router.push("/admin/aircraft-types");
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
  max-width: 860px;
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

/* Form card */
.content-card {
  background: #ffffff;
  border-radius: 16px;
  padding: 36px;
  box-shadow: 0 16px 36px rgba(15, 23, 42, 0.08);
}

.content-card h2 {
  font-size: 1.35rem;
  font-weight: 900;
  color: #07172c;
  margin: 0 0 28px;
}

/* Form elements */
.form-group {
  display: flex;
  flex-direction: column;
  gap: 7px;
  margin-bottom: 20px;
}

.form-label {
  font-size: 0.9rem;
  font-weight: 700;
  color: #374151;
}

.required {
  color: #f01818;
}

.form-input {
  width: 100%;
  padding: 12px 16px;
  border: 1.5px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.95rem;
  color: #111827;
  background: #ffffff;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
  box-sizing: border-box;
}

.form-input::placeholder {
  color: #9ca3af;
}

.form-input:focus {
  outline: none;
  border-color: #ff5a00;
  box-shadow: 0 0 0 3px rgba(255, 90, 0, 0.1);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.section-divider {
  border: none;
  border-top: 1.5px solid #e5e7eb;
  margin: 8px 0 24px;
}

.section-title {
  font-size: 1rem;
  font-weight: 800;
  color: #07172c;
  margin: 0 0 18px;
}

/* Action buttons */
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 32px;
  padding-top: 24px;
  border-top: 1.5px solid #e5e7eb;
}

.cancel-btn {
  padding: 12px 24px;
  background: #ffffff;
  color: #374151;
  border: 1.5px solid #d1d5db;
  border-radius: 10px;
  font-weight: 700;
  font-size: 0.95rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: 0.2s ease;
}

.cancel-btn:hover {
  border-color: #9ca3af;
  background: #f9fafb;
}

.submit-btn {
  padding: 12px 28px;
  background: linear-gradient(135deg, #f01818 0%, #ff5a00 45%, #ffc400 100%);
  color: #ffffff;
  border: none;
  border-radius: 10px;
  font-weight: 800;
  font-size: 0.95rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  box-shadow: 0 8px 18px rgba(240, 24, 24, 0.22);
  transition: 0.2s ease;
}

.submit-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 10px 22px rgba(240, 24, 24, 0.3);
}

.submit-btn:disabled,
.cancel-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.error-msg {
  margin-top: 20px;
  padding: 14px 18px;
  background: #fff1f1;
  color: #b91c1c;
  border: 1px solid #fecaca;
  border-radius: 10px;
  font-size: 0.95rem;
  font-weight: 600;
  display: flex;
  align-items: center;
}

.seat-counter {
  margin-top: 20px;
  padding: 12px 18px;
  background: #f0fdf4;
  color: #166534;
  border: 1px solid #bbf7d0;
  border-radius: 10px;
  font-size: 0.92rem;
  display: flex;
  align-items: center;
  transition: 0.2s ease;
}

.seat-counter--danger {
  background: #fff1f1;
  color: #b91c1c;
  border-color: #fecaca;
}

.seat-limit-msg {
  margin-left: 4px;
  font-weight: 700;
}
</style>
