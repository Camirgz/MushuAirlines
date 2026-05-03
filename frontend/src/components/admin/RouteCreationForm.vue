<template>
    <div>

        <nav class="navbar bg-white shadow-sm px-4 py-2">
            <a class="navbar-brand d-flex align-items-center gap-2" href="#">
                <img src="@/assets/logo.png" width="42" height="42" class="rounded-2" />
                <div>
                    <div class="brand-name">Mushu Airlines</div>
                    <div class="brand-tagline">Vuela con el dragón</div>
                </div>
            </a>

            <div class="d-flex align-items-center gap-4">
                <a href="#" class="nav-link-item">
                    <img src="@/assets/BuscarVuelos.png" width="16" />
                    Buscar vuelos
                </a>
                <a href="#" class="nav-link-item">
                    <img src="@/assets/MisVuelos.png" width="16" />
                    Mis vuelos
                </a>
                <a href="#" class="nav-link-item">
                    <img src="@/assets/CheckIn.png" width="16" />
                    Check-in
                </a>
                <a href="/admin" class="btn btn-gradient">
                    <img src="@/assets/Gestion.png" width="16" />
                    Gestión
                </a>

                <a href="/logout" class="btn btn-outline-danger rounded-pill px-3 py-1">
                    <img src="@/assets/Usuario.png" width="16" />
                    Logout
                </a>
            </div>
        </nav>

        <div class="admin-header container mt-5">
            <div class="admin-banner">
                <h1 style="font-weight: bold">
                    <img src="@/assets/GestionBox.png" width="44" class="me-2" />
                    Gestión de Rutas
                </h1>
                <p>Panel de administración para operadores de Mushu Airlines</p>
            </div>
        </div>

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
                            <label>Aeropuerto de Origen*</label>
                            <select v-model="form.originAirport" class="form-control" required>
                                <option v-for="a in airports" :key="a.id" :value="a.id">{{ a.name }}</option>
                            </select>
                        </div>

                        <div class="col-md-6 form-group">
                            <label>Aeropuerto de Destino*</label>
                            <select v-model="form.destinationAirport" class="form-control" required>
                                <option v-for="a in airports" :key="a.id" :value="a.id">{{ a.name }}</option>
                            </select>
                        </div>

                        <div class="col-md-4 form-group">
                            <label>Hora de Salida*</label>
                            <div class="input-box">
                                <input type="time" v-model="form.departureTime" placeholder="00:00" required />
                            </div>
                        </div>

                        <div class="col-md-4 form-group">
                            <label>Hora de Llegada*</label>
                            <div class="input-box">
                                <input type="time" v-model="form.arrivalTime" placeholder="00:00" required />
                            </div>
                        </div>

                        <div class="col-md-4 form-group">
                            <label>Duración*</label>
                            <div class="input-box">
                                <input type="text" v-model="form.duration" placeholder="00:00" required />
                            </div>
                        </div>
                    </div>

                    <div class="row mt-3">
                        <div class="col-md-6 form-group">
                            <label>Tipo de Aeronave*</label>
                            <select v-model="form.aircraftTypeId" class="form-control" required>
                                <option v-for="plane in aircraftTypes" :key="plane.id" :value="plane.id">{{ plane.model }}</option>
                            </select>
                        </div>
                        <div class="col-md-6 form-group">
                            <label>Código**</label>
                            <div class="input-box">
                                <input type="text" v-model="form.code" placeholder="XX0000" required />
                            </div>
                        </div>
                    </div>

                    <div class="form-group mt-4">
                        <label>Frecuencia*</label>
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

                    <h5 class="mt-4" style="font-weight: bold ; font-size: x-large">Tarifas</h5>
                    <div class="row">
                        <div class="col-md-6 form-group">
                            <label>Primera Clase*</label>
                            <div class="input-box">
                                <input type="number" v-model.number="form.priceFirstClass" placeholder="₡ 0.00" />
                            </div>
                        </div>

                        <div class="col-md-6 form-group">
                            <label>Clase Turista*</label>
                            <div class="input-box">
                                <input type="number" v-model.number="form.priceEconomy" placeholder="₡ 0.00" />
                            </div>
                        </div>
                    </div>

                    <h5 class="mt-4">Políticas de Equipaje</h5>
                    <div class="row">
                        <div class="col-md-3 form-group">
                            <label>Precio equipaje de mano*</label>
                            <div class="input-box">
                                <input type="number" v-model.number="form.handBagPrice" placeholder="₡ 0.00" />
                            </div>
                        </div>

                        <div class="col-md-3 form-group">
                            <label>Peso equipaje de mano*</label>
                            <div class="input-box">
                                <input type="number" v-model.number="form.handBagWeight" placeholder="0.00" />
                            </div>
                        </div>

                        <div class="col-md-3 form-group">
                            <label>Precio equipaje documentado*</label>
                            <div class="input-box">
                                <input type="number" v-model.number="form.bagPrice" placeholder="₡ 0.00" />
                            </div>
                        </div>

                        <div class="col-md-3 form-group">
                            <label>Peso equipaje documentado*</label>
                            <div class="input-box">
                                <input type="number" v-model.number="form.bagWeight" placeholder="0.00" />
                            </div>
                        </div>

                        <div class="col-md-4 form-group mt-2">
                            <label>Multiplicador*</label>
                            <div class="input-box">
                                <input type="number" v-model.number="form.bagMultiplier" placeholder="0.00" />
                            </div>
                        </div>
                    </div>

                    <button type="submit" class="search-btn mt-4" style="font-size: large ; font-weight: bold">
                        Crear ruta
                    </button>

                </form>
            </div>
        </div>
    </div>
<div>


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
                                {{ route.originAirport }} → {{ route.destinationAirport }}
                        </div>

                        <div class="route-sub">
                            {{ route.aircraftTypeId }} •
                            <span v-for="day in route.frequency" :key="day">
                                {{ day + " "}}
                            </span>
                        </div>

                        <div class="route-sub small">
                            Salida: {{ route.departureTime }} ·
                            Llegada: {{ route.arrivalTime }}
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
                            ₡{{ route.priceFirstClass.toLocaleString() }}
                        </div>

                       <div class="col-md-4">
                            Clase Turista:
                            ₡{{ route.priceEconomy.toLocaleString() }}
                       </div>
                       
                    </div>
                        <div class="row mt-2">
                            <div class="col-md-4">
                                Equipaje de mano:
                                ₡{{ route.handBagPrice }} · {{ route.handBagWeight }}kg
                            </div>

                            <div class="col-md-4">
                                Equipaje documentado:
                                ₡{{ route.bagPrice }} · {{ route.bagWeight }}kg
                            </div>

                            <div class="col-md-4">
                                Multiplicador:
                                {{ route.bagMultiplier }}
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
                frequency: []
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
                // hardcodeado por mientras
                airports: [
                    { id: "MAD", name: "Madrid-Barajas" },
                    { id: "JFK", name: "JFK" },
                    { id: "SJO", name: "Juan Santamaría" },
                    { id: "LHR", name: "Londres-Heathrow" },
                    { id: "CDG", name: "París-Charles de Gaulle" },
                    { id: "FRA", name: "Fráncfort" },
                    { id: "AMS", name: "Ámsterdam-Schiphol" },
                    { id: "BCN", name: "Barcelona-El Prat" }
                ],
                // hardcodeado por mientras
                aircraftTypes: [
                    { id: "A320", model: "Airbus A320" },
                    { id: "B737", model: "Boeing 737" },
                    { id: "E190", model: "Embraer 190" },
                    { id: "A321", model: "Airbus A321" },
                    { id: "B787", model: "Boeing 787" },
                    { id: "E195", model: "Embraer 195" }
                ],
                routes: []
            };
        },
        async mounted() {
            await this.loadRoutes();
        },
        methods: {
            validateForm() {
                const f = this.form;

                const isValid =
                    f.originAirport !== "" &&
                    f.destinationAirport !== "" &&
                    f.departureTime !== "" &&
                    f.arrivalTime !== "" &&
                    f.duration !== "" &&
                    f.aircraftTypeId !== "" &&
                    f.aircraftTypeId !== undefined &&
                    f.code !== "" &&
                    Array.isArray(f.frequency) &&
                    f.frequency.length > 0;

                return isValid;
            },

            async saveFlight() {
                this.successMessage = "";
                this.errorMessage = "";

                if (!this.validateForm()) {
                    this.errorMessage = "Todos los campos son requeridos";
                    return;
                }

                try {
                    const payload = {
                        ...this.form,
                        frequency: this.form.frequency

                    };

                    await axios.post("http://localhost:5103/api/routecreation", payload);

                    this.successMessage = "Vuelo creado correctamente.";

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
                        bagMultiplier: 0
                    };

                } catch (err) {
                    this.errorMessage = err.response.data.title || "Error al crear el vuelo";
                }
            },
            async loadRoutes() {
                try {
                    const response = await axios.get("http://localhost:5103/api/routecreation");
                    this.routes = response.data;
                } catch (error) {
                    console.error("Error cargando rutas:", error);
                }
            },
            toggleRoute(route) {
                if (this.selectedRoute === route.code) {
                    this.selectedRoute = null;
                } else {
                    this.selectedRoute = route.code;
                }
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

    .nav-link-item {
        color: #333;
        font-size: 0.9rem;
        text-decoration: none;
    }

        .nav-link-item:hover {
            color: #e74c3c;
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

    .route-card {
        background: white;
        border-radius: 16px;
        padding: 88px;
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