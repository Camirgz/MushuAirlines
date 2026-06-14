<template>
    <div class="routes-page">
    <nav class="navbar bg-white shadow-sm px-4 py-2">
        <RouterLink class="navbar-brand d-flex align-items-center gap-2" to="#">
            <img src="@/assets/logo.png" width="42" height="42" class="rounded-2" />
            <div>
                <div class="brand-name">Mushu Airlines</div>
                <div class="brand-tagline">Vuela con el dragón</div>
            </div>
        </RouterLink>

        <div class="d-flex align-items-center gap-4">
            <RouterLink to="#" class="nav-link-item">
                <i class="bi bi-search me-2"></i>
                Buscar vuelos
            </RouterLink>

            <RouterLink to="#" class="nav-link-item">
                <i class="bi bi-briefcase me-2"></i>
                Mis vuelos
            </RouterLink>

            <RouterLink to="#" class="nav-link-item">
                <i class="bi bi-calendar-check me-2"></i>
                Check-in
            </RouterLink>

            <div class="position-relative">
                <button class="management-btn" type="button" @click="toggleDropdown">
                    <i class="bi bi-gear me-2"></i>
                    Gestión
                    <i class="bi ms-2" :class="isDropdownOpen ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
                </button>

                <div v-if="isDropdownOpen" class="management-dropdown">
                    <RouterLink to="/admin" class="dropdown-item-custom" @click="closeDropdown">
                        <i class="bi bi-grid"></i>
                        <span>Página principal interna</span>
                    </RouterLink>

                    <RouterLink to="/admin/aircraft-types" class="dropdown-item-custom" @click="closeDropdown">
                        <i class="bi bi-airplane"></i>
                        <span>Tipos de aeronaves</span>
                    </RouterLink>

                    <RouterLink to="/admin/routes" class="dropdown-item-custom" @click="closeDropdown">
                        <i class="bi bi-geo-alt"></i>
                        <span>Rutas</span>
                    </RouterLink>

                    <RouterLink to="/admin/airports" class="dropdown-item-custom" @click="closeDropdown">
                        <i class="bi bi-airplane-engines"></i>
                        <span>Aeropuertos</span>
                    </RouterLink>

                    <RouterLink to="/admin/users" class="dropdown-item-custom" @click="closeDropdown">
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

        <main class="admin-main">
            <section class="admin-hero">
                <i class="bi bi-geo-alt hero-icon"></i>
                <div>
                    <h1>Rutas Existentes</h1>
                    <p>Panel de administración de itinerarios para Mushu Airlines</p>
                </div>
            </section>

            <section class="content-card">
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
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-if="filteredRoutes.length === 0">
                                <td colspan="5" class="empty-msg">No se encontraron rutas disponibles.</td>
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
            </section>
        </main>
    </div>
</template>

<script>
    import axios from "axios";

    export default {
        name: "RoutesAdminPage",
        data() {
            return {
                routes: [],
                airports: [],
                searchQuery: "",
                currentPage: 1,
                pageSize: 10,
                isLoading: false,
                errorMessage: "",
                isDropdownOpen: false,
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
            }
        },
        methods: {
            async loadData() {
                this.isLoading = true;
                this.errorMessage = "";
                try {
                    const [routesRes] = await Promise.all([
                        axios.get("http://localhost:5103/api/routecreation")
                    ]);

                    this.routes = routesRes.data.map(r => {
                        const origin = this.airports.find(a => a.code === r.originAirport);
                        const dest = this.airports.find(a => a.code === r.destinationAirport);
                        return {
                            ...r,
                            originCity: origin?.city || r.originAirport,
                            destinationCity: dest?.city || r.destinationAirport,
                            frequency: Array.isArray(r.frequency) ? r.frequency : (r.frequency || "").split(",").filter(Boolean),
                            priceFirstClass: Number(r.priceFirstClass) || 0,
                            priceEconomy: Number(r.priceEconomy) || 0
                        };
                    });
                } catch (error) {
                    this.errorMessage = "No se pudieron cargar las rutas. Intente de nuevo.";
                } finally {
                    this.isLoading = false;
                }
            },
            toggleDropdown() {
                this.isDropdownOpen = !this.isDropdownOpen;
            },
            closeDropdown() {
                this.isDropdownOpen = false;
            },
            goToPage(page) {
                if (page >= 1 && page <= this.totalPages) this.currentPage = page;
            }
        },
        mounted() {
            this.loadData();
        }
    };
</script>

<style scoped>

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

    .admin-page {
        min-height: 100vh;
        background: #f8f9fa;
    }

    .admin-main {
        max-width: 1120px;
        margin: 0 auto;
        padding: 34px 24px 80px;
    }

    .admin-hero {
        background: linear-gradient(90deg, #e60000, #f0a500);
        color: #ffffff;
        border-radius: 16px;
        padding: 34px 38px;
        display: flex;
        align-items: center;
        gap: 22px;
        box-shadow: 0 18px 32px rgba(231, 76, 60, 0.2);
        margin-bottom: 32px;
    }

    .hero-icon {
        font-size: 2.6rem;
    }

    .admin-hero h1 {
        font-size: 1.95rem;
        font-weight: 900;
        margin: 0;
    }

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

    .brand-name {
        font-weight: 700;
        font-size: 1.05rem;
    }

    .brand-tagline {
        font-size: 0.7rem;
        color: #888;
    }

    .admin-banner {
        background: linear-gradient(90deg, #e60000, #f0a500);
        padding: 28px;
        border-radius: 12px;
        color: white;
    }

        .admin-banner h2 {
            font-weight: 800;
        }

        .admin-banner p {
            margin: 0;
            opacity: 0.9;
        }
</style>