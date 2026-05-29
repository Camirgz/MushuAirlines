<template>
  <div class="passenger-page">

    <nav class="navbar bg-white shadow-sm px-4 py-2">
      <a class="navbar-brand d-flex align-items-center gap-2" href="/">
        <img src="@/assets/logo.png" alt="Logo" width="42" height="42" class="rounded-2" />
        <div>
          <div class="brand-name">Mushu Airlines</div>
          <div class="brand-tagline">Vuela con el dragón</div>
        </div>
      </a>
      <div class="d-flex align-items-center gap-4">
        <a href="#" class="nav-link-item">
          <i class="bi bi-briefcase me-1"></i>Mis vuelos
        </a>
        <a href="#" class="nav-link-item">
          <i class="bi bi-calendar-check me-1"></i>Check-in
        </a>
        <a href="/login" class="btn btn-outline-danger rounded-pill px-3 py-1 admin-btn">
          <i class="bi bi-person me-1"></i>Admin Login
        </a>
      </div>
    </nav>

    <main class="page-main">

      <AdminHero
        icon="bi bi-person"
        title="Información de Pasajeros"
        subtitle="Complete los datos de todos los pasajeros"
      />

      <AdminCard>
        <div
          v-for="(passenger, index) in passengers"
          :key="index"
        >
          <div v-if="index > 0" class="passenger-separator">
            <hr class="section-line" />
          </div>

          <div class="passenger-header">
            <h3 class="passenger-title">Pasajero {{ index + 1 }}</h3>
            <button
              v-if="passengers.length > 1"
              class="btn-remove"
              type="button"
              @click="removePassenger(index)"
            >
              <i class="bi bi-trash3"></i> Eliminar
            </button>
          </div>

          <div class="form-grid">
            <div class="form-group">
              <label class="field-label">Nombre <span class="required">*</span></label>
              <input
                type="text"
                class="field-input"
                v-model="passenger.firstName"
                placeholder="Ingrese nombre"
              />
            </div>

            <div class="form-group">
              <label class="field-label">Apellidos <span class="required">*</span></label>
              <input
                type="text"
                class="field-input"
                v-model="passenger.lastName"
                placeholder="Ingrese apellidos"
              />
            </div>

            <div class="form-group">
              <label class="field-label">Tipo de Documento <span class="required">*</span></label>
              <select class="field-input field-select" v-model="passenger.documentType">
                <option value="" disabled>Seleccione tipo</option>
                <option>Pasaporte</option>
                <option>Cédula de Identidad</option>
                <option>Cédula de Residencia</option>
                <option>DIMEX</option>
              </select>
            </div>

            <div class="form-group">
              <label class="field-label">Número de Documento <span class="required">*</span></label>
              <input
                type="text"
                class="field-input"
                v-model="passenger.documentNumber"
                placeholder="Ingrese número"
              />
            </div>

            <div class="form-group">
              <label class="field-label">Fecha de Nacimiento <span class="required">*</span></label>
              <input
                type="date"
                class="field-input"
                v-model="passenger.birthDate"
              />
            </div>

            <div class="form-group">
              <label class="field-label">Correo Electrónico <span class="required">*</span></label>
              <input
                type="email"
                class="field-input"
                v-model="passenger.email"
                placeholder="ejemplo@correo.com"
              />
            </div>

            <div class="form-group">
              <label class="field-label">Teléfono <span class="required">*</span></label>
              <input
                type="tel"
                class="field-input"
                v-model="passenger.phone"
                placeholder="+506 00000000"
              />
            </div>
          </div>
        </div>

        <button class="btn-add-passenger" type="button" @click="addPassenger">
          <i class="bi bi-plus-lg"></i> Agregar Pasajero
        </button>
      </AdminCard>

      <div class="action-row">
        <button class="btn-back" type="button">Volver</button>
        <button class="btn-continue" type="button">Continuar al Pago</button>
      </div>

    </main>

    <AppFooter />
  </div>
</template>

<script>
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";
import AppFooter from "@/components/layout/AppFooter.vue";

export default {
  name: "PassengerInfoPage",

  components: {
    AdminHero,
    AdminCard,
    AppFooter,
  },

  data() {
    return {
      passengers: [this.emptyPassenger()],
    };
  },

  methods: {
    emptyPassenger() {
      return {
        firstName: "",
        lastName: "",
        documentType: "",
        documentNumber: "",
        birthDate: "",
        email: "",
        phone: "",
      };
    },

    addPassenger() {
      this.passengers.push(this.emptyPassenger());
    },

    removePassenger(index) {
      this.passengers.splice(index, 1);
    },
  },
};
</script>

<style scoped>
.passenger-page {
  min-height: 100vh;
  background: #f8f9fa;
  color: #1a1a1a;
  display: flex;
  flex-direction: column;
}

/* ── Navbar ── */
.navbar {
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.brand-name {
  font-weight: 800;
  font-size: 1rem;
  color: #1a1a1a;
  line-height: 1.2;
}

.brand-tagline {
  font-size: 0.72rem;
  color: #888;
}

.nav-link-item {
  color: #555;
  text-decoration: none;
  font-size: 0.9rem;
  font-weight: 500;
  transition: color 0.2s;
}

.nav-link-item:hover {
  color: #e74c3c;
}

.admin-btn {
  font-size: 0.85rem;
  font-weight: 600;
}

/* ── Main container ── */
.page-main {
  max-width: 860px;
  margin: 0 auto;
  padding: 40px 24px 72px;
  flex: 1;
  width: 100%;
}

/* ── Passenger section ── */
.passenger-separator {
  margin-bottom: 4px;
}

.passenger-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.passenger-title {
  font-size: 1.05rem;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0;
}

.btn-remove {
  background: none;
  border: 1.5px solid #e74c3c;
  color: #e74c3c;
  border-radius: 8px;
  padding: 5px 12px;
  font-size: 0.82rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 5px;
  transition: background 0.2s, color 0.2s;
}

.btn-remove:hover {
  background: #e74c3c;
  color: #fff;
}

/* ── Form grid ── */
.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px 24px;
  margin-bottom: 28px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field-label {
  font-size: 0.875rem;
  font-weight: 700;
  color: #374151;
}

.required {
  color: #e74c3c;
}

.field-input {
  height: 46px;
  padding: 11px 14px;
  border: 1.5px solid #e0e0e0;
  border-radius: 10px;
  font-size: 0.93rem;
  color: #1a1a1a;
  background: #fff;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
  width: 100%;
  box-sizing: border-box;
}

.field-input:focus {
  border-color: #ff5a00;
  box-shadow: 0 0 0 3px rgba(255, 90, 0, 0.1);
}

.field-select {
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%23888' d='M6 8L1 3h10z'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 14px center;
  padding-right: 36px;
  cursor: pointer;
}

/* ── Add passenger button ── */
.btn-add-passenger {
  width: 100%;
  padding: 12px;
  border: 1.5px dashed #e74c3c;
  border-radius: 10px;
  background: transparent;
  color: #e74c3c;
  font-size: 0.93rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  transition: background 0.2s, color 0.2s;
  margin-top: 4px;
}

.btn-add-passenger:hover {
  background: #fff5f5;
}

/* ── Action row ── */
.action-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 24px;
  gap: 12px;
}

.btn-back {
  background: #fff;
  border: 1.5px solid #d1d5db;
  color: #374151;
  border-radius: 10px;
  padding: 12px 32px;
  font-size: 0.93rem;
  font-weight: 600;
  cursor: pointer;
  transition: border-color 0.2s, color 0.2s;
}

.btn-back:hover {
  border-color: #9ca3af;
  color: #111827;
}

.btn-continue {
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  border: none;
  color: #fff;
  border-radius: 10px;
  padding: 12px 36px;
  font-size: 0.93rem;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.2s;
}

.btn-continue:hover {
  opacity: 0.88;
}

/* ── Responsive ── */
@media (max-width: 640px) {
  .form-grid {
    grid-template-columns: 1fr;
  }

  .action-row {
    flex-direction: column-reverse;
  }

  .btn-back,
  .btn-continue {
    width: 100%;
    text-align: center;
    justify-content: center;
  }
}
</style>
