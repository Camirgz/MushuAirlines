<template>
  <AdminPageLayout>
    <template v-if="isListMode">
      <AdminHero
        title="Lista de Aeronaves"
        subtitle="Panel de administración para operadores de Mushu Airlines"
        icon="bi bi-airplane"
        back-to="/admin"
        back-text="Volver al panel"
      />

      <div v-if="successMessage" class="success-message">
        <i class="bi bi-check-circle-fill"></i>
        <span>{{ successMessage }}</span>
      </div>

      <div v-if="errorMessage" class="error-message">
        <i class="bi bi-exclamation-circle-fill"></i>
        <span>{{ errorMessage }}</span>
      </div>

      <AdminCard class="aircraft-card">
        <div class="card-header-row">
          <h2>
            Aeronaves ({{ filteredAircraftTypes.length }}
            <span v-if="searchQuery" class="total-hint">
              de {{ aircraftTypes.length }}
            </span>)
          </h2>

          <RouterLink
            v-if="isAdmin"
            to="/admin/aircraft-types/create"
            class="create-aircraft-btn"
          >
            <i class="bi bi-plus-lg me-2"></i>
            Crear Aeronave
          </RouterLink>
        </div>

        <div class="search-wrapper">
          <i class="bi bi-search"></i>

          <input
            v-model="searchQuery"
            type="text"
            placeholder="Buscar por modelo o tipo..."
            aria-label="Buscar aeronave"
          />
        </div>

        <div v-if="isLoading" class="status-message">
          <i class="bi bi-arrow-repeat spin"></i>
          <span>Cargando aeronaves...</span>
        </div>

        <template v-else>
          <div v-if="filteredAircraftTypes.length > 0" class="table-wrapper">
            <table class="aircraft-table">
              <thead>
                <tr>
                  <th>Modelo</th>
                  <th>Tipo</th>
                  <th>Peso (KG)</th>
                  <th>Capacidad</th>
                  <th>Acciones</th>
                </tr>
              </thead>

              <tbody>
                <tr
                  v-for="aircraft in paginatedAircraftTypes"
                  :key="aircraft.id ?? aircraft.model"
                >
                  <td>
                    <span class="model-code">{{ aircraft.model }}</span>
                  </td>

                  <td>{{ aircraft.type }}</td>

                  <td>{{ formatNumber(aircraft.weightKg) }} kg</td>

                  <td>
                    <span class="capacity-badge">
                      {{ aircraft.capacity }} asientos
                    </span>
                  </td>

                  <td>
                    <div class="actions-wrapper">
                      <button
                        type="button"
                        class="view-btn"
                        @click="openAircraftDetails(aircraft)"
                      >
                        <i class="bi bi-eye me-1"></i>
                        Ver
                      </button>

                      <button
                        v-if="isAdmin"
                        type="button"
                        class="edit-btn"
                        @click="openAircraftEdit(aircraft)"
                      >
                        <i class="bi bi-pencil me-1"></i>
                        Editar
                      </button>

                      <button
                        v-if="isAdmin"
                        type="button"
                        class="delete-btn"
                        @click="openDeleteModal(aircraft)"
                      >
                        <i class="bi bi-trash me-1"></i>
                        Eliminar
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>

            <div v-if="totalPages > 1" class="pagination">
              <button
                class="page-btn"
                type="button"
                :disabled="currentPage === 1"
                @click="goToPage(currentPage - 1)"
              >
                <i class="bi bi-chevron-left"></i>
              </button>

              <button
                v-for="page in totalPages"
                :key="page"
                class="page-btn"
                type="button"
                :class="{ 'page-btn--active': page === currentPage }"
                @click="goToPage(page)"
              >
                {{ page }}
              </button>

              <button
                class="page-btn"
                type="button"
                :disabled="currentPage === totalPages"
                @click="goToPage(currentPage + 1)"
              >
                <i class="bi bi-chevron-right"></i>
              </button>

              <span class="page-info">
                Página {{ currentPage }} de {{ totalPages }}
              </span>
            </div>
          </div>

          <div v-else class="empty-state">
            <div class="empty-icon">
              <i class="bi bi-airplane"></i>
            </div>

            <h3>{{ emptyTitle }}</h3>
            <p>{{ emptyDescription }}</p>

            <RouterLink
              v-if="isAdmin"
              to="/admin/aircraft-types/create"
              class="empty-create-btn"
            >
              <i class="bi bi-plus-lg me-2"></i>
              Crear primera aeronave
            </RouterLink>
          </div>
        </template>
      </AdminCard>
    </template>

    <template v-else-if="selectedAircraft">
      <AdminHero
        title="Detalles de la Aeronave"
        subtitle="Información completa de la aeronave"
        icon="bi bi-airplane"
      />

      <AdminCard class="details-card">
        <div class="detail-group detail-full">
          <span class="detail-label">Modelo</span>
          <span class="model-code model-code-large">
            {{ selectedAircraft.model }}
          </span>
        </div>

        <hr class="details-line" />

        <div class="detail-grid">
          <div class="detail-group">
            <span class="detail-label">Tipo de aeronave</span>
            <p>{{ selectedAircraft.type }}</p>
          </div>

          <div class="detail-group">
            <span class="detail-label">Peso soportado</span>
            <p>{{ formatNumber(selectedAircraft.weightKg) }} kg</p>
          </div>
        </div>

        <hr class="details-line" />

        <div class="detail-group detail-full">
          <span class="detail-label">Primera Clase</span>

          <div class="class-summary first-class-summary">
            <div class="summary-box">
              <span>Filas</span>
              <strong>{{ selectedAircraft.firstClassRows }}</strong>
            </div>

            <div class="summary-box">
              <span>Asientos/fila</span>
              <strong>{{ selectedAircraft.firstClassSeatsPerRow }}</strong>
            </div>

            <div class="summary-box summary-box-strong">
              <span>Subtotal</span>
              <strong>{{ firstClassSubtotal(selectedAircraft) }}</strong>
            </div>
          </div>
        </div>

        <hr class="details-line" />

        <div class="detail-group detail-full">
          <span class="detail-label">Clase Turista</span>

          <div class="class-summary economy-summary">
            <div class="summary-box">
              <span>Filas</span>
              <strong>{{ selectedAircraft.economyRows }}</strong>
            </div>

            <div class="summary-box">
              <span>Asientos/fila</span>
              <strong>{{ selectedAircraft.economySeatsPerRow }}</strong>
            </div>

            <div class="summary-box summary-box-strong">
              <span>Subtotal</span>
              <strong>{{ economySubtotal(selectedAircraft) }}</strong>
            </div>
          </div>
        </div>

        <hr class="details-line" />

        <div class="detail-group detail-full">
          <span class="detail-label">Capacidad total</span>

          <div class="total-capacity-box">
            <i class="bi bi-people"></i>
            <strong>{{ selectedAircraft.capacity }} asientos</strong>
          </div>
        </div>

        <hr class="details-line" />

        <button type="button" class="close-details-btn" @click="closeMode">
            Cerrar
        </button>
      </AdminCard>
    </template>

    <template v-else-if="editingAircraft">
      <AdminHero
        title="Editar Aeronave"
        subtitle="Solo se permite aumentar valores numéricos"
        icon="bi bi-airplane"
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
        <form @submit.prevent="saveAircraftChanges">

          <div class="detail-group detail-full">
            <span class="detail-label">Modelo</span>
            <span class="model-code model-code-large">
              {{ editingAircraft.model }}
            </span>
          </div>
                    
          <hr class="details-line" />

          <div class="detail-group">
              <span class="detail-label">Tipo de aeronave</span>
              <p>{{ editingAircraft.type }}</p>
          </div>

          <hr class="details-line" />

          <div
            class="form-group"
            :class="{ 'has-error': editSubmitted && editErrors.weightKg }"
          >
            <label class="form-label">
              Peso soportado (kg) <span class="required">*</span>
            </label>

            <input
              v-model.number="editForm.weightKg"
              type="number"
              class="form-input"
              :min="editingAircraft.weightKg"
              required
            />

            <small>Actual: {{ formatNumber(editingAircraft.weightKg) }} kg</small>

            <small
              v-if="editSubmitted && editErrors.weightKg"
              class="error-text"
            >
              El peso soportado debe ser mayor al valor actual.
            </small>
          </div>

          <hr class="section-divider" />

          <h3 class="section-title">Primera Clase</h3>

          <div class="form-row">
            <div
              class="form-group"
              :class="{ 'has-error': editSubmitted && editErrors.firstClassRows }"
            >
              <label class="form-label">
                Cantidad de filas <span class="required">*</span>
              </label>

              <input
                v-model.number="editForm.firstClassRows"
                type="number"
                class="form-input"
                :min="editingAircraft.firstClassRows"
                required
              />

              <small>Actual: {{ editingAircraft.firstClassRows }}</small>

              <small
                v-if="editSubmitted && editErrors.firstClassRows"
                class="error-text"
              >
                Debe ser mayor al valor actual.
              </small>
            </div>

            <div
              class="form-group"
              :class="{
                'has-error':
                  editSubmitted && editErrors.firstClassSeatsPerRow,
              }"
            >
              <label class="form-label">
                Asientos por fila <span class="required">*</span>
              </label>

              <input
                v-model.number="editForm.firstClassSeatsPerRow"
                type="number"
                class="form-input"
                :min="editingAircraft.firstClassSeatsPerRow"
                required
              />

              <small>Actual: {{ editingAircraft.firstClassSeatsPerRow }}</small>

              <small
                v-if="editSubmitted && editErrors.firstClassSeatsPerRow"
                class="error-text"
              >
                Debe ser mayor al valor actual.
              </small>
            </div>
          </div>

          <hr class="section-divider" />

          <h3 class="section-title">Clase Turista</h3>

          <div class="form-row">
            <div
              class="form-group"
              :class="{ 'has-error': editSubmitted && editErrors.economyRows }"
            >
              <label class="form-label">
                Cantidad de filas <span class="required">*</span>
              </label>

              <input
                v-model.number="editForm.economyRows"
                type="number"
                class="form-input"
                :min="editingAircraft.economyRows"
                required
              />

              <small>Actual: {{ editingAircraft.economyRows }}</small>

              <small
                v-if="editSubmitted && editErrors.economyRows"
                class="error-text"
              >
                Debe ser mayor al valor actual.
              </small>
            </div>

            <div
              class="form-group"
              :class="{
                'has-error':
                  editSubmitted && editErrors.economySeatsPerRow,
              }"
            >
              <label class="form-label">
                Asientos por fila <span class="required">*</span>
              </label>

              <input
                v-model.number="editForm.economySeatsPerRow"
                type="number"
                class="form-input"
                :min="editingAircraft.economySeatsPerRow"
                required
              />

              <small>Actual: {{ editingAircraft.economySeatsPerRow }}</small>

              <small
                v-if="editSubmitted && editErrors.economySeatsPerRow"
                class="error-text"
              >
                Debe ser mayor al valor actual.
              </small>
            </div>
          </div>

          <div
            class="seat-counter"
            :class="{ 'seat-counter--danger': editTotalSeats >= 1000 }"
          >
            <i class="bi bi-person-fill me-2"></i>

            Nueva capacidad total:
            <strong>{{ editTotalSeats }} asientos</strong>

            <span v-if="editTotalSeats >= 1000" class="seat-limit-msg">
              — máximo permitido: 999
            </span>
          </div>

          <div class="edit-note">
            <i class="bi bi-info-circle"></i>
            Solo se permite aumentar los valores. No se puede disminuir ni mantener
            el mismo número.
          </div>

          <div class="edit-actions">
            <button
              type="submit"
              class="save-btn"
              :disabled="isSubmitting || !canSaveEdit"
            >
              {{ isSubmitting ? "Guardando..." : "Guardar cambios" }}
            </button>

            <button type="button" class="cancel-btn" @click="closeMode">
              Cancelar
            </button>
          </div>
        </form>
      </AdminCard>
    </template>

    <div
      v-if="showDeleteModal"
      class="delete-modal-overlay"
      @click.self="closeDeleteModal"
    >
      <div class="delete-modal">
        <button
          type="button"
          class="delete-modal-close"
          @click="closeDeleteModal"
          aria-label="Cerrar modal"
          :disabled="deletingAircraft"
        >
          <i class="bi bi-x-lg"></i>
        </button>

        <div class="delete-modal-icon">
          <i class="bi bi-trash"></i>
        </div>

        <h3 class="delete-modal-title">Eliminar aeronave</h3>

        <p class="delete-modal-text">
          ¿Estás seguro de eliminar
          <strong>{{ aircraftToDelete?.model }}</strong>?
        </p>

        <p class="delete-modal-warning">
          Esta acción es irreversible.
        </p>

        <div class="delete-modal-actions">
          <button
            type="button"
            class="delete-cancel-btn"
            @click="closeDeleteModal"
            :disabled="deletingAircraft"
          >
            Cancelar
          </button>

          <button
            type="button"
            class="delete-confirm-btn"
            @click="confirmDeleteAircraft"
            :disabled="deletingAircraft"
          >
            {{ deletingAircraft ? "Eliminando..." : "Eliminar" }}
          </button>
        </div>
      </div>
    </div>
  </AdminPageLayout>
</template>

<script>
import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";

import {
  getAircraftTypes,
  getAircraftTypeById,
  updateAircraftType,
  deleteAircraftType,
} from "../../services/AircraftTypesService";

const minPage = 1;
const maxSeatCapacity = 1000;

export default {
  name: "AircraftTypesPage",

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
      aircraftTypes: [],
      searchQuery: "",
      currentPage: 1,
      pageSize: 10,
      isLoading: false,
      isSubmitting: false,
      errorMessage: "",
      successMessage: "",
      selectedAircraft: null,
      editingAircraft: null,
      showDeleteModal: false,
      aircraftToDelete: null,
      deletingAircraft: false,
      editSubmitted: false,
      editErrors: {},
      editForm: {
        weightKg: null,
        economyRows: null,
        economySeatsPerRow: null,
        firstClassRows: null,
        firstClassSeatsPerRow: null,
      },
      userRole: null,
    };
  },

  computed: {
    isAdmin() {
      return this.userRole === "Administrator";
    },

    isListMode() {
      return !this.selectedAircraft && !this.editingAircraft;
    },

    filteredAircraftTypes() {
      const query = this.searchQuery.trim().toLowerCase();

      if (!query) {
        return this.aircraftTypes;
      }

      return this.aircraftTypes.filter((aircraft) => {
        return (
          (aircraft.model ?? "").toLowerCase().includes(query) ||
          (aircraft.type ?? "").toLowerCase().includes(query)
        );
      });
    },

    totalPages() {
      return Math.ceil(this.filteredAircraftTypes.length / this.pageSize) || minPage;
    },

    paginatedAircraftTypes() {
      const start = (this.currentPage - minPage) * this.pageSize;

      return this.filteredAircraftTypes.slice(start, start + this.pageSize);
    },

    emptyTitle() {
      return this.aircraftTypes.length === 0
        ? "No hay aeronaves creadas"
        : "No se encontraron aeronaves";
    },

    emptyDescription() {
      return this.aircraftTypes.length === 0
        ? "Cuando registre aeronaves, aparecerán en esta lista."
        : "Intente buscar por otro modelo o tipo.";
    },

    editTotalSeats() {
      const economy =
        (this.editForm.economyRows || 0) *
        (this.editForm.economySeatsPerRow || 0);

      const firstClass =
        (this.editForm.firstClassRows || 0) *
        (this.editForm.firstClassSeatsPerRow || 0);

      return economy + firstClass;
    },

    canSaveEdit() {
      if (!this.editingAircraft) {
        return false;
      }

      const hasIncreasedValue =
        this.editForm.weightKg > this.editingAircraft.weightKg ||
        this.editForm.economyRows > this.editingAircraft.economyRows ||
        this.editForm.economySeatsPerRow >
          this.editingAircraft.economySeatsPerRow ||
        this.editForm.firstClassRows > this.editingAircraft.firstClassRows ||
        this.editForm.firstClassSeatsPerRow >
          this.editingAircraft.firstClassSeatsPerRow;

      const hasDecreasedValue =
        this.editForm.weightKg < this.editingAircraft.weightKg ||
        this.editForm.economyRows < this.editingAircraft.economyRows ||
        this.editForm.economySeatsPerRow <
          this.editingAircraft.economySeatsPerRow ||
        this.editForm.firstClassRows < this.editingAircraft.firstClassRows ||
        this.editForm.firstClassSeatsPerRow <
          this.editingAircraft.firstClassSeatsPerRow;

      return (
        this.editTotalSeats < maxSeatCapacity &&
        hasIncreasedValue &&
        !hasDecreasedValue
      );
    },
  },

  watch: {
    searchQuery() {
      this.currentPage = minPage;
    },
  },

  mounted() {
    this.loadUser();
    this.loadAircraftTypes();
  },

  methods: {
    getRoleFromToken() {
      const token = localStorage.getItem("token");

      if (!token) {
        return null;
      }

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

    loadAircraftTypes() {
      this.isLoading = true;
      this.errorMessage = "";

      getAircraftTypes()
        .then((response) => {
          this.aircraftTypes = response.data.map((aircraft) =>
            this.normalizeAircraft(aircraft)
          );
        })
        .catch(() => {
          this.errorMessage =
            "No se pudieron cargar las aeronaves. Intente de nuevo.";
        })
        .finally(() => {
          this.isLoading = false;
        });
    },

    normalizeAircraft(aircraft) {
      const firstClassRows = Number(
        aircraft.firstClassRows ??
          aircraft.FirstClassRows ??
          aircraft.firstClass?.rowCount ??
          aircraft.FirstClass?.RowCount ??
          0
      );

      const firstClassSeatsPerRow = Number(
        aircraft.firstClassSeatsPerRow ??
          aircraft.FirstClassSeatsPerRow ??
          aircraft.firstClass?.seatsPerRow ??
          aircraft.FirstClass?.SeatsPerRow ??
          0
      );

      const economyRows = Number(
        aircraft.economyRows ??
          aircraft.EconomyRows ??
          aircraft.economyClass?.rowCount ??
          aircraft.EconomyClass?.RowCount ??
          0
      );

      const economySeatsPerRow = Number(
        aircraft.economySeatsPerRow ??
          aircraft.EconomySeatsPerRow ??
          aircraft.economyClass?.seatsPerRow ??
          aircraft.EconomyClass?.SeatsPerRow ??
          0
      );

      const calculatedCapacity =
        firstClassRows * firstClassSeatsPerRow +
        economyRows * economySeatsPerRow;

      return {
        id:
          aircraft.id ??
          aircraft.Id ??
          aircraft.code ??
          aircraft.Code ??
          aircraft.aircraftTypeId ??
          aircraft.AircraftTypeId ??
          null,
        model: aircraft.model ?? aircraft.Model ?? "",
        type: aircraft.type ?? aircraft.Type ?? "",
        weightKg: Number(aircraft.weightKg ?? aircraft.WeightKg ?? 0),
        firstClassRows,
        firstClassSeatsPerRow,
        economyRows,
        economySeatsPerRow,
        capacity: Number(
          aircraft.capacity ?? aircraft.Capacity ?? calculatedCapacity
        ),
      };
    },

    async openAircraftDetails(aircraft) {
      this.successMessage = "";
      this.errorMessage = "";
      this.searchQuery = "";

      const aircraftId =
        aircraft.id ??
        aircraft.code ??
        aircraft.Code ??
        null;

      if (!aircraftId) {
        this.selectedAircraft = { ...aircraft };
        this.editingAircraft = null;
        window.scrollTo({ top: 0, behavior: "smooth" });
        return;
      }

      try {
        const response = await getAircraftTypeById(aircraftId);
        this.selectedAircraft = this.normalizeAircraft(response.data);
        this.editingAircraft = null;

        window.scrollTo({ top: 0, behavior: "smooth" });
      } catch (error) {
        this.selectedAircraft = { ...aircraft };
        this.editingAircraft = null;
        window.scrollTo({ top: 0, behavior: "smooth" });
      }
    },

    async openAircraftEdit(aircraft) {
      this.successMessage = "";
      this.errorMessage = "";
      this.editSubmitted = false;
      this.editErrors = {};
      this.searchQuery = "";

      const aircraftId =
        aircraft.id ??
        aircraft.code ??
        aircraft.Code ??
        null;

      let currentAircraft = { ...aircraft };

      if (aircraftId) {
        try {
          const response = await getAircraftTypeById(aircraftId);
          currentAircraft = this.normalizeAircraft(response.data);
        } catch (error) {
          currentAircraft = { ...aircraft };
        }
      }

      this.editingAircraft = currentAircraft;
      this.selectedAircraft = null;

      this.editForm = {
        weightKg: currentAircraft.weightKg,
        economyRows: currentAircraft.economyRows,
        economySeatsPerRow: currentAircraft.economySeatsPerRow,
        firstClassRows: currentAircraft.firstClassRows,
        firstClassSeatsPerRow: currentAircraft.firstClassSeatsPerRow,
      };

      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    closeMode() {
      this.selectedAircraft = null;
      this.editingAircraft = null;
      this.successMessage = "";
      this.errorMessage = "";
      this.editSubmitted = false;
      this.editErrors = {};

      window.scrollTo({ top: 0, behavior: "smooth" });
    },

    validateEditForm() {
      this.editErrors = {};

      const hasIncreasedValue =
        this.editForm.weightKg > this.editingAircraft.weightKg ||
        this.editForm.economyRows > this.editingAircraft.economyRows ||
        this.editForm.economySeatsPerRow >
          this.editingAircraft.economySeatsPerRow ||
        this.editForm.firstClassRows > this.editingAircraft.firstClassRows ||
        this.editForm.firstClassSeatsPerRow >
          this.editingAircraft.firstClassSeatsPerRow;

      if (this.editForm.weightKg < this.editingAircraft.weightKg) {
        this.editErrors.weightKg = true;
      }

      if (this.editForm.firstClassRows < this.editingAircraft.firstClassRows) {
        this.editErrors.firstClassRows = true;
      }

      if (
        this.editForm.firstClassSeatsPerRow <
        this.editingAircraft.firstClassSeatsPerRow
      ) {
        this.editErrors.firstClassSeatsPerRow = true;
      }

      if (this.editForm.economyRows < this.editingAircraft.economyRows) {
        this.editErrors.economyRows = true;
      }

      if (
        this.editForm.economySeatsPerRow <
        this.editingAircraft.economySeatsPerRow
      ) {
        this.editErrors.economySeatsPerRow = true;
      }

      if (this.editTotalSeats >= maxSeatCapacity) {
        this.editErrors.capacity = true;
      }

      if (!hasIncreasedValue) {
        this.editErrors.noChanges = true;
      }

      return Object.keys(this.editErrors).length === 0;
    },

    saveAircraftChanges() {
      this.editSubmitted = true;
      this.successMessage = "";
      this.errorMessage = "";

      if (!this.validateEditForm()) {
        this.errorMessage =
          "Debe aumentar al menos un valor. No se permite disminuir valores y la capacidad total debe ser menor a 1000.";
        return;
      }

      this.isSubmitting = true;

      const payload = {
        WeightKg: this.editForm.weightKg,
        EconomyRows: this.editForm.economyRows,
        EconomySeatsPerRow: this.editForm.economySeatsPerRow,
        FirstClassRows: this.editForm.firstClassRows,
        FirstClassSeatsPerRow: this.editForm.firstClassSeatsPerRow,
      };

      updateAircraftType(this.editingAircraft.id, payload)
        .then(() => {
          const updatedAircraft = this.normalizeAircraft({
            ...this.editingAircraft,
            weightKg: this.editForm.weightKg,
            economyRows: this.editForm.economyRows,
            economySeatsPerRow: this.editForm.economySeatsPerRow,
            firstClassRows: this.editForm.firstClassRows,
            firstClassSeatsPerRow: this.editForm.firstClassSeatsPerRow,
            capacity: this.editTotalSeats,
          });

          const aircraftIndex = this.aircraftTypes.findIndex(
            (aircraft) =>
              aircraft.id === this.editingAircraft.id ||
              aircraft.model === this.editingAircraft.model
          );

          if (aircraftIndex !== -1) {
            this.aircraftTypes.splice(aircraftIndex, 1, updatedAircraft);
          }

          this.successMessage = "La aeronave fue actualizada correctamente.";
          this.editingAircraft = null;
          this.editSubmitted = false;
          this.editErrors = {};

          window.scrollTo({ top: 0, behavior: "smooth" });
        })
        .catch((error) => {
          this.errorMessage = this.parseError(error);
        })
        .finally(() => {
          this.isSubmitting = false;
        });
    },

    firstClassSubtotal(aircraft) {
      return aircraft.firstClassRows * aircraft.firstClassSeatsPerRow;
    },

    economySubtotal(aircraft) {
      return aircraft.economyRows * aircraft.economySeatsPerRow;
    },

    formatNumber(value) {
      return Number(value ?? 0).toLocaleString();
    },

    openDeleteModal(aircraft) {
      this.aircraftToDelete = { ...aircraft };
      this.showDeleteModal = true;
      this.successMessage = "";
      this.errorMessage = "";
    },

    closeDeleteModal() {
      if (this.deletingAircraft) {
        return;
      }

      this.showDeleteModal = false;
      this.aircraftToDelete = null;
    },

    confirmDeleteAircraft() {
      if (!this.aircraftToDelete) {
        return;
      }

      this.deletingAircraft = true;
      this.successMessage = "";
      this.errorMessage = "";

      deleteAircraftType(this.aircraftToDelete.id)
        .then(() => {
          this.aircraftTypes = this.aircraftTypes.filter(
            (aircraft) => aircraft.id !== this.aircraftToDelete.id
          );

          if (this.currentPage > this.totalPages) {
            this.currentPage = this.totalPages;
          }

          this.successMessage = `Aeronave "${this.aircraftToDelete.model}" eliminada correctamente.`;

          this.showDeleteModal = false;
          this.aircraftToDelete = null;

          window.scrollTo({ top: 0, behavior: "smooth" });
        })
        .catch((error) => {
          this.errorMessage = this.parseError(error);
        })
        .finally(() => {
          this.deletingAircraft = false;
        });
    },

    goToPage(page) {
      if (page < minPage || page > this.totalPages) {
        return;
      }

      this.currentPage = page;
    },

    parseError(error) {
      if (!error.response) {
        return "No se pudo conectar con el servidor. Verifique su conexión e intente de nuevo.";
      }

      const data = error.response.data;

      if (data && typeof data === "object") {
        if (data.errors) {
          const messages = Object.values(data.errors).flat();

          if (messages.length) {
            return messages.join(" ");
          }
        }

        if (data.title) {
          return data.title;
        }

        return "Ocurrió un error inesperado. Intente de nuevo.";
      }

      if (typeof data === "string") {
        if (data.includes("aeronave")) {
          return data;
        }

        if (data.startsWith("Ya existe")) {
          return data;
        }

        if (data.startsWith("La capacidad total")) {
          return data;
        }

        if (data.includes("CHK_SeatsPerRow")) {
          return "El número de asientos por fila no está dentro del rango permitido para este tipo de aeronave.";
        }

        if (data.includes("CHECK constraint")) {
          return "Los datos ingresados no cumplen las restricciones de la aeronave. Verifique los valores e intente de nuevo.";
        }

        if (data.includes("PRIMARY KEY") || data.includes("UNIQUE KEY")) {
          return "Ya existe una aeronave registrada con esos datos.";
        }

        if (data.includes("FOREIGN KEY")) {
          return "Uno de los valores ingresados no corresponde a un registro existente.";
        }
      }

      return "Ocurrió un error al guardar la aeronave. Verifique los datos e intente de nuevo.";
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

.aircraft-card {
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

.total-hint {
  font-weight: 400;
  color: #888;
}

.create-aircraft-btn,
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

.create-aircraft-btn:hover,
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

.status-message {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 24px;
  color: #888;
  font-size: 0.92rem;
  font-weight: 600;
}

.table-wrapper {
  overflow-x: auto;
}

.aircraft-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}

.aircraft-table thead {
  background: #f8f9fa;
}

.aircraft-table th {
  padding: 14px 24px;
  color: #888;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  text-align: left;
  font-weight: 700;
}

.aircraft-table td {
  padding: 18px 24px;
  border-top: 1px solid #f0f0f0;
  color: #333;
}

.model-code {
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

.model-code-large {
  font-size: 1rem;
  padding: 8px 13px;
  margin-top: 4px;
}

.capacity-badge {
  display: inline-block;
  padding: 5px 12px;
  background: #fff3e0;
  color: #e65c00;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 700;
}

.actions-wrapper {
  display: flex;
  align-items: center;
  gap: 14px;
}

.view-btn,
.edit-btn,
.delete-btn {
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

.delete-btn {
  color: #ef0012;
}

.delete-btn:hover {
  color: #c80010;
}

.view-btn:hover,
.edit-btn:hover,
.delete-btn:hover {
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

.pagination {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 20px 24px 24px;
  border-top: 1px solid #f3f4f6;
}

.page-btn {
  min-width: 36px;
  height: 36px;
  padding: 0 10px;
  border: 1.5px solid #e0e0e0;
  border-radius: 8px;
  background: #ffffff;
  color: #374151;
  font-size: 0.88rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.15s ease;
}

.page-btn:hover:not(:disabled) {
  border-color: #e74c3c;
  color: #e74c3c;
}

.page-btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.page-btn--active {
  background: linear-gradient(to right, #e74c3c, #f39c12);
  border-color: transparent;
  color: #ffffff;
}

.page-info {
  margin-left: 8px;
  font-size: 0.85rem;
  color: #888;
}

.back-list-btn {
  border: none;
  background: transparent;
  color: #e74c3c;
  font-size: 0.9rem;
  font-weight: 800;
  padding: 0;
  margin-bottom: 16px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.back-list-btn:hover {
  text-decoration: underline;
}

.details-card,
.edit-card {
  width: 100%;
  padding: 36px;
  font-size: 0.9rem;
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

.detail-full {
  width: 100%;
}

.detail-label {
  color: #888;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.detail-group p {
  margin: 0;
  color: #333;
  font-size: 0.9rem;
  font-weight: 600;
}

.details-line {
  border: none;
  border-top: 1px solid #f0f0f0;
  margin: 24px 0;
}

.class-summary {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
}

.summary-box {
  border-radius: 12px;
  padding: 14px 16px;
  text-align: center;
}

.summary-box span {
  display: block;
  font-size: 0.75rem;
  font-weight: 700;
  margin-bottom: 8px
}

.summary-box strong {
  font-size: 1rem;
  font-weight: 700;
}

.first-class-summary .summary-box {
  background: #fff7ed;
  color: #ea580c;
  border: 1px solid #fed7aa;
}

.first-class-summary .summary-box-strong {
  background: #ffedd5;
}

.economy-summary .summary-box {
  background: #eff6ff;
  color: #0b5cff;
  border: 1px solid #bfdbfe;
}

.economy-summary .summary-box-strong {
  background: #dbeafe;
}

.total-capacity-box {
  margin-top: 4px;
  background: #ecfdf5;
  color: #047857;
  border: 1px solid #bbf7d0;
  border-radius: 12px;
  padding: 18px 20px;
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 0.9rem;
}

.total-capacity-box i {
  font-size: 1.2rem;
}

.details-actions,
.edit-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
  margin-top: 26px;
}

.close-details-btn,
.edit-details-btn,
.save-btn,
.cancel-btn {
  width: 100%;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 700;
  padding: 14px 18px;
  cursor: pointer;
  transition: background 0.2s ease, transform 0.2s ease, opacity 0.2s ease;
}

.close-details-btn,
.cancel-btn {
  background: #f3f4f6;
  color: #1f2937;
}

.close-details-btn:hover,
.cancel-btn:hover {
  background: #e5e7eb;
  transform: translateY(-1px);
}

.edit-details-btn,
.save-btn {
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
}

.edit-details-btn:hover,
.save-btn:hover:not(:disabled) {
  opacity: 0.9;
  transform: translateY(-1px);
}

.save-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
  transform: none;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 7px;
  margin-bottom: 20px;
}

.form-label {
  font-size: 0.88rem;
  font-weight: 700;
  color: #374151;
}

.required {
  color: #e74c3c;
}

.form-input {
  width: 100%;
  padding: 12px 16px;
  border: 1.5px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.9rem;
  color: #333;
  background: #ffffff;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
  box-sizing: border-box;
}

.form-input:focus {
  outline: none;
  border-color: #ff5f00;
  box-shadow: 0 0 0 3px rgba(255, 95, 0, 0.12);
}

.form-group small {
  color: #6b7280;
  font-size: 0.76rem;
}

.form-group.has-error input {
  border-color: #e74c3c;
  background: #fff7f7;
}

.error-text {
  color: #e74c3c !important;
  font-weight: 700;
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
  gap: 4px;
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

.edit-note {
  margin-top: 16px;
  padding: 12px 14px;
  border-radius: 10px;
  background: #eff6ff;
  color: #1d4ed8;
  border: 1px solid #bfdbfe;
  font-size: 0.86rem;
  font-weight: 700;
  display: flex;
  gap: 8px;
  align-items: center;
}

.delete-modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 3000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  background: rgba(15, 23, 42, 0.45);
  backdrop-filter: blur(8px);
}

.delete-modal {
  position: relative;
  width: min(420px, 100%);
  background: #ffffff;
  border-radius: 22px;
  padding: 28px 26px 24px;
  box-shadow: 0 25px 60px rgba(15, 23, 42, 0.22);
  text-align: center;
}

.delete-modal-close {
  position: absolute;
  top: 18px;
  right: 18px;
  border: none;
  background: #f8fafc;
  color: #94a3b8;
  width: 34px;
  height: 34px;
  border-radius: 999px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.delete-modal-close:hover:not(:disabled) {
  color: #475569;
  background: #f1f5f9;
}

.delete-modal-icon {
  width: 56px;
  height: 56px;
  margin: 0 auto 18px;
  border-radius: 18px;
  background: #fff1f2;
  color: #ef4444;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.55rem;
}

.delete-modal-title {
  margin: 0 0 10px;
  color: #0f172a;
  font-size: 1.25rem;
  font-weight: 800;
}

.delete-modal-text {
  margin: 0;
  color: #475569;
  font-size: 0.95rem;
  line-height: 1.55;
}

.delete-modal-warning {
  margin: 12px 0 24px;
  color: #ef4444;
  font-size: 0.9rem;
  font-weight: 800;
}

.delete-modal-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.delete-cancel-btn,
.delete-confirm-btn {
  border: none;
  border-radius: 14px;
  padding: 14px 18px;
  font-size: 0.95rem;
  font-weight: 800;
  cursor: pointer;
  transition: opacity 0.2s ease, transform 0.2s ease, background 0.2s ease;
}

.delete-cancel-btn {
  background: #f3f4f6;
  color: #1f2937;
}

.delete-confirm-btn {
  background: linear-gradient(to right, #e11d48, #f97316);
  color: #ffffff;
}

.delete-cancel-btn:hover:not(:disabled),
.delete-confirm-btn:hover:not(:disabled) {
  transform: translateY(-1px);
}

.delete-cancel-btn:disabled,
.delete-confirm-btn:disabled,
.delete-modal-close:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }

  to {
    transform: rotate(360deg);
  }
}

.spin {
  display: inline-block;
  animation: spin 0.8s linear infinite;
}

@media (max-width: 768px) {
  .card-header-row {
    align-items: stretch;
    flex-direction: column;
  }

  .create-aircraft-btn {
    justify-content: center;
  }

  .detail-grid,
  .form-row,
  .class-summary,
  .details-actions,
  .edit-actions,
  .delete-modal-actions {
    grid-template-columns: 1fr;
  }

  .actions-wrapper {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .details-card,
  .edit-card {
    padding: 24px;
  }

  .seat-counter {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
