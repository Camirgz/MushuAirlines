<template>
  <div class="flight-card" :class="{ 'flight-card--stopover': resultType === 'stopover' }">

    <!-- Airline row + price row — shared by both variants -->
    <div class="result-card-header">
      <div class="airline-row">
        <div class="airline-icon">
          <i class="bi bi-airplane-fill"></i>
        </div>
        <span class="airline-name">Mushu Airlines</span>
      </div>

      <div class="price-row">
        <div class="price-item">
          <span class="price-class-label">Turista</span>
          <span class="price-amount">${{ economyPrice.toLocaleString() }}</span>
          <span class="price-total" v-if="passengerCount > 1">
            {{ passengerCount }} pax · ${{ (economyPrice * passengerCount).toLocaleString() }}
          </span>
        </div>
        <span class="price-separator">|</span>
        <div class="price-item">
          <span class="price-class-label">Business</span>
          <span class="price-amount">${{ firstClassPrice.toLocaleString() }}</span>
          <span class="price-total" v-if="passengerCount > 1">
            {{ passengerCount }} pax · ${{ (firstClassPrice * passengerCount).toLocaleString() }}
          </span>
        </div>
      </div>
    </div>

    <!-- Direct flight: single leg -->
    <template v-if="resultType === 'direct'">
      <div class="leg-row">
        <div class="leg-endpoint">
          <div class="leg-time">{{ directFlight.departureTime }}</div>
          <template v-if="getAirport(directFlight.origin)">
            <div class="leg-iata-name"><span class="leg-iata">{{ directFlight.origin }}</span>, {{ getAirport(directFlight.origin).airportName }}</div>
            <div class="leg-location">{{ getAirport(directFlight.origin).city }}, {{ getAirport(directFlight.origin).country }}</div>
          </template>
          <div class="leg-iata-name" v-else><span class="leg-iata">{{ directFlight.origin }}</span></div>
          <div class="leg-date">{{ directFlight.date }}</div>
        </div>

        <div class="leg-connector">
          <div class="connector-duration">{{ directFlight.durationLabel }}</div>
          <div class="connector-track">
            <div class="track-dot"></div>
            <div class="track-plane"><i class="bi bi-airplane-fill"></i></div>
            <div class="track-dot"></div>
          </div>
          <div class="connector-tag">Vuelo directo</div>
        </div>

        <div class="leg-endpoint leg-endpoint--right">
          <div class="leg-time">{{ directFlight.arrivalTime }}</div>
          <template v-if="getAirport(directFlight.destination)">
            <div class="leg-iata-name"><span class="leg-iata">{{ directFlight.destination }}</span>, {{ getAirport(directFlight.destination).airportName }}</div>
            <div class="leg-location">{{ getAirport(directFlight.destination).city }}, {{ getAirport(directFlight.destination).country }}</div>
          </template>
          <div class="leg-iata-name" v-else><span class="leg-iata">{{ directFlight.destination }}</span></div>
          <div class="leg-date">{{ directFlight.arrivalDate }}</div>
        </div>
      </div>
    </template>

    <!-- Stopover itinerary: two legs separated by the layover indicator -->
    <template v-else>
      <!-- Leg 1 -->
      <div class="leg-block">
        <div class="leg-row">
          <div class="leg-endpoint">
            <div class="leg-time">{{ leg1.departureTime }}</div>
            <template v-if="getAirport(leg1.origin)">
              <div class="leg-iata-name"><span class="leg-iata">{{ leg1.origin }}</span>, {{ getAirport(leg1.origin).airportName }}</div>
              <div class="leg-location">{{ getAirport(leg1.origin).city }}, {{ getAirport(leg1.origin).country }}</div>
            </template>
            <div class="leg-iata-name" v-else><span class="leg-iata">{{ leg1.origin }}</span></div>
            <div class="leg-date">{{ leg1.date }}</div>
          </div>

          <div class="leg-connector">
            <div class="connector-duration">{{ leg1.durationLabel }}</div>
            <div class="connector-track">
              <div class="track-dot"></div>
              <div class="track-plane"><i class="bi bi-airplane-fill"></i></div>
              <div class="track-dot"></div>
            </div>
          </div>

          <div class="leg-endpoint leg-endpoint--right">
            <div class="leg-time">{{ leg1.arrivalTime }}</div>
            <template v-if="getAirport(leg1.destination)">
              <div class="leg-iata-name"><span class="leg-iata">{{ leg1.destination }}</span>, {{ getAirport(leg1.destination).airportName }}</div>
              <div class="leg-location">{{ getAirport(leg1.destination).city }}, {{ getAirport(leg1.destination).country }}</div>
            </template>
            <div class="leg-iata-name" v-else><span class="leg-iata">{{ leg1.destination }}</span></div>
            <div class="leg-date">{{ leg1.arrivalDate }}</div>
          </div>
        </div>
      </div>

      <!-- Layover indicator between the two legs -->
      <div class="layover-bar">
        <div class="layover-line"></div>
        <div class="layover-badge">
          <i class="bi bi-hourglass-split"></i>
          Escala de {{ formattedLayover }} en {{ connectionCity }}
        </div>
        <div class="layover-line"></div>
      </div>

      <!-- Leg 2 -->
      <div class="leg-block">
        <div class="leg-row">
          <div class="leg-endpoint">
            <div class="leg-time">{{ leg2.departureTime }}</div>
            <template v-if="getAirport(leg2.origin)">
              <div class="leg-iata-name"><span class="leg-iata">{{ leg2.origin }}</span>, {{ getAirport(leg2.origin).airportName }}</div>
              <div class="leg-location">{{ getAirport(leg2.origin).city }}, {{ getAirport(leg2.origin).country }}</div>
            </template>
            <div class="leg-iata-name" v-else><span class="leg-iata">{{ leg2.origin }}</span></div>
            <div class="leg-date">{{ leg2.date }}</div>
          </div>

          <div class="leg-connector">
            <div class="connector-duration">{{ leg2.durationLabel }}</div>
            <div class="connector-track">
              <div class="track-dot"></div>
              <div class="track-plane"><i class="bi bi-airplane-fill"></i></div>
              <div class="track-dot"></div>
            </div>
          </div>

          <div class="leg-endpoint leg-endpoint--right">
            <div class="leg-time">{{ leg2.arrivalTime }}</div>
            <template v-if="getAirport(leg2.destination)">
              <div class="leg-iata-name"><span class="leg-iata">{{ leg2.destination }}</span>, {{ getAirport(leg2.destination).airportName }}</div>
              <div class="leg-location">{{ getAirport(leg2.destination).city }}, {{ getAirport(leg2.destination).country }}</div>
            </template>
            <div class="leg-iata-name" v-else><span class="leg-iata">{{ leg2.destination }}</span></div>
            <div class="leg-date">{{ leg2.arrivalDate }}</div>
          </div>
        </div>
      </div>
    </template>

    <!-- Select button — shared footer -->
    <div class="result-card-footer">
      <button class="select-btn" @click="$emit('select', selectPayload)">
        <i class="bi bi-cart3 me-1"></i>Seleccionar
      </button>
    </div>

  </div>
</template>

<script>
export default {
  name: 'FlightResultCard',

  props: {
    resultType: {
      type: String,
      required: true,
    },
    directFlight: {
      type: Object,
      default: null,
    },
    leg1: {
      type: Object,
      default: null,
    },
    leg2: {
      type: Object,
      default: null,
    },
    layoverMinutes: {
      type: Number,
      default: 0,
    },
    connectionCity: {
      type: String,
      default: '',
    },
    passengerCount: {
      type: Number,
      default: 1,
    },
    airports: {
      type: Array,
      default: () => [],
    },
  },

  emits: ['select'],

  methods: {
    getAirport(code) {
      return this.airports.find(a => a.code === code) || null
    },
  },

  computed: {
    isStopover() {
      return this.resultType === 'stopover'
    },

    economyPrice() {
      if (!this.isStopover) return this.directFlight.priceEconomy
      return this.leg1.priceEconomy + this.leg2.priceEconomy
    },

    firstClassPrice() {
      if (!this.isStopover) return this.directFlight.priceFirstClass
      return this.leg1.priceFirstClass + this.leg2.priceFirstClass
    },

    formattedLayover() {
      const hours = Math.floor(this.layoverMinutes / 60)
      const minutes = this.layoverMinutes % 60
      return minutes === 0 ? `${hours}h` : `${hours}h ${minutes}m`
    },

    selectPayload() {
      if (!this.isStopover) return this.directFlight
      return {
        type: 'stopover',
        leg1: this.leg1,
        leg2: this.leg2,
        layoverMinutes: this.layoverMinutes,
        connectionCity: this.connectionCity,
      }
    },
  },
}
</script>

<style scoped>
/* ─── Base card ─────────────────────────────────────────── */
.flight-card {
  background: white;
  border-radius: 12px;
  padding: 18px 22px 14px;
  margin-bottom: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.06);
  border: 1px solid transparent;
  transition: box-shadow 0.2s, border-color 0.2s;
}

.flight-card:hover {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.11);
  border-color: #f0f0f0;
}

.flight-card--stopover {
  border-left: 3px solid var(--color-accent-warm);
}

/* ─── Card header: airline + price ───────────────────────── */
.result-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.airline-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.airline-icon {
  width: 30px;
  height: 30px;
  background: #fff0ee;
  border-radius: 7px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
  font-size: 0.85rem;
  flex-shrink: 0;
}

.airline-name {
  font-size: 0.88rem;
  font-weight: 600;
  color: #333;
}

.price-row {
  display: flex;
  align-items: center;
  gap: 10px;
}

.price-item {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 1px;
}

.price-class-label {
  font-size: 0.68rem;
  color: #aaa;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.price-amount {
  font-size: 1.15rem;
  font-weight: 800;
  background: var(--gradient-brand);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.price-total {
  font-size: 0.7rem;
  color: #999;
  font-weight: 500;
  margin-top: 1px;
}

.price-separator {
  color: #ddd;
  font-size: 1.2rem;
  line-height: 1;
  margin-top: 2px;
}

/* ─── Leg row: departure ──── connector ──── arrival ─────── */
.leg-block {
  /* visually groups each leg; no extra margin for stopovers */
}

.leg-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 4px 0;
}

.leg-endpoint {
  min-width: 64px;
}

.leg-endpoint--right {
  text-align: right;
}

.leg-time {
  font-size: 1.3rem;
  font-weight: 700;
  color: #1a1a1a;
  line-height: 1;
}

.leg-iata-name {
  font-size: 0.78rem;
  color: #444;
  margin-top: 4px;
  line-height: 1.3;
}

.leg-iata {
  font-weight: 700;
  color: var(--color-primary);
}

.leg-location {
  font-size: 0.72rem;
  color: #999;
  margin-top: 1px;
  line-height: 1.3;
}

.leg-date {
  font-size: 0.72rem;
  color: #aaa;
  margin-top: 3px;
}

/* ─── Flight track between endpoints ─────────────────────── */
.leg-connector {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.connector-duration {
  font-size: 0.75rem;
  color: #888;
}

/*
  The track container is tall enough to fully contain the plane icon.
  The visible gray line is drawn by ::before so it doesn't constrain height.
*/
.connector-track {
  width: 100%;
  position: relative;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.connector-track::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 0;
  right: 0;
  height: 2px;
  background: #e0e0e0;
  transform: translateY(-50%);
}

.track-dot {
  position: absolute;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #ccc;
  top: 50%;
  transform: translateY(-50%);
  z-index: 1;
}

.track-dot:first-child {
  left: 0;
}

.track-dot:last-child {
  right: 0;
}

.track-plane {
  background: white;
  padding: 0 5px;
  color: var(--color-primary);
  font-size: 0.8rem;
  z-index: 2;
  position: relative;
}

.connector-tag {
  font-size: 0.68rem;
  color: #aaa;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

/* ─── Layover divider between the two legs ───────────────── */
.layover-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 12px 0;
}

.layover-line {
  flex: 1;
  height: 1px;
  background: #ece9e9;
  border-top: 1px dashed #ddd;
}

.layover-badge {
  display: flex;
  align-items: center;
  gap: 6px;
  background: #fff8f2;
  border: 1px solid #fddcc7;
  border-radius: 99px;
  padding: 5px 12px;
  font-size: 0.78rem;
  font-weight: 600;
  color: var(--color-accent-warm);
  white-space: nowrap;
  flex-shrink: 0;
}

.layover-badge i {
  font-size: 0.8rem;
}

/* ─── Card footer: select button ─────────────────────────── */
.result-card-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 14px;
  padding-top: 12px;
  border-top: 1px solid #f4f4f4;
}

.select-btn {
  background: var(--gradient-brand);
  color: white;
  border: none;
  border-radius: 8px;
  padding: 10px 22px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: opacity 0.2s;
}

.select-btn:hover {
  opacity: 0.88;
}
</style>
