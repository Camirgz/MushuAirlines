<template>
    <div>

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

        <main class="admin-header container mt-5">
            <div class="admin-banner">
                <h1 style="font-weight: bold">
                    <img src="@/assets/GestionBox.png" width="44" class="me-2" />
                    Gestión de Rutas
                </h1>
                <p>Panel de administración para operadores de Mushu Airlines</p>
            </div>
        </main>

        <div class="container mt-4 mb-5 flight-container">
            <div class="flight-card">
                <div v-if="successMessage" class="alert-success-custom">
                    {{ successMessage }}
                </div>
                <div v-if="errorMessage" class="alert-error-custom">
                    {{ errorMessage }}
                </div>
                <form @submit.prevent="saveFlight">
                    <h3 style="font-weight: bold;">+ Crear nueva ruta</h3>
                    <div class="row">
                        <div class="col-md-6 form-group">
                            <label>Aeropuerto de Origen<span>*</span></label>
                            <select v-model="form.originAirport" class="form-control" required>
                                <option value="" disabled>Seleccione un aeropuerto</option>
                                <option v-for="a in airports" :key="a.code" :value="a.code">{{ a.airportName }} ({{ a.city }})</option>
                            </select>
                        </div>

                        <div class="col-md-6 form-group">
                            <label>Aeropuerto de Destino<span>*</span></label>
                            <select v-model="form.destinationAirport" class="form-control" required>
                                <option value="" disabled>Seleccione un aeropuerto</option>
                                <option v-for="a in airports" :key="a.code" :value="a.code">{{ a.airportName }} ({{ a.city }})</option>
                            </select>
                        </div>

                        <div class="col-md-4 form-group">
                            <label>Hora de Salida<span>*</span></label>
                            <div class="input-box">
                                <input type="time" v-model="form.departureTime" placeholder="00:00" required />
                            </div>
                        </div>

                        <div class="col-md-4 form-group">
                            <label>Hora de Llegada<span>*</span></label>
                            <div class="input-box">
                                <input type="time" v-model="form.arrivalTime" placeholder="00:00" required />
                            </div>
                        </div>

                        <div class="col-md-4 form-group">
                            <label>Duración<span>*</span></label>
                            <div class="input-box">
                                <input type="text" v-model="form.duration" placeholder="00:00" required @keypress="onlyNumbersDuration" @input="formatDuration" />
                            </div>
                        </div>
                    </div>

                    <div class="row mt-3">
                        <div class="col-md-6 form-group">
                            <label>Tipo de Aeronave<span>*</span></label>
                            <select v-model="form.aircraftTypeId" class="form-control" required>
                                <option value="" disabled>Seleccione un tipo de aeronave</option>
                                <option v-for="type in aircraftTypes" :key="type.id" :value="type.type">
                                    {{ type.type }} — {{ type.model }}
                                </option>
                            </select>
                        </div>
                        <div class="col-md-6 form-group">
                            <label>Código<span>*</span></label>
                            <div class="input-box">
                                <input type="text" minlength="6" maxlength="6" v-model="form.code" placeholder="XX0000" required @input="isValidCode($event)" />
                            </div>
                        </div>
                    </div>

                    <div class="form-group mt-4">
                        <label>Frecuencia<span>*</span></label>
                        <div class="day-container">
                            <label v-for="dia in days"
                                   :key="dia.val"
                                   class="day-pill"
                                   :class="{ active: form.frequency.includes(dia.val) }">
                                <input type="checkbox" :value="dia.val" v-model="form.frequency" hidden />
                                {{ dia.label }}
                            </label>
                        </div>
                    </div>

                    <div class="row mt-3">
                        <div class="col-md-6 form-group">
                            <label>Fecha de inicio<span>*</span></label>
                            <div class="input-box">
                                <input type="date" v-model="form.startDate" required />
                            </div>
                        </div>
                        <div class="col-md-6 form-group">
                            <label>Fecha de finalización<span>*</span></label>
                            <div class="input-box">
                                <input type="date" v-model="form.finalizationDate" required />
                            </div>
                        </div>
                    </div>

                    <h5 class="mt-4" style="font-weight: bold; font-size: x-large">Tarifas</h5>
                    <div class="row">
                        <div class="col-md-6 form-group">
                            <label>Primera Clase $<span>*</span></label>
                            <div class="input-box">
                                <input type="number" min="0" step="0.01" v-model.number="form.priceFirstClass" placeholder="$ 0.00" @keypress="onlyNumbers" />
                            </div>
                        </div>

                        <div class="col-md-6 form-group">
                            <label>Clase Turista $<span>*</span></label>
                            <div class="input-box">
                                <input type="number" min="0" step="0.01" v-model.number="form.priceEconomy" placeholder="$ 0.00" @keypress="onlyNumbers" />
                            </div>
                        </div>
                    </div>

                    <h5 class="mt-4">Políticas de Equipaje</h5>
                    <div class="row">
                        <div class="col-md-3 form-group">
                            <label>Precio equipaje de mano $<span>*</span></label>
                            <div class="input-box">
                                <input type="number" min="0" step="0.01" v-model.number="form.handBagPrice" placeholder="$ 0.00" @keypress="onlyNumbers" />
                            </div>
                        </div>

                        <div class="col-md-3 form-group">
                            <label>Peso equipaje de mano (kg)<span>*</span></label>
                            <div class="input-box">
                                <input type="number" min="0" step="0.01" v-model.number="form.handBagWeight" placeholder="0.00" @keypress="onlyNumbers" />
                            </div>
                        </div>

                        <div class="col-md-3 form-group">
                            <label>Precio equipaje documentado $<span>*</span></label>
                            <div class="input-box">
                                <input type="number" min="0" step="0.01" v-model.number="form.bagPrice" placeholder="$ 0.00" @keypress="onlyNumbers" />
                            </div>
                        </div>

                        <div class="col-md-3 form-group">
                            <label>Peso equipaje documentado (kg)<span>*</span></label>
                            <div class="input-box">
                                <input type="number" min="0" step="0.01" v-model.number="form.bagWeight" placeholder="0.00" @keypress="onlyNumbers" />
                            </div>
                        </div>

                        <div class="col-md-4 form-group mt-2">
                            <label>Multiplicador<span>*</span></label>
                            <div class="input-box">
                                <input type="number" min="0" step="0.01" v-model.number="form.bagMultiplier" placeholder="0.00" @keypress="onlyNumbers" />
                            </div>
                        </div>
                    </div>

                    <button type="submit" class="search-btn mt-4" style="font-size: large; font-weight: bold">
                        Crear ruta
                    </button>
                </form>
            </div>
        </div>

        <div class="container mt-4 mb-5">
            <div class="route-card">
                <h2 class="mb-4" style="font-weight: bold;">
                    <img src="@/assets/Vuelos.png" width="32" class="me-2" />
                    Rutas existentes
                </h2>

                <div v-for="route in routes"
                     :key="route.code"
                     @click="toggleRoute(route)"
                     class="route-item d-flex flex-column">

                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <div class="route-title">
                                <strong>{{ route.code }}</strong>
                                <span class="mx-2">•</span>
                                {{ route.originCity }} → {{ route.destinationCity }}
                            </div>

                            <div class="route-sub">
                                {{ route.aircraftTypeId }} •
                                <span v-for="day in route.frequency" :key="day">
                                    {{ day + " " }}
                                </span>
                            </div>

                            <div class="route-sub small">
                                Salida: {{ route.departureTime }} ·
                                Llegada: {{ route.arrivalTime }} ·
                                Duración: {{ route.duration }}
                            </div>
                        </div>

                        <div class="route-arrow">
                            {{ selectedRoute === route.code ? '⌄' : '›' }}
                        </div>
                    </div>

                    <div v-if="selectedRoute === route.code" class="route-details mt-3 route-sub small">
                        <div class="row mt-2">
                            <div class="col-md-4">
                                Primera Clase:
                                ${{ (route.priceFirstClass || 0).toLocaleString() }}
                            </div>
                            <div class="col-md-4">
                                Clase Turista:
                                ${{ (route.priceEconomy || 0).toLocaleString() }}
                            </div>
                        </div>
                        <div class="row mt-2">
                            <div class="col-md-4">
                                Equipaje de mano:
                                ${{ route.handBagPrice }} · {{ route.handBagWeight }}kg
                            </div>
                            <div class="col-md-4">
                                Equipaje documentado:
                                ${{ route.bagPrice }} · {{ route.bagWeight }}kg
                            </div>
                            <div class="col-md-4">
                                Multiplicador:
                                {{ route.bagMultiplier }}
                            </div>
                        </div>
                        <div class="row mt-2">
                            <div class="col-md-4">
                                Vigencia: {{ route.startDate }} → {{ route.finalizationDate }}
                            </div>
                            <div class="col-md-4">
                                Origen: {{ route.originAirport }} ({{ route.originCity }})
                            </div>
                            <div class="col-md-4">
                                Destino: {{ route.destinationAirport }} ({{ route.destinationCity }})
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>
</template>

<script>
    import axios from "axios";
    import API_BASE_URL from "@/config/api";

    export default {
        data() {
            return {
                successMessage: "",
                errorMessage: "",
                selectedRoute: null,
                form: {
                    code: "",
                    priceFirstClass: 0,
                    priceEconomy: 0,
                    handBagPrice: 0,
                    handBagWeight: 0,
                    bagPrice: 0,
                    bagWeight: 0,
                    bagMultiplier: 0,
                    originAirport: "",
                    destinationAirport: "",
                    departureTime: "",
                    arrivalTime: "",
                    duration: "",
                    aircraftTypeId: "",
                    frequency: [],
                    startDate: "",
                    finalizationDate: "",
                    economyClassCapacity: 0,
                    firstClassCapacity: 0
                },
                days: [
                    { label: 'L', val: 'Lunes' },
                    { label: 'M', val: 'Martes' },
                    { label: 'X', val: 'Miércoles' },
                    { label: 'J', val: 'Jueves' },
                    { label: 'V', val: 'Viernes' },
                    { label: 'S', val: 'Sábado' },
                    { label: 'D', val: 'Domingo' }
                ],
                airports: [],
                aircraftTypes: [],
                routes: [],
                isDropdownOpen: false
            };
        },

        async mounted() {
            await this.loadAirports();
            await this.loadRoutes();
            await this.loadAircraftTypes();
        },

        watch: {
            'form.aircraftTypeId'(newName) {
                const selected = this.aircraftTypes.find(t => t.type === newName);
                if (selected) {
                    this.form.firstClassCapacity =
                        (selected.firstClassRows || 0) * (selected.firstClassSeatsPerRow || 0);
                    this.form.economyClassCapacity =
                        (selected.economyRows || 0) * (selected.economySeatsPerRow || 0);
                }
            }
        },

        methods: {
            validateForm() {

                const filledData = (
                    this.form.originAirport !== "" &&
                    this.form.destinationAirport !== "" &&
                    this.form.departureTime !== "" &&
                    this.form.arrivalTime !== "" &&
                    this.form.duration !== "" &&
                    this.form.aircraftTypeId !== "" &&
                    this.form.code !== "" &&
                    Array.isArray(this.form.frequency) &&
                    this.form.frequency.length > 0 &&
                    this.form.startDate !== "" &&
                    this.form.finalizationDate !== ""
                );

                if (!filledData) {
                    this.errorMessage = "Todos los campos son requeridos";
                    return false;
                }

                if (this.form.destinationAirport === this.form.originAirport) {
                    this.errorMessage = "El aeropuerto de destino no puede ser el mismo que el de origen";
                    return false;
                }

                if (!this.isValidDuration(this.form.duration)) {
                    this.errorMessage = "Duración inválida (HH:mm)";
                    return false;
                }
                if (!this.validateDates()) {
                    return false;
                }

                return true;
            },

            async saveFlight() {
                this.successMessage = "";
                this.errorMessage = "";

                if (!this.validateForm()) {
                    return;
                }

                try {
                    const originAirport = this.airports.find(a => a.code === this.form.originAirport);
                    const destAirport = this.airports.find(a => a.code === this.form.destinationAirport);

                    await axios.post(`${API_BASE_URL}/api/routecreation`, {
                        ...this.form,
                        frequency: this.form.frequency,
                        originCity: originAirport?.city || "",
                        destinationCity: destAirport?.city || ""
                    });

                    this.successMessage = "Vuelo creado correctamente.";
                    await this.loadRoutes();
                    this.$router.push("/admin/routes");

                    this.form = {
                        code: "",
                        originAirport: "",
                        destinationAirport: "",
                        departureTime: "",
                        arrivalTime: "",
                        duration: "",
                        aircraftTypeId: "",
                        frequency: [],
                        priceFirstClass: 0,
                        priceEconomy: 0,
                        handBagPrice: 0,
                        handBagWeight: 0,
                        bagPrice: 0,
                        bagWeight: 0,
                        bagMultiplier: 0,
                        startDate: "",
                        finalizationDate: "",
                        economyClassCapacity: 0,
                        firstClassCapacity: 0
                    };
                } catch (err) {
                    this.errorMessage =
                        err.response?.data?.title ||
                        (typeof err.response?.data === "string" ? err.response.data : null) ||
                        "Error al crear el vuelo";
                }
            },

            async loadAirports() {
                try {
                    const response = await axios.get(`${API_BASE_URL}/api/airport`);
                    this.airports = response.data.map(a => ({
                        code: a.code ?? a.Code,
                        airportName: a.airportName ?? a.AirportName,
                        city: a.city ?? a.City,
                        country: a.country ?? a.Country
                    }));
                } catch (error) {
                    this.errorMessage = "No se pudieron cargar los aeropuertos.";
                }
            },

            async loadRoutes() {
                try {
                    const response = await axios.get(`${API_BASE_URL}/api/routecreation`);
                    this.routes = response.data.map(r => {
                        const originAirport = this.airports.find(a => a.code === r.originAirport);
                        const destAirport = this.airports.find(a => a.code === r.destinationAirport);
                        return {
                            code: r.code,
                            originAirport: r.originAirport,
                            originCity: originAirport?.city || r.originAirport,
                            destinationAirport: r.destinationAirport,
                            destinationCity: destAirport?.city || r.destinationAirport,
                            departureTime: r.departureTime,
                            arrivalTime: r.arrivalTime,
                            duration: r.duration,
                            aircraftTypeId: r.aircraftTypeId,
                            startDate: r.startDate,
                            finalizationDate: r.finalizationDate,
                            frequency: Array.isArray(r.frequency)
                                ? r.frequency
                                : (r.frequency || "").split(",").map(d => d.trim()).filter(Boolean),
                            priceFirstClass: Number(r.priceFirstClass) || 0,
                            priceEconomy: Number(r.priceEconomy) || 0,
                            handBagPrice: Number(r.handBagPrice) || 0,
                            handBagWeight: Number(r.handBagWeight) || 0,
                            bagPrice: Number(r.bagPrice) || 0,
                            bagWeight: Number(r.bagWeight) || 0,
                            bagMultiplier: Number(r.bagMultiplier) || 0,
                            economyClassCapacity: r.economyClassCapacity ?? 0,
                            firstClassCapacity: r.firstClassCapacity ?? 0
                        };
                    });
                } catch (error) {
                    console.error("Error cargando rutas:", error);
                }
            },

            async loadAircraftTypes() {
                try {
                    const response = await axios.get(`${API_BASE_URL}/api/aircraft`);
                    this.aircraftTypes = response.data;
                } catch (error) {
                    this.errorMessage = "No se pudieron cargar los tipos de avión.";
                }
            },

            toggleRoute(route) {
                this.selectedRoute = this.selectedRoute === route.code ? null : route.code;
            },

            onlyNumbersDuration(event) {
                const char = String.fromCharCode(event.keyCode);
                if (!/[0-9:]/.test(char)) event.preventDefault();
                if (char === ':' && event.target.value.includes(':')) event.preventDefault();
            },

            onlyNumbers(event) {
                const char = String.fromCharCode(event.keyCode);
                if (!/[0-9.]/.test(char)) event.preventDefault();
                if (char === '.' && event.target.value.includes('.')) event.preventDefault();
            },

            isValidDuration(duration) {
                const regex = /^(\d{2}):([0-5]\d)$/;
                if (!regex.test(duration)) return false;
                const [hours, minutes] = duration.split(':').map(Number);
                return hours >= 0 && minutes <= 59;
            },

            formatDuration() {
                let raw = this.form.duration.replace(/[^0-9]/g, '').slice(0, 4);
                let hours = raw.slice(0, 2);
                let minutes = raw.slice(2, 4);
                if (minutes.length === 2 && Number(minutes) > 59) minutes = '59';
                this.form.duration = minutes ? `${hours}:${minutes}` : hours;
            },
            toggleDropdown() {
                this.isDropdownOpen = !this.isDropdownOpen;
            },
            closeDropdown() {
                this.isDropdownOpen = false;
            },
            goToPage(page) {
                if (page >= 1 && page <= this.totalPages) this.currentPage = page;
            },
            isValidCode(event) {
                let value = event.target.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
                let letters = value.slice(0, 2).replace(/[^A-Z]/g, '');
                let numbers = value.slice(2, 6).replace(/[^0-9]/g, '');
                this.form.code = letters + numbers;
                const regex = /^[A-Z]{2}\d{4}$/;
                return regex.test(this.form.code);
            },
            validateDates() {
                if (!this.form.startDate || !this.form.finalizationDate) {
                    this.errorMessage = "Ambas fechas son obligatorias";
                    return false;
                }

                const start = new Date(this.form.startDate);
                const end = new Date(this.form.finalizationDate);

                if (isNaN(start.getTime()) || isNaN(end.getTime())) {
                    this.errorMessage = "El formato de las fechas no es válido (use MM/DD/YYYY)";
                    return false;
                }

                const today = new Date();
                today.setHours(0, 0, 0, 0);
                start.setHours(0, 0, 0, 0);
                end.setHours(0, 0, 0, 0);

                if (start.getTime() < today.getTime()) {
                    this.errorMessage = "La fecha de inicio no puede ser anterior a la fecha de hoy";
                    return false;
                }

                if (end.getTime() <= start.getTime()) {
                    this.errorMessage = "La fecha de finalización debe ser posterior a la fecha de inicio";
                    return false;
                }

                this.errorMessage = "";
                return true;
            }

        }
    };
</script>

<style scoped>
    .route-card {
        background: white;
        border-radius: 16px;
        padding: 28px;
        box-shadow: 0 10px 40px rgba(0,0,0,0.15);
    }

    .route-item {
        border: 1px solid #e5e5e5;
        border-radius: 14px;
        padding: 22px 26px;
        margin-bottom: 18px;
        transition: all 0.2s ease;
        cursor: pointer;
    }

        .route-item:hover {
            box-shadow: 0 5px 20px rgba(0,0,0,0.08);
        }

    .route-title {
        font-size: 1.25rem;
        color: #212529;
        font-weight: 400;
    }

    .route-sub {
        font-size: 1rem;
        color: #6c757d;
        margin-top: 6px;
    }

    .route-arrow {
        font-size: 26px;
        color: #999;
    }

    .brand-name {
        font-weight: 700;
        font-size: 1.05rem;
    }

    .brand-tagline {
        font-size: 0.7rem;
        color: #888;
    }

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

    .btn-gradient {
        background: linear-gradient(to right, #e74c3c, #f39c12);
        color: white;
        border-radius: 20px;
        padding: 6px 14px;
        border: none;
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

    .flight-card {
        background: white;
        border-radius: 16px;
        padding: 28px;
        box-shadow: 0 10px 40px rgba(0,0,0,0.15);
    }

    .input-box {
        border: 1.5px solid #e0e0e0;
        border-radius: 8px;
        padding: 10px;
    }

        .input-box:focus-within {
            border-color: #e74c3c;
        }

        .input-box input,
        .input-box select {
            border: none;
            outline: none;
            width: 100%;
        }

    .day-container {
        display: flex;
        gap: 10px;
    }

    .day-pill {
        padding: 6px 12px;
        border-radius: 20px;
        border: 1px solid #ddd;
        cursor: pointer;
    }

        .day-pill.active {
            background: linear-gradient(to right, #e74c3c, #f39c12);
            color: white;
        }

    .search-btn {
        width: 100%;
        padding: 14px;
        background: linear-gradient(to right, #e74c3c, #f39c12);
        color: white;
        border: none;
        border-radius: 8px;
    }

    .alert-success-custom {
        background: #e6f4ea;
        color: #2e7d32;
        padding: 14px;
        border-radius: 10px;
        border: 1px solid #b7dfc2;
        margin-bottom: 20px;
    }

    .alert-error-custom {
        background: #fdecea;
        color: #c62828;
        padding: 14px;
        border-radius: 10px;
        border: 1px solid #f5c6cb;
        margin-bottom: 20px;
    }
</style>