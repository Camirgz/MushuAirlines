<template>
  <div class="summary-card" v-if="flight">

    <div class="card-header">
      <div class="route-row">
        <span class="airport">{{ flight.origin }}</span>
        <i class="bi bi-airplane-fill plane-icon"></i>
        <span class="airport">{{ flight.destination }}</span>
      </div>
      <span class="card-label">Resumen de compra</span>
    </div>

    <div class="card-body">

      <div class="info-group">
        <div class="info-item">
          <i class="bi bi-calendar3 item-icon"></i>
          <span>{{ formattedDate }}</span>
        </div>
        <div class="info-item" v-if="flight.code">
          <i class="bi bi-tag item-icon"></i>
          <span>Ruta {{ flight.code }}</span>
        </div>
      </div>

      <div class="divider"></div>

      <div class="info-group">
        <div class="group-label">Pasajeros</div>
        <div class="info-item">
          <i class="bi bi-people-fill item-icon"></i>
          <span>{{ seats.length }} pasajero{{ seats.length !== 1 ? 's' : '' }}</span>
        </div>
        <div class="info-sub" v-if="fcCount > 0">
          <i class="bi bi-award-fill sub-icon sub-icon--accent"></i>
          <span>{{ fcCount }} × Primera Clase</span>
        </div>
        <div class="info-sub" v-if="ecCount > 0">
          <i class="bi bi-person-fill sub-icon"></i>
          <span>{{ ecCount }} × Turista</span>
        </div>
      </div>

      <template v-if="hasBaggage">
        <div class="divider"></div>
        <div class="info-group">
          <div class="group-label">Equipaje</div>
          <div class="info-sub" v-if="totalHandCount > 0">
            <i class="bi bi-briefcase-fill sub-icon"></i>
            <span>{{ totalHandCount }} × Mano</span>
            <span v-if="flight.handBagWeight" class="detail-hint">máx. {{ flight.handBagWeight }} kg</span>
          </div>
          <div class="info-sub" v-if="totalCheckedCount > 0">
            <i class="bi bi-archive-fill sub-icon"></i>
            <span>{{ totalCheckedCount }} × Documentado</span>
            <span v-if="flight.bagWeight" class="detail-hint">máx. {{ flight.bagWeight }} kg</span>
          </div>
        </div>
      </template>

      <div class="divider"></div>

      <div class="info-group">
        <div class="group-label">Desglose</div>
        <div class="price-item" v-if="fcCount > 0">
          <span>{{ fcCount }} × Primera Clase</span>
          <span class="price-val">${{ fcTotal.toLocaleString() }}</span>
        </div>
        <div class="price-item" v-if="ecCount > 0">
          <span>{{ ecCount }} × Turista</span>
          <span class="price-val">${{ ecTotal.toLocaleString() }}</span>
        </div>

        <!-- Direct flight baggage -->
        <template v-if="!flight.isStopover">
          <div class="price-item" v-if="totalHandCount > 0">
            <span>{{ totalHandCount }} × Mano</span>
            <span class="price-val">${{ handBagTotal.toLocaleString() }}</span>
          </div>
          <div class="price-item" v-if="totalCheckedCount > 0">
            <span>{{ totalCheckedCount }} × Documentado</span>
            <span class="price-val">${{ checkedBagTotal.toLocaleString() }}</span>
          </div>
        </template>

        <!-- Stopover baggage per leg -->
        <template v-if="flight.isStopover && hasBaggage">
          <div class="price-section-label">Vuelo 1</div>
          <div class="price-item" v-if="totalHandCount > 0">
            <span class="price-item--indented">{{ totalHandCount }} × Mano</span>
            <span class="price-val">${{ leg1HandBagTotal.toLocaleString() }}</span>
          </div>
          <div class="price-item" v-if="totalCheckedCount > 0">
            <span class="price-item--indented">{{ totalCheckedCount }} × Documentado</span>
            <span class="price-val">${{ leg1CheckedBagTotal.toLocaleString() }}</span>
          </div>
          <div class="price-section-label">Vuelo 2</div>
          <div class="price-item" v-if="totalHandCount > 0">
            <span class="price-item--indented">{{ totalHandCount }} × Mano</span>
            <span class="price-val">${{ leg2HandBagTotal.toLocaleString() }}</span>
          </div>
          <div class="price-item" v-if="totalCheckedCount > 0">
            <span class="price-item--indented">{{ totalCheckedCount }} × Documentado</span>
            <span class="price-val">${{ leg2CheckedBagTotal.toLocaleString() }}</span>
          </div>
        </template>
      </div>

    </div>

    <div class="card-footer">
      <span class="total-label">Total estimado</span>
      <span class="total-amount">${{ total.toLocaleString() }}</span>
    </div>

  </div>
</template>

<script>
const MONTHS = ['ene.', 'feb.', 'mar.', 'abr.', 'may.', 'jun.',
                'jul.', 'ago.', 'sep.', 'oct.', 'nov.', 'dic.'];

export default {
  name: 'PurchaseSummaryCard',

  props: {
    flight:     { type: Object,  required: true },
    flight2:    { type: Object,  default: null },
    seats:      { type: Array,   default: () => [] },
    passengers: { type: Array,   default: () => [] },
  },

  computed: {
    fcCount() {
      return this.seats.filter(s => s.seatClass === 'FirstClass').length;
    },
    ecCount() {
      return this.seats.filter(s => s.seatClass === 'Economy').length;
    },
    totalHandCount() {
      return this.passengers.reduce((sum, p) => sum + (p.handBagCount || 0), 0);
    },
    totalCheckedCount() {
      return this.passengers.reduce((sum, p) => sum + (p.checkedBagCount || 0), 0);
    },
    hasBaggage() {
      return this.totalHandCount > 0 || this.totalCheckedCount > 0;
    },
    fcTotal() {
      return this.fcCount * (this.flight.priceFirstClass || 0);
    },
    ecTotal() {
      return this.ecCount * (this.flight.priceEconomy || 0);
    },
    // Direct flight totals
    handBagTotal() {
      const price      = this.flight.handBagPrice  || 0;
      const multiplier = this.flight.bagMultiplier || 1;
      return this.passengers.reduce((sum, p) => sum + this.bagSubtotal(p.handBagCount || 0, price, multiplier), 0);
    },
    checkedBagTotal() {
      const price      = this.flight.bagPrice      || 0;
      const multiplier = this.flight.bagMultiplier || 1;
      return this.passengers.reduce((sum, p) => sum + this.bagSubtotal(p.checkedBagCount || 0, price, multiplier), 0);
    },
    // Stopover leg 1 totals
    leg1HandBagTotal() {
      const price      = this.flight.leg1HandBagPrice  || 0;
      const multiplier = this.flight.leg1BagMultiplier || 1;
      return this.passengers.reduce((sum, p) => sum + this.bagSubtotal(p.handBagCount || 0, price, multiplier), 0);
    },
    leg1CheckedBagTotal() {
      const price      = this.flight.leg1BagPrice      || 0;
      const multiplier = this.flight.leg1BagMultiplier || 1;
      return this.passengers.reduce((sum, p) => sum + this.bagSubtotal(p.checkedBagCount || 0, price, multiplier), 0);
    },
    // Stopover leg 2 totals
    leg2HandBagTotal() {
      const price      = this.flight2?.handBagPrice  || 0;
      const multiplier = this.flight2?.bagMultiplier || 1;
      return this.passengers.reduce((sum, p) => sum + this.bagSubtotal(p.handBagCount || 0, price, multiplier), 0);
    },
    leg2CheckedBagTotal() {
      const price      = this.flight2?.bagPrice      || 0;
      const multiplier = this.flight2?.bagMultiplier || 1;
      return this.passengers.reduce((sum, p) => sum + this.bagSubtotal(p.checkedBagCount || 0, price, multiplier), 0);
    },
    total() {
      if (this.flight.isStopover) {
        return this.fcTotal + this.ecTotal
          + this.leg1HandBagTotal + this.leg1CheckedBagTotal
          + this.leg2HandBagTotal + this.leg2CheckedBagTotal;
      }
      return this.fcTotal + this.ecTotal + this.handBagTotal + this.checkedBagTotal;
    },
    formattedDate() {
      if (!this.flight?.flightDate) return '';
      const parts = this.flight.flightDate.split('-');
      if (parts.length !== 3) return this.flight.flightDate;
      const [year, month, day] = parts;
      return `${parseInt(day)} ${MONTHS[parseInt(month) - 1]} ${year}`;
    },
  },

  methods: {
    bagSubtotal(count, unitPrice, multiplier) {
      if (count === 0) return 0;
      if (count === 1) return unitPrice;
      return unitPrice + (count - 1) * unitPrice * multiplier;
    },
  },
};
</script>

<style scoped>
.summary-card {
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.10);
  overflow: hidden;
  font-size: 0.875rem;
  color: #1a1a1a;
}

/* ── Header ── */
.card-header {
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  padding: 18px 20px 14px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.route-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.airport {
  font-size: 1.3rem;
  font-weight: 900;
  color: #fff;
  letter-spacing: 0.03em;
}

.plane-icon {
  color: rgba(255, 255, 255, 0.85);
  font-size: 0.95rem;
  flex-shrink: 0;
}

.card-label {
  font-size: 0.7rem;
  color: rgba(255, 255, 255, 0.8);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

/* ── Body ── */
.card-body {
  padding: 16px 20px;
  display: flex;
  flex-direction: column;
}

.divider {
  height: 1px;
  background: #f0f0f0;
  margin: 12px 0;
}

.info-group {
  display: flex;
  flex-direction: column;
  gap: 7px;
}

.group-label {
  font-size: 0.68rem;
  font-weight: 800;
  color: #aaa;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  margin-bottom: 1px;
}

.info-item {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #374151;
  font-weight: 500;
}

.item-icon {
  color: #e74c3c;
  font-size: 0.82rem;
  width: 14px;
  flex-shrink: 0;
}

.info-sub {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #555;
  padding-left: 22px;
  font-weight: 400;
}

.sub-icon {
  font-size: 0.78rem;
  color: #bbb;
  width: 12px;
  flex-shrink: 0;
}

.sub-icon--accent {
  color: #f39c12;
}

.detail-hint {
  font-size: 0.7rem;
  color: #bbb;
  margin-left: auto;
  white-space: nowrap;
}

/* ── Price breakdown ── */
.price-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
  color: #555;
  padding-left: 4px;
}

.price-val {
  font-weight: 700;
  color: #374151;
  white-space: nowrap;
}

.price-section-label {
  font-size: 0.68rem;
  font-weight: 700;
  color: #e74c3c;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  margin-top: 6px;
  padding-left: 4px;
}

.price-item--indented {
  padding-left: 8px;
}

/* ── Footer / Total ── */
.card-footer {
  border-top: 1.5px solid #f0f0f0;
  padding: 14px 20px;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.total-label {
  font-size: 0.68rem;
  color: #aaa;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  font-weight: 700;
}

.total-amount {
  font-size: 1.5rem;
  font-weight: 900;
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}
</style>
