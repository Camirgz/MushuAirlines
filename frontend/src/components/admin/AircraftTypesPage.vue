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

                <button class="logout-btn" @click="Logout">
                    <i class="bi bi-box-arrow-right me-2"></i>
                    Logout
                </button>
            </div>
        </nav>

        <!-- Main content -->
        <main class="admin-main">
            <!-- Hero banner -->
            <section class="admin-hero">
                <i class="bi bi-airplane hero-icon"></i>
                <div>
                    <h1>Lista de Aeronaves</h1>
                    <p>Panel de administración para operadores de Mushu Airlines</p>
                </div>
            </section>

            <!-- Content card -->
            <section class="content-card">
                <div class="card-header">
                    <h2>
                        Aeronaves ({{ FilteredAircraftTypes.length }}
                        <span v-if="SearchQuery" class="total-hint">de {{ AircraftTypes.length }}</span>)
                    </h2>
                    <RouterLink to="/admin/aircraft-types/create" class="create-btn">
                        <i class="bi bi-plus-lg me-2"></i>
                        Crear Aeronave
                    </RouterLink>
                </div>

                <div class="search-bar">
                    <i class="bi bi-search search-icon"></i>
                    <input v-model="SearchQuery"
                           type="text"
                           class="search-input"
                           placeholder="Buscar por modelo o tipo..." />
                    <button v-if="SearchQuery" class="search-clear" @click="SearchQuery = ''">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>

                <div v-if="IsLoading" class="status-msg">
                    <i class="bi bi-arrow-repeat spin me-2"></i>
                    Cargando aeronaves...
                </div>

                <div v-else-if="ErrorMessage" class="error-msg">
                    <i class="bi bi-exclamation-circle me-2"></i>
                    {{ ErrorMessage }}
                </div>

                <template v-else>
                    <table class="aircraft-table">
                        <thead>
                            <tr>
                                <th>MODELO</th>
                                <th>TIPO</th>
                                <th>PESO (KG)</th>
                                <th>CAPACIDAD</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-if="FilteredAircraftTypes.length === 0">
                                <td colspan="4" class="empty-msg">No se encontraron aeronaves.</td>
                            </tr>
                            <tr v-for="Aircraft in PaginatedAircraftTypes" :key="Aircraft.Id">
                                <td>{{ Aircraft.model }}</td>
                                <td class="type-cell">{{ Aircraft.type }}</td>
                                <td>{{ Aircraft.weightKg.toLocaleString() }}</td>
                                <td>
                                    <span class="capacity-badge">{{ Aircraft.capacity }} asientos</span>
                                </td>
                            </tr>
                        </tbody>
                    </table>

                    <div v-if="TotalPages > 1" class="pagination">
                        <button class="page-btn"
                                :disabled="CurrentPage === 1"
                                @click="GoToPage(CurrentPage - 1)">
                            <i class="bi bi-chevron-left"></i>
                        </button>

                        <button v-for="Page in TotalPages"
                                :key="Page"
                                class="page-btn"
                                :class="{ 'page-btn--active': Page === CurrentPage }"
                                @click="GoToPage(Page)">
                            {{ Page }}
                        </button>

                        <button class="page-btn"
                                :disabled="CurrentPage === TotalPages"
                                @click="GoToPage(CurrentPage + 1)">
                            <i class="bi bi-chevron-right"></i>
                        </button>

                        <span class="page-info">
                            Página {{ CurrentPage }} de {{ TotalPages }}
                        </span>
                    </div>
                </template>
            </section>
        </main>
    </div>
</template>

<script>
    import { GetAircraftTypes } from "../../services/AircraftTypesService";

    export default {
        name: "AircraftTypesPage",

        computed: {
            FilteredAircraftTypes() {
                const Query = this.SearchQuery.trim().toLowerCase();
                if (!Query) return this.AircraftTypes;
                return this.AircraftTypes.filter(
                    (A) =>
                        (A.model ?? "").toLowerCase().includes(Query) ||
                        (A.type ?? "").toLowerCase().includes(Query)
                );
            },
            TotalPages() {
                return Math.ceil(this.FilteredAircraftTypes.length / this.PageSize) || 1;
            },
            PaginatedAircraftTypes() {
                const Start = (this.CurrentPage - 1) * this.PageSize;
                return this.FilteredAircraftTypes.slice(Start, Start + this.PageSize);
            },
        },

        watch: {
            SearchQuery() {
                this.CurrentPage = 1;
            },
        },

        data() {
            return {
                IsDropdownOpen: false,
                AircraftTypes: [],
                SearchQuery: "",
                CurrentPage: 1,
                PageSize: 10,
                IsLoading: false,
                ErrorMessage: "",
            };
        },

        mounted() {
            this.LoadAircraftTypes();
        },

        methods: {
            ToggleDropdown() {
                this.IsDropdownOpen = !this.IsDropdownOpen;
            },
            CloseDropdown() {
                this.IsDropdownOpen = false;
            },
            LoadAircraftTypes() {
                this.IsLoading = true;
                this.ErrorMessage = "";

                GetAircraftTypes()
                    .then((Response) => {
                        this.AircraftTypes = Response.data;
                    })
                    .catch(() => {
                        this.ErrorMessage = "No se pudieron cargar las aeronaves. Intente de nuevo.";
                    })
                    .finally(() => {
                        this.IsLoading = false;
                    });
            },
            GoToPage(Page) {
                if (Page < 1 || Page > this.TotalPages) return;
                this.CurrentPage = Page;
            },
            Logout() {
                localStorage.removeItem("token");
                this.$router.push("/");
            },
        },
    };
</script>

<style scoped>
    .admin-page {
        min-height: 100vh;
        background: var(--bg-page);
        color: var(--text-dark);
    }

    /* Navbar */
    .navbar {
        position: sticky;
        top: 0;
        z-index: 100;
        min-height: var(--navbar-min-height);
        display: flex;
        justify-content: space-between;
        align-items: center;
        background: var(--navbar-bg);
        border-bottom: var(--navbar-border);
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
        color: var(--text-dark);
        line-height: 1.1;
    }

    .brand-tagline {
        font-size: 0.78rem;
        color: var(--text-medium);
    }

    .nav-actions {
        display: flex;
        align-items: center;
        gap: 18px;
    }

    .nav-link-item {
        text-decoration: none;
        color: var(--text-dark);
        font-size: 0.95rem;
        font-weight: 600;
        display: flex;
        align-items: center;
        transition: 0.2s ease;
    }

        .nav-link-item:hover {
            color: var(--color-primary-hover);
        }

    /* Management dropdown */
    .management-wrapper {
        position: relative;
    }

    .management-btn {
        padding: 11px 18px;
        background: var(--gradient-brand-diagonal);
        color: #ffffff;
        border: none;
        border-radius: var(--radius-btn);
        font-weight: 800;
        cursor: pointer;
        display: flex;
        align-items: center;
        box-shadow: var(--shadow-btn-primary);
        transition: 0.2s ease;
    }

        .management-btn:hover {
            transform: translateY(-1px);
            box-shadow: var(--shadow-btn-primary-hover);
        }

    .management-dropdown {
        position: absolute;
        top: 56px;
        right: 0;
        width: 340px;
        background: var(--bg-card);
        border: 1px solid var(--border-color);
        border-radius: var(--radius-panel);
        box-shadow: var(--shadow-dropdown);
        padding: 8px;
        z-index: 200;
    }

    .dropdown-item-custom {
        display: flex;
        align-items: center;
        gap: 12px;
        text-decoration: none;
        color: var(--text-dark);
        padding: 12px 14px;
        border-radius: 9px;
        font-size: 0.9rem;
        font-weight: 700;
        transition: 0.2s ease;
    }

        .dropdown-item-custom i {
            color: var(--color-accent-soft);
            font-size: 1rem;
        }

        .dropdown-item-custom:hover {
            background: var(--bg-dropdown-hover);
            color: var(--color-accent-soft);
        }

        .dropdown-item-custom.router-link-exact-active {
            background: var(--gradient-brand-diagonal);
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
        color: var(--color-primary-hover);
        border-radius: var(--radius-btn);
        font-weight: 800;
        display: flex;
        align-items: center;
        background: var(--bg-card);
        transition: 0.2s ease;
    }

        .logout-btn:hover {
            background: var(--color-primary-hover);
            color: #ffffff;
            box-shadow: 0 8px 18px rgba(240, 24, 24, 0.18);
        }

    /* Main */
    .admin-main {
        max-width: var(--content-max-width);
        margin: 0 auto;
        padding: var(--content-padding);
    }

    /* Hero */
    .admin-hero {
        background: var(--gradient-hero);
        color: #ffffff;
        border-radius: var(--radius-card);
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

    /* Content card */
    .content-card {
        background: var(--bg-card);
        border-radius: var(--radius-card);
        padding: 32px;
        box-shadow: var(--shadow-card);
    }

    .card-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 28px;
    }

        .card-header h2 {
            font-size: 1.35rem;
            font-weight: 900;
            color: #07172c;
            margin: 0;
        }

    .create-btn {
        text-decoration: none;
        padding: 11px 22px;
        background: var(--gradient-brand-diagonal);
        color: #ffffff;
        border-radius: var(--radius-btn);
        font-weight: 800;
        display: flex;
        align-items: center;
        box-shadow: var(--shadow-btn-primary);
        transition: 0.2s ease;
    }

        .create-btn:hover {
            transform: translateY(-1px);
            box-shadow: var(--shadow-btn-primary-hover);
        }

    /* Table */
    .aircraft-table {
        width: 100%;
        border-collapse: collapse;
    }

        .aircraft-table thead tr {
            border-bottom: 2px solid var(--border-color);
        }

        .aircraft-table th {
            text-align: left;
            font-size: 0.78rem;
            font-weight: 700;
            color: var(--text-medium);
            letter-spacing: 0.05em;
            padding: 0 16px 14px 0;
        }

        .aircraft-table tbody tr {
            border-bottom: 1px solid #f3f4f6;
            transition: background 0.15s ease;
        }

            .aircraft-table tbody tr:last-child {
                border-bottom: none;
            }

            .aircraft-table tbody tr:hover {
                background: #fffaf5;
            }

        .aircraft-table td {
            padding: 18px 16px 18px 0;
            font-size: 0.95rem;
            color: var(--text-dark);
        }

    .type-cell {
        color: var(--color-accent-warm);
        font-weight: 600;
    }

    .total-hint {
        font-weight: 400;
        font-size: 1rem;
        color: var(--text-medium);
    }

    /* Search bar */
    .search-bar {
        position: relative;
        display: flex;
        align-items: center;
        margin-bottom: 24px;
    }

    .search-icon {
        position: absolute;
        left: 14px;
        color: var(--text-muted);
        font-size: 0.95rem;
        pointer-events: none;
    }

    .search-input {
        width: 100%;
        padding: 11px 40px;
        border: 1.5px solid var(--border-color);
        border-radius: var(--radius-btn);
        font-size: 0.95rem;
        color: var(--text-dark);
        background: var(--bg-card);
        transition: border-color 0.2s ease, box-shadow 0.2s ease;
    }

        .search-input::placeholder {
            color: var(--text-muted);
        }

        .search-input:focus {
            outline: none;
            border-color: var(--color-accent-warm);
            box-shadow: 0 0 0 3px rgba(255, 90, 0, 0.1);
        }

    .search-clear {
        position: absolute;
        right: 12px;
        background: none;
        border: none;
        color: var(--text-muted);
        cursor: pointer;
        padding: 4px;
        display: flex;
        align-items: center;
        font-size: 0.8rem;
        transition: color 0.15s ease;
    }

        .search-clear:hover {
            color: #374151;
        }

    .empty-msg {
        text-align: center;
        padding: 32px 0;
        color: var(--text-medium);
        font-size: 0.95rem;
    }

    /* Pagination */
    .pagination {
        display: flex;
        align-items: center;
        gap: 6px;
        margin-top: 24px;
        padding-top: 20px;
        border-top: 1px solid #f3f4f6;
    }

    .page-btn {
        min-width: 36px;
        height: 36px;
        padding: 0 10px;
        border: 1.5px solid var(--border-color);
        border-radius: var(--radius-input);
        background: var(--bg-card);
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
            border-color: var(--color-accent-warm);
            color: var(--color-accent-warm);
        }

        .page-btn:disabled {
            opacity: 0.35;
            cursor: not-allowed;
        }

    .page-btn--active {
        background: var(--gradient-brand-diagonal);
        border-color: transparent;
        color: #ffffff;
    }

        .page-btn--active:hover {
            border-color: transparent;
            color: #ffffff;
        }

    .page-info {
        margin-left: 8px;
        font-size: 0.85rem;
        color: var(--text-medium);
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

    .status-msg {
        padding: 24px 0;
        color: var(--text-medium);
        font-size: 0.95rem;
        display: flex;
        align-items: center;
    }

    .error-msg {
        padding: 16px 20px;
        background: #fff1f1;
        color: #b91c1c;
        border: 1px solid #fecaca;
        border-radius: var(--radius-btn);
        font-size: 0.95rem;
        font-weight: 600;
        display: flex;
        align-items: center;
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
</style>