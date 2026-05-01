<template>
  <div>

    <!--
      NAVBAR
      Contains the logo with airline name and tagline, navigation links, and an admin login button.
    -->
    <nav class="navbar bg-white shadow-sm px-4 py-2">

      <!-- Logo: image + airline name + tagline -->
      <a class="navbar-brand d-flex align-items-center gap-2" href="#">
        <img src="@/assets/logo.png" alt="Logo" width="42" height="42" class="rounded-2" />
        <div>
          <div class="brand-name">Mushu Airlines</div>
          <div class="brand-tagline">Vuela con el dragón</div>
        </div>
      </a>

      <!-- Navigation links + Admin button -->
      <div class="d-flex align-items-center gap-4">
        <a href="#" class="nav-link-item">
          <i class="bi bi-briefcase me-1"></i>Mis vuelos
        </a>
        <a href="#" class="nav-link-item">
          <i class="bi bi-calendar-check me-1"></i>Check-in
        </a>
        <!-- Button that redirects to the admin login page -->
        <a href="/login" class="btn btn-outline-danger rounded-pill px-3 py-1 admin-btn">
          <i class="bi bi-person me-1"></i>Admin Login
        </a>
      </div>
    </nav>

    <!--
      HERO SECTION
      Main section with a red-to-orange gradient background.
      Contains the headline, subtitle, and the flight search card.
    -->
    <div class="hero-section">

      <!-- Decorative airplane emoji in the background, purely visual -->
      <div class="hero-plane-bg">✈</div>

      <!-- Centered content wrapper -->
      <div class="hero-content">
        <h1 class="hero-title">Encuentra tu vuelo perfecto</h1>
        <p class="hero-subtitle">Viaja con Mushu Airlines a más de 100 destinos alrededor del mundo</p>

        <!--
          SEARCH CARD
          White card with shadow containing all search controls.
          Includes origin/destination autocomplete fields, date pickers, and the search button.
        -->
        <div class="search-card">

          <!--
            SEARCH FIELDS
            4-column grid: Origin, Destination, Departure Date, Return Date.
            Each airport field has live autocomplete powered by the airports CSV.
          -->
          <div class="search-fields">

            <!--
              PASSENGERS FIELD
              Counter with +/- buttons to select the number of passengers.
            -->
            <div class="field-group">
              <label class="field-label"><i class="bi bi-people me-1"></i>Pasajeros</label>
              <div class="input-box passenger-selector">
                <button class="passenger-btn" type="button" @click="passengerCount = Math.max(1, passengerCount - 1)">−</button>
                <span class="passenger-count">{{ passengerCount }}</span>
                <button class="passenger-btn" type="button" @click="passengerCount = Math.min(9, passengerCount + 1)">+</button>
              </div>
            </div>

            <!--
              ORIGIN FIELD
              Text input with autocomplete dropdown.
              Filters airports by IATA code, city, or country as the user types.
              Selecting a suggestion stores the full airport object in selectedOrigin.
            -->
            <div class="field-group autocomplete-wrapper">
              <label class="field-label">Origen</label>
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
              <!-- Dropdown list of matching origin airports -->
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

            <!--
              DESTINATION FIELD
              Same as the origin field but stores the selection in selectedDestination.
            -->
            <div class="field-group autocomplete-wrapper">
              <label class="field-label">Destino</label>
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
              <!-- Dropdown list of matching destination airports -->
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

            <!-- DEPARTURE DATE: always visible -->
            <div class="field-group">
              <label class="field-label">Fecha de ida</label>
              <div class="input-box">
                <input type="date" v-model="departureDate" />
              </div>
            </div>

            <!-- RETURN DATE: always visible for round-trip -->
            <div class="field-group">
              <label class="field-label">Fecha de regreso</label>
              <div class="input-box">
                <input type="date" v-model="returnDate" />
              </div>
            </div>

          </div>

          <!-- Error message shown when the user submits without valid origin/destination -->
          <p v-if="errorMsg" class="error-msg">{{ errorMsg }}</p>

          <!-- Search button: triggers searchFlights() on click -->
          <button class="search-btn" @click="searchFlights">
            <i class="bi bi-search me-2"></i>Buscar vuelos
          </button>

        </div>
      </div>
    </div>

    <!--
      RESULTS SECTION
      Only visible after a search has been performed (hasSearched === true).
      Contains the filters sidebar on the left and the flight cards on the right.
    -->
    <div class="results-section" v-if="hasSearched">
      <div class="results-layout">

        <!--
          FILTERS PANEL
          Left sidebar with three interactive filters: price, duration, and stops.
          Filters are reactive — moving any slider instantly updates filteredFlights.
        -->
        <aside class="filters-panel">
          <h3 class="filters-title">Filtros</h3>

          <!-- Price filter: range slider that sets the maximum price -->
          <div class="filter-group">
            <div class="filter-header">
              <span>Precio</span>
              <span class="filter-value">${{ filterPriceMin }} - ${{ filterPriceMax }}</span>
            </div>
            <input
              type="range"
              v-model.number="filterPriceMax"
              :min="filterPriceMin"
              :max="priceSliderRange"
              step="5"
              class="range-slider"
            />
          </div>

          <!-- Duration filter: range slider that sets the maximum flight hours -->
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

          <!--
            Stops filter: radio buttons to show all flights,
            direct only, or flights with stops only.
          -->
          <div class="filter-group">
            <div class="filter-header"><span>Escalas</span></div>
            <div class="stops-options">
              <label class="stop-option" :class="{ active: filterStops === 'all' }">
                <input type="radio" v-model="filterStops" value="all" />
                Todos los vuelos
              </label>
              <label class="stop-option" :class="{ active: filterStops === 'direct' }">
                <input type="radio" v-model="filterStops" value="direct" />
                Solo vuelos directos
              </label>
              <label class="stop-option" :class="{ active: filterStops === 'stops' }">
                <input type="radio" v-model="filterStops" value="stops" />
                Con escalas
              </label>
            </div>
          </div>
        </aside>

        <!--
          FLIGHTS PANEL
          Right column listing all flight cards that match the search and active filters.
          Shows an empty state message when no flights pass the filters.
        -->
        <div class="flights-panel">

          <!-- Counter showing how many flights pass the active filters -->
          <p class="results-count">{{ filteredFlights.length }} vuelos disponibles</p>

          <!-- Empty state when no flights match the current filters -->
          <div v-if="filteredFlights.length === 0" class="no-results">
            <i class="bi bi-airplane"></i>
            <p>No hay vuelos disponibles para esta búsqueda.</p>
          </div>

          <!--
            FLIGHT CARD
            Repeated for every flight in filteredFlights.
            Shows airline, price, departure/arrival times, duration, stops badge, and select button.
          -->
          <div v-for="flight in filteredFlights" :key="flight.id" class="flight-card">

            <!-- Card header: airline name on the left, price on the right -->
            <div class="flight-card-header">
              <div class="airline-info">
                <div class="airline-icon">
                  <i class="bi bi-airplane-fill"></i>
                </div>
                <span class="airline-name">Mushu Airlines</span>
              </div>
              <div class="price-section">
                <span class="price-label">Precio por persona</span>
                <span class="price-amount">${{ flight.price }}</span>
                <div class="price-total-row">
                  <span class="price-total-label">
                    Total · {{ passengerCount }} {{ passengerCount === 1 ? 'pasajero' : 'pasajeros' }}
                  </span>
                  <span class="price-total-amount">${{ flight.price * passengerCount }}</span>
                </div>
              </div>
            </div>

            <!--
              Flight row: departure time/code → center line with duration and stops badge → arrival time/code.
              The badge color changes depending on whether the flight is direct or has stops.
            -->
            <div class="flight-row">
              <div class="flight-time-info">
                <div class="time">{{ flight.departureTime }}</div>
                <div class="airport-code-small">
                  <i class="bi bi-geo-alt-fill" style="font-size:0.7rem;margin-right:2px;"></i>{{ flight.origin }}
                </div>
              </div>

              <div class="flight-middle">
                <div class="duration-badge-container">
                  <span class="duration-text">{{ flight.durationLabel }}</span>
                  <!-- Yellow badge for direct flights, red badge for flights with stops -->
                  <div class="stops-badge" :class="flight.stops === 0 ? 'directo' : 'con-escala'">
                    <i class="bi bi-airplane-fill" style="font-size:0.65rem;"></i>
                    {{ flight.stops === 0 ? 'Directo' : flight.stops + ' escala' }}
                  </div>
                </div>
                <!-- Decorative horizontal line between origin and destination -->
                <div class="flight-line"></div>
              </div>

              <div class="flight-time-info text-end">
                <div class="time">{{ flight.arrivalTime }}</div>
                <div class="airport-code-small">
                  <i class="bi bi-geo-alt-fill" style="font-size:0.7rem;margin-right:2px;"></i>{{ flight.destination }}
                </div>
              </div>

              <!-- Select button for this flight (functionality pending) -->
              <div class="select-section">
                <button class="select-btn">Seleccionar</button>
              </div>
            </div>

            <!-- Card footer: total flight duration -->
            <div class="flight-footer">
              <i class="bi bi-clock" style="font-size:0.78rem;margin-right:4px;color:#888;"></i>
              <span class="duration-footer">Duración total: {{ flight.durationLabel }}</span>
            </div>

          </div>
        </div>

      </div>
    </div>

    <!--
      ADVENTURE SECTION
      Motivational section shown only when the user has not searched yet.
      Disappears as soon as the first search is performed.
    -->
    <div v-if="!hasSearched" class="adventure-section">
      <h2 class="adventure-title">¿Listo para tu próxima aventura?</h2>
      <p class="adventure-subtitle">Comienza tu búsqueda arriba y descubre nuestras mejores ofertas</p>
    </div>

  </div>
</template>

<script>
// Converts a duration string "HH:MM" to a decimal number of hours (e.g. "1:30" → 1.5)
function durationToHours(dur) {
  const [h, m] = dur.split(':')
  return parseInt(h) + parseInt(m) / 60
}

// Converts "HH:MM" to a human-readable label (e.g. "1:30" → "1h 30min", "2:00" → "2h")
function durationToLabel(dur) {
  const [h, m] = dur.split(':')
  const hours = parseInt(h), minutes = parseInt(m)
  return minutes === 0 ? `${hours}h` : `${hours}h ${minutes}min`
}

// Calculates a flight price based on its id and duration in hours
// Uses a simple formula to generate varied prices from CSV data
function calcPrice(id, hours) {
  return Math.round(40 + hours * 25 + (id % 5) * 10)
}

// Parses the airports CSV and returns an array of { code, city, country } objects
function parseAirportsCsv(text) {
  return text.trim().split('\n').slice(1).filter(l => l.trim()).map(line => {
    const parts = line.split(',')
    return { code: parts[0].trim(), city: parts[1].trim(), country: parts[2].trim() }
  })
}

// Parses the flights CSV and returns an array of flight objects with all fields needed by the UI
function parseFlightsCsv(text) {
  return text.trim().split('\n').slice(1).filter(l => l.trim()).map(line => {
    const parts = line.split(',')
    const id = parseInt(parts[0])
    const duration = parts[4].trim()
    const hours = durationToHours(duration)
    return {
      id,
      origin: parts[2].trim(),
      destination: parts[3].trim(),
      duration,
      departureTime: parts[5].trim(),
      arrivalTime: parts[6].trim(),
      date: parts[7].trim(),
      durationHours: hours,
      durationLabel: durationToLabel(duration),
      // Flights over 3.5 hours are assumed to have 1 stop (temporary logic based on CSV data)
      stops: hours > 3.5 ? 1 : 0,
      price: calcPrice(id, hours),
    }
  })
}

export default {
  name: 'LandingPage',

  data() {
    return {
      // Data loaded from CSV files when the component is created
      airports: [],
      flights: [],

      // Dates selected by the user
      departureDate: '',
      returnDate: '',

      // Origin field state: typed text, selected airport object, and dropdown visibility
      originQuery: '',
      selectedOrigin: null,
      showOriginDropdown: false,

      // Destination field state: typed text, selected airport object, and dropdown visibility
      destinationQuery: '',
      selectedDestination: null,
      showDestinationDropdown: false,

      // Controls whether the results section is visible
      hasSearched: false,

      // Flights matching the searched route, before any filters are applied
      searchedFlights: [],

      // Filter panel state
      filterPriceMin: 0,
      filterPriceMax: 500,
      priceSliderRange: 500,   // Dynamic slider maximum adjusted to the search results
      filterDurationMax: 24,
      filterStops: 'all',      // 'all' | 'direct' | 'stops'

      // Number of passengers selected (1–9)
      passengerCount: 1,

      // Error message displayed below the search fields
      errorMsg: '',
    }
  },

  // Both CSV files are fetched in parallel when the component is created
  async created() {
    const [airportRes, flightRes] = await Promise.all([
      fetch('/data/Airports.csv'),
      fetch('/data/FlightSchedule.csv'),
    ])
    this.airports = parseAirportsCsv(await airportRes.text())
    this.flights = parseFlightsCsv(await flightRes.text())
  },

  computed: {
    // Filters airports for the origin autocomplete based on what the user types.
    // Matches against IATA code, city name, and country, ignoring accents and case.
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

    // Same as originSuggestions but for the destination field
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

    // Applies all three active filters (price, duration, stops) to the searched flights
    filteredFlights() {
      return this.searchedFlights.filter(f => {
        if (f.price > this.filterPriceMax) return false
        if (f.durationHours > this.filterDurationMax) return false
        if (this.filterStops === 'direct' && f.stops > 0) return false
        if (this.filterStops === 'stops' && f.stops === 0) return false
        return true
      })
    },
  },

  methods: {
    // Clears the previous selection and opens the dropdown when the user types in the origin field
    onOriginInput() {
      this.selectedOrigin = null
      this.showOriginDropdown = true
    },
    // Closes the dropdown with a small delay to allow the user to click a suggestion before it hides
    onOriginBlur() {
      setTimeout(() => { this.showOriginDropdown = false }, 200)
    },
    // Stores the selected airport and fills the input with its display name
    selectOrigin(airport) {
      this.selectedOrigin = airport
      this.originQuery = `${airport.code} - ${airport.city}, ${airport.country}`
      this.showOriginDropdown = false
    },

    // Same three methods as origin but for the destination field
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

    // Runs when the search button is clicked
    searchFlights() {
      // Validation: both fields must have an airport selected from the dropdown
      if (!this.selectedOrigin || !this.selectedDestination) {
        this.errorMsg = 'Por favor selecciona origen y destino de la lista.'
        return
      }
      // Validation: origin and destination cannot be the same airport
      if (this.selectedOrigin.code === this.selectedDestination.code) {
        this.errorMsg = 'El origen y destino no pueden ser iguales.'
        return
      }

      this.errorMsg = ''
      const origin = this.selectedOrigin.code
      const destination = this.selectedDestination.code

      // Filter all CSV flights that match the searched route
      this.searchedFlights = this.flights.filter(f => f.origin === origin && f.destination === destination)

      // Dynamically adjust filter slider maximums to match the results
      const maxPrice = this.searchedFlights.length > 0
        ? Math.max(...this.searchedFlights.map(f => f.price))
        : 500
      const maxDuration = this.searchedFlights.length > 0
        ? Math.max(...this.searchedFlights.map(f => f.durationHours))
        : 24

      this.priceSliderRange = maxPrice + 50
      this.filterPriceMax = maxPrice + 50
      this.filterPriceMin = 0
      this.filterDurationMax = Math.ceil(maxDuration) + 1
      this.filterStops = 'all'
      this.hasSearched = true

      // Smooth-scroll to the results section after Vue updates the DOM
      this.$nextTick(() => {
        const el = document.querySelector('.results-section')
        if (el) el.scrollIntoView({ behavior: 'smooth' })
      })
    },
  },
}
</script>

<style scoped>
/* =====================
   NAVBAR
   Sticky top bar with a high z-index to stay above all other content
   ===================== */
.navbar {
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

/* Airline name in bold */
.brand-name {
  font-weight: 700;
  font-size: 1.05rem;
  color: #1a1a1a;
  line-height: 1.2;
}

/* Small grey tagline below the brand name */
.brand-tagline {
  font-size: 0.7rem;
  color: #888;
}

/* Navbar links without underline; turn red on hover */
.nav-link-item {
  text-decoration: none;
  color: #333;
  font-size: 0.92rem;
  display: flex;
  align-items: center;
}
.nav-link-item:hover { color: #e74c3c; }

/* Admin button: red border on white background; inverts on hover */
.admin-btn {
  font-size: 0.88rem;
  border-color: #e74c3c;
  color: #e74c3c;
}
.admin-btn:hover {
  background: #e74c3c;
  color: white;
}

/* =====================
   HERO SECTION
   Red-to-orange gradient background that holds the main search card
   ===================== */
.hero-section {
  background: linear-gradient(135deg, #c0392b 0%, #e74c3c 35%, #e67e22 70%, #f0a500 100%);
  min-height: 460px;
  display: flex;
  align-items: center;
  padding: 56px 24px 72px;
  position: relative;
  overflow: hidden;
}

/* Large semi-transparent decorative airplane in the background */
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

/* Centered content wrapper with a maximum width */
.hero-content {
  max-width: 940px;
  margin: 0 auto;
  width: 100%;
  position: relative;
  z-index: 1;
}

/* Large white bold headline */
.hero-title {
  color: white;
  font-size: 2.8rem;
  font-weight: 800;
  margin-bottom: 10px;
}

/* Slightly transparent subtitle below the headline */
.hero-subtitle {
  color: rgba(255, 255, 255, 0.9);
  font-size: 1.05rem;
  margin-bottom: 28px;
}

/* =====================
   SEARCH CARD
   White rounded card with shadow containing all search controls
   ===================== */
.search-card {
  background: white;
  border-radius: 16px;
  padding: 28px 32px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.18);
}

/* 5-column grid for the search fields */
.search-fields {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 16px;
  margin-bottom: 20px;
}

/* Small bold label above each field */
.field-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: #333;
  margin-bottom: 6px;
}

/* Input wrapper with rounded border; border turns red when focused */
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
  border-color: #e74c3c;
}

/* Location pin icon inside the input */
.field-icon {
  color: #bbb;
  font-size: 0.95rem;
  flex-shrink: 0;
}

/* Text input without its own border (border is handled by .input-box) */
.input-box input[type="text"] {
  border: none;
  outline: none;
  width: 100%;
  font-size: 0.88rem;
  color: #333;
}
.input-box input[type="text"]::placeholder { color: #bbb; }

/* Date input without its own border */
.input-box input[type="date"] {
  border: none;
  outline: none;
  width: 100%;
  font-size: 0.88rem;
  color: #999;
  cursor: pointer;
}

/* =====================
   AUTOCOMPLETE
   Dropdown that appears below the airport input with filtered suggestions
   ===================== */
.autocomplete-wrapper {
  position: relative;
}

/* Absolutely-positioned suggestion list below the input */
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
  box-shadow: 0 8px 28px rgba(0,0,0,0.15);
  z-index: 200;
  max-height: 400px;
  overflow-y: auto;
}

/* Each suggestion row */
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

/* IATA code badge */
.airport-code {
  font-weight: 700;
  font-size: 15px;
  color: #e74c3c;
  min-width: 48px;
  flex-shrink: 0;
  letter-spacing: 0.5px;
}

/* Wrapper for city + country stacked vertically */
.airport-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

/* City name — primary text */
.airport-city {
  font-size: 15px;
  font-weight: 600;
  color: #1a1a1a;
  line-height: 1.3;
}

/* Country name — secondary text */
.airport-country {
  font-size: 13px;
  color: #888;
  line-height: 1.3;
}

/* =====================
   ERROR MESSAGE AND SEARCH BUTTON
   ===================== */

/* Red error text shown below the fields */
.error-msg {
  color: #e74c3c;
  font-size: 0.83rem;
  margin-bottom: 12px;
  margin-top: -8px;
}

/* Full-width search button with a red-to-orange gradient */
.search-btn {
  width: 100%;
  padding: 14px;
  background: linear-gradient(to right, #e74c3c, #f39c12);
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

/* =====================
   RESULTS SECTION
   Light grey background holding the two-column layout: filters + flights
   ===================== */
.results-section {
  background: #f4f6f9;
  min-height: 400px;
  padding: 36px 24px;
}

/* Two-column layout: fixed 260px sidebar + flexible flights column */
.results-layout {
  max-width: 1100px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 24px;
  align-items: start;
}

/* =====================
   FILTERS PANEL
   Left sidebar with the three interactive filter controls
   ===================== */
.filters-panel {
  background: white;
  border-radius: 12px;
  padding: 20px 22px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.07);
}

.filters-title {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 20px;
}

/* Individual filter block */
.filter-group {
  margin-bottom: 22px;
}

/* Row with filter name on the left and current value on the right */
.filter-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
  font-size: 0.88rem;
  font-weight: 600;
  color: #333;
}

/* Current filter value displayed in red */
.filter-value {
  font-weight: 400;
  color: #e74c3c;
  font-size: 0.82rem;
}

/* Range slider with red accent color */
.range-slider {
  width: 100%;
  accent-color: #e74c3c;
  cursor: pointer;
}

/* Stops filter options stacked vertically */
.stops-options {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

/* Each stop filter option: radio + label text */
.stop-option {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.85rem;
  cursor: pointer;
  color: #555;
  padding: 2px 0;
}
.stop-option input[type="radio"] {
  accent-color: #e74c3c;
  cursor: pointer;
}

/* Active option highlighted in red and bold */
.stop-option.active { color: #e74c3c; font-weight: 600; }

/* =====================
   FLIGHTS PANEL
   Right column with the list of flight cards
   ===================== */
.flights-panel {
  display: flex;
  flex-direction: column;
  gap: 0;
}

/* Text showing how many flights pass the current filters */
.results-count {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 16px;
}

/* Centered empty state when no flights are available */
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

/* =====================
   FLIGHT CARD
   Individual card for each flight in the filtered results
   ===================== */
.flight-card {
  background: white;
  border-radius: 12px;
  padding: 18px 22px 14px;
  margin-bottom: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.06);
  transition: box-shadow 0.2s;
}
.flight-card:hover { box-shadow: 0 4px 20px rgba(0,0,0,0.12); }

/* Card header: airline on the left, price on the right */
.flight-card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 14px;
}

.airline-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* Small square icon with a light pink background for the airline logo */
.airline-icon {
  width: 32px;
  height: 32px;
  background: #fff0ee;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #e74c3c;
  font-size: 0.9rem;
}

.airline-name {
  font-size: 0.88rem;
  font-weight: 600;
  color: #333;
}

.price-section {
  text-align: right;
}

/* Small "per person" label above the price */
.price-label {
  display: block;
  font-size: 0.72rem;
  color: #e74c3c;
  margin-bottom: 2px;
}

/* Large price with a red-to-orange gradient text color */
.price-amount {
  font-size: 1.5rem;
  font-weight: 800;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

/* Main flight row: origin block → center connector → destination block → button */
.flight-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

/* Block showing the departure or arrival time and airport code */
.flight-time-info {
  min-width: 60px;
}

/* Large bold time */
.time {
  font-size: 1.35rem;
  font-weight: 700;
  color: #1a1a1a;
  line-height: 1;
}

/* Small IATA code below the time */
.airport-code-small {
  font-size: 0.78rem;
  color: #888;
  margin-top: 3px;
}

/* Center section between origin and destination: duration, badge, and connecting line */
.flight-middle {
  flex: 1;
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.duration-badge-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;
}

/* Grey duration text */
.duration-text {
  font-size: 0.78rem;
  color: #888;
}

/* Pill badge indicating direct flight or stops */
.stops-badge {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 3px 10px;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
}

/* Yellow style for direct flights */
.stops-badge.directo {
  background: #fef9e7;
  color: #d4a017;
  border: 1px solid #f5d576;
}

/* Red style for flights with stops */
.stops-badge.con-escala {
  background: #fff0ee;
  color: #e74c3c;
  border: 1px solid #f5b7b1;
}

/* Thin horizontal line with small dot endpoints connecting origin and destination */
.flight-line {
  width: 100%;
  height: 1.5px;
  background: #e0e0e0;
  position: relative;
}
.flight-line::before,
.flight-line::after {
  content: '';
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #ccc;
}
.flight-line::before { left: 0; }
.flight-line::after { right: 0; }

/* Right-aligned container for the select button */
.select-section {
  min-width: 110px;
  text-align: right;
}

/* Select button with a red-to-orange gradient */
.select-btn {
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: white;
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: opacity 0.2s;
}
.select-btn:hover { opacity: 0.88; }

/* Passenger selector inside the input-box */
.passenger-selector {
  justify-content: space-between;
  padding: 8px 12px;
}

/* +/- buttons */
.passenger-btn {
  background: none;
  border: 1.5px solid #e0e0e0;
  border-radius: 6px;
  width: 28px;
  height: 28px;
  font-size: 1.1rem;
  line-height: 1;
  cursor: pointer;
  color: #e74c3c;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: background 0.15s, border-color 0.15s;
}
.passenger-btn:hover {
  background: #fff0ee;
  border-color: #e74c3c;
}

/* Passenger count number in the center */
.passenger-count {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
  min-width: 24px;
  text-align: center;
}

/* Total price row below the individual price */
.price-total-row {
  margin-top: 4px;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 1px;
}

/* "Total · N pasajero(s)" label */
.price-total-label {
  font-size: 0.7rem;
  color: #aaa;
}

/* Total amount in a smaller but still prominent style */
.price-total-amount {
  font-size: 1rem;
  font-weight: 700;
  color: #888;
}

/* Card footer showing total duration, separated by a thin top border */
.flight-footer {
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid #f0f0f0;
  font-size: 0.78rem;
  color: #aaa;
}

/* =====================
   ADVENTURE SECTION
   Call-to-action shown when the user has not performed any search yet
   ===================== */
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
</style>
