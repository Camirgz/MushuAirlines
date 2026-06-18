<template>
  <AdminPageLayout>
    <AdminHero
      title="Gestión de aeropuertos"
      subtitle="Panel de administración para operadores de Mushu Airlines"
      icon="bi bi-airplane-engines"
      back-to="/admin/airports"
      back-text="Volver a la lista"
    />

    <AdminCard class="create-airport-card">
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
            id="country"
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
            id="city"
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

        <div
          class="form-group"
          :class="{ 'has-error': submitted && (errors.nameRequired || errors.nameInvalid) }"
        >
          <label for="airportName">Nombre del Aeropuerto <span>*</span></label>

          <input
            id="airportName"
            v-model.trim="form.airportName"
            type="text"
            maxlength="200"
            placeholder="Ej: Aeropuerto Internacional Juan Santamaría"
          />

          <div class="helper-row">
            <small>{{ airportNameLength }}/200 caracteres</small>

            <small v-if="submitted && errors.nameRequired" class="error-text">
              Debe ingresar el nombre del aeropuerto.
            </small>

            <small v-if="submitted && errors.nameInvalid" class="error-text">
              El nombre del aeropuerto no debe contener números ni caracteres especiales como #, !, %, $.
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
            placeholder="EJ: SJO"
            @input="formatCode"
          />

          <div class="helper-row">
            <small>3 caracteres en mayúsculas. Ej: SJO, MAD, MEX</small>

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
    </AdminCard>
  </AdminPageLayout>
</template>

<script>
import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";
import API_BASE_URL from "@/config/api";

const BaseURL = `${API_BASE_URL}/api/airport`;

export default {
  name: "AirportCreationForm",

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
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
    async loadCountries() {
      try {
        const response = await fetch(`${BaseURL}/countries`);

        if (!response.ok) {
          throw new Error("No se pudieron cargar los países.");
        }

        const data = await response.json();

        this.countries = data
          .map((item) => item.country ?? item.Country)
          .filter((country) => country);
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
          `${BaseURL}/cities?country=${encodeURIComponent(this.form.country)}`
        );

        if (!response.ok) {
          throw new Error("No se pudieron cargar las ciudades.");
        }

        const data = await response.json();

        this.cities = data
          .map((item) => item.city ?? item.City)
          .filter((city) => city);
      } catch (error) {
        this.alertType = "warning";
        this.alertTitle = "Error de conexión";
        this.alertMessage = "No se pudieron cargar las ciudades.";
      }
    },

    formatCode() {
      const MaxCodeLength = 3;

      this.form.code = this.form.code
        .toUpperCase()
        .replace(/[^A-Z]/g, "")
        .slice(0, MaxCodeLength);
    },

    validateForm() {
      this.errors = {};

      const airportNameRegex = /^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s.'-]+$/;
      const MaxCodeLength = 3;

      if (!this.form.country) {
        this.errors.country = true;
      }

      if (!this.form.city) {
        this.errors.city = true;
      }

      if (!this.form.airportName.trim()) {
        this.errors.nameRequired = true;
      } else if (!airportNameRegex.test(this.form.airportName.trim())) {
        this.errors.nameInvalid = true;
      }

      if (!this.form.code || this.form.code.length !== MaxCodeLength) {
        this.errors.code = true;
      }

      return Object.keys(this.errors).length === 0;
    },

    delay(milliseconds) {
      return new Promise((resolve) => {
        setTimeout(resolve, milliseconds);
      });
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
        await this.delay(1200);

        const response = await fetch(BaseURL, {
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

          await this.delay(1800);
          return;
        }

        this.alertType = "success";
        this.alertTitle = "Aeropuerto creado";
        this.alertMessage = "El aeropuerto fue creado correctamente.";

        await this.delay(1800);

        this.$router.push("/admin/airports");
      } catch (error) {
        this.alertType = "warning";
        this.alertTitle = "Error de conexión";
        this.alertMessage = "No se pudo conectar con el backend.";

        await this.delay(1800);
      } finally {
        this.isSubmitting = false;
      }
    },
  },
};
</script>

<style scoped>
.create-airport-card {
  width: 100%;
}

.create-airport-card h2 {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
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
  font-weight: 700;
  color: #333;
}

.form-group label span {
  color: #e74c3c;
}

.form-group input,
.form-group select {
  width: 100%;
  height: 48px;
  border: 1.5px solid #e0e0e0;
  border-radius: 8px;
  padding: 0 16px;
  font-size: 0.9rem;
  color: #333;
  background: #ffffff;
  outline: none;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.form-group input::placeholder {
  color: #bbb;
}

.form-group input:focus,
.form-group select:focus {
  border-color: #e74c3c;
  box-shadow: 0 0 0 3px rgba(231, 76, 60, 0.12);
}

.form-group select:disabled {
  background: #f8f9fa;
  cursor: not-allowed;
  color: #999;
}

.form-group.has-error input,
.form-group.has-error select {
  border-color: #e74c3c;
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
  color: #888;
  font-size: 0.78rem;
}

.error-text {
  color: #e74c3c !important;
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
  font-weight: 700;
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
  border-radius: 8px;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.submit-btn:hover:not(:disabled) {
  opacity: 0.9;
  transform: translateY(-1px);
}

.submit-btn:disabled {
  opacity: 0.75;
  cursor: not-allowed;
}

@media (max-width: 640px) {
  .create-airport-card {
    padding: 24px 20px;
  }
}
</style>