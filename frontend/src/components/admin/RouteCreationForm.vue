<template>
    <AdminPageLayout>

        <div class="admin-banner mb-4">
            <h1 style="font-weight: bold">
                <img src="@/assets/GestionBox.png" width="44" class="me-2" />
                Gestión de Rutas
            </h1>
            <p>Panel de administración para operadores de Mushu Airlines</p>
        </div>

        <div class="mb-5 flight-container">
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
                            <label>Aeronave<span>*</span></label>
                            <select v-model.number="form.aircraftCode" class="form-control" required>
                                <option :value="null" disabled>Seleccione una aeronave</option>

                                <option
                                    v-for="aircraft in aircraftTypes"
                                    :key="aircraft.id"
                                    :value="aircraft.id"
                                >
                                    {{ aircraft.type }} — {{ aircraft.model }}
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

        <div class="mb-5">
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

    </AdminPageLayout>
</template>

<script>
    import axios from "axios";
    import API_BASE_URL from "@/config/api";
    import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";

    export default {
        components: {
            AdminPageLayout,
        },

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
                    aircraftCode: null,
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
                routes: []
            };
        },

        async mounted() {
            await this.loadAirports();
            await this.loadAircraftTypes();
            await this.loadRoutes();
        },

        watch: {
            'form.aircraftCode'(selectedAircraftCode) {
                if (!selectedAircraftCode) return;

                const selected = this.aircraftTypes.find(
                    aircraft => aircraft.id === selectedAircraftCode
                );

                if (selected) {
                    this.form.aircraftTypeId = selected.model;

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
                    this.form.aircraftCode != null &&
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
                    const selectedAircraft = this.aircraftTypes.find(
                        aircraft => aircraft.id === this.form.aircraftCode
                    );
                    await axios.post(`${API_BASE_URL}/api/routecreation`, {
                        ...this.form,
                        aircraftCode: selectedAircraft?.id ?? null,
                        aircraftTypeId: selectedAircraft?.model ?? "",
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
                        aircraftCode: null,
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

                    this.aircraftTypes = response.data.map(aircraft => ({
                        id: aircraft.id ?? aircraft.Id,
                        model: aircraft.model ?? aircraft.Model,
                        type: aircraft.type ?? aircraft.Type,
                        weightKg: aircraft.weightKg ?? aircraft.WeightKg,
                        capacity: aircraft.capacity ?? aircraft.Capacity,
                        economyRows: aircraft.economyRows ?? aircraft.EconomyRows,
                        economySeatsPerRow: aircraft.economySeatsPerRow ?? aircraft.EconomySeatsPerRow,
                        firstClassRows: aircraft.firstClassRows ?? aircraft.FirstClassRows,
                        firstClassSeatsPerRow: aircraft.firstClassSeatsPerRow ?? aircraft.FirstClassSeatsPerRow
                    }));
                } catch (error) {
                    this.errorMessage = "No se pudieron cargar las aeronaves.";
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