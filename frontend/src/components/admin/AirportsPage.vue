<template>
  <AdminPageLayout>
    <template v-if="!selectedAirport">
      <AdminHero
        title="Lista de Aeropuertos"
        subtitle="Panel de administración para operadores de Mushu Airlines"
        icon="bi bi-airplane-engines"
        back-to="/admin"
        back-text="Volver al panel"
      />

      <div v-if="successMessage" class="success-message">
        <i class="bi bi-check-circle-fill"></i>
        <span>{{ successMessage }}</span>
      </div>
      
      <AdminCard class="airports-card">
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
      </AdminCard>
    </template>

    <template v-else>
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
          <span class="airport-code airport-code-large">{{ selectedAirport.code }}</span>
        </div>

        <hr class="details-line" />

        <button type="button" class="close-details-btn" @click="closeDetails">
          Cerrar
        </button>
      </AdminCard>
    </template>
  </AdminPageLayout>
</template>

<script>
import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue"

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
    async loadAirports() {
      this.loading = true;

      try {
        const response = await fetch("http://localhost:5103/api/airport");

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
  font-weight: 700;
}

.success-message i {
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

.view-btn {
  border: none;
  background: transparent;
  color: #e74c3c;
  font-size: 0.88rem;
  font-weight: 700;
  padding: 0;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
}

.view-btn:hover {
  color: #c0392b;
  text-decoration: underline;
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
  color: #888;
  font-size: 0.82rem;
  font-weight: 700;
}

.detail-group p {
  margin: 0;
  color: #333;
  font-size: 1.05rem;
}

.details-line {
  border: none;
  border-top: 1px solid #f0f0f0;
  margin: 24px 0 16px;
}

.close-details-btn {
  width: 100%;
  border: none;
  border-radius: 8px;
  background: #f8f9fa;
  color: #333;
  font-size: 0.9rem;
  font-weight: 700;
  padding: 14px 18px;
  cursor: pointer;
  transition: background 0.2s ease;
}

.close-details-btn:hover {
  background: #f0f0f0;
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
}
</style>
