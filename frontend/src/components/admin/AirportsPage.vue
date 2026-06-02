<template>
  <AdminPageLayout>
    <template v-if="isListMode">
      <AdminHero
        title="Lista de Aeropuertos"
        subtitle="Panel de administración para operadores de Mushu Airlines"
        icon="bi bi-airplane-engines"
      />

      <div v-if="errorMessage" class="error-message">
        <i class="bi bi-exclamation-circle-fill"></i>
        <span>{{ errorMessage }}</span>
      </div>

      <AdminCard class="airports-card">
        <div class="card-header-row">
          <h2>Aeropuertos ({{ filteredAirports.length }})</h2>

          <RouterLink
            v-if="isAdmin"
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
                <th>País</th>
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

                <td>{{ airport.country }}</td>

                <td>{{ airport.city }}</td>

                <td>
                  <div class="actions-wrapper">
                    <button
                      type="button"
                      class="view-btn"
                      @click="openAirportDetails(airport)"
                    >
                      <i class="bi bi-eye me-1"></i>
                      Ver
                    </button>

                    <button
                      v-if="isAdmin"
                      type="button"
                      class="edit-btn"
                      @click="openAirportEdit(airport)"
                    >
                      <i class="bi bi-pencil me-1"></i>
                      Editar
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-else class="empty-state">
          <div v-if="isAdmin">
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
        </div>
      </AdminCard>
    </template>

    <template v-else-if="selectedAirport">
      <AdminHero
        title="Detalles del Aeropuerto"
        subtitle="Información completa del aeropuerto"
        icon="bi bi-airplane-engines"
      />

      <AdminCard class="details-card">
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
          <span class="airport-code airport-code-large">
            {{ selectedAirport.code }}
          </span>
        </div>

        <hr class="details-line" />

        <button type="button" class="close-details-btn" @click="closeDetails">
          Cerrar
        </button>
      </AdminCard>
    </template>

    <template v-else-if="editingAirport">
      <AdminHero
        title="Editar Aeropuerto"
        subtitle="Modificación del aeropuerto"
        icon="bi bi-airplane-engines"
      />

      <div v-if="successMessage" class="success-message">
        <i class="bi bi-check-circle-fill"></i>
        <span>{{ successMessage }}</span>
      </div>

      <div v-if="errorMessage" class="error-message">
        <i class="bi bi-exclamation-circle-fill"></i>
        <span>{{ errorMessage }}</span>
      </div>

      <AdminCard class="edit-card">
        <form @submit.prevent="saveAirportChanges">
        <div
          class="form-group"
          :class="{ 'has-error': editSubmitted && editErrors.nameInvalid }"
        >
          <label for="airportName">Nombre del Aeropuerto</label>

          <input
            id="airportName"
            ref="airportNameInput"
            v-model.trim="editAirportName"
            type="text"
            maxlength="200"
            placeholder="Ingrese el nombre del aeropuerto"
          />

          <div class="helper-row">
            <small>{{ editNameLength }}/200 caracteres</small>

            <small v-if="editSubmitted && editErrors.nameInvalid" class="error-text">
              El nombre del aeropuerto no debe contener números ni caracteres especiales como #, !, %, $.
            </small>
          </div>
        </div>

          <div class="detail-grid">
            <div class="detail-group">
              <span class="detail-label">País</span>
              <p>{{ editingAirport.country }}</p>
            </div>

            <div class="detail-group">
              <span class="detail-label">Ciudad</span>
              <p>{{ editingAirport.city }}</p>
            </div>
          </div>

          <div class="detail-group detail-full">
            <span class="detail-label">Código del Aeropuerto</span>
            <span class="airport-code airport-code-large">
              {{ editingAirport.code }}
            </span>
          </div>

          <hr class="details-line" />

          <div class="edit-actions">
            <button
              type="submit"
              class="save-btn"
              :disabled="saving || !canSaveEdit"
            >
              {{ saving ? "Guardando..." : "Guardar cambios" }}
            </button>

            <button type="button" class="cancel-btn" @click="cancelEdit">
              Cancelar
            </button>
          </div>
        </form>
      </AdminCard>
    </template>
  </AdminPageLayout>
</template>

<script>
import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";

import axios from "axios";

const BaseURL = "http://localhost:5103/api/airport";

export default {
  name: "AirportsPage",

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
      searchText: "",
      selectedAirport: null,
      editingAirport: null,
      editAirportName: "",
      editErrors: {},
      editSubmitted: false,
      airports: [],
      loading: false,
      saving: false,
      successMessage: "",
      errorMessage: "",
      userRole: null,
    };
  },

  computed: {
    isAdmin() {
      return this.userRole === "Administrator";
    },

    isListMode() {
      return !this.selectedAirport && !this.editingAirport;
    },

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

    editNameLength() {
      return this.editAirportName.length;
    },

    canSaveEdit() {
      if (!this.editingAirport) {
        return false;
      }

      const newName = this.editAirportName.trim();
      const currentName = this.editingAirport.airportName.trim();
      const maxLenghtAirportName = 200;

      return (
        newName.length > 0 &&
        newName.length <= maxLenghtAirportName &&
        newName !== currentName
      );
    },
  },

  mounted() {
    this.loadUser();
    this.loadAirports();
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

    loadUser() {
      this.userRole = this.getRoleFromToken();
    },

    async loadAirports() {
      this.loading = true;
      this.errorMessage = "";

      try {
        const response = await axios.get(BaseURL);

        this.airports = response.data.map((airport) => ({
          code: airport.code ?? airport.Code,
          airportName: airport.airportName ?? airport.AirportName,
          city: airport.city ?? airport.City,
          country: airport.country ?? airport.Country,
        }));
      } catch (error) {
        this.errorMessage = "No se pudieron cargar los aeropuertos.";
      } finally {
        this.loading = false;
      }
    },

    openAirportDetails(airport) {
      this.selectedAirport = { ...airport };
      this.editingAirport = null;
      this.searchText = "";
      this.successMessage = "";
      this.errorMessage = "";

      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    closeDetails() {
      this.selectedAirport = null;
      this.errorMessage = "";

      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    openAirportEdit(airport) {
      this.editingAirport = { ...airport };
      this.editAirportName = airport.airportName;
      this.selectedAirport = null;
      this.searchText = "";
      this.successMessage = "";
      this.errorMessage = "";
      this.editErrors = {};
      this.editSubmitted = false;

      this.$nextTick(() => {
        this.$refs.airportNameInput?.focus();
      });

      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    cancelEdit() {
      this.editingAirport = null;
      this.editAirportName = "";
      this.editErrors = {};
      this.editSubmitted = false;
      this.errorMessage = "";

      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    validateEditForm() {
      this.editErrors = {};

      const airportNameRegex = /^[A-Za-zÁÉÍÓÚáéíóúÑñÜü\s.'-]+$/;
      const newName = this.editAirportName.trim();

      if (newName && !airportNameRegex.test(newName)) {
        this.editErrors.nameInvalid = true;
      }

      return Object.keys(this.editErrors).length === 0;
    },

    delay(milliseconds) {
      return new Promise((resolve) => {
        setTimeout(resolve, milliseconds);
      });
    },

    async saveAirportChanges() {
      this.editSubmitted = true;
      this.successMessage = "";
      this.errorMessage = "";

      if (!this.validateEditForm()) {
        this.errorMessage = "El nombre del aeropuerto contiene números o caracteres especiales no permitidos.";
        return;
      }

      this.saving = true;

      const newName = this.editAirportName.trim();

      try {
        await this.delay(1200);

        await axios.put(`${BaseURL}/${this.editingAirport.code}`, {
          code: this.editingAirport.code,
          airportName: newName,
          city: this.editingAirport.city,
          country: this.editingAirport.country,
        });

        const airportIndex = this.airports.findIndex(
          (airport) => airport.code === this.editingAirport.code
        );

        if (airportIndex !== -1) {
          this.airports.splice(airportIndex, 1, {
            ...this.airports[airportIndex],
            airportName: newName,
          });
        }

        this.successMessage = "El nombre del aeropuerto fue actualizado correctamente.";

        await this.delay(1800);

        this.editingAirport = null;
        this.editAirportName = "";
        this.editErrors = {};
        this.editSubmitted = false;

        window.scrollTo({ top: 0, behavior: "smooth" });
      } catch (error) {
        this.errorMessage = "No se pudo actualizar el aeropuerto.";

        if (error.response?.data) {
          this.errorMessage = error.response.data;
        }

        await this.delay(1800);
      } finally {
        this.saving = false;
      }
    },
  },
};
</script>

<style scoped>
.success-message,
.error-message {
  display: flex;
  align-items: center;
  gap: 10px;
  border-radius: 12px;
  padding: 14px 16px;
  margin-bottom: 20px;
  font-size: 0.92rem;
  font-weight: 700;
}

.success-message {
  background: #ecfdf5;
  color: #166534;
  border: 1px solid #bbf7d0;
}

.error-message {
  background: #fef2f2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.success-message i,
.error-message i {
  font-size: 1.1rem;
}

.airports-card {
  padding: 0;
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
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
}

.create-airport-btn,
.empty-create-btn {
  text-decoration: none;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
  border: none;
  border-radius: 8px;
  padding: 11px 18px;
  font-size: 0.9rem;
  font-weight: 600;
  display: inline-flex;
  align-items: center;
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.create-airport-btn:hover,
.empty-create-btn:hover {
  color: #ffffff;
  opacity: 0.9;
  transform: translateY(-1px);
}

.search-wrapper {
  margin: 0 24px 24px;
  height: 44px;
  border: 1.5px solid #e0e0e0;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 13px;
  background: #ffffff;
  transition: border-color 0.2s ease;
}

.search-wrapper:focus-within {
  border-color: #e74c3c;
}

.search-wrapper i {
  color: #bbb;
  font-size: 1rem;
}

.search-wrapper input {
  width: 100%;
  height: 100%;
  border: none;
  outline: none;
  color: #333;
  font-size: 0.88rem;
}

.search-wrapper input::placeholder {
  color: #bbb;
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
  background: #f8f9fa;
}

.airports-table th {
  padding: 14px 24px;
  color: #888;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  text-align: left;
  font-weight: 700;
}

.airports-table td {
  padding: 18px 24px;
  border-top: 1px solid #f0f0f0;
  color: #333;
}

.airport-code {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: #fff0ee;
  color: #e74c3c;
  border-radius: 6px;
  padding: 4px 9px;
  font-size: 0.78rem;
  font-weight: 700;
}

.airport-code-large {
  font-size: 1rem;
  padding: 8px 13px;
  margin-top: 4px;
}

.actions-wrapper {
  display: flex;
  align-items: center;
  gap: 14px;
}

.view-btn,
.edit-btn {
  border: none;
  background: transparent;
  font-size: 0.88rem;
  font-weight: 700;
  padding: 0;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
}

.view-btn {
  color: #e74c3c;
}

.edit-btn {
  color: #2563eb;
}

.view-btn:hover,
.edit-btn:hover {
  text-decoration: underline;
}

.view-btn:hover {
  color: #c0392b;
}

.edit-btn:hover {
  color: #1d4ed8;
}

.empty-state {
  margin: 0 24px 24px;
  border: 1.5px dashed #e0e0e0;
  background: #f8f9fa;
  border-radius: 12px;
  padding: 34px 24px;
  text-align: center;
}

.empty-icon {
  width: 56px;
  height: 56px;
  margin: 0 auto 14px;
  border-radius: 14px;
  background: #fff0ee;
  color: #e74c3c;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.6rem;
}

.empty-state h3 {
  margin: 0 0 8px;
  font-size: 1rem;
  font-weight: 700;
  color: #333;
}

.empty-state p {
  margin: 0 auto 18px;
  max-width: 440px;
  color: #888;
  font-size: 0.92rem;
  line-height: 1.5;
}

.details-card {
  width: 100%;
  max-width: 100%;
  margin: 0;
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
  color: #666f83;
  font-size: 0.82rem;
  font-weight: 700;
}

.detail-group p {
  margin: 0;
  color: #001233;
  font-size: 1.05rem;
}

.details-line {
  border: none;
  border-top: 1px solid var(--border-color);
  margin: 24px 0 16px;
}

.details-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.edit-details-btn,
.close-details-btn {
  width: 100%;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 700;
  padding: 14px 18px;
  cursor: pointer;
  transition: background 0.2s ease, transform 0.2s ease;
}

.close-details-btn {
  background: #f3f4f6;
  color: #1f2937;
}

.close-details-btn:hover {
  transform: translateY(-1px);
}

.close-details-btn:hover {
  background: #e5e7eb;
}

.hero-back-btn {
  border: none;
  background: transparent;
  color: #ffffff;
  padding: 0;
  margin-bottom: 16px;
  font-size: 0.88rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.hero-back-btn:hover {
  text-decoration: underline;
}

.edit-card {
  width: 100%;
}

.form-group {
  display: flex;
  flex-direction: column;
  margin-bottom: 26px;
}

.form-group label {
  color: #666f83;
  font-size: 0.82rem;
  font-weight: 700;
  margin-bottom: 8px;
}

.form-group input {
  width: 100%;
  height: 40px;
  border: 1.5px solid #d1d5db;
  border-radius: 8px;
  padding: 0 14px;
  font-size: 0.9rem;
  color: #001233;
  outline: none;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.form-group input:focus {
  border-color: #ff5f00;
  box-shadow: 0 0 0 3px rgba(255, 95, 0, 0.12);
}

.form-group small {
  margin-top: 6px;
  color: #666f83;
  font-size: 0.76rem;
}

.edit-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.save-btn,
.cancel-btn {
  border: none;
  border-radius: 8px;
  padding: 13px 18px;
  font-size: 0.9rem;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.2s ease, transform 0.2s ease, background 0.2s ease;
}

.save-btn {
  background: #ff5f00;
  color: #ffffff;
}

.save-btn:hover:not(:disabled),
.cancel-btn:hover {
  transform: translateY(-1px);
}

.save-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.cancel-btn {
  background: #f3f4f6;
  color: #1f2937;
}

.cancel-btn:hover {
  background: #e5e7eb;
}

@media (max-width: 768px) {
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

  .actions-wrapper {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .details-actions,
  .edit-actions {
    grid-template-columns: 1fr;
  }
}

.helper-row {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 4px;
  margin-top: 6px;
}

.error-text {
  color: #e74c3c !important;
  font-weight: 700;
}

.form-group.has-error input {
  border-color: #e74c3c;
  background: #fff7f7;
}

.form-group.has-error input:focus {
  border-color: #e74c3c;
  box-shadow: 0 0 0 3px rgba(231, 76, 60, 0.12);
}
</style>
