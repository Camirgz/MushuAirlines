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
                <form @submit.prevent="saveFlight">

                    <h3 style="font-weight: bold;">+ Crear nueva ruta</h3>

                    <!-- INFO -->
                    <div class="row">
                        <div class="col-md-6 form-group">
                            <label>Aeropuerto de Origen*</label>
                            <select v-model="form.originId" class="form-control" required>
                                <option v-for="a in airports" :key="a.id" :value="a.id">{{ a.name }}</option>
                            </select>
                        </div>

                        <div class="col-md-6 form-group">
                            <label>Aeropuerto de Destino*</label>
                            <select v-model="form.destinationId" class="form-control" required>
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
                            <select v-model="form.aircraftId" class="form-control" required>
                                <option v-for="plane in aircraftTypes" :key="plane.id" :value="plane.id">{{ plane.model }}</option>
                            </select>
                        </div>
                        <div class="col-md-6 form-group">
                            <label>Estado Inicial del Vuelo*</label>
                            <select v-model="form.status" class="form-control" required>
                                <option value="Pendiente">Pendiente</option>
                                <option value="A Tiempo">A Tiempo</option>
                                <option value="Inicio Abordaje">Inicio Abordaje / Abordando</option>
                                <option value="Cancelado">Cancelado</option>
                                <option value="Retrasado">Retrasado</option>
                            </select>
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

                    <h5 class="mt-4" style="font-weight: bold">Tarifas y Asientos</h5>
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


                    <button type="submit" class="search-btn mt-4" style="font-weight: bold">
                        Crear ruta
                    </button>

                </form>
            </div>
        </div>

    </div>
</template>

<script>
    import axios from "axios";

    export default {
        data() {
            return {
                form: {
                    originId: "", destinationId: "", frequency: [],
                    departureTime: "", arrivalTime: "", duration: ""
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
                airports: [
                    { id: "MAD", name: "Madrid-Barajas" },
                    { id: "JFK", name: "JFK" },
                    { id: "SJO", name: "Juan Santamaría" }
                ],
                aircraftTypes: [
                    { id: "A320", model: "Airbus A320" },
                    { id: "B737", model: "Boeing 737" },
                    { id: "E190", model: "Embraer 190" }
                ]
            };
        },
        methods: {
            saveFlight() {
                axios.post("http://localhost:5103/api/flights", this.form)
                    .then(() => alert("Vuelo creado"))
                    .catch(err => console.error(err));
            }
        }
    };
</script>

<style scoped>

    /* NAVBAR */
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

    /* BOTÓN GRADIENT */
    .btn-gradient {
        background: linear-gradient(to right, #e74c3c, #f39c12);
        color: white;
        border-radius: 20px;
        padding: 6px 14px;
        border: none;
    }

    /* HEADER */
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

    /* CARD */
    .flight-card {
        background: white;
        border-radius: 16px;
        padding: 28px;
        box-shadow: 0 10px 40px rgba(0,0,0,0.15);
    }

    /* INPUTS */
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

    /* DAYS */
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

    /* BUTTON */
    .search-btn {
        width: 100%;
        padding: 14px;
        background: linear-gradient(to right, #e74c3c, #f39c12);
        color: white;
        border: none;
        border-radius: 8px;
    }
</style>