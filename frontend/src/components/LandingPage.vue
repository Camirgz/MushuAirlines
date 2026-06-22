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

      <div class="nav-actions">
        <router-link to="/my-reservation" class="nav-link-item">
            <i class="bi bi-briefcase me-1"></i>
            Mis vuelos
        </router-link>
        <a href="#" class="nav-link-item">
          <i class="bi bi-calendar-check me-1"></i>Check-in
        </a>

        <!-- Management dropdown — admins only -->
        <div v-if="isAdmin" class="management-wrapper" ref="managementMenu">
          <button class="management-btn" type="button" @click.stop="toggleDropdown">
            <i class="bi bi-gear me-1"></i>
            Gestión
            <i class="bi ms-1" :class="isDropdownOpen ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
          </button>
          <div v-if="isDropdownOpen" class="management-dropdown">
            <RouterLink to="/admin" class="dropdown-item-custom" @click="closeDropdown">
              <i class="bi bi-grid"></i><span>Página principal interna</span>
            </RouterLink>
            <RouterLink to="/admin/aircraft-types" class="dropdown-item-custom" @click="closeDropdown">
              <i class="bi bi-airplane"></i><span>Tipos de aeronaves</span>
            </RouterLink>
            <RouterLink to="/admin/routes" class="dropdown-item-custom" @click="closeDropdown">
              <i class="bi bi-geo-alt"></i><span>Rutas</span>
            </RouterLink>
            <RouterLink to="/admin/airports" class="dropdown-item-custom" @click="closeDropdown">
              <i class="bi bi-airplane-engines"></i><span>Aeropuertos</span>
            </RouterLink>
            <RouterLink to="/admin/users" class="dropdown-item-custom" @click="closeDropdown">
              <i class="bi bi-people"></i><span>Usuarios administradores y operarios</span>
            </RouterLink>

            <div class="dropdown-divider"></div>

            <RouterLink to="/admin/reports/flight-detail" class="dropdown-item-custom" @click="closeDropdown">
              <i class="bi bi-airplane-fill"></i><span>Reporte vuelo detallado</span>
            </RouterLink>
            <RouterLink to="/admin/reports/monthly-income" class="dropdown-item-custom" @click="closeDropdown">
              <i class="bi bi-bar-chart-line-fill"></i><span>Reporte ingresos por mes</span>
            </RouterLink>
          </div>
        </div>

        <!-- Profile button (logged-in) / Admin Login link (logged-out) -->
        <div v-if="isLoggedIn" class="profile-wrapper" ref="profileMenu">
          <button class="profile-btn" type="button" @click.stop="toggleProfileMenu">
            <i class="bi bi-person"></i>
          </button>
          <div v-if="isProfileMenuOpen" class="profile-dropdown">
            <RouterLink to="/admin/profile" class="profile-dropdown-item" @click="closeProfileMenu">
              <i class="bi bi-person"></i>
              <span>Perfil</span>
            </RouterLink>
            <button type="button" class="profile-dropdown-item logout-dropdown-btn" @click="logout">
              <i class="bi bi-box-arrow-right"></i>
              <span>Logout</span>
            </button>
          </div>
        </div>
        <a v-else href="/login" class="btn btn-outline-danger rounded-pill px-3 py-1 admin-btn">
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
                  v-for="s in originSuggestions"
                  :key="s.type + '-' + s.value"
                  class="autocomplete-item"
                  :class="s.type === 'airport' ? 'autocomplete-item--airport' : 'autocomplete-item--city'"
                  @mousedown.prevent="selectOrigin(s)"
                >
                  <i class="bi" :class="s.type === 'city' ? 'bi-globe2' : 'bi-airplane'"></i>
                  <span class="suggestion-label">{{ s.label }}</span>
                </div>
              </div>
            </div>

            <!-- Destination -->
            <div class="field-group autocomplete-wrapper">
              <label class="field-label"><i class="bi bi-geo-alt-fill me-1"></i>Destino</label>
              <div class="input-box" :class="{ focused: showDestinationDropdown }">
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
                  v-for="s in destinationSuggestions"
                  :key="s.type + '-' + s.value"
                  class="autocomplete-item"
                  :class="s.type === 'airport' ? 'autocomplete-item--airport' : 'autocomplete-item--city'"
                  @mousedown.prevent="selectDestination(s)"
                >
                  <i class="bi" :class="s.type === 'city' ? 'bi-globe2' : 'bi-airplane'"></i>
                  <span class="suggestion-label">{{ s.label }}</span>
                </div>
              </div>
            </div>

            <!-- Date -->
            <div class="field-group">
              <label class="field-label"><i class="bi bi-calendar3 me-1"></i>Fecha de salida</label>
              <div class="input-box date-field" :class="{ 'input-date-error': dateError }">
                <input
                  type="text"
                  :value="departureDateDisplay"
                  @input="onDateInput"
                  placeholder="dd/mm/aaaa"
                  maxlength="10"
                  inputmode="numeric"
                  class="date-text-input"
                />
                <button type="button" class="date-picker-btn" @click="openDatePicker">
                  <i class="bi bi-calendar3"></i>
                </button>
                <input
                  type="date"
                  ref="datePicker"
                  :value="departureDate"
                  @change="onDatePickerChange"
                  :min="todayStr"
                  :max="maxDateStr"
                  class="date-picker-hidden"
                  tabindex="-1"
                />
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

          <p v-if="dateError" class="date-error-msg">{{ dateError }}</p>
          <p v-if="errorMsg" class="error-msg">{{ errorMsg }}</p>

          <button class="search-btn" @click="searchFlights">
            <i class="bi bi-search me-2"></i>Buscar vuelos
          </button>
        </div>
      </div>
    </div>

    <div class="results-section" v-if="hasSearched">
      <div class="results-layout">

        <div class="sidebar-col">

        <aside class="side-card">
          <h3 class="side-card-title"><i class="bi bi-sort-down me-2"></i>Ordenar por</h3>
          <div class="sort-row-list">
            <button
              class="sort-row"
              :class="{ 'sort-row--active': sortBy === 'price-economy' }"
              @click="sortBy = 'price-economy'"
            >
              <i class="bi bi-ticket-perforated"></i> Precio Turista
            </button>
            <button
              class="sort-row"
              :class="{ 'sort-row--active': sortBy === 'price-business' }"
              @click="sortBy = 'price-business'"
            >
              <i class="bi bi-briefcase"></i> Precio Business
            </button>
            <button
              class="sort-row"
              :class="{ 'sort-row--active': sortBy === 'duration-asc' }"
              @click="sortBy = 'duration-asc'"
            >
              <i class="bi bi-clock"></i> Duración
            </button>
            <button
              v-if="flightSearchMode === 'stopover'"
              class="sort-row"
              :class="{ 'sort-row--active': sortBy === 'layover-asc' }"
              @click="sortBy = 'layover-asc'"
            >
              <i class="bi bi-hourglass-split"></i> Escala más corta
            </button>
            <button
              v-if="flightSearchMode === 'stopover'"
              class="sort-row"
              :class="{ 'sort-row--active': sortBy === 'layover-desc' }"
              @click="sortBy = 'layover-desc'"
            >
              <i class="bi bi-hourglass"></i> Escala más larga
            </button>
          </div>
        </aside>

        <aside class="side-card filters-panel">
          <h3 class="side-card-title">Filtros</h3>

          <div class="filter-group">
            <div class="filter-header">
              <span>Precio</span>
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
            <div class="dual-range">
              <div class="dual-range-track" :style="priceTrackStyle"></div>
              <input
                type="range"
                v-model.number="filterPriceMin"
                :min="0"
                :max="priceSliderRange"
                step="100"
                class="range-slider"
              />
              <input
                type="range"
                v-model.number="filterPriceMax"
                :min="0"
                :max="priceSliderRange"
                step="100"
                class="range-slider"
              />
            </div>
            <div class="range-inputs">
              <div class="range-input-group">
                <span>$</span>
                <input type="number" v-model.number="filterPriceMin" :min="0" :max="filterPriceMax" step="100" />
              </div>
              <span class="range-separator">–</span>
              <div class="range-input-group">
                <span>$</span>
                <input type="number" v-model.number="filterPriceMax" :min="filterPriceMin" :max="priceSliderRange" step="100" />
              </div>
            </div>
          </div>

          <div class="filter-group">
            <div class="filter-header">
              <span>Duración</span>
            </div>
            <div class="dual-range">
              <div class="dual-range-track" :style="durationTrackStyle"></div>
              <input
                type="range"
                v-model.number="filterDurationMin"
                min="0"
                :max="filterDurationMax"
                step="0.5"
                class="range-slider"
              />
              <input
                type="range"
                v-model.number="filterDurationMax"
                :min="filterDurationMin"
                max="24"
                step="0.5"
                class="range-slider"
              />
            </div>
            <div class="range-inputs">
              <div class="range-input-group">
                <input type="number" v-model.number="filterDurationMin" min="0" :max="filterDurationMax" step="0.5" />
                <span>h</span>
              </div>
              <span class="range-separator">–</span>
              <div class="range-input-group">
                <input type="number" v-model.number="filterDurationMax" :min="filterDurationMin" max="24" step="0.5" />
                <span>h</span>
              </div>
            </div>
          </div>
        </aside>

        </div><!-- end .sidebar-col -->

        <div class="flights-panel">

          <!-- Stopover itineraries first when mode is 'stopover' -->
          <template v-if="flightSearchMode === 'stopover'">
            <p class="results-count">
              {{ filteredStopoverResults.length }} {{ filteredStopoverResults.length === 1 ? 'itinerario con escala disponible' : 'itinerarios con escala disponibles' }}
            </p>

            <div v-if="filteredStopoverResults.length === 0" class="no-results">
              <i class="bi bi-arrow-left-right"></i>
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
              :airports="airports"
              @select="handleStopoverSelect"
            />
          </template>

          <!-- Direct flights -->
          <p class="results-count" :class="{ 'results-count--section': flightSearchMode === 'stopover' }">
            {{ filteredDirectFlights.length }} {{ filteredDirectFlights.length === 1 ? 'vuelo directo disponible' : 'vuelos directos disponibles' }}
          </p>

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
            :airports="airports"
            @select="openFlightDetails"
          />

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
                <span class="modal-class-price">${{ selectedFlight.priceFirstClass.toLocaleString() }} / persona</span>
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
                <span class="modal-class-price">${{ selectedFlight.priceEconomy.toLocaleString() }} / persona</span>
              </div>
              <div class="modal-qty-ctrl">
                <span class="modal-qty-val readonly">{{ economyCount }}</span>
              </div>
            </div>
            <p v-if="firstClassCount + economyCount !== passengerCount" class="modal-class-warning">
              La suma debe ser {{ passengerCount }} pasajero(s)
            </p>
          </div>

          <!-- Purchase summary -->
          <div class="modal-receipt">
            <div class="modal-section-title">Resumen</div>

            <div class="receipt-line" v-if="firstClassCount > 0">
              <span>{{ firstClassCount }} × Primera Clase</span>
              <span>${{ (firstClassCount * selectedFlight.priceFirstClass).toLocaleString() }}</span>
            </div>
            <div class="receipt-line" v-if="economyCount > 0">
              <span>{{ economyCount }} × Clase Turista</span>
              <span>${{ (economyCount * selectedFlight.priceEconomy).toLocaleString() }}</span>
            </div>
            <div class="receipt-total">
              <span>Total</span>
              <span>${{ modalTotal.toLocaleString() }}</span>
            </div>
          </div>

          <div v-if="seatAvailabilityError" class="modal-seat-error">
            <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ seatAvailabilityError }}
          </div>

          <div class="modal-actions">
            <button class="modal-cancel-btn" @click="closeFlightDetails">Cancelar</button>
            <button
              class="modal-cart-btn"
              :disabled="passengerCount === 0"
              @click="startPurchase"
            >
              <i class="bi bi-arrow-right-circle me-2"></i>Continuar con la compra
            </button>
          </div>

        </div>
      </div>
    </Transition>

    <!-- Booking modal — stopover flights -->
    <Transition name="modal-fade">
      <div class="modal-overlay" v-if="selectedStopover" @click.self="closeStopoverModal">
        <div class="flight-modal">

          <button class="modal-close-btn" @click="closeStopoverModal">
            <i class="bi bi-x-lg"></i>
          </button>

          <div class="modal-airline-header">
            <div class="modal-airline-info">
              <div class="airline-icon">
                <i class="bi bi-airplane-fill"></i>
              </div>
              <div>
                <div class="modal-airline-name">Mushu Airlines</div>
                <div class="modal-flight-id">Itinerario con escala en {{ selectedStopover.connectionCity }}</div>
              </div>
            </div>
          </div>

          <!-- Leg 1 -->
          <div class="modal-route-section">
            <div class="modal-endpoint">
              <div class="modal-time">{{ selectedStopover.leg1.departureTime }}</div>
              <div class="modal-city">{{ getAirportCity(selectedStopover.leg1.origin) }}</div>
              <div class="modal-iata">{{ selectedStopover.leg1.origin }}</div>
              <div class="modal-date">{{ selectedStopover.leg1.date }}</div>
            </div>
            <div class="modal-route-middle">
              <div class="modal-duration-label">{{ selectedStopover.leg1.durationLabel }}</div>
              <div class="modal-route-line">
                <div class="modal-route-dot left"></div>
                <div class="modal-route-plane"><i class="bi bi-airplane-fill"></i></div>
                <div class="modal-route-dot right"></div>
              </div>
            </div>
            <div class="modal-endpoint right">
              <div class="modal-time">{{ selectedStopover.leg1.arrivalTime }}</div>
              <div class="modal-city">{{ getAirportCity(selectedStopover.leg1.destination) }}</div>
              <div class="modal-iata">{{ selectedStopover.leg1.destination }}</div>
              <div class="modal-date">{{ selectedStopover.leg1.arrivalDate }}</div>
            </div>
          </div>

          <!-- Layover indicator -->
          <div class="modal-layover-bar">
            <div class="modal-layover-line"></div>
            <div class="modal-layover-badge">
              <i class="bi bi-hourglass-split me-1"></i>
              Escala de
              {{ Math.floor(selectedStopover.layoverMinutes / 60) }}h
              {{ selectedStopover.layoverMinutes % 60 > 0 ? (selectedStopover.layoverMinutes % 60) + 'm' : '' }}
              en {{ selectedStopover.connectionCity }}
            </div>
            <div class="modal-layover-line"></div>
          </div>

          <!-- Leg 2 -->
          <div class="modal-route-section">
            <div class="modal-endpoint">
              <div class="modal-time">{{ selectedStopover.leg2.departureTime }}</div>
              <div class="modal-city">{{ getAirportCity(selectedStopover.leg2.origin) }}</div>
              <div class="modal-iata">{{ selectedStopover.leg2.origin }}</div>
              <div class="modal-date">{{ selectedStopover.leg2.date }}</div>
            </div>
            <div class="modal-route-middle">
              <div class="modal-duration-label">{{ selectedStopover.leg2.durationLabel }}</div>
              <div class="modal-route-line">
                <div class="modal-route-dot left"></div>
                <div class="modal-route-plane"><i class="bi bi-airplane-fill"></i></div>
                <div class="modal-route-dot right"></div>
              </div>
            </div>
            <div class="modal-endpoint right">
              <div class="modal-time">{{ selectedStopover.leg2.arrivalTime }}</div>
              <div class="modal-city">{{ getAirportCity(selectedStopover.leg2.destination) }}</div>
              <div class="modal-iata">{{ selectedStopover.leg2.destination }}</div>
              <div class="modal-date">{{ selectedStopover.leg2.arrivalDate }}</div>
            </div>
          </div>

          <div class="modal-info-grid">
            <div class="modal-info-item">
              <span class="modal-info-label"><i class="bi bi-globe me-1"></i>País destino</span>
              <span class="modal-info-value">{{ getAirportCountry(selectedStopover.leg2.destination) }}</span>
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
                <span class="modal-class-price">${{ (selectedStopover.leg1.priceFirstClass + selectedStopover.leg2.priceFirstClass).toLocaleString() }} / persona</span>
              </div>
              <div class="modal-qty-ctrl">
                <button class="modal-qty-btn" @click="stopoverFirstClassCount = Math.max(0, stopoverFirstClassCount - 1)">−</button>
                <span class="modal-qty-val">{{ stopoverFirstClassCount }}</span>
                <button class="modal-qty-btn" @click="stopoverFirstClassCount = Math.min(passengerCount, stopoverFirstClassCount + 1)">+</button>
              </div>
            </div>
            <div class="modal-class-row">
              <div class="modal-class-info">
                <span class="modal-class-name">Clase Turista</span>
                <span class="modal-class-price">${{ (selectedStopover.leg1.priceEconomy + selectedStopover.leg2.priceEconomy).toLocaleString() }} / persona</span>
              </div>
              <div class="modal-qty-ctrl">
                <span class="modal-qty-val readonly">{{ stopoverEconomyCount }}</span>
              </div>
            </div>
            <p v-if="stopoverFirstClassCount + stopoverEconomyCount !== passengerCount" class="modal-class-warning">
              La suma debe ser {{ passengerCount }} pasajero(s)
            </p>
          </div>

          <!-- Purchase summary -->
          <div class="modal-receipt">
            <div class="modal-section-title">Resumen</div>
            <div class="receipt-line" v-if="stopoverFirstClassCount > 0">
              <span>{{ stopoverFirstClassCount }} × Primera Clase</span>
              <span>${{ (stopoverFirstClassCount * (selectedStopover.leg1.priceFirstClass + selectedStopover.leg2.priceFirstClass)).toLocaleString() }}</span>
            </div>
            <div class="receipt-line" v-if="stopoverEconomyCount > 0">
              <span>{{ stopoverEconomyCount }} × Clase Turista</span>
              <span>${{ (stopoverEconomyCount * (selectedStopover.leg1.priceEconomy + selectedStopover.leg2.priceEconomy)).toLocaleString() }}</span>
            </div>
            <div class="receipt-total">
              <span>Total</span>
              <span>${{ stopoverModalTotal.toLocaleString() }}</span>
            </div>
          </div>

          <div v-if="stopoverSeatAvailabilityError" class="modal-seat-error">
            <i class="bi bi-exclamation-triangle-fill me-2"></i>{{ stopoverSeatAvailabilityError }}
          </div>

          <div class="modal-actions">
            <button class="modal-cancel-btn" @click="closeStopoverModal">Cancelar</button>
            <button
              class="modal-cart-btn"
              :disabled="passengerCount === 0"
              @click="startStopoverPurchase"
            >
              <i class="bi bi-arrow-right-circle me-2"></i>Continuar con la compra
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
import { usePurchaseFlow } from '@/composables/usePurchaseFlow'
import { checkAvailability } from '@/services/PurchaseService'
import API_BASE_URL from '@/config/api'

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

// Returns true when the flight runs on the given date (weekday + active date range).
function flightOperatesOnDate(flight, dateStr) {
  if (!dateStr) return true
  const [year, month, day] = dateStr.split('-').map(Number)
  const date = new Date(year, month - 1, day)
  if (flight.finalizationDate) {
    const [ey, em, ed] = flight.finalizationDate.split('-').map(Number)
    if (new Date(ey, em - 1, ed) < date) return false
  }
  if (!flight.frequency || flight.frequency.length === 0) return true
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
    finalizationDate: r.finalizationDate || null,
  }
}

export default {
  name: 'LandingPage',

  components: { FlightResultCard },

  data() {
    return {
      userRole: null,
      isDropdownOpen: false,
      isProfileMenuOpen: false,

      airports: [],
      flights: [],

      departureDate: '',
      departureDateDisplay: '',

      originQuery: '',
      selectedOrigin: null,
      showOriginDropdown: false,
      originSuggestions: [],

      destinationQuery: '',
      selectedDestination: null,
      showDestinationDropdown: false,
      destinationSuggestions: [],

      // 'direct' shows direct routes; 'stopover' shows two-leg connections.
      flightSearchMode: 'direct',

      hasSearched: false,
      directFlightResults: [],
      stopoverResults: [],

      filterPriceMin: 0,
      filterPriceMax: 500,
      priceSliderRange: 500,
      filterDurationMin: 0,
      filterDurationMax: 24,
      filterPriceClass: 'economy',
      maxEconomyPrice: 100000,
      maxBusinessPrice: 200000,
      sortBy: 'price-economy',

      passengerCount: 1,
      errorMsg: '',
      selectedFlight: null,
      firstClassCount: 0,
      seatAvailabilityError: null,

      selectedStopover: null,
      stopoverFirstClassCount: 0,
      stopoverSeatAvailabilityError: null,
    }
  },

  async created() {
    try {
      const airportRes = await fetch(`${API_BASE_URL}/api/airport`)
      this.airports = await airportRes.json()
    } catch (e) {
      console.error('Error cargando aeropuertos:', e)
    }
    try {
      const flightsRes = await fetch(`${API_BASE_URL}/api/flights`)
      const flights = await flightsRes.json()
      console.log('[DEBUG] primer vuelo raw del API:', JSON.stringify(flights[0]))
      this.flights = flights.map(routeToFlight)
      console.log('[DEBUG] primer vuelo mapeado finalizationDate:', this.flights[0]?.finalizationDate)
    } catch (e) {
      console.error('Error cargando vuelos:', e)
    }
  },

  computed: {
    isLoggedIn() {
      return this.userRole !== null
    },

    isAdmin() {
      return this.userRole === 'Administrator'
    },

    todayStr() {
      return new Date().toISOString().split('T')[0]
    },

    maxDateStr() {
      const d = new Date()
      d.setFullYear(d.getFullYear() + 1)
      return d.toISOString().split('T')[0]
    },

    dateError() {
      if (!this.departureDateDisplay) return ''
      if (!this.departureDate) return 'Formato: dd/mm/aaaa'
      if (this.departureDate < this.todayStr) return 'Esta fecha ya pasó'
      if (this.departureDate > this.maxDateStr) return 'El límite de reserva es de un año'
      return ''
    },

    filteredDirectFlights() {
      const isBusiness = this.filterPriceClass === 'business'
      return this.directFlightResults
        .filter(f => {
          const price = isBusiness ? f.priceFirstClass : f.priceEconomy
          if (price < this.filterPriceMin || price > this.filterPriceMax) return false
          if (f.durationHours < this.filterDurationMin || f.durationHours > this.filterDurationMax) return false
          return true
        })
        .sort((a, b) => {
          if (this.sortBy === 'price-economy') return a.priceEconomy - b.priceEconomy
          if (this.sortBy === 'price-business') return a.priceFirstClass - b.priceFirstClass
          if (this.sortBy === 'duration-asc' || this.sortBy === 'layover-asc' || this.sortBy === 'layover-desc') return a.durationHours - b.durationHours
          return a.priceEconomy - b.priceEconomy
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
          if (combinedPrice < this.filterPriceMin || combinedPrice > this.filterPriceMax) return false
          if (combinedDuration < this.filterDurationMin || combinedDuration > this.filterDurationMax) return false
          return true
        })
        .sort((a, b) => {
          const durA = a.leg1.durationHours + a.leg2.durationHours
          const durB = b.leg1.durationHours + b.leg2.durationHours
          const econA = a.leg1.priceEconomy + a.leg2.priceEconomy
          const econB = b.leg1.priceEconomy + b.leg2.priceEconomy
          const busA = a.leg1.priceFirstClass + a.leg2.priceFirstClass
          const busB = b.leg1.priceFirstClass + b.leg2.priceFirstClass
          if (this.sortBy === 'price-economy') return econA - econB
          if (this.sortBy === 'price-business') return busA - busB
          if (this.sortBy === 'duration-asc') return durA - durB
          if (this.sortBy === 'layover-asc') return a.layoverMinutes - b.layoverMinutes
          if (this.sortBy === 'layover-desc') return b.layoverMinutes - a.layoverMinutes
          return econA - econB
        })
    },

    economyCount() {
      return this.passengerCount - this.firstClassCount
    },

    modalTotal() {
      if (!this.selectedFlight) return 0
      const f = this.selectedFlight
      return (this.firstClassCount * f.priceFirstClass) + (this.economyCount * f.priceEconomy)
    },

    stopoverEconomyCount() {
      return this.passengerCount - this.stopoverFirstClassCount
    },

    stopoverModalTotal() {
      if (!this.selectedStopover) return 0
      const l1 = this.selectedStopover.leg1
      const l2 = this.selectedStopover.leg2
      return this.stopoverFirstClassCount * (l1.priceFirstClass + l2.priceFirstClass)
           + this.stopoverEconomyCount    * (l1.priceEconomy    + l2.priceEconomy)
    },

    priceTrackStyle() {
      const range = this.priceSliderRange
      if (!range) return {}
      const low = (this.filterPriceMin / range) * 100
      const high = (this.filterPriceMax / range) * 100
      return {
        background: `linear-gradient(to right, #e5e7eb ${low}%, var(--color-primary) ${low}%, var(--color-primary) ${high}%, #e5e7eb ${high}%)`
      }
    },

    durationTrackStyle() {
      const low = (this.filterDurationMin / 24) * 100
      const high = (this.filterDurationMax / 24) * 100
      return {
        background: `linear-gradient(to right, #e5e7eb ${low}%, var(--color-primary) ${low}%, var(--color-primary) ${high}%, #e5e7eb ${high}%)`
      }
    },
  },

  mounted() {
    this.userRole = this.getRoleFromToken()
    document.addEventListener('click', this.handleOutsideClick)
  },

  beforeUnmount() {
    document.removeEventListener('click', this.handleOutsideClick)
  },

  watch: {
    filterPriceMin(val) {
      const clamped = Math.max(0, Math.min(+val || 0, this.filterPriceMax))
      if (clamped !== val) this.filterPriceMin = clamped
    },
    filterPriceMax(val) {
      const clamped = Math.max(this.filterPriceMin, Math.min(+val || this.priceSliderRange, this.priceSliderRange))
      if (clamped !== val) this.filterPriceMax = clamped
    },
    filterDurationMin(val) {
      const clamped = Math.max(0, Math.min(+val || 0, this.filterDurationMax))
      if (clamped !== val) this.filterDurationMin = clamped
    },
    filterDurationMax(val) {
      const clamped = Math.max(this.filterDurationMin, Math.min(+val || 24, 24))
      if (clamped !== val) this.filterDurationMax = clamped
    },
  },

  methods: {
    getRoleFromToken() {
      const token = localStorage.getItem('token')
      if (!token) return null
      try {
        const payload = JSON.parse(atob(token.split('.')[1]))
        return (
          payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
          payload.role ||
          payload.Role ||
          null
        )
      } catch {
        return null
      }
    },

    toggleDropdown() {
      this.isDropdownOpen = !this.isDropdownOpen
      this.isProfileMenuOpen = false
    },

    closeDropdown() {
      this.isDropdownOpen = false
    },

    toggleProfileMenu() {
      this.isProfileMenuOpen = !this.isProfileMenuOpen
      this.isDropdownOpen = false
    },

    closeProfileMenu() {
      this.isProfileMenuOpen = false
    },

    handleOutsideClick(event) {
      const managementMenu = this.$refs.managementMenu
      const profileMenu = this.$refs.profileMenu
      if (managementMenu && !managementMenu.contains(event.target)) {
        this.isDropdownOpen = false
      }
      if (profileMenu && !profileMenu.contains(event.target)) {
        this.isProfileMenuOpen = false
      }
    },

    logout() {
      localStorage.removeItem('token')
      this.userRole = null
      this.isDropdownOpen = false
      this.isProfileMenuOpen = false
    },

    async onOriginInput() {
      this.selectedOrigin = null
      this.showOriginDropdown = true
      const q = this.originQuery.trim()
      if (!q) { this.originSuggestions = []; return }
      try {
        const res = await fetch(`${API_BASE_URL}/api/airport/suggestions?q=${encodeURIComponent(q)}`)
        this.originSuggestions = await res.json()
      } catch (e) {
        this.originSuggestions = []
      }
    },
    onOriginBlur() {
      setTimeout(() => { this.showOriginDropdown = false }, 200)
    },
    selectOrigin(s) {
      this.selectedOrigin = { type: s.type, value: s.value }
      this.originQuery = s.label
      this.originSuggestions = []
      this.showOriginDropdown = false
    },

    async onDestinationInput() {
      this.selectedDestination = null
      this.showDestinationDropdown = true
      const q = this.destinationQuery.trim()
      if (!q) { this.destinationSuggestions = []; return }
      try {
        const res = await fetch(`${API_BASE_URL}/api/airport/suggestions?q=${encodeURIComponent(q)}`)
        this.destinationSuggestions = await res.json()
      } catch (e) {
        this.destinationSuggestions = []
      }
    },
    onDestinationBlur() {
      setTimeout(() => { this.showDestinationDropdown = false }, 200)
    },
    selectDestination(s) {
      this.selectedDestination = { type: s.type, value: s.value }
      this.destinationQuery = s.label
      this.destinationSuggestions = []
      this.showDestinationDropdown = false
    },

    async searchFlights() {
      if (!this.selectedOrigin || !this.selectedDestination) {
        this.errorMsg = 'Por favor selecciona origen y destino de la lista.'
        return
      }
      if (this.selectedOrigin.value === this.selectedDestination.value) {
        this.errorMsg = 'El origen y destino no pueden ser iguales.'
        return
      }
      if (!this.departureDate) {
        this.errorMsg = 'Por favor selecciona una fecha de salida.'
        return
      }
      if (this.dateError) {
        this.errorMsg = this.dateError
        return
      }

      this.errorMsg = ''

      // Two parallel fetches:
      // 1. Direct flights — API filters by origin/destination/capacity
      // 2. All available flights for this date — used by the stopover finder
      let directFlights = []
      let availableFlights = this.flights
      try {
        const { value: originVal, type: originType } = this.selectedOrigin
        const { value: destVal, type: destType } = this.selectedDestination
        const [directRes, allRes] = await Promise.all([
          fetch(`${API_BASE_URL}/api/flights?date=${this.departureDate}&origin=${encodeURIComponent(originVal)}&originType=${originType}&destination=${encodeURIComponent(destVal)}&destinationType=${destType}`),
          fetch(`${API_BASE_URL}/api/flights?date=${this.departureDate}`)
        ])
        directFlights = (await directRes.json()).map(routeToFlight)
        availableFlights = (await allRes.json()).map(routeToFlight)
      } catch (e) {
        console.error('Error consultando vuelos:', e)
      }

      // Direct flights — API already filtered by origin/destination; keep frequency/date check
      this.directFlightResults = directFlights
        .filter(f => flightOperatesOnDate(f, this.departureDate))
        .map(f => ({
          ...f,
          date: this.departureDate,
          arrivalDate: isOvernightFlight(f)
            ? addDaysToDateString(this.departureDate, 1)
            : this.departureDate,
        }))

      // Stopover connections — expand city selection to array of airport codes
      const originCodes = this.selectedOrigin.type === 'city'
        ? this.airports.filter(a => a.city === this.selectedOrigin.value).map(a => a.code)
        : [this.selectedOrigin.value]
      const destCodes = this.selectedDestination.type === 'city'
        ? this.airports.filter(a => a.city === this.selectedDestination.value).map(a => a.code)
        : [this.selectedDestination.value]

      const rawConnections = findStopoverConnections(availableFlights, originCodes, destCodes, this.departureDate)

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
      this.filterDurationMin = 0
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
      this.seatAvailabilityError = null
      document.body.style.overflow = 'hidden'
    },

    closeFlightDetails() {
      this.selectedFlight = null
      document.body.style.overflow = ''
    },

    handleStopoverSelect(connection) {
      this.selectedStopover              = connection
      this.stopoverFirstClassCount       = 0
      this.stopoverSeatAvailabilityError = null
      document.body.style.overflow = 'hidden'
    },

    closeStopoverModal() {
      this.selectedStopover        = null
      document.body.style.overflow = ''
    },

    async startStopoverPurchase() {
      const conn = this.selectedStopover
      const leg1 = conn.leg1
      const leg2 = conn.leg2

      const [avail1, avail2] = await Promise.all([
        checkAvailability(leg1.id, leg1.date, this.passengerCount),
        checkAvailability(leg2.id, leg2.date, this.passengerCount),
      ])

      if (!avail1 || !avail2) {
        const leg = !avail1 ? leg1 : leg2
        this.stopoverSeatAvailabilityError =
          `Lo sentimos, el vuelo ${leg.origin}→${leg.destination} ya no tiene asientos disponibles para la cantidad de pasajeros solicitada.`
        return
      }

      const seats = [
        ...Array.from({ length: this.stopoverFirstClassCount }, (_, i) => ({
          passengerIndex: i,
          seatClass:      'FirstClass',
          seatNumber:     i + 1,
        })),
        ...Array.from({ length: this.stopoverEconomyCount }, (_, i) => ({
          passengerIndex: this.stopoverFirstClassCount + i,
          seatClass:      'Economy',
          seatNumber:     this.stopoverFirstClassCount + i + 1,
        })),
      ]

      const { setFlight } = usePurchaseFlow()
      setFlight(
        {
          code:            leg1.id,
          flightDate:      leg1.date,
          origin:          leg1.origin,
          destination:     leg2.destination,
          originCity:      leg1.originCity      ?? '',
          destinationCity: leg2.destinationCity ?? '',
          departureTime:   leg1.departureTime,
          arrivalTime:     leg2.arrivalTime,
          priceEconomy:     leg1.priceEconomy    + leg2.priceEconomy,
          priceFirstClass:  leg1.priceFirstClass + leg2.priceFirstClass,
          passengerCount:   this.passengerCount,
          handBagPrice:    (leg1.handBagPrice ?? 0) + (leg2.handBagPrice ?? 0),
          handBagWeight:    Math.min(leg1.handBagWeight ?? 0, leg2.handBagWeight ?? 0),
          bagPrice:        (leg1.bagPrice ?? 0) * (leg1.bagMultiplier ?? 1)
                         + (leg2.bagPrice ?? 0) * (leg2.bagMultiplier ?? 1),
          bagWeight:        Math.min(leg1.bagWeight ?? 0, leg2.bagWeight ?? 0),
          bagMultiplier:    1,
          isStopover:       true,
          connectionCity:   conn.connectionCity,
          layoverMinutes:   conn.layoverMinutes,
          leg1HandBagPrice:  leg1.handBagPrice  ?? 0,
          leg1BagPrice:      leg1.bagPrice      ?? 0,
          leg1BagMultiplier: leg1.bagMultiplier ?? 1,
        },
        seats,
        {
          code:          leg2.id,
          flightDate:    leg2.date,
          origin:        leg2.origin,
          destination:   leg2.destination,
          handBagPrice:  leg2.handBagPrice  ?? 0,
          bagPrice:      leg2.bagPrice      ?? 0,
          bagMultiplier: leg2.bagMultiplier ?? 1,
        }
      )

      this.closeStopoverModal()
      this.$router.push('/purchase/passengers')
    },

    async startPurchase() {
      const f = this.selectedFlight

      // Verify seat availability before navigating
      const available = await checkAvailability(f.id, f.date, this.passengerCount)
      if (!available) {
        this.seatAvailabilityError = 'Lo sentimos, este vuelo ya no tiene asientos disponibles para la cantidad de pasajeros solicitada.'
        return
      }

      // Build one seat entry per passenger, ordered First Class first then Economy.
      const seats = [
        ...Array.from({ length: this.firstClassCount }, (_, i) => ({
          passengerIndex: i,
          seatClass:      'FirstClass',
          seatNumber:     i + 1,
        })),
        ...Array.from({ length: this.economyCount }, (_, i) => ({
          passengerIndex: this.firstClassCount + i,
          seatClass:      'Economy',
          seatNumber:     this.firstClassCount + i + 1,
        })),
      ]

      const { setFlight } = usePurchaseFlow()
      setFlight({
        code:            f.id,
        flightDate:      f.date,
        origin:          f.origin,
        destination:     f.destination,
        originCity:      f.originCity      ?? '',
        destinationCity: f.destinationCity ?? '',
        departureTime:   f.departureTime,
        arrivalTime:     f.arrivalTime,
        priceEconomy:     f.priceEconomy,
        priceFirstClass:  f.priceFirstClass,
        passengerCount:   this.passengerCount,
        handBagPrice:     f.handBagPrice    ?? 0,
        handBagWeight:    f.handBagWeight   ?? 0,
        bagPrice:         f.bagPrice        ?? 0,
        bagWeight:        f.bagWeight       ?? 0,
        bagMultiplier:    f.bagMultiplier   ?? 1,
      }, seats)

      this.closeFlightDetails()
      this.$router.push('/purchase/passengers')
    },

    setFilterPriceClass(className) {
      this.filterPriceClass = className
      const max = className === 'business' ? this.maxBusinessPrice : this.maxEconomyPrice
      this.priceSliderRange = max + 5000
      this.filterPriceMax = max + 5000
    },

    openDatePicker() {
      const picker = this.$refs.datePicker
      if (picker.showPicker) picker.showPicker()
      else picker.click()
    },

    onDatePickerChange(e) {
      const val = e.target.value
      if (!val) return
      this.departureDate = val
      const [yyyy, mm, dd] = val.split('-')
      this.departureDateDisplay = `${dd}/${mm}/${yyyy}`
    },

    onDateInput(e) {
      const digits = e.target.value.replace(/\D/g, '').slice(0, 8)
      let result = ''
      for (let i = 0; i < digits.length; i++) {
        if (i === 2 || i === 4) result += '/'
        result += digits[i]
      }
      this.departureDateDisplay = result
      this.$nextTick(() => { e.target.value = result })
      if (digits.length === 8) {
        this.departureDate = `${digits.slice(4, 8)}-${digits.slice(2, 4)}-${digits.slice(0, 2)}`
      } else {
        this.departureDate = ''
      }
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

.nav-actions {
  display: flex;
  align-items: center;
  gap: 18px;
}

.nav-link-item {
  text-decoration: none;
  color: #333;
  font-size: 0.92rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  transition: color 0.2s;
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

/* ─── Management dropdown ─────────────────────────────────── */
.management-wrapper {
  position: relative;
}

.management-btn {
  padding: 8px 16px;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
  border: none;
  border-radius: 999px;
  font-size: 0.88rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.management-btn:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

.management-dropdown {
  position: absolute;
  top: 48px;
  right: 0;
  width: 320px;
  background: #ffffff;
  border: 1.5px solid #ddd;
  border-radius: 0 0 10px 10px;
  box-shadow: 0 8px 28px rgba(0, 0, 0, 0.15);
  padding: 8px;
  z-index: 200;
}

.dropdown-item-custom {
  display: flex;
  align-items: center;
  gap: 12px;
  text-decoration: none;
  color: #1a1a1a;
  padding: 12px 14px;
  border-radius: 8px;
  font-size: 0.88rem;
  font-weight: 600;
  transition: background 0.15s ease, color 0.15s ease;
}

.dropdown-item-custom i {
  color: #e74c3c;
  font-size: 0.95rem;
}

.dropdown-item-custom:hover {
  background: #fff5f5;
  color: #e74c3c;
}

.dropdown-item-custom.router-link-active,
.dropdown-item-custom.router-link-exact-active {
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
}

.dropdown-item-custom.router-link-active i,
.dropdown-item-custom.router-link-exact-active i {
  color: #ffffff;
}

.dropdown-divider {
  height: 1px;
  background: #f0f0f0;
  margin: 6px 8px;
}

/* ─── Profile button ──────────────────────────────────────── */
.profile-wrapper {
  position: relative;
}

.profile-btn {
  width: 42px;
  height: 42px;
  border: 1.5px solid #d1d5db;
  border-radius: 50%;
  background: #ffffff;
  color: #111827;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: 0.2s ease;
}

.profile-btn:hover {
  border-color: #e74c3c;
  color: #e74c3c;
}

.profile-dropdown {
  position: absolute;
  top: 52px;
  right: 0;
  width: 190px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  box-shadow: 0 14px 32px rgba(15, 23, 42, 0.16);
  padding: 8px;
  z-index: 300;
}

.profile-dropdown-item {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 12px;
  border: none;
  background: transparent;
  text-decoration: none;
  color: #111827;
  padding: 11px 12px;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: 0.2s ease;
}

.profile-dropdown-item i {
  color: #6b7280;
}

.profile-dropdown-item:hover {
  background: #fff4ed;
  color: #e74c3c;
}

.profile-dropdown-item:hover i {
  color: #e74c3c;
}

.logout-dropdown-btn {
  text-align: left;
}

/* ─── Hero ────────────────────────────────────────────────── */
.hero-section {
  background: linear-gradient(135deg, #c0392b 0%, #e74c3c 35%, #e67e22 70%, #f0a500 100%);
  min-height: 460px;
  display: flex;
  align-items: center;
  padding: 56px 24px 72px;
  position: relative;
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

.date-field {
  position: relative;
}

.date-text-input {
  border: none;
  outline: none;
  width: 100%;
  font-size: 0.88rem;
  color: #333;
  background: transparent;
}

.date-text-input::placeholder {
  color: #bbb;
}

.date-picker-btn {
  background: none;
  border: none;
  padding: 0;
  cursor: pointer;
  color: #bbb;
  font-size: 0.9rem;
  flex-shrink: 0;
  line-height: 1;
  transition: color 0.15s;
}

.date-picker-btn:hover {
  color: var(--color-primary);
}

.date-picker-hidden {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
  pointer-events: none;
  top: 0;
  left: 0;
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
  gap: 10px;
  padding: 11px 16px;
  cursor: pointer;
  transition: background 0.15s;
  border-bottom: 1px solid #f0f0f0;
}
.autocomplete-item:last-child { border-bottom: none; }
.autocomplete-item:hover { background: #fff5f5; }

.autocomplete-item--city {
  font-weight: 700;
  font-size: 0.93rem;
  color: #1a1a1a;
  background: #fafafa;
}
.autocomplete-item--city i {
  color: var(--color-primary);
  font-size: 1rem;
}

.autocomplete-item--airport {
  font-size: 0.85rem;
  color: #444;
}
.autocomplete-item--airport i {
  color: #aaa;
  font-size: 0.85rem;
}

.suggestion-label {
  line-height: 1.3;
}

/* ─── Misc form elements ──────────────────────────────────── */
.date-error-msg {
  color: #e53e3e;
  font-size: 0.83rem;
  margin-bottom: 8px;
  margin-top: 0;
}

.input-date-error {
  border-color: #e53e3e !important;
}

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
  /* visual card styles are provided by .side-card */
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

.dual-range {
  position: relative;
  height: 28px;
  margin-bottom: 2px;
}

.dual-range-track {
  position: absolute;
  left: 0;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
  height: 5px;
  border-radius: 3px;
  pointer-events: none;
  z-index: 0;
}

.dual-range input[type="range"] {
  position: absolute;
  width: 100%;
  top: 50%;
  transform: translateY(-50%);
  margin: 0;
  padding: 0;
  height: 5px;
  -webkit-appearance: none;
  appearance: none;
  background: transparent;
  pointer-events: none;
  outline: none;
  z-index: 1;
}

.dual-range input[type="range"]::-webkit-slider-runnable-track {
  background: transparent;
  height: 5px;
  border: none;
}

.dual-range input[type="range"]::-webkit-slider-thumb {
  -webkit-appearance: none;
  appearance: none;
  pointer-events: all;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: var(--color-primary);
  cursor: pointer;
  border: 2px solid #fff;
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.25);
  margin-top: -5.5px;
}

.dual-range input[type="range"]::-moz-range-track {
  background: transparent;
  height: 5px;
  border: none;
}

.dual-range input[type="range"]::-moz-range-thumb {
  pointer-events: all;
  width: 14px;
  height: 14px;
  border-radius: 50%;
  background: var(--color-primary);
  cursor: pointer;
  border: 2px solid #fff;
  box-shadow: 0 1px 5px rgba(0, 0, 0, 0.25);
}

.range-slider {
  width: 100%;
  accent-color: var(--color-primary);
  cursor: pointer;
}

.range-inputs {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 8px;
}

.range-input-group {
  display: flex;
  align-items: center;
  gap: 3px;
  flex: 1;
  background: #f3f4f6;
  border-radius: 6px;
  padding: 4px 7px;
  border: 1px solid #e5e7eb;
  transition: border-color 0.15s;
}

.range-input-group:focus-within {
  border-color: var(--color-primary);
}

.range-input-group span {
  font-size: 0.72rem;
  color: #888;
  flex-shrink: 0;
}

.range-input-group input[type="number"] {
  width: 100%;
  min-width: 0;
  border: none;
  background: transparent;
  font-size: 0.8rem;
  color: #333;
  outline: none;
  -moz-appearance: textfield;
}

.range-input-group input[type="number"]::-webkit-outer-spin-button,
.range-input-group input[type="number"]::-webkit-inner-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

.range-separator {
  font-size: 0.8rem;
  color: #aaa;
  flex-shrink: 0;
}

.flights-panel {
  display: flex;
  flex-direction: column;
}

/* ─── Sidebar column ─────────────────────────────────────── */
.sidebar-col {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.side-card {
  background: white;
  border-radius: 12px;
  padding: 20px 22px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.07);
}

.side-card-title {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 16px;
}

/* ─── Sort rows inside the sort card ─────────────────────── */
.sort-row-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.sort-row {
  display: flex;
  align-items: center;
  gap: 9px;
  width: 100%;
  padding: 8px 10px;
  border: none;
  border-radius: 8px;
  background: transparent;
  font-size: 0.85rem;
  font-weight: 500;
  color: #555;
  cursor: pointer;
  text-align: left;
  transition: background 0.15s, color 0.15s;
}

.sort-row i {
  font-size: 0.9rem;
  width: 16px;
  text-align: center;
  flex-shrink: 0;
}

.sort-row:hover {
  background: #fff0ee;
  color: var(--color-primary);
}

.sort-row--active {
  background: #fff0ee;
  color: var(--color-primary);
  font-weight: 600;
}

.sort-row--active i {
  color: var(--color-primary);
}

.results-count {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 16px;
}

.results-count--section {
  margin-top: 28px;
  padding-top: 20px;
  border-top: 1px solid #e5e7eb;
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

.modal-seat-error {
  background: #fff5f5;
  border: 1.5px solid #fca5a5;
  color: #b91c1c;
  border-radius: 10px;
  padding: 10px 14px;
  font-size: 0.85rem;
  font-weight: 600;
  margin-bottom: 12px;
}

.modal-layover-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 12px 0;
}
.modal-layover-line {
  flex: 1;
  height: 1px;
  background: #e8e8e8;
}
.modal-layover-badge {
  font-size: 0.78rem;
  font-weight: 700;
  color: #e67e22;
  background: #fff4e8;
  border: 1px solid #fcd9a4;
  border-radius: 20px;
  padding: 4px 12px;
  white-space: nowrap;
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
