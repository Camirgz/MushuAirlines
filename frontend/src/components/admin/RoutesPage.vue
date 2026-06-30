<template>
  <AdminPageLayout>
    <AdminHero
        title="Rutas Existentes"
        subtitle="Panel de administración de itinerarios para Mushu Airlines"
        icon="bi bi-geo-alt"
        back-to="/admin"
        back-text="Volver al panel"
    />

    <div v-if="successMessage" class="success-message">
        <i class="bi bi-check-circle-fill"></i>
        <span>{{ successMessage }}</span>
    </div>

    <AdminCard>
        <div class="card-header">
            <h2>
                Rutas ({{ filteredRoutes.length }}
                <span v-if="searchQuery" class="total-hint">de {{ routes.length }}</span>)
            </h2>
            <RouterLink to="/admin/routes/create-route" class="create-btn">
                <i class="bi bi-plus-lg me-2"></i>
                Crear Ruta
            </RouterLink>
        </div>

        <div class="search-bar">
            <i class="bi bi-search search-icon"></i>
            <input v-model="searchQuery"
                    type="text"
                    class="search-input"
                    placeholder="Buscar por código, ciudad de origen o destino..." />
            <button v-if="searchQuery" class="search-clear" @click="searchQuery = ''">
                <i class="bi bi-x-lg"></i>
            </button>
        </div>

        <div v-if="isLoading" class="status-msg">
            <i class="bi bi-arrow-repeat spin me-2"></i>
            Cargando rutas...
        </div>

        <div v-else-if="errorMessage" class="error-msg">
            <i class="bi bi-exclamation-circle me-2"></i>
            {{ errorMessage }}
        </div>

        <template v-else>
            <table class="aircraft-table">
                <thead>
                    <tr>
                        <th>CÓDIGO</th>
                        <th>ORIGEN / DESTINO</th>
                        <th>FRECUENCIA</th>
                        <th>HORARIO Y DURACIÓN</th>
                        <th>TARIFAS ($)</th>
                        <th>ACCIONES</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-if="filteredRoutes.length === 0">
                        <td colspan="6" class="empty-msg">No se encontraron rutas disponibles.</td>
                    </tr>
                    <tr v-for="route in paginatedRoutes" :key="route.code">
                        <td class="fw-bold text-dark">{{ route.code }}</td>
                        <td>
                            <div class="d-flex align-items-center gap-2">
                                <span class="city-text">{{ route.originCity }}</span>
                                <i class="bi bi-arrow-right text-muted"></i>
                                <span class="city-text">{{ route.destinationCity }}</span>
                            </div>
                            <small class="text-muted d-block">{{ route.originAirport }} → {{ route.destinationAirport }}</small>
                        </td>
                        <td>
                            <div class="day-pills-container">
                                <span v-for="day in route.frequency" :key="day" class="day-tag">
                                    {{ day }}
                                </span>
                            </div>
                        </td>
                        <td>
                            <div class="time-text">
                                <i class="bi bi-clock-history me-1"></i>
                                {{ route.departureTime }} - {{ route.arrivalTime }}
                            </div>
                            <small class="duration-badge">{{ route.duration }}</small>
                        </td>
                        <td>
                            <div class="price-container">
                                <span class="price-badge premium">Primera clase: {{ route.priceFirstClass.toLocaleString() }}</span>
                                <span class="price-badge economy">Clase turista: {{ route.priceEconomy.toLocaleString() }}</span>
                            </div>
                        </td>
                        <td>
                            <div class="actions-wrapper">
                                <button
                                    type="button"
                                    class="view-btn"
                                    @click="openRouteDetails(route)"
                                >
                                    <i class="bi bi-eye me-1"></i>
                                    Ver
                                </button>

                                <button
                                    type="button"
                                    class="delete-btn"
                                    @click="openDeleteModal(route)"
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
                <button class="page-btn" :disabled="currentPage === 1" @click="goToPage(currentPage - 1)">
                    <i class="bi bi-chevron-left"></i>
                </button>

                <button v-for="page in totalPages"
                        :key="page"
                        class="page-btn"
                        :class="{ 'page-btn--active': page === currentPage }"
                        @click="goToPage(page)">
                    {{ page }}
                </button>

                <button class="page-btn" :disabled="currentPage === totalPages" @click="goToPage(currentPage + 1)">
                    <i class="bi bi-chevron-right"></i>
                </button>

                <span class="page-info">
                    Página {{ currentPage }} de {{ totalPages }}
                </span>
            </div>
        </template>
    </AdminCard>

    <div
        v-if="showDetailsModal"
        class="route-details-overlay"
        @click.self="closeRouteDetails"
    >
        <div class="route-details-modal">
            <button
                type="button"
                class="route-details-close"
                @click="closeRouteDetails"
                aria-label="Cerrar detalles"
            >
                <i class="bi bi-x-lg"></i>
            </button>

            <div class="route-details-hero">
                <i class="bi bi-geo-alt route-details-hero-icon"></i>
                <div>
                    <h2>Detalles de la Ruta</h2>
                    <p>Información completa de la ruta seleccionada</p>
                </div>
            </div>

            <div v-if="selectedRoute" class="route-details-card">
                <section class="route-section">
                    <span class="section-label">Código de ruta</span>

                    <div class="route-code-box">
                        {{ selectedRoute.code }}
                    </div>
                </section>

                <section class="route-section route-main-grid">
                    <div>
                        <span class="section-label">Origen</span>
                        <strong>
                            {{ formatAirport(selectedRoute.originAirport, selectedRoute.originCity) }}
                        </strong>
                    </div>

                    <div>
                        <span class="section-label">Destino</span>
                        <strong>
                            {{ formatAirport(selectedRoute.destinationAirport, selectedRoute.destinationCity) }}
                        </strong>
                    </div>
                </section>

                <section class="route-section">
                    <span class="section-label">Aeronave</span>

                    <div class="aircraft-box">
                        <i class="bi bi-airplane"></i>
                        <strong>{{ selectedRoute.aircraftTypeId }}</strong>
                    </div>
                </section>

                <section class="route-section">
                    <span class="section-label">Horario</span>

                    <div class="schedule-grid">
                        <div class="schedule-box">
                            <span>Salida</span>
                            <strong>{{ selectedRoute.departureTime }}</strong>
                        </div>

                        <div class="schedule-box">
                            <span>Llegada</span>
                            <strong>{{ selectedRoute.arrivalTime }}</strong>
                        </div>

                        <div class="schedule-box">
                            <span>Duración</span>
                            <strong>{{ selectedRoute.duration }}</strong>
                        </div>
                    </div>
                </section>

                <section class="route-section">
                    <span class="section-label">Frecuencia</span>

                    <div class="frequency-box">
                        {{ formatFrequency(selectedRoute.frequency) }}
                    </div>
                </section>

                <section class="route-section route-main-grid">
                    <div>
                        <span class="section-label">Fecha de inicio</span>
                        <strong>{{ selectedRoute.startDate }}</strong>
                    </div>

                    <div>
                        <span class="section-label">Fecha de finalización</span>
                        <strong>{{ selectedRoute.finalizationDate }}</strong>
                    </div>
                </section>

                <section class="route-section">
                    <span class="section-label">Tarifas</span>

                    <div class="fare-grid">
                        <div class="fare-box first-class">
                            <span>Primera clase</span>
                            <strong>${{ formatMoney(selectedRoute.priceFirstClass) }}</strong>
                        </div>

                        <div class="fare-box economy-class">
                            <span>Clase turista</span>
                            <strong>${{ formatMoney(selectedRoute.priceEconomy) }}</strong>
                        </div>
                    </div>
                </section>

                <section class="route-section">
                    <span class="section-label">Capacidad</span>

                    <div class="capacity-box">
                        <i class="bi bi-people"></i>
                        <strong>
                            Primera: {{ selectedRoute.firstClassCapacity }}
                            /
                            Turista: {{ selectedRoute.economyClassCapacity }}
                        </strong>
                    </div>
                </section>

                <section class="route-section">
                    <span class="section-label">Políticas de equipaje</span>

                    <div class="baggage-grid">
                        <div class="baggage-box">
                            <span>Equipaje de mano</span>
                            <strong>
                            ${{ formatMoney(selectedRoute.handBagPrice) }}
                            ·
                            {{ selectedRoute.handBagWeight }} kg
                            </strong>
                        </div>

                        <div class="baggage-box">
                            <span>Equipaje documentado</span>
                            <strong>
                            ${{ formatMoney(selectedRoute.bagPrice) }}
                            ·
                            {{ selectedRoute.bagWeight }} kg
                            </strong>
                        </div>

                        <div class="baggage-box">
                            <span>Multiplicador</span>
                            <strong>{{ selectedRoute.bagMultiplier }}</strong>
                        </div>
                    </div>
                </section>

                <button
                    type="button"
                    class="route-details-bottom-btn"
                    @click="closeRouteDetails"
                >
                    Cerrar
                </button>
            </div>
        </div>
    </div>
    
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
                :disabled="deletingRoute"
            >
                <i class="bi bi-x-lg"></i>
            </button>

            <div class="delete-modal-icon">
                <i class="bi bi-trash"></i>
            </div>

            <h3 class="delete-modal-title">Eliminar ruta</h3>

            <p class="delete-modal-text">
                ¿Estás seguro de eliminar a
                <strong>{{ routeToDelete?.code }}</strong>?
            </p>

            <p class="delete-modal-warning">
                Esta acción es irreversible.
            </p>

            <div class="delete-modal-actions">
                <button
                    type="button"
                    class="delete-cancel-btn"
                    @click="closeDeleteModal"
                    :disabled="deletingRoute"
                >
                    Cancelar
                </button>

                <button
                    type="button"
                    class="delete-confirm-btn"
                    @click="confirmDeleteRoute"
                    :disabled="deletingRoute"
                >
                    {{ deletingRoute ? "Eliminando..." : "Eliminar" }}
                </button>
            </div>
        </div>
    </div>

  </AdminPageLayout>
</template>

<script>
import axios from "axios";
import API_BASE_URL from "@/config/api";
import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";

export default {
  name: "RoutesAdminPage",

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

    data() {
    return {
        routes: [],
        airports: [],
        searchQuery: "",
        currentPage: 1,
        pageSize: 10,
        isLoading: false,
        errorMessage: "",
        successMessage: "",
        selectedRoute: null,
        showDetailsModal: false,
        showDeleteModal: false,
        routeToDelete: null,
        deletingRoute: false,
    };
    },

  computed: {
    filteredRoutes() {
      const query = this.searchQuery.trim().toLowerCase();
      if (!query) return this.routes;
      return this.routes.filter(r =>
        r.code.toLowerCase().includes(query) ||
        r.originCity.toLowerCase().includes(query) ||
        r.destinationCity.toLowerCase().includes(query)
      );
    },
    totalPages() {
      return Math.ceil(this.filteredRoutes.length / this.pageSize) || 1;
    },
    paginatedRoutes() {
      const start = (this.currentPage - 1) * this.pageSize;
      return this.filteredRoutes.slice(start, start + this.pageSize);
    },
  },

  methods: {
    async loadData() {
    this.isLoading = true;
    this.errorMessage = "";

    try {
        const [routesRes, airportsRes] = await Promise.all([
        axios.get(`${API_BASE_URL}/api/routecreation`),
        axios.get(`${API_BASE_URL}/api/airport`),
        ]);

        this.airports = airportsRes.data;

        this.routes = routesRes.data.map((r) => {
            const origin = this.airports.find((a) => a.code === r.originAirport);
            const dest = this.airports.find((a) => a.code === r.destinationAirport);

            return {
                ...r,
                originCity: origin?.city || r.originCity || r.originAirport,
                destinationCity: dest?.city || r.destinationCity || r.destinationAirport,
                frequency: Array.isArray(r.frequency)
                ? r.frequency
                : (r.frequency || "").split(",").filter(Boolean),
                priceFirstClass: Number(r.priceFirstClass) || 0,
                priceEconomy: Number(r.priceEconomy) || 0,
                handBagPrice: Number(r.handBagPrice) || 0,
                handBagWeight: Number(r.handBagWeight) || 0,
                bagPrice: Number(r.bagPrice) || 0,
                bagWeight: Number(r.bagWeight) || 0,
                bagMultiplier: Number(r.bagMultiplier) || 0,
                firstClassCapacity: Number(r.firstClassCapacity) || 0,
                economyClassCapacity: Number(r.economyClassCapacity) || 0,
            };
        });
    } catch (error) {
        this.errorMessage = "No se pudieron cargar las rutas. Intente de nuevo.";
    } finally {
        this.isLoading = false;
    }
    },

    formatAirport(code, city) {
    if (!code && !city) {
        return "No disponible";
    }

    if (!city || city === code) {
        return code;
    }

    return `${code} - ${city}`;
    },

    formatMoney(value) {
    return Number(value || 0).toLocaleString();
    },

    formatFrequency(frequency) {
    if (!frequency) {
        return "No disponible";
    }

    if (Array.isArray(frequency)) {
        return frequency.join(", ");
    }

    return frequency;
    },

    openRouteDetails(route) {
    this.selectedRoute = { ...route };
    this.showDetailsModal = true;
    this.successMessage = "";
    this.errorMessage = "";
    },

    closeRouteDetails() {
    this.showDetailsModal = false;
    this.selectedRoute = null;
    },

    openDeleteModal(route) {
        this.routeToDelete = { ...route };
        this.showDeleteModal = true;
        this.successMessage = "";
        this.errorMessage = "";
    },

    closeDeleteModal() {
        if (this.deletingRoute) {
            return;
        }

        this.showDeleteModal = false;
        this.routeToDelete = null;
    },

    async confirmDeleteRoute() {
        if (!this.routeToDelete) {
            return;
        }

        this.deletingRoute = true;
        this.successMessage = "";
        this.errorMessage = "";

        try {
            await axios.delete(`${API_BASE_URL}/api/routecreation/${this.routeToDelete.code}`);

            this.routes = this.routes.filter(
            route => route.code !== this.routeToDelete.code
            );

            if (this.currentPage > this.totalPages) {
            this.currentPage = this.totalPages;
            }

            this.successMessage = "Ruta eliminado correctamente.";

            this.showDeleteModal = false;
            this.routeToDelete = null;

            window.scrollTo({ top: 0, behavior: "smooth" });
        } catch (error) {
            this.errorMessage =
            typeof error.response?.data === "string"
                ? error.response.data
                : "No se pudo eliminar la ruta.";
        } finally {
            this.deletingRoute = false;
        }
    },

    goToPage(page) {
      if (page >= 1 && page <= this.totalPages) this.currentPage = page;
    },
  },

  mounted() {
    this.loadData();
  },
};
</script>

<style scoped>
    .content-card {
        background: #ffffff;
        border-radius: 16px;
        padding: 32px;
        box-shadow: 0 10px 40px rgba(0,0,0,0.15);
    }

    .card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 28px;
    }

    .create-btn {
        text-decoration: none;
        padding: 11px 22px;
        background: linear-gradient(to right, #e74c3c, #f39c12);
        color: white;
        border-radius: 10px;
        font-weight: 800;
        display: flex;
        align-items: center;
        transition: 0.2s ease;
    }

    .aircraft-table {
        width: 100%;
        border-collapse: collapse;
    }

        .aircraft-table th {
            text-align: left;
            font-size: 0.75rem;
            font-weight: 700;
            color: #6b7280;
            letter-spacing: 0.05em;
            padding: 0 16px 14px 0;
            border-bottom: 2px solid #f3f4f6;
        }

        .aircraft-table td {
            padding: 18px 16px 18px 0;
            border-bottom: 1px solid #f3f4f6;
            vertical-align: middle;
        }

    .city-text {
        font-weight: 700;
        color: #1f2937;
    }

    .day-pills-container {
        display: flex;
        gap: 4px;
        flex-wrap: wrap;
    }

    .day-tag {
        background: #f3f4f6;
        padding: 2px 8px;
        border-radius: 6px;
        font-size: 0.7rem;
        font-weight: 700;
        color: #4b5563;
    }

    .time-text {
        font-weight: 700;
        font-size: 0.9rem;
    }

    .duration-badge {
        color: #e74c3c;
        font-weight: 600;
        font-size: 0.8rem;
    }

    .price-container {
        display: flex;
        flex-direction: column;
        gap: 4px;
    }

    .price-badge {
        padding: 2px 10px;
        border-radius: 20px;
        font-size: 0.75rem;
        font-weight: 800;
        width: fit-content;
    }

        .price-badge.premium {
            background: #fff3e0;
            color: #e67e22;
        }

        .price-badge.economy {
            background: #e1f5fe;
            color: #0288d1;
        }

    .search-bar {
        position: relative;
        margin-bottom: 24px;
    }

    .search-input {
        width: 100%;
        padding: 11px 40px;
        border: 1.5px solid #e0e0e0;
        border-radius: 10px;
    }

    .search-icon {
        position: absolute;
        left: 14px;
        top: 14px;
        color: #9ca3af;
    }

    .pagination {
        display: flex;
        align-items: center;
        gap: 6px;
        margin-top: 24px;
    }

    .page-btn {
        min-width: 36px;
        height: 36px;
        border: 1px solid #ddd;
        border-radius: 8px;
        background: white;
        cursor: pointer;
    }

    .page-btn--active {
        background: linear-gradient(to right, #e74c3c, #f39c12);
        color: white;
        border: none;
    }

    .spin {
        animation: rotate 1s linear infinite;
        display: inline-block;
    }

    @keyframes rotate {
        from {
            transform: rotate(0deg);
        }

        to {
            transform: rotate(360deg);
        }
    }

    .btn-gradient {
        background: linear-gradient(to right, #e74c3c, #f39c12);
        color: white;
        border-radius: 20px;
        padding: 6px 14px;
        border: none;
    }

    .success-message {
        display: flex;
        align-items: center;
        gap: 10px;
        border-radius: 12px;
        padding: 14px 16px;
        margin-bottom: 20px;
        font-size: 0.92rem;
        font-weight: 700;
        background: #ecfdf5;
        color: #166534;
        border: 1px solid #bbf7d0;
    }

    .actions-wrapper {
        display: flex;
        align-items: center;
        gap: 14px;
    }

    .view-btn,
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

    .delete-btn {
        color: #ef0012;
    }

    .view-btn:hover,
    .delete-btn:hover {
        text-decoration: underline;
    }

    .view-btn:hover {
        color: #c0392b;
    }

    .delete-btn:hover {
        color: #c80010;
    }

    .modal-overlay,
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

    .details-modal,
    .delete-modal {
        position: relative;
        width: min(720px, 100%);
        max-height: 90vh;
        overflow-y: auto;
        background: #ffffff;
        border-radius: 22px;
        padding: 28px 26px 24px;
        box-shadow: 0 25px 60px rgba(15, 23, 42, 0.22);
    }

    .delete-modal {
        width: min(420px, 100%);
        text-align: center;
    }

    .modal-close,
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

    .modal-close:hover,
    .delete-modal-close:hover:not(:disabled) {
        color: #475569;
        background: #f1f5f9;
    }

    .modal-title {
        margin: 0 0 24px;
        color: #0f172a;
        font-size: 1.35rem;
        font-weight: 800;
    }

    .details-grid {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 16px;
    }

    .detail-item {
        border: 1px solid #f1f5f9;
        border-radius: 12px;
        padding: 14px;
        background: #f8fafc;
    }

    .detail-item span {
        display: block;
        color: #64748b;
        font-size: 0.78rem;
        font-weight: 800;
        margin-bottom: 6px;
    }

    .detail-item strong {
        color: #0f172a;
        font-size: 0.95rem;
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

    .route-details-overlay {
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

    .route-details-modal {
        position: relative;
        width: min(760px, 100%);
        max-height: 92vh;
        overflow-y: auto;
        background: #f8fafc;
        border-radius: 22px;
        box-shadow: 0 25px 60px rgba(15, 23, 42, 0.25);
    }

    .route-details-close {
        position: absolute;
        top: 22px;
        right: 22px;
        z-index: 2;
        border: none;
        background: rgba(255, 255, 255, 0.85);
        color: #94a3b8;
        width: 36px;
        height: 36px;
        border-radius: 999px;
        cursor: pointer;
        display: inline-flex;
        align-items: center;
        justify-content: center;
    }

    .route-details-close:hover {
        color: #475569;
        background: #ffffff;
    }

    .route-details-hero {
        display: flex;
        align-items: center;
        gap: 22px;
        padding: 36px 44px;
        border-radius: 22px 22px 0 0;
        background: linear-gradient(to right, #d63b31, #f39c12);
        color: #ffffff;
    }

    .route-details-hero-icon {
        font-size: 3rem;
    }

    .route-details-hero h2 {
        margin: 0;
        font-size: 2rem;
        font-weight: 900;
    }

    .route-details-hero p {
        margin: 8px 0 0;
        font-size: 0.95rem;
        color: #fff7ed;
    }

    .route-details-card {
        background: #ffffff;
        margin: 24px;
        padding: 28px;
        border-radius: 18px;
        box-shadow: 0 12px 35px rgba(15, 23, 42, 0.08);
    }

    .route-section {
        padding: 22px 0;
        border-bottom: 1px solid #f1f5f9;
    }

    .route-section:first-child {
        padding-top: 0;
    }

    .route-section:last-of-type {
        border-bottom: none;
    }

    .section-label {
        display: block;
        color: #6b7280;
        font-size: 0.78rem;
        font-weight: 900;
        text-transform: uppercase;
        letter-spacing: 0.05em;
        margin-bottom: 12px;
    }

    .route-code-box {
        width: 100%;
        padding: 15px 18px;
        border-radius: 10px;
        background: #fff1f2;
        color: #ef0012;
        font-size: 1rem;
        font-weight: 900;
        text-align: center;
    }

    .route-main-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 24px;
    }

    .route-main-grid strong {
        color: #001233;
        font-size: 1rem;
    }

    .aircraft-box,
    .frequency-box,
    .capacity-box {
        width: 100%;
        border-radius: 12px;
        padding: 18px;
        font-weight: 800;
    }

    .aircraft-box {
        display: flex;
        align-items: center;
        gap: 12px;
        background: #fff7ed;
        border: 1px solid #fed7aa;
        color: #f97316;
    }

    .frequency-box {
        background: #f8fafc;
        border: 1px solid #e5e7eb;
        color: #001233;
        line-height: 1.6;
    }

    .schedule-grid,
    .fare-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 14px;
    }

    .fare-grid {
        grid-template-columns: repeat(2, 1fr);
    }

    .schedule-box,
    .fare-box,
    .baggage-box {
        border-radius: 12px;
        padding: 18px;
        text-align: center;
    }

    .schedule-box {
        background: #f8fafc;
        border: 1px solid #e5e7eb;
    }

    .schedule-box span,
    .fare-box span,
    .baggage-box span {
        display: block;
        color: #64748b;
        font-size: 0.78rem;
        font-weight: 800;
        margin-bottom: 8px;
    }

    .schedule-box strong,
    .fare-box strong,
    .baggage-box strong {
        color: #001233;
        font-size: 1rem;
    }

    .first-class {
        background: #fff7ed;
        border: 1px solid #fed7aa;
    }

    .first-class strong {
        color: #ea580c;
    }

    .economy-class {
        background: #eff6ff;
        border: 1px solid #bfdbfe;
    }

    .economy-class strong {
        color: #2563eb;
    }

    .capacity-box {
        display: flex;
        align-items: center;
        gap: 12px;
        background: #ecfdf5;
        border: 1px solid #bbf7d0;
        color: #047857;
    }

    .baggage-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 14px;
    }

    .baggage-box {
        background: #f8fafc;
        border: 1px solid #e5e7eb;
    }

    .route-details-bottom-btn {
        width: 100%;
        border: none;
        border-radius: 10px;
        margin-top: 24px;
        padding: 14px;
        background: #f3f4f6;
        color: #111827;
        font-weight: 900;
        cursor: pointer;
    }

    .route-details-bottom-btn:hover {
        background: #e5e7eb;
    }

    @media (max-width: 768px) {
        .details-grid,
        .delete-modal-actions {
            grid-template-columns: 1fr;
        }

        .actions-wrapper {
            flex-direction: column;
            align-items: flex-start;
            gap: 8px;
        }

        .route-details-hero {
            padding: 30px 24px;
        }

        .route-details-hero h2 {
            font-size: 1.5rem;
        }

        .route-details-card {
            margin: 16px;
            padding: 20px;
        }

        .route-main-grid,
        .schedule-grid,
        .fare-grid,
        .baggage-grid {
            grid-template-columns: 1fr;
        }
    }
</style>
