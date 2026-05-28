<template>
  <div>

    <nav class="navbar bg-white shadow-sm px-4 py-2">
      <a class="navbar-brand d-flex align-items-center gap-2" href="#">
        <img src="@/assets/logo.png" alt="Logo" width="42" height="42" class="rounded-2" />
        <div>
          <div class="brand-name">Mushu Airlines</div>
          <div class="brand-tagline">Vuela con el dragón</div>
        </div>
      </a>

      <div class="d-flex align-items-center gap-4">
        <a href="#" class="nav-link-item">
          <i class="bi bi-briefcase me-1"></i>Mis vuelos
        </a>
        <a href="#" class="nav-link-item">
          <i class="bi bi-calendar-check me-1"></i>Check-in
        </a>
        <a href="/login" class="btn btn-outline-danger rounded-pill px-3 py-1 admin-btn">
          <i class="bi bi-person me-1"></i>Admin Login
        </a>
      </div>
    </nav>

    <div class="hero-section">
      <div class="hero-plane-bg">✈</div>

      <div class="hero-content">
        <h1 class="hero-title">Encuentra tu vuelo perfecto</h1>
        <p class="hero-subtitle">Viaja con Mushu Airlines a más de 100 destinos alrededor del mundo</p>

        <div class="search-card">
          <div class="search-fields">

            <!-- Origin -->
            <div class="field-group autocomplete-wrapper">
              <label class="field-label"><i class="bi bi-geo-alt me-1"></i>Origen</label>
              <div class="input-box" :class="{ focused: showOriginDropdown }">
                <i class="bi bi-geo-alt field-icon"></i>
                <input
                  type="text"
                  placeholder="Ciudad o aeropuerto"
                  v-model="originQuery"
                  @input="onOriginInput"
                  @focus="showOriginDropdown = true"
                  @blur="onOriginBlur"
                  autocomplete="off"
                />
              </div>
              <div class="autocomplete-dropdown" v-if="showOriginDropdown && originSuggestions.length">
                <div
                  v-for="airport in originSuggestions"
                  :key="airport.code"
                  class="autocomplete-item"
                  @mousedown.prevent="selectOrigin(airport)"
                >
                  <span class="airport-code">{{ airport.code }}</span>
                  <span class="airport-text">
                    <span class="airport-city">{{ airport.city }}</span>
                    <span class="airport-country">{{ airport.country }}</span>
                  </span>
                </div>
              </div>
            </div>

            <!-- Destination -->
            <div class="field-group autocomplete-wrapper">
              <label class="field-label"><i class="bi bi-geo-alt-fill me-1"></i>Destino</label>
              <div class="input-box" :class="{ focused: showDestinationDropdown }">
                <i class="bi bi-geo-alt field-icon"></i>
                <input
                  type="text"
                  placeholder="Ciudad o aeropuerto"
                  v-model="destinationQuery"
                  @input="onDestinationInput"
                  @focus="showDestinationDropdown = true"
                  @blur="onDestinationBlur"
                  autocomplete="off"
                />
              </div>
              <div class="autocomplete-dropdown" v-if="showDestinationDropdown && destinationSuggestions.length">
                <div
                  v-for="airport in destinationSuggestions"
                  :key="airport.code"
                  class="autocomplete-item"
                  @mousedown.prevent="selectDestination(airport)"
                >
                  <span class="airport-code">{{ airport.code }}</span>
                  <span class="airport-text">
                    <span class="airport-city">{{ airport.city }}</span>
                    <span class="airport-country">{{ airport.country }}</span>
                  </span>
                </div>
              </div>
            </div>

            <!-- Date -->
            <div class="field-group">
              <label class="field-label"><i class="bi bi-calendar3 me-1"></i>Fecha de salida</label>
              <div class="input-box">
                <input type="date" v-model="departureDate" />
              </div>
            </div>

            <!-- Passengers -->
            <div class="field-group">
              <label class="field-label"><i class="bi bi-people me-1"></i>Pasajeros</label>
              <div class="input-box passenger-selector">
                <button class="passenger-btn" type="button" @click="passengerCount = Math.max(1, passengerCount - 1)">−</button>
                <span class="passenger-count">{{ passengerCount }}</span>
                <button class="passenger-btn" type="button" @click="passengerCount = Math.min(9, passengerCount + 1)">+</button>
              </div>
            </div>

            <!-- Flight type toggle -->
            <div class="field-group toggle-field-group">
              <label class="field-label"><i class="bi bi-airplane me-1"></i>Tipo de vuelo</label>
              <div class="flight-type-toggle" role="group" aria-label="Tipo de vuelo">
                <button
                  class="toggle-option"
                  :class="{ active: flightSearchMode === 'direct' }"
                  type="button"
                  @click="flightSearchMode = 'direct'"
                >
                  <i class="bi bi-arrow-right me-1"></i>Directo
                </button>
                <button
                  class="toggle-option"
                  :class="{ active: flightSearchMode === 'stopover' }"
                  type="button"
                  @click="flightSearchMode = 'stopover'"
                >
                  <i class="bi bi-arrow-left-right me-1"></i>Con escala
                </button>
              </div>
            </div>

          </div>

          <p v-if="errorMsg" class="error-msg">{{ errorMsg }}</p>

          <button class="search-btn" @click="searchFlights">
            <i class="bi bi-search me-2"></i>Buscar vuelos
          </button>
        </div>
      </div>
    </div>

    <div class="results-section" v-if="hasSearched">
      <div class="results-layout">

        <aside class="filters-panel">
          <h3 class="filters-title">Filtros</h3>

          <div class="filter-group">
            <div class="filter-header">
              <span>Precio</span>
              <span class="filter-value">₡{{ filterPriceMin.toLocaleString() }} - ₡{{ filterPriceMax.toLocaleString() }}</span>
            </div>
            <div class="price-class-toggle" role="group" aria-label="Clase de precio">
              <button
                class="price-class-btn"
                :class="{ active: filterPriceClass === 'economy' }"
                type="button"
                @click="setFilterPriceClass('economy')"
              >Turista</button>
              <button
                class="price-class-btn"
                :class="{ active: filterPriceClass === 'business' }"
                type="button"
                @click="setFilterPriceClass('business')"
              >Business</button>
            </div>
            <input
              type="range"
              v-model.number="filterPriceMax"
              :min="filterPriceMin"
              :max="priceSliderRange"
              step="100"
              class="range-slider"
            />
          </div>

          <div class="filter-group">
            <div class="filter-header">
              <span>Duración</span>
              <span class="filter-value">0h - {{ filterDurationMax }}h</span>
            </div>
            <input
              type="range"
              v-model.number="filterDurationMax"
              min="0"
              max="24"
              step="0.5"
              class="range-slider"
            />
          </div>
        </aside>

        <div class="flights-panel">

          <!-- Direct results -->
          <template v-if="flightSearchMode === 'direct'">
            <p class="results-count">{{ filteredDirectFlights.length }} vuelos directos disponibles</p>

            <div v-if="filteredDirectFlights.length === 0" class="no-results">
              <i class="bi bi-airplane"></i>
              <p>No hay vuelos directos disponibles para esta búsqueda.</p>
            </div>

            <FlightResultCard
              v-for="flight in filteredDirectFlights"
              :key="flight.id"
              result-type="direct"
              :direct-flight="flight"
              :passenger-count="passengerCount"
              @select="openFlightDetails"
            />
          </template>

          <!-- Stopover results -->
          <template v-else>
            <p class="results-count">{{ filteredStopoverResults.length }} itinerarios con escala disponibles</p>

            <div v-if="filteredStopoverResults.length === 0" class="no-results">
              <i class="bi bi-airplane"></i>
              <p>No hay itinerarios con escala disponibles para esta búsqueda.</p>
            </div>

            <FlightResultCard
              v-for="(connection, index) in filteredStopoverResults"
              :key="index"
              result-type="stopover"
              :leg1="connection.leg1"
              :leg2="connection.leg2"
              :layover-minutes="connection.layoverMinutes"
              :connection-city="connection.connectionCity"
              :passenger-count="passengerCount"
              @select="handleStopoverSelect"
            />
          </template>

        </div>
      </div>
    </div>

    <div v-if="!hasSearched" class="adventure-section">
      <h2 class="adventure-title">¿Listo para tu próxima aventura?</h2>
      <p class="adventure-subtitle">Comienza tu búsqueda arriba y descubre nuestras mejores ofertas</p>
    </div>

    <!-- Booking modal — direct flights only -->
    <Transition name="modal-fade">
      <div class="modal-overlay" v-if="selectedFlight" @click.self="closeFlightDetails">
        <div class="flight-modal">

          <button class="modal-close-btn" @click="closeFlightDetails">
            <i class="bi bi-x-lg"></i>
          </button>

          <div class="modal-airline-header">
            <div class="modal-airline-info">
              <div class="airline-icon">
                <i class="bi bi-airplane-fill"></i>
              </div>
              <div>
                <div class="modal-airline-name">Mushu Airlines</div>
                <div class="modal-flight-id">Vuelo #{{ selectedFlight.id.toString().padStart(3, '0') }}</div>
              </div>
            </div>
          </div>

          <div class="modal-route-section">
            <div class="modal-endpoint">
              <div class="modal-time">{{ selectedFlight.departureTime }}</div>
              <div class="modal-city">{{ getAirportCity(selectedFlight.origin) }}</div>
              <div class="modal-iata">{{ selectedFlight.origin }}</div>
              <div class="modal-date">{{ selectedFlight.date }}</div>
            </div>

            <div class="modal-route-middle">
              <div class="modal-duration-label">{{ selectedFlight.durationLabel }}</div>
              <div class="modal-route-line">
                <div class="modal-route-dot left"></div>
                <div class="modal-route-plane"><i class="bi bi-airplane-fill"></i></div>
                <div class="modal-route-dot right"></div>
              </div>
            </div>

            <div class="modal-endpoint right">
              <div class="modal-time">{{ selectedFlight.arrivalTime }}</div>
              <div class="modal-city">{{ getAirportCity(selectedFlight.destination) }}</div>
              <div class="modal-iata">{{ selectedFlight.destination }}</div>
              <div class="modal-date">{{ selectedFlight.arrivalDate }}</div>
            </div>
          </div>

          <div class="modal-info-grid">
            <div class="modal-info-item">
              <span class="modal-info-label"><i class="bi bi-clock me-1"></i>Duración</span>
              <span class="modal-info-value">{{ selectedFlight.durationLabel }}</span>
            </div>
            <div class="modal-info-item">
              <span class="modal-info-label"><i class="bi bi-airplane me-1"></i>Aeronave</span>
              <span class="modal-info-value">{{ selectedFlight.aircraftTypeId }}</span>
            </div>
            <div class="modal-info-item">
              <span class="modal-info-label"><i class="bi bi-globe me-1"></i>País destino</span>
              <span class="modal-info-value">{{ getAirportCountry(selectedFlight.destination) }}</span>
            </div>
            <div class="modal-info-item">
              <span class="modal-info-label"><i class="bi bi-people me-1"></i>Pasajeros</span>
              <span class="modal-info-value">{{ passengerCount }}</span>
            </div>
          </div>

          <!-- Seat class distribution -->
          <div class="modal-section-block">
            <div class="modal-section-title">Distribución de clase</div>
            <div class="modal-class-row">
              <div class="modal-class-info">
                <span class="modal-class-name">Primera Clase</span>
                <span class="modal-class-price">₡{{ selectedFlight.priceFirstClass.toLocaleString() }} / persona</span>
              </div>
              <div class="modal-qty-ctrl">
                <button class="modal-qty-btn" @click="firstClassCount = Math.max(0, firstClassCount - 1)">−</button>
                <span class="modal-qty-val">{{ firstClassCount }}</span>
                <button class="modal-qty-btn" @click="firstClassCount = Math.min(passengerCount, firstClassCount + 1)">+</button>
              </div>
            </div>
            <div class="modal-class-row">
              <div class="modal-class-info">
                <span class="modal-class-name">Clase Turista</span>
                <span class="modal-class-price">₡{{ selectedFlight.priceEconomy.toLocaleString() }} / persona</span>
              </div>
              <div class="modal-qty-ctrl">
                <span class="modal-qty-val readonly">{{ economyCount }}</span>
              </div>
            </div>
            <p v-if="firstClassCount + economyCount !== passengerCount" class="modal-class-warning">
              La suma debe ser {{ passengerCount }} pasajero(s)
            </p>
          </div>

          <!-- Luggage -->
          <div class="modal-section-block">
            <div class="modal-section-title">Equipaje</div>
            <div class="modal-class-row">
              <div class="modal-class-info">
                <span class="modal-class-name">Equipaje de mano</span>
                <span class="modal-class-price">₡{{ selectedFlight.handBagPrice.toLocaleString() }} · máx {{ selectedFlight.handBagWeight }}kg c/u</span>
              </div>
              <div class="modal-qty-ctrl">
                <button class="modal-qty-btn" @click="handBagsCount = Math.max(0, handBagsCount - 1)">−</button>
                <span class="modal-qty-val">{{ handBagsCount }}</span>
                <button class="modal-qty-btn" @click="handBagsCount++">+</button>
              </div>
            </div>
            <div class="modal-class-row">
              <div class="modal-class-info">
                <span class="modal-class-name">Equipaje documentado</span>
                <span class="modal-class-price">₡{{ selectedFlight.bagPrice.toLocaleString() }} · máx {{ selectedFlight.bagWeight }}kg c/u
                  <span v-if="selectedFlight.bagMultiplier !== 1"> (×{{ selectedFlight.bagMultiplier }})</span>
                </span>
              </div>
              <div class="modal-qty-ctrl">
                <button class="modal-qty-btn" @click="checkedBagsCount = Math.max(0, checkedBagsCount - 1)">−</button>
                <span class="modal-qty-val">{{ checkedBagsCount }}</span>
                <button class="modal-qty-btn" @click="checkedBagsCount++">+</button>
              </div>
            </div>
          </div>

          <!-- Purchase summary -->
          <div class="modal-receipt">
            <div class="modal-section-title">Resumen</div>

            <div class="receipt-line" v-if="firstClassCount > 0">
              <span>{{ firstClassCount }} × Primera Clase</span>
              <span>₡{{ (firstClassCount * selectedFlight.priceFirstClass).toLocaleString() }}</span>
            </div>
            <div class="receipt-line" v-if="economyCount > 0">
              <span>{{ economyCount }} × Clase Turista</span>
              <span>₡{{ (economyCount * selectedFlight.priceEconomy).toLocaleString() }}</span>
            </div>
            <div class="receipt-line" v-if="handBagsCount > 0">
              <span>{{ handBagsCount }} × Equipaje de mano</span>
              <span>₡{{ (handBagsCount * selectedFlight.handBagPrice).toLocaleString() }}</span>
            </div>
            <div class="receipt-line" v-if="checkedBagsCount > 0">
              <span>{{ checkedBagsCount }} × Equipaje documentado
                <span v-if="selectedFlight.bagMultiplier !== 1">(×{{ selectedFlight.bagMultiplier }})</span>
              </span>
              <span>₡{{ (checkedBagsCount * selectedFlight.bagPrice * selectedFlight.bagMultiplier).toLocaleString() }}</span>
            </div>

            <div class="receipt-total">
              <span>Total</span>
              <span>₡{{ modalTotal.toLocaleString() }}</span>
            </div>
          </div>

          <div class="modal-actions">
            <button class="modal-cancel-btn" @click="closeFlightDetails">Cancelar</button>
            <button class="modal-cart-btn" :disabled="firstClassCount + economyCount === 0">
              <i class="bi bi-cart3 me-2"></i>Agregar al carrito
            </button>
          </div>

        </div>
      </div>
    </Transition>

  </div>
</template>

<script>
import FlightResultCard from './FlightResultCard.vue'
import { findStopoverConnections, isOvernightFlight, addDaysToDateString } from '@/services/connectionFinder.js'

function durationToHours(dur) {
  const [h, m] = dur.split(':')
  return parseInt(h) + parseInt(m) / 60
}

function durationToLabel(dur) {
  const [h, m] = dur.split(':')
  const hours = parseInt(h), minutes = parseInt(m)
  return minutes === 0 ? `${hours}h` : `${hours}h ${minutes}min`
}

// Maps JS Date.getDay() (0=Sunday) to the Spanish day names stored in the DB.
const WEEKDAY_NAMES = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado']

// Returns true when no date is provided or the flight runs on that weekday.
function flightOperatesOnDate(flight, dateStr) {
  if (!dateStr || !flight.frequency || flight.frequency.length === 0) return true
  const [year, month, day] = dateStr.split('-').map(Number)
  const date = new Date(year, month - 1, day)
  const dayName = WEEKDAY_NAMES[date.getDay()]
  return flight.frequency.includes(dayName)
}

function routeToFlight(r) {
  const hours = durationToHours(r.duration)
  return {
    id: r.code,
    origin: r.originAirport,
    destination: r.destinationAirport,
    originCity: r.originCity,
    destinationCity: r.destinationCity,
    duration: r.duration,
    departureTime: r.departureTime,
    arrivalTime: r.arrivalTime,
    durationHours: hours,
    durationLabel: durationToLabel(r.duration),
    aircraftTypeId: r.aircraftTypeId,
    price: r.priceEconomy,
    priceFirstClass: r.priceFirstClass,
    priceEconomy: r.priceEconomy,
    handBagPrice: r.handBagPrice,
    handBagWeight: r.handBagWeight,
    bagPrice: r.bagPrice,
    bagWeight: r.bagWeight,
    bagMultiplier: r.bagMultiplier,
    // Normalise frequency to always be an array of Spanish day names.
    frequency: Array.isArray(r.frequency)
      ? r.frequency
      : (r.frequency || '').split(',').map(d => d.trim()).filter(Boolean),
  }
}

export default {
  name: 'LandingPage',

  components: { FlightResultCard },

  data() {
    return {
      airports: [],
      flights: [],

      departureDate: '',

      originQuery: '',
      selectedOrigin: null,
      showOriginDropdown: false,

      destinationQuery: '',
      selectedDestination: null,
      showDestinationDropdown: false,

      // 'direct' shows direct routes; 'stopover' shows two-leg connections.
      flightSearchMode: 'direct',

      hasSearched: false,
      directFlightResults: [],
      stopoverResults: [],

      filterPriceMin: 0,
      filterPriceMax: 500,
      priceSliderRange: 500,
      filterDurationMax: 24,
      filterPriceClass: 'economy',
      maxEconomyPrice: 100000,
      maxBusinessPrice: 200000,

      passengerCount: 1,
      errorMsg: '',
      selectedFlight: null,
      firstClassCount: 0,
      handBagsCount: 0,
      checkedBagsCount: 0,
    }
  },

  async created() {
    try {
      const airportRes = await fetch('http://localhost:5103/api/AirportCreation')
      this.airports = await airportRes.json()
    } catch (e) {
      console.error('Error cargando aeropuertos:', e)
    }
    try {
      const flightsRes = await fetch('http://localhost:5103/api/flights')
      const flights = await flightsRes.json()
      this.flights = flights.map(routeToFlight)
    } catch (e) {
      console.error('Error cargando vuelos:', e)
    }
  },

  computed: {
    originSuggestions() {
      if (!this.originQuery) return []
      const normalize = s => s.toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, '')
      const query = normalize(this.originQuery)
      return this.airports.filter(a =>
        a.code.toLowerCase().startsWith(query) ||
        normalize(a.city).includes(query) ||
        normalize(a.country).includes(query)
      ).slice(0, 8)
    },

    destinationSuggestions() {
      if (!this.destinationQuery) return []
      const normalize = s => s.toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, '')
      const query = normalize(this.destinationQuery)
      return this.airports.filter(a =>
        a.code.toLowerCase().startsWith(query) ||
        normalize(a.city).includes(query) ||
        normalize(a.country).includes(query)
      ).slice(0, 8)
    },

    filteredDirectFlights() {
      const isBusiness = this.filterPriceClass === 'business'
      return this.directFlightResults
        .filter(f => {
          const price = isBusiness ? f.priceFirstClass : f.priceEconomy
          if (price > this.filterPriceMax) return false
          if (f.durationHours > this.filterDurationMax) return false
          return true
        })
        .sort((a, b) => {
          const priceA = isBusiness ? a.priceFirstClass : a.priceEconomy
          const priceB = isBusiness ? b.priceFirstClass : b.priceEconomy
          return priceA - priceB
        })
    },

    filteredStopoverResults() {
      const isBusiness = this.filterPriceClass === 'business'
      return this.stopoverResults
        .filter(conn => {
          const combinedPrice = isBusiness
            ? conn.leg1.priceFirstClass + conn.leg2.priceFirstClass
            : conn.leg1.priceEconomy + conn.leg2.priceEconomy
          const combinedDuration = conn.leg1.durationHours + conn.leg2.durationHours
          if (combinedPrice > this.filterPriceMax) return false
          if (combinedDuration > this.filterDurationMax) return false
          return true
        })
        .sort((a, b) => {
          const priceA = isBusiness
            ? a.leg1.priceFirstClass + a.leg2.priceFirstClass
            : a.leg1.priceEconomy + a.leg2.priceEconomy
          const priceB = isBusiness
            ? b.leg1.priceFirstClass + b.leg2.priceFirstClass
            : b.leg1.priceEconomy + b.leg2.priceEconomy
          return priceA - priceB
        })
    },

    economyCount() {
      return this.passengerCount - this.firstClassCount
    },

    modalTotal() {
      if (!this.selectedFlight) return 0
      const f = this.selectedFlight
      const tickets = (this.firstClassCount * f.priceFirstClass) + (this.economyCount * f.priceEconomy)
      const handBags = this.handBagsCount * f.handBagPrice
      const checkedBags = this.checkedBagsCount * f.bagPrice * f.bagMultiplier
      return tickets + handBags + checkedBags
    },
  },

  methods: {
    onOriginInput() {
      this.selectedOrigin = null
      this.showOriginDropdown = true
    },
    onOriginBlur() {
      setTimeout(() => { this.showOriginDropdown = false }, 200)
    },
    selectOrigin(airport) {
      this.selectedOrigin = airport
      this.originQuery = `${airport.code} - ${airport.city}, ${airport.country}`
      this.showOriginDropdown = false
    },

    onDestinationInput() {
      this.selectedDestination = null
      this.showDestinationDropdown = true
    },
    onDestinationBlur() {
      setTimeout(() => { this.showDestinationDropdown = false }, 200)
    },
    selectDestination(airport) {
      this.selectedDestination = airport
      this.destinationQuery = `${airport.code} - ${airport.city}, ${airport.country}`
      this.showDestinationDropdown = false
    },

    searchFlights() {
      if (!this.selectedOrigin || !this.selectedDestination) {
        this.errorMsg = 'Por favor selecciona origen y destino de la lista.'
        return
      }
      if (this.selectedOrigin.code === this.selectedDestination.code) {
        this.errorMsg = 'El origen y destino no pueden ser iguales.'
        return
      }

      this.errorMsg = ''
      const origin = this.selectedOrigin.code
      const destination = this.selectedDestination.code

      // Direct flights —————————————————————————————————————
      this.directFlightResults = this.flights
        .filter(f => f.origin === origin && f.destination === destination)
        .filter(f => flightOperatesOnDate(f, this.departureDate))
        .map(f => ({
          ...f,
          date: this.departureDate,
          arrivalDate: isOvernightFlight(f)
            ? addDaysToDateString(this.departureDate, 1)
            : this.departureDate,
        }))

      // Stopover connections ————————————————————————————————
      const rawConnections = findStopoverConnections(this.flights, origin, destination, this.departureDate)

      this.stopoverResults = rawConnections.map(conn => {
        const leg1IsOvernight = isOvernightFlight(conn.leg1)
        const leg1ArrivalDate = leg1IsOvernight
          ? addDaysToDateString(this.departureDate, 1)
          : this.departureDate

        const leg2IsOvernight = isOvernightFlight(conn.leg2)
        const leg2ArrivalDate = leg2IsOvernight
          ? addDaysToDateString(leg1ArrivalDate, 1)
          : leg1ArrivalDate

        return {
          leg1: { ...conn.leg1, date: this.departureDate, arrivalDate: leg1ArrivalDate },
          leg2: { ...conn.leg2, date: leg1ArrivalDate, arrivalDate: leg2ArrivalDate },
          layoverMinutes: conn.layoverMinutes,
          connectionCity: conn.connectionCity,
          connectionAirportCode: conn.connectionAirportCode,
        }
      })

      // Compute per-class price maxes across both result sets.
      const allEconomyPrices = [
        ...this.directFlightResults.map(f => f.priceEconomy),
        ...this.stopoverResults.map(c => c.leg1.priceEconomy + c.leg2.priceEconomy),
      ]
      const allBusinessPrices = [
        ...this.directFlightResults.map(f => f.priceFirstClass),
        ...this.stopoverResults.map(c => c.leg1.priceFirstClass + c.leg2.priceFirstClass),
      ]
      const allDurations = [
        ...this.directFlightResults.map(f => f.durationHours),
        ...this.stopoverResults.map(c => c.leg1.durationHours + c.leg2.durationHours),
      ]

      this.maxEconomyPrice = allEconomyPrices.length > 0 ? Math.max(...allEconomyPrices) : 100000
      this.maxBusinessPrice = allBusinessPrices.length > 0 ? Math.max(...allBusinessPrices) : 200000

      const activeMax = this.filterPriceClass === 'business' ? this.maxBusinessPrice : this.maxEconomyPrice
      const maxDuration = allDurations.length > 0 ? Math.max(...allDurations) : 24

      this.priceSliderRange = activeMax + 5000
      this.filterPriceMax = activeMax + 5000
      this.filterPriceMin = 0
      this.filterDurationMax = Math.ceil(maxDuration) + 1

      this.hasSearched = true

      this.$nextTick(() => {
        const el = document.querySelector('.results-section')
        if (el) el.scrollIntoView({ behavior: 'smooth' })
      })
    },

    openFlightDetails(flight) {
      this.selectedFlight = flight
      this.firstClassCount = 0
      this.handBagsCount = 0
      this.checkedBagsCount = 0
      document.body.style.overflow = 'hidden'
    },

    closeFlightDetails() {
      this.selectedFlight = null
      document.body.style.overflow = ''
    },

    handleStopoverSelect(connection) {
      // Stopover booking flow — available in a future update.
      console.log('Itinerario con escala seleccionado:', connection)
    },

    setFilterPriceClass(className) {
      this.filterPriceClass = className
      const max = className === 'business' ? this.maxBusinessPrice : this.maxEconomyPrice
      this.priceSliderRange = max + 5000
      this.filterPriceMax = max + 5000
    },

    getAirportCity(code) {
      const airport = this.airports.find(a => a.code === code)
      return airport ? airport.city : code
    },

    getAirportCountry(code) {
      const airport = this.airports.find(a => a.code === code)
      return airport ? airport.country : ''
    },
  },
}
</script>

<style scoped>
.navbar {
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.brand-name {
  font-weight: 700;
  font-size: 1.05rem;
  color: #1a1a1a;
  line-height: 1.2;
}

.brand-tagline {
  font-size: 0.7rem;
  color: #888;
}

.nav-link-item {
  text-decoration: none;
  color: #333;
  font-size: 0.92rem;
  display: flex;
  align-items: center;
}
.nav-link-item:hover { color: var(--color-primary); }

.admin-btn {
  font-size: 0.88rem;
  border-color: var(--color-primary);
  color: var(--color-primary);
}
.admin-btn:hover {
  background: #e74c3c;
  color: white;
}

/* ─── Hero ────────────────────────────────────────────────── */
.hero-section {
  background: linear-gradient(135deg, #c0392b 0%, #e74c3c 35%, #e67e22 70%, #f0a500 100%);
  min-height: 460px;
  display: flex;
  align-items: center;
  padding: 56px 24px 72px;
  position: relative;
  overflow: hidden;
}

.hero-plane-bg {
  position: absolute;
  right: 8%;
  top: 50%;
  transform: translateY(-60%) rotate(-15deg);
  font-size: 220px;
  opacity: 0.12;
  color: white;
  pointer-events: none;
  user-select: none;
}

.hero-content {
  max-width: 940px;
  margin: 0 auto;
  width: 100%;
  position: relative;
  z-index: 1;
}

.hero-title {
  color: white;
  font-size: 2.8rem;
  font-weight: 800;
  margin-bottom: 10px;
}

.hero-subtitle {
  color: rgba(255, 255, 255, 0.9);
  font-size: 1.05rem;
  margin-bottom: 28px;
}

/* ─── Search card ─────────────────────────────────────────── */
.search-card {
  background: white;
  border-radius: 16px;
  padding: 28px 32px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.18);
}

.search-fields {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr auto auto;
  gap: 16px;
  margin-bottom: 20px;
  align-items: end;
}

.field-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: #333;
  margin-bottom: 6px;
}

.input-box {
  display: flex;
  align-items: center;
  border: 1.5px solid #e0e0e0;
  border-radius: 8px;
  padding: 10px 12px;
  gap: 8px;
  background: white;
  transition: border-color 0.2s;
}
.input-box.focused,
.input-box:focus-within {
  border-color: var(--color-primary);
}

.field-icon {
  color: #bbb;
  font-size: 0.95rem;
  flex-shrink: 0;
}

.input-box input[type="text"] {
  border: none;
  outline: none;
  width: 100%;
  font-size: 0.88rem;
  color: #333;
}
.input-box input[type="text"]::placeholder { color: #bbb; }

.input-box input[type="date"] {
  border: none;
  outline: none;
  width: 100%;
  font-size: 0.88rem;
  color: #999;
  cursor: pointer;
}

/* ─── Passenger stepper ───────────────────────────────────── */
.passenger-selector {
  justify-content: space-between;
  padding: 8px 12px;
}

.passenger-btn {
  background: none;
  border: 1.5px solid #e0e0e0;
  border-radius: 6px;
  width: 28px;
  height: 28px;
  font-size: 1.1rem;
  line-height: 1;
  cursor: pointer;
  color: var(--color-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: background 0.15s, border-color 0.15s;
}

.passenger-btn:hover {
  background: #fff0ee;
  border-color: var(--color-primary);
}

.passenger-count {
  font-size: 1rem;
  font-weight: 400;
  color: #1a1a1a;
  min-width: 20px;
  text-align: center;
}

/* ─── Flight type toggle ──────────────────────────────────── */
.toggle-field-group {
  display: flex;
  flex-direction: column;
}

.flight-type-toggle {
  display: flex;
  background: #f3f4f6;
  border-radius: var(--radius-pill);
  padding: 4px;
  gap: 2px;
}

.toggle-option {
  padding: 9px 14px;
  border-radius: var(--radius-pill);
  border: none;
  cursor: pointer;
  font-size: 0.82rem;
  font-weight: 600;
  color: #666;
  background: transparent;
  display: flex;
  align-items: center;
  white-space: nowrap;
  transition: background 0.18s ease, color 0.18s ease, box-shadow 0.18s ease;
}

.toggle-option.active {
  background: white;
  color: var(--color-primary);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
}

/* ─── Autocomplete ────────────────────────────────────────── */
.autocomplete-wrapper {
  position: relative;
}

.autocomplete-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  min-width: 300px;
  right: 0;
  background: white;
  border: 1.5px solid #ddd;
  border-top: none;
  border-radius: 0 0 10px 10px;
  box-shadow: 0 8px 28px rgba(0, 0, 0, 0.15);
  z-index: 200;
  max-height: 400px;
  overflow-y: auto;
}

.autocomplete-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 14px 18px;
  cursor: pointer;
  transition: background 0.15s;
  border-bottom: 1px solid #f0f0f0;
}
.autocomplete-item:last-child { border-bottom: none; }
.autocomplete-item:hover { background: #fff5f5; }

.airport-code {
  font-weight: 700;
  font-size: 15px;
  color: var(--color-primary);
  min-width: 48px;
  flex-shrink: 0;
  letter-spacing: 0.5px;
}

.airport-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.airport-city {
  font-size: 15px;
  font-weight: 600;
  color: #1a1a1a;
  line-height: 1.3;
}

.airport-country {
  font-size: 13px;
  color: #888;
  line-height: 1.3;
}

/* ─── Misc form elements ──────────────────────────────────── */
.error-msg {
  color: var(--color-primary);
  font-size: 0.83rem;
  margin-bottom: 12px;
  margin-top: -8px;
}

.search-btn {
  width: 100%;
  padding: 14px;
  background: var(--gradient-brand);
  color: white;
  font-size: 1rem;
  font-weight: 600;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.2s;
}
.search-btn:hover { opacity: 0.9; }

/* ─── Results section ─────────────────────────────────────── */
.results-section {
  background: #f4f6f9;
  min-height: 400px;
  padding: 36px 24px;
}

.results-layout {
  max-width: 1100px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 24px;
  align-items: start;
}


.filters-panel {
  background: white;
  border-radius: 12px;
  padding: 20px 22px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.07);
}

.filters-title {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 20px;
}

.filter-group {
  margin-bottom: 22px;
}

.filter-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
  font-size: 0.88rem;
  font-weight: 600;
  color: #333;
}

.filter-value {
  font-weight: 400;
  color: var(--color-primary);
  font-size: 0.82rem;
}

/* ─── Price class toggle inside filters panel ─────────────── */
.price-class-toggle {
  display: flex;
  background: #f3f4f6;
  border-radius: var(--radius-pill);
  padding: 3px;
  gap: 2px;
  margin-bottom: 10px;
}

.price-class-btn {
  flex: 1;
  padding: 6px 10px;
  border-radius: var(--radius-pill);
  border: none;
  cursor: pointer;
  font-size: 0.78rem;
  font-weight: 600;
  color: #666;
  background: transparent;
  transition: background 0.18s ease, color 0.18s ease, box-shadow 0.18s ease;
}

.price-class-btn.active {
  background: white;
  color: var(--color-primary);
  box-shadow: 0 1px 6px rgba(0, 0, 0, 0.12);
}

.range-slider {
  width: 100%;
  accent-color: var(--color-primary);
  cursor: pointer;
}

.flights-panel {
  display: flex;
  flex-direction: column;
}

.results-count {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 16px;
}

.no-results {
  text-align: center;
  padding: 48px 24px;
  color: #aaa;
  background: white;
  border-radius: 12px;
}
.no-results i {
  font-size: 2.5rem;
  display: block;
  margin-bottom: 12px;
}

/* ─── Promo section ───────────────────────────────────────── */
.adventure-section {
  text-align: center;
  padding: 64px 24px;
  background: #f8f9fa;
}

.adventure-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 10px;
}

.adventure-subtitle {
  color: #888;
  font-size: 0.95rem;
}

/* ─── Booking modal ───────────────────────────────────────── */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.55);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 16px;
}

.flight-modal {
  background: white;
  border-radius: 16px;
  padding: 28px 32px;
  max-width: 580px;
  width: 100%;
  position: relative;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.25);
  max-height: 90vh;
  overflow-y: auto;
}

.modal-close-btn {
  position: absolute;
  top: 16px;
  right: 16px;
  background: #f4f6f9;
  border: none;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #666;
  font-size: 0.85rem;
  transition: background 0.15s, color 0.15s;
}
.modal-close-btn:hover { background: #ffe5e2; color: var(--color-primary); }

.modal-airline-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 22px;
}

.modal-airline-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.airline-icon {
  width: 32px;
  height: 32px;
  background: #fff0ee;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
  font-size: 0.9rem;
}

.modal-airline-name {
  font-size: 0.95rem;
  font-weight: 700;
  color: #1a1a1a;
}

.modal-flight-id {
  font-size: 0.75rem;
  color: #aaa;
  margin-top: 2px;
}

.modal-route-section {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  background: #f8f9fa;
  border-radius: 12px;
  padding: 20px 24px;
  margin-bottom: 20px;
}

.modal-endpoint { min-width: 80px; }
.modal-endpoint.right { text-align: right; }

.modal-time {
  font-size: 2rem;
  font-weight: 800;
  color: #1a1a1a;
  line-height: 1;
}

.modal-city {
  font-size: 0.85rem;
  font-weight: 600;
  color: #333;
  margin-top: 4px;
}

.modal-iata {
  font-size: 0.8rem;
  color: var(--color-primary);
  font-weight: 700;
  margin-top: 2px;
}

.modal-date {
  font-size: 0.75rem;
  color: #aaa;
  margin-top: 2px;
}

.modal-route-middle {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding-top: 8px;
}

.modal-duration-label {
  font-size: 0.78rem;
  color: #888;
  font-weight: 500;
}

.modal-route-line {
  width: 100%;
  height: 2px;
  background: #e0e0e0;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-route-dot {
  position: absolute;
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #ccc;
  top: 50%;
  transform: translateY(-50%);
}
.modal-route-dot.left  { left: 0; }
.modal-route-dot.right { right: 0; }

.modal-route-plane {
  background: white;
  padding: 0 6px;
  color: var(--color-primary);
  font-size: 0.9rem;
  z-index: 1;
  position: relative;
}

.modal-info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px 24px;
  margin-bottom: 20px;
}

.modal-info-item {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.modal-info-label {
  font-size: 0.75rem;
  color: #aaa;
  font-weight: 500;
}

.modal-info-value {
  font-size: 0.9rem;
  font-weight: 600;
  color: #1a1a1a;
}

.modal-section-block {
  border: 1px solid #f0f0f0;
  border-radius: 10px;
  padding: 14px 16px;
  margin-bottom: 12px;
}

.modal-section-title {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.07em;
  color: #aaa;
  margin-bottom: 10px;
}

.modal-class-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px dashed #f0f0f0;
}
.modal-class-row:last-of-type { border-bottom: none; }

.modal-class-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.modal-class-name {
  font-size: 0.9rem;
  font-weight: 600;
  color: #1a1a1a;
}

.modal-class-price {
  font-size: 0.76rem;
  color: #888;
}

.modal-class-warning {
  font-size: 0.75rem;
  color: var(--color-primary);
  margin: 6px 0 0;
}

.modal-qty-ctrl {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #f8f9fa;
  border-radius: 999px;
  padding: 4px 12px;
}

.modal-qty-btn {
  background: none;
  border: none;
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--color-primary);
  cursor: pointer;
  padding: 0;
  line-height: 1;
}
.modal-qty-btn:hover { color: #c0392b; }

.modal-qty-val {
  font-size: 0.95rem;
  font-weight: 700;
  min-width: 18px;
  text-align: center;
  color: #1a1a1a;
}
.modal-qty-val.readonly { color: #555; }

.modal-receipt {
  background: #fff8f7;
  border: 1px solid #fde8e5;
  border-radius: 10px;
  padding: 14px 16px;
  margin-bottom: 16px;
}

.receipt-line {
  display: flex;
  justify-content: space-between;
  font-size: 0.85rem;
  color: #555;
  padding: 5px 0;
  border-bottom: 1px dashed #fde8e5;
}

.receipt-total {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 10px;
  padding-top: 10px;
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
}

.receipt-total span:last-child {
  font-size: 1.25rem;
  background: var(--gradient-brand);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.modal-actions {
  display: flex;
  gap: 12px;
}

.modal-cancel-btn {
  flex: 1;
  padding: 12px;
  background: white;
  color: #555;
  border: 1.5px solid #e0e0e0;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: background 0.15s;
}
.modal-cancel-btn:hover { background: #f4f6f9; }

.modal-cart-btn {
  flex: 2;
  padding: 12px;
  background: var(--gradient-brand);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.2s;
}
.modal-cart-btn:hover { opacity: 0.88; }
.modal-cart-btn:disabled { opacity: 0.4; cursor: not-allowed; }

/* ─── Modal transition ────────────────────────────────────── */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.22s ease;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}
.modal-fade-enter-active .flight-modal,
.modal-fade-leave-active .flight-modal {
  transition: transform 0.22s ease, opacity 0.22s ease;
}
.modal-fade-enter-from .flight-modal,
.modal-fade-leave-to .flight-modal {
  transform: scale(0.95) translateY(12px);
  opacity: 0;
}
</style>
