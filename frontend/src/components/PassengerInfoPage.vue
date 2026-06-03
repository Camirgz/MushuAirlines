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
      <div class="purchase-layout">

        <aside class="sidebar-col">
          <PurchaseSummaryCard
            v-if="flight"
            :flight="flight"
            :seats="purchaseState.seats"
            :baggage="baggage"
          />
        </aside>

        <div class="content-col">

        <AdminHero
          icon="bi bi-person"
          title="Información de Pasajeros"
          subtitle="Complete los datos de todos los pasajeros"
        />

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
                :class="['field-input', { 'field-input--error': fieldErrors[`${index}_firstName`] }]"
                v-model="passenger.firstName"
                placeholder="Ingrese nombre"
                @input="clearFieldError(index, 'firstName')"
              />
            </div>

            <div class="form-group">
              <label class="field-label">Apellidos <span class="required">*</span></label>
              <input
                type="text"
                :class="['field-input', { 'field-input--error': fieldErrors[`${index}_lastName`] }]"
                v-model="passenger.lastName"
                placeholder="Ingrese apellidos"
                @input="clearFieldError(index, 'lastName')"
              />
            </div>

            <div class="form-group">
              <label class="field-label">Género <span class="required">*</span></label>
              <select
                :class="['field-input field-select', { 'field-input--error': fieldErrors[`${index}_gender`] }]"
                v-model="passenger.gender"
                @change="clearFieldError(index, 'gender')"
              >
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
                :class="['field-input', { 'field-input--error': fieldErrors[`${index}_passportCountry`] }]"
                v-model="passenger.passportCountry"
                placeholder="Ej: Costa Rica"
                @input="clearFieldError(index, 'passportCountry')"
              />
            </div>

            <div class="form-group">
              <label class="field-label">Fecha de Nacimiento <span class="required">*</span></label>
              <input
                type="date"
                :class="['field-input', { 'field-input--error': fieldErrors[`${index}_birthDate`] }]"
                v-model="passenger.birthDate"
                :max="yesterdayDate"
                @change="clearFieldError(index, 'birthDate')"
              />
            </div>

            <!-- Email and phone only for the titular passenger -->
            <template v-if="index === 0">
              <div class="form-group">
                <label class="field-label">Correo Electrónico <span class="required">*</span></label>
                <input
                  type="email"
                  :class="['field-input', { 'field-input--error': fieldErrors[`${index}_email`] }]"
                  v-model="passenger.email"
                  placeholder="ejemplo@correo.com"
                  @input="clearFieldError(index, 'email')"
                />
              </div>

              <div class="form-group">
                <label class="field-label">Teléfono <span class="required">*</span></label>
                <input
                  type="tel"
                  :class="['field-input', { 'field-input--error': fieldErrors[`${index}_phone`] }]"
                  v-model="passenger.phone"
                  placeholder="+506 00000000"
                  @input="clearFieldError(index, 'phone')"
                />
              </div>
            </template>

          </div>
        </div>

        <button
          v-if="passengers.length < maxPassengers"
          class="btn-add-passenger"
          type="button"
          @click="addPassenger"
        >
          <i class="bi bi-plus-lg"></i> Agregar Pasajero
        </button>
        <div v-else class="max-passengers-note">
          <i class="bi bi-info-circle me-1"></i>
          Ya alcanzaste el máximo de {{ maxPassengers }} pasajero(s) para esta compra.
        </div>

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
              <div class="baggage-weight-info" v-if="flight && flight.handBagWeight">
                Máx. {{ flight.handBagWeight }} kg por pieza · ₡{{ (flight.handBagPrice || 0).toLocaleString() }} c/u
              </div>
              <div class="baggage-fields baggage-fields--single">
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
              </div>
            </div>

            <div class="baggage-type">
              <div class="baggage-type-header">
                <i class="bi bi-archive-fill"></i>
                <span>Equipaje Documentado</span>
              </div>
              <div class="baggage-weight-info" v-if="flight && flight.bagWeight">
                Máx. {{ flight.bagWeight }} kg por pieza · ₡{{ (flight.bagPrice || 0).toLocaleString() }} c/u
              </div>
              <div class="baggage-fields baggage-fields--single">
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

        </div><!-- end content-col -->
      </div><!-- end purchase-layout -->
    </main>

    <AppFooter />
  </div>
</template>

<script>
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";
import AppFooter from "@/components/layout/AppFooter.vue";
import PurchaseSummaryCard from "@/components/purchase/PurchaseSummaryCard.vue";
import { usePurchaseFlow } from "@/composables/usePurchaseFlow";
import { checkAvailability, checkPassengerDuplicates } from "@/services/PurchaseService";

export default {
  name: "PassengerInfoPage",

  components: {
    AdminHero,
    AdminCard,
    AppFooter,
    PurchaseSummaryCard,
  },

  setup() {
    const { state, hasFlight, setPassengers } = usePurchaseFlow();
    return { purchaseState: state, hasFlight, setPassengers };
  },

  data() {
    return {
      passengers:     [this.emptyPassenger()],
      baggage:        { handCount: 0, checkedCount: 0 },
      validationError: null,
      fieldErrors:    {},
    };
  },

  created() {
    if (!this.hasFlight) {
      this.$router.push("/");
      return;
    }

    // Restore passenger data if user navigated back from PaymentForm
    const savedPassengers = this.purchaseState.passengers;
    if (savedPassengers.length > 0) {
      this.passengers = savedPassengers.map(p => ({ ...p }));
    } else {
      const count = Math.max(1, this.purchaseState.seats.length);
      this.passengers = Array.from({ length: count }, () => this.emptyPassenger());
    }

    // Restore baggage from state (if user navigated back) or pre-fill from flight modal
    const savedBaggage = this.purchaseState.baggage;
    const f            = this.purchaseState.flight;
    if (savedBaggage.handCount > 0 || savedBaggage.checkedCount > 0) {
      this.baggage.handCount    = savedBaggage.handCount;
      this.baggage.checkedCount = savedBaggage.checkedCount;
    } else if (f) {
      this.baggage.handCount    = f.handBagsCount    ?? 0;
      this.baggage.checkedCount = f.checkedBagsCount ?? 0;
    }
  },

  computed: {
    flight() {
      return this.purchaseState.flight;
    },

    maxPassengers() {
      return this.purchaseState.flight?.passengerCount ?? 9;
    },

    yesterdayDate() {
      const d = new Date();
      d.setDate(d.getDate() - 1);
      return d.toISOString().split("T")[0];
    },

  },

  methods: {
    emptyPassenger() {
      return {
        firstName:       "",
        lastName:        "",
        gender:          "",
        passportCountry: "",
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
      const errors = {};
      let hasInvalidBirthDate = false;

      const today = new Date();
      today.setHours(0, 0, 0, 0);

      for (let i = 0; i < this.passengers.length; i++) {
        const p = this.passengers[i];
        if (!p.firstName.trim())       errors[`${i}_firstName`]       = true;
        if (!p.lastName.trim())        errors[`${i}_lastName`]        = true;
        if (!p.gender)                 errors[`${i}_gender`]          = true;
        if (!p.passportCountry.trim()) errors[`${i}_passportCountry`] = true;

        if (!p.birthDate) {
          errors[`${i}_birthDate`] = true;
        } else if (new Date(p.birthDate) >= today) {
          errors[`${i}_birthDate`] = true;
          hasInvalidBirthDate = true;
        }

        if (i === 0) {
          if (!p.email.trim())  errors[`${i}_email`]  = true;
          if (!p.phone.trim())  errors[`${i}_phone`]  = true;
        }
      }

      this.fieldErrors = errors;

      if (Object.keys(errors).length > 0) {
        this.validationError = hasInvalidBirthDate
          ? "La fecha de nacimiento debe ser anterior al día de hoy."
          : "Por favor complete todos los campos requeridos marcados en rojo.";
        return false;
      }

      // Within-purchase duplicate check
      const seen = new Map();
      for (let i = 0; i < this.passengers.length; i++) {
        const p   = this.passengers[i];
        const key = `${p.firstName.trim().toLowerCase()}|${p.lastName.trim().toLowerCase()}|${p.birthDate}|${p.passportCountry.trim().toLowerCase()}`;
        if (seen.has(key)) {
          this.validationError = `El Pasajero ${i + 1} tiene los mismos datos que el Pasajero ${seen.get(key) + 1}. No se puede agregar el mismo pasajero dos veces en la misma compra.`;
          return false;
        }
        seen.set(key, i);
      }

      this.validationError = null;
      return true;
    },

    clearFieldError(index, field) {
      const key = `${index}_${field}`;
      if (this.fieldErrors[key]) {
        const updated = { ...this.fieldErrors };
        delete updated[key];
        this.fieldErrors = updated;
      }
    },

    async continueToPayment() {
      if (!this.validateAllPassengers()) return;

      const flight2 = this.purchaseState.flight2;

      // Flight-level duplicate check — same passenger already booked on this flight?
      const checksLeg1 = checkPassengerDuplicates(
        this.flight.code,
        this.flight.flightDate,
        this.passengers
      );
      const checksLeg2 = flight2
        ? checkPassengerDuplicates(flight2.code, flight2.flightDate, this.passengers)
        : Promise.resolve({ hasDuplicates: false, duplicates: [] });

      const [result1, result2] = await Promise.all([checksLeg1, checksLeg2]);
      if (result1.hasDuplicates || result2.hasDuplicates) {
        const allDups = [...new Set([...result1.duplicates, ...result2.duplicates])];
        this.validationError = `Ya existe una reserva en este vuelo para: ${allDups.join(', ')}. Un pasajero no puede tener más de un boleto en el mismo vuelo.`;
        return;
      }

      const seatCount = this.purchaseState.seats.length;
      const availLeg1 = checkAvailability(this.flight.code, this.flight.flightDate, seatCount);
      const availLeg2 = flight2
        ? checkAvailability(flight2.code, flight2.flightDate, seatCount)
        : Promise.resolve(true);

      const [avail1, avail2] = await Promise.all([availLeg1, availLeg2]);
      if (!avail1 || !avail2) {
        this.validationError = "Lo sentimos, este vuelo ya no tiene asientos disponibles. Por favor regrese y seleccione otro vuelo.";
        return;
      }

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
  max-width: 1160px;
  margin: 0 auto;
  padding: 40px 24px 72px;
  flex: 1;
  width: 100%;
}

/* ── Two-column purchase layout ── */
.purchase-layout {
  display: flex;
  align-items: flex-start;
  gap: 28px;
}

.sidebar-col {
  width: 256px;
  flex-shrink: 0;
  position: sticky;
  top: 80px;
}

.content-col {
  flex: 1;
  min-width: 0;
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

.field-input--error {
  border-color: #e74c3c !important;
  box-shadow: 0 0 0 3px rgba(231, 76, 60, 0.12) !important;
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

/* ── Max passengers note ── */
.max-passengers-note {
  width: 100%;
  padding: 11px 16px;
  border: 1.5px solid #fed7aa;
  border-radius: 10px;
  background: #fff7ed;
  color: #92400e;
  font-size: 0.88rem;
  font-weight: 600;
  margin-top: 4px;
  box-sizing: border-box;
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

.baggage-fields--single {
  grid-template-columns: 1fr;
}

.baggage-weight-info {
  font-size: 0.8rem;
  color: #888;
  margin-bottom: 10px;
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
@media (max-width: 900px) {
  .sidebar-col {
    display: none;
  }
}

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
