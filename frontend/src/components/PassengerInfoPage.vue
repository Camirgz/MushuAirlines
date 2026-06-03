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

      <!-- Flight summary banner -->
      <div class="flight-summary" v-if="flight">
        <div class="flight-summary-route">
          <span class="summary-airport">{{ flight.origin }}</span>
          <i class="bi bi-arrow-right summary-arrow"></i>
          <span class="summary-airport">{{ flight.destination }}</span>
        </div>
        <div class="flight-summary-meta">
          <span><i class="bi bi-calendar3 me-1"></i>{{ flight.flightDate }}</span>
          <span class="summary-sep">·</span>
          <span><i class="bi bi-person me-1"></i>{{ purchaseState.seats.length }} pasajero(s)</span>
          <span class="summary-sep">·</span>
          <span><i class="bi bi-award me-1"></i>{{ flightClassSummary }}</span>
        </div>
      </div>

      <AdminCard>

        <!-- ── Passenger list ── -->
        <div
          v-for="(passenger, index) in passengers"
          :key="index"
        >
          <div v-if="index > 0" class="passenger-separator">
            <hr />
          </div>

          <div class="passenger-header">
            <div class="passenger-title-group">
              <h3 class="passenger-title">Pasajero {{ index + 1 }}</h3>
              <span v-if="index === 0" class="titular-badge">Titular de la Compra</span>
            </div>
            <button
              v-if="index > 0"
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
              <label class="field-label">Género <span class="required">*</span></label>
              <select class="field-input field-select" v-model="passenger.gender">
                <option value="" disabled>Seleccione género</option>
                <option value="Hombre">Hombre</option>
                <option value="Mujer">Mujer</option>
                <option value="NoEspecifica">No especifica</option>
              </select>
            </div>

            <div class="form-group">
              <label class="field-label">País del Pasaporte <span class="required">*</span></label>
              <input
                type="text"
                class="field-input"
                v-model="passenger.passportCountry"
                placeholder="Ej: Costa Rica"
              />
            </div>

            <div class="form-group">
              <label class="field-label">Número de Pasaporte <span class="required">*</span></label>
              <input
                type="text"
                class="field-input"
                v-model="passenger.passportNumber"
                placeholder="Ingrese número de pasaporte"
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

            <!-- Email and phone only for the titular passenger -->
            <template v-if="index === 0">
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
            </template>

          </div>
        </div>

        <button class="btn-add-passenger" type="button" @click="addPassenger">
          <i class="bi bi-plus-lg"></i> Agregar Pasajero
        </button>

        <!-- ── Baggage section ── -->
        <div class="baggage-separator">
          <hr />
        </div>

        <div class="baggage-section">
          <h3 class="passenger-title" style="margin-bottom: 20px;">Equipaje</h3>

          <div class="baggage-grid">

            <div class="baggage-type">
              <div class="baggage-type-header">
                <i class="bi bi-briefcase-fill"></i>
                <span>Equipaje de Mano</span>
              </div>
              <div class="baggage-fields">
                <div class="form-group">
                  <label class="field-label">Cantidad</label>
                  <input
                    type="number"
                    class="field-input"
                    v-model.number="baggage.handCount"
                    min="0"
                    placeholder="0"
                  />
                </div>
                <div class="form-group">
                  <label class="field-label">Peso por pieza (kg)</label>
                  <input
                    type="number"
                    class="field-input"
                    v-model.number="baggage.handWeight"
                    min="0"
                    step="0.5"
                    placeholder="0"
                  />
                </div>
              </div>
            </div>

            <div class="baggage-type">
              <div class="baggage-type-header">
                <i class="bi bi-archive-fill"></i>
                <span>Equipaje Documentado</span>
              </div>
              <div class="baggage-fields">
                <div class="form-group">
                  <label class="field-label">Cantidad</label>
                  <input
                    type="number"
                    class="field-input"
                    v-model.number="baggage.checkedCount"
                    min="0"
                    placeholder="0"
                  />
                </div>
                <div class="form-group">
                  <label class="field-label">Peso por pieza (kg)</label>
                  <input
                    type="number"
                    class="field-input"
                    v-model.number="baggage.checkedWeight"
                    min="0"
                    step="0.5"
                    placeholder="0"
                  />
                </div>
              </div>
            </div>

          </div>
        </div>

      </AdminCard>

      <div v-if="validationError" class="validation-error">
        <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ validationError }}
      </div>

      <div class="action-row">
        <button class="btn-back" type="button" @click="goBack">Volver</button>
        <button class="btn-continue" type="button" @click="continueToPayment">Continuar al Pago</button>
      </div>

    </main>

    <AppFooter />
  </div>
</template>

<script>
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";
import AppFooter from "@/components/layout/AppFooter.vue";
import { usePurchaseFlow } from "@/composables/usePurchaseFlow";

export default {
  name: "PassengerInfoPage",

  components: {
    AdminHero,
    AdminCard,
    AppFooter,
  },

  setup() {
    const { state, hasFlight, setPassengers } = usePurchaseFlow();
    return { purchaseState: state, hasFlight, setPassengers };
  },

  data() {
    return {
      passengers: [this.emptyPassenger()],
      baggage: {
        handCount:     0,
        handWeight:    0,
        checkedCount:  0,
        checkedWeight: 0,
      },
      validationError: null,
    };
  },

  created() {
    if (!this.hasFlight) {
      this.$router.push("/");
      return;
    }
    const seatCount = this.purchaseState.seats.length;
    if (seatCount > 1) {
      this.passengers = Array.from({ length: seatCount }, () => this.emptyPassenger());
    }
  },

  computed: {
    flight() {
      return this.purchaseState.flight;
    },

    flightClassSummary() {
      const seats = this.purchaseState.seats;
      const fc = seats.filter((s) => s.seatClass === "FirstClass").length;
      const ec = seats.filter((s) => s.seatClass === "Economy").length;
      const parts = [];
      if (fc > 0) parts.push(`${fc} Primera Clase`);
      if (ec > 0) parts.push(`${ec} Turista`);
      return parts.join(" · ");
    },
  },

  methods: {
    emptyPassenger() {
      return {
        firstName:       "",
        lastName:        "",
        gender:          "",
        passportCountry: "",
        passportNumber:  "",
        birthDate:       "",
        email:           "",
        phone:           "",
      };
    },

    addPassenger() {
      this.passengers.push(this.emptyPassenger());
    },

    removePassenger(index) {
      this.passengers.splice(index, 1);
    },

    goBack() {
      this.$router.push("/");
    },

    validateAllPassengers() {
      for (let i = 0; i < this.passengers.length; i++) {
        const p     = this.passengers[i];
        const label = `Pasajero ${i + 1}`;

        if (!p.firstName.trim())
          return this.setError(`${label}: el nombre es requerido.`);
        if (!p.lastName.trim())
          return this.setError(`${label}: el apellido es requerido.`);
        if (!p.gender)
          return this.setError(`${label}: el género es requerido.`);
        if (!p.passportCountry.trim())
          return this.setError(`${label}: el país del pasaporte es requerido.`);
        if (!p.passportNumber.trim())
          return this.setError(`${label}: el número de pasaporte es requerido.`);
        if (!p.birthDate)
          return this.setError(`${label}: la fecha de nacimiento es requerida.`);

        if (i === 0) {
          if (!p.email.trim())
            return this.setError(`${label}: el correo electrónico es requerido.`);
          if (!p.phone.trim())
            return this.setError(`${label}: el teléfono es requerido.`);
        }
      }
      this.validationError = null;
      return true;
    },

    setError(msg) {
      this.validationError = msg;
      return false;
    },

    continueToPayment() {
      if (!this.validateAllPassengers()) return;
      this.setPassengers(this.passengers, this.baggage);
      this.$router.push("/payment");
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

/* ── Flight summary banner ── */
.flight-summary {
  background: linear-gradient(135deg, rgba(231,76,60,0.07) 0%, rgba(243,156,18,0.07) 100%);
  border: 1.5px solid rgba(231,76,60,0.18);
  border-radius: 12px;
  padding: 14px 20px;
  margin-bottom: 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: wrap;
}

.flight-summary-route {
  display: flex;
  align-items: center;
  gap: 10px;
}

.summary-airport {
  font-size: 1.25rem;
  font-weight: 800;
  color: #1a1a1a;
  letter-spacing: 0.04em;
}

.summary-arrow {
  color: #e74c3c;
  font-size: 1rem;
}

.flight-summary-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.85rem;
  color: #555;
  flex-wrap: wrap;
}

.summary-sep {
  color: #ccc;
}

/* ── Passenger section ── */
.passenger-separator {
  margin: 8px 0;
}

.passenger-separator hr,
.baggage-separator hr {
  border: none;
  border-top: 1.5px solid #f0f0f0;
  margin: 0;
}

.passenger-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.passenger-title-group {
  display: flex;
  align-items: center;
  gap: 10px;
}

.passenger-title {
  font-size: 1.05rem;
  font-weight: 700;
  color: #1a1a1a;
  margin: 0;
}

.titular-badge {
  display: inline-block;
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  color: #fff;
  font-size: 0.72rem;
  font-weight: 700;
  padding: 3px 10px;
  border-radius: 20px;
  letter-spacing: 0.02em;
  white-space: nowrap;
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

/* ── Baggage section ── */
.baggage-separator {
  margin: 28px 0 24px;
}

.baggage-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
}

.baggage-type-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.93rem;
  font-weight: 700;
  color: #374151;
  margin-bottom: 14px;
}

.baggage-type-header i {
  font-size: 1.1rem;
  color: #e74c3c;
}

.baggage-fields {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

/* ── Validation error ── */
.validation-error {
  background: #fff5f5;
  border: 1.5px solid #fca5a5;
  color: #b91c1c;
  border-radius: 10px;
  padding: 12px 16px;
  font-size: 0.88rem;
  font-weight: 600;
  margin-top: 8px;
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

  .baggage-grid {
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
