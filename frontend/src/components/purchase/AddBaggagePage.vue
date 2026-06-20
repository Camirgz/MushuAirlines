<template>
  <div>
    <AdminPageLayout>

      <AdminHero
        title="Agregar Maletas Documentadas"
        subtitle="Agrega maletas documentadas a tu compra. Solo puedes aumentar la cantidad por pasajero."
        icon="bi bi-archive-fill"
        back-to="/my-reservation/report"
        back-text="Volver a mi reserva"
      />

      <div v-if="loading" class="loading-state">
        <i class="bi bi-arrow-repeat spin me-2"></i> Cargando información de la compra...
      </div>

      <div v-else-if="error" class="error-box">
        <i class="bi bi-exclamation-circle-fill me-2"></i>{{ error }}
      </div>

      <template v-else-if="purchase">

      <!-- Flight info banner -->
      <AdminCard>
        <div class="flight-banner">
          <div class="flight-route">
            <span class="route-airport">{{ bannerOrigin }}</span>
            <i class="bi bi-airplane-fill route-plane"></i>
            <span class="route-airport">{{ bannerDestination }}</span>
          </div>
          <div class="flight-meta">
            <span class="meta-chip">
              <i class="bi bi-calendar3"></i>
              {{ bannerDate }}
            </span>
            <span class="meta-chip">
              <i class="bi bi-tag"></i>
              {{ bannerFlightNumber }}
            </span>
            <span class="meta-chip">
              <i class="bi bi-receipt"></i>
              {{ bannerInvoiceNumber }}
            </span>
            <span class="meta-chip meta-chip--price">
              <i class="bi bi-archive-fill"></i>
              ${{ BAG_PRICE }} / maleta documentada
            </span>
          </div>
        </div>
      </AdminCard>

      <!-- Content grid -->
      <div class="content-grid">

        <!-- Passengers column -->
        <div class="passengers-col">
          <AdminCard>
            <h2 class="section-heading">
              <i class="bi bi-people-fill"></i>
              Pasajeros
            </h2>

            <div class="passenger-list">
              <div
                v-for="(passenger, idx) in passengers"
                :key="idx"
                class="passenger-row"
                :class="{ 'passenger-row--active': passenger.extraBags > 0 }"
              >
                <div class="passenger-left">
                  <div class="passenger-avatar">
                    <i class="bi bi-person-fill"></i>
                  </div>
                  <div class="passenger-details">
                    <span class="passenger-name">{{ passenger.name }}</span>
                    <span class="passenger-seat-class">{{ passenger.seatClass }}</span>
                  </div>
                </div>

                <div class="passenger-right">
                  <div class="current-bags-badge">
                    <i class="bi bi-archive-fill"></i>
                    <span>{{ passenger.currentBags }} actuales</span>
                  </div>

                  <div class="bag-counter">
                    <button
                      class="counter-btn counter-btn--minus"
                      :disabled="passenger.extraBags === 0"
                      @click="removeBag(idx)"
                      aria-label="Quitar maleta"
                    >
                      <i class="bi bi-dash-lg"></i>
                    </button>

                    <div class="counter-display">
                      <span class="counter-extra">+{{ passenger.extraBags }}</span>
                      <span class="counter-label">nueva{{ passenger.extraBags !== 1 ? 's' : '' }}</span>
                    </div>

                    <button
                      class="counter-btn counter-btn--plus"
                      @click="addBag(idx)"
                      aria-label="Agregar maleta"
                    >
                      <i class="bi bi-plus-lg"></i>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </AdminCard>
        </div>

        <!-- Summary sidebar -->
        <div class="summary-col">
          <div class="summary-sticky">
            <AdminCard>
              <h2 class="section-heading">
                <i class="bi bi-receipt"></i>
                Resumen de cargo
              </h2>

              <template v-if="totalExtraBags > 0">
                <div class="summary-list">
                  <div
                    v-for="(passenger, idx) in passengersWithExtra"
                    :key="idx"
                    class="summary-row"
                  >
                    <div class="summary-row-info">
                      <span class="summary-name">{{ passenger.name }}</span>
                      <span class="summary-qty">
                        {{ passenger.extraBags }} maleta{{ passenger.extraBags !== 1 ? 's' : '' }}
                        × ${{ BAG_PRICE }}
                      </span>
                    </div>
                    <span class="summary-subtotal">
                      ${{ (passenger.extraBags * BAG_PRICE).toLocaleString() }}
                    </span>
                  </div>
                </div>
              </template>

              <div v-else class="summary-empty">
                <i class="bi bi-archive"></i>
                <span>Aún no has seleccionado maletas adicionales.</span>
              </div>

              <div class="summary-divider"></div>

              <template v-if="!showPaymentForm">
                <div class="total-block">
                  <span class="total-label">Total a pagar</span>
                  <span class="total-amount">${{ totalToPay.toLocaleString() }}</span>
                </div>

                <button
                  class="pay-btn"
                  :disabled="totalExtraBags === 0"
                  @click="handlePay"
                >
                  <i class="bi bi-credit-card-fill"></i>
                  Pagar maletas adicionales
                </button>

                <p class="pay-hint" v-if="totalExtraBags === 0">
                  Agrega al menos una maleta para continuar.
                </p>
              </template>

              <CardPaymentForm
                v-else
                :total="totalToPay"
                :paying="paying"
                :api-error="paymentApiError"
                @submit="handleCardSubmit"
                @cancel="showPaymentForm = false"
              />
            </AdminCard>
          </div>
        </div>

      </div>

      </template>

    </AdminPageLayout>
  </div>
</template>

<script>
import AdminPageLayout from '@/components/layout/AdminPageLayout.vue';
import AdminHero from '@/components/admin/ui/AdminHero.vue';
import AdminCard from '@/components/admin/ui/AdminCard.vue';
import { getPurchaseData, validatePayment } from '@/services/PurchaseService';
import { addBaggage } from '@/services/ReservationService';
import CardPaymentForm from '@/components/purchase/CardPaymentForm.vue';

const BAG_PRICE = 35;

export default {
  name: 'AddBaggagePage',

  components: { AdminPageLayout, AdminHero, AdminCard, CardPaymentForm },

  data() {
    return {
      BAG_PRICE,
      purchase: null,
      passengerExtras: [],
      loading: true,
      error: null,
      paying: false,
      showPaymentForm: false,
      paymentApiError: null,
    };
  },

  computed: {
    purchaseId() {
      return Number(this.$route.params.id);
    },
    passengers() {
      if (!this.purchase) return [];
      return this.purchase.tickets.map((ticket, idx) => {
        const detail = (this.purchase.passengerBaggageDetails ?? [])
          .find(d => d.passengerFullName === ticket.passengerFullName);
        return {
          name: ticket.passengerFullName,
          seatClass: this.translateClass(ticket.seatClass),
          currentBags: detail?.checkedBagCount ?? 0,
          extraBags: this.passengerExtras[idx] ?? 0,
        };
      });
    },
    passengersWithExtra() {
      return this.passengers.filter(p => p.extraBags > 0);
    },
    totalExtraBags() {
      return this.passengers.reduce((sum, p) => sum + p.extraBags, 0);
    },
    totalToPay() {
      return this.totalExtraBags * BAG_PRICE;
    },
    bannerOrigin() {
      return this.purchase?.originAirport ?? '—';
    },
    bannerDestination() {
      return this.purchase?.destinationAirport ?? '—';
    },
    bannerDate() {
      if (!this.purchase?.departureDate) return '—';
      return new Date(this.purchase.departureDate).toLocaleDateString('es-CR', {
        day: 'numeric', month: 'short', year: 'numeric',
      });
    },
    bannerFlightNumber() {
      return this.purchase?.flightNumber ?? '—';
    },
    bannerInvoiceNumber() {
      return this.purchase?.invoiceNumber ?? '—';
    },
  },

  async mounted() {
    try {
      this.purchase = await getPurchaseData(this.purchaseId);
      this.passengerExtras = new Array(this.purchase.tickets.length).fill(0);
    } catch (err) {
      this.error = err.message ?? 'No se pudo cargar la información de la compra.';
    } finally {
      this.loading = false;
    }
  },

  methods: {
    translateClass(seatClass) {
      if (seatClass === 'FirstClass') return 'Primera Clase';
      if (seatClass === 'Economy')    return 'Clase Turista';
      return seatClass ?? '';
    },

    addBag(idx) {
      this.passengerExtras[idx]++;
    },
    removeBag(idx) {
      if (this.passengerExtras[idx] > 0) {
        this.passengerExtras[idx]--;
      }
    },

    handlePay() {
      this.paymentApiError = null;
      this.showPaymentForm = true;
    },

    async handleCardSubmit(cardData) {
      this.paying = true;
      this.paymentApiError = null;
      try {
        await validatePayment(cardData);
      } catch (err) {
        this.paymentApiError = err.message ?? 'Tarjeta rechazada.';
        this.paying = false;
        return;
      }
      try {
        const payload = this.passengersWithExtra.map(p => ({
          passengerFullName: p.name,
          extraBags: p.extraBags,
        }));
        await addBaggage(payload);
        this.$router.push('/my-reservation/report');
      } catch (err) {
        this.paymentApiError = err.message ?? 'Error al procesar el pago de maletas.';
      } finally {
        this.paying = false;
      }
    },
  },
};
</script>

<style scoped>
/* ── Flight banner ── */
.flight-banner {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.flight-route {
  display: flex;
  align-items: center;
  gap: 12px;
}

.route-airport {
  font-size: 1.6rem;
  font-weight: 900;
  color: #111827;
  letter-spacing: 0.03em;
}

.route-plane {
  font-size: 1.1rem;
  color: #e74c3c;
}

.flight-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.meta-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: #f3f4f6;
  color: #374151;
  font-size: 0.8rem;
  font-weight: 600;
  padding: 5px 11px;
  border-radius: 999px;
}

.meta-chip i {
  color: #9ca3af;
  font-size: 0.75rem;
}

.meta-chip--price {
  background: #fff4ed;
  color: #c0392b;
}

.meta-chip--price i {
  color: #e74c3c;
}

/* ── Content grid ── */
.content-grid {
  display: grid;
  grid-template-columns: 1fr 340px;
  gap: 24px;
  margin-top: 24px;
  align-items: start;
}

/* ── Section heading ── */
.section-heading {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1rem;
  font-weight: 800;
  color: #111827;
  margin: 0 0 22px;
}

.section-heading i {
  color: #e74c3c;
  font-size: 1rem;
}

/* ── Passenger rows ── */
.passenger-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.passenger-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 16px 18px;
  border-radius: 12px;
  border: 1.5px solid #e5e7eb;
  background: #fafafa;
  transition: border-color 0.2s, background 0.2s, box-shadow 0.2s;
}

.passenger-row--active {
  border-color: #e74c3c;
  background: #fff9f9;
  box-shadow: 0 2px 12px rgba(231, 76, 60, 0.10);
}

.passenger-left {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.passenger-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: linear-gradient(135deg, #e74c3c, #f39c12);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 1rem;
  flex-shrink: 0;
}

.passenger-details {
  display: flex;
  flex-direction: column;
  gap: 3px;
  min-width: 0;
}

.passenger-name {
  font-weight: 700;
  font-size: 0.9rem;
  color: #111827;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.passenger-seat-class {
  font-size: 0.72rem;
  font-weight: 600;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

.passenger-right {
  display: flex;
  align-items: center;
  gap: 14px;
  flex-shrink: 0;
}

.current-bags-badge {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 0.75rem;
  font-weight: 600;
  color: #6b7280;
  white-space: nowrap;
}

.current-bags-badge i {
  font-size: 0.72rem;
}

/* ── Bag counter ── */
.bag-counter {
  display: flex;
  align-items: center;
  gap: 8px;
}

.counter-btn {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 0.85rem;
  transition: background 0.15s, transform 0.1s, opacity 0.15s;
  flex-shrink: 0;
}

.counter-btn--plus {
  background: linear-gradient(135deg, #e74c3c, #f39c12);
  color: #fff;
  box-shadow: 0 4px 12px rgba(231, 76, 60, 0.28);
}

.counter-btn--plus:hover {
  transform: scale(1.08);
  box-shadow: 0 6px 16px rgba(231, 76, 60, 0.36);
}

.counter-btn--minus {
  background: #f3f4f6;
  color: #6b7280;
}

.counter-btn--minus:hover:not(:disabled) {
  background: #e5e7eb;
  color: #374151;
}

.counter-btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.counter-display {
  width: 44px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1px;
}

.counter-extra {
  font-size: 1.1rem;
  font-weight: 900;
  color: #e74c3c;
  line-height: 1;
}

.counter-label {
  font-size: 0.6rem;
  font-weight: 700;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

/* ── Summary sidebar ── */
.summary-sticky {
  position: sticky;
  top: 82px;
}

.summary-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 4px;
}

.summary-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.summary-row-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.summary-name {
  font-size: 0.82rem;
  font-weight: 700;
  color: #374151;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.summary-qty {
  font-size: 0.72rem;
  color: #9ca3af;
}

.summary-subtotal {
  font-size: 0.88rem;
  font-weight: 800;
  color: #111827;
  white-space: nowrap;
  flex-shrink: 0;
}

.summary-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  padding: 24px 0 20px;
  color: #9ca3af;
  font-size: 0.82rem;
  text-align: center;
}

.summary-empty i {
  font-size: 2rem;
  color: #d1d5db;
}

.summary-divider {
  height: 1.5px;
  background: #f0f0f0;
  margin: 18px 0 14px;
}

/* ── Total block ── */
.total-block {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  margin-bottom: 20px;
}

.total-label {
  font-size: 0.72rem;
  font-weight: 800;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

.total-amount {
  font-size: 1.9rem;
  font-weight: 900;
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

/* ── Pay button ── */
.pay-btn {
  width: 100%;
  padding: 14px 20px;
  border: none;
  border-radius: 10px;
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  color: #fff;
  font-size: 0.95rem;
  font-weight: 800;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  box-shadow: 0 8px 20px rgba(231, 76, 60, 0.28);
  transition: opacity 0.15s, transform 0.15s, box-shadow 0.15s;
}

.pay-btn:hover:not(:disabled) {
  opacity: 0.92;
  transform: translateY(-1px);
  box-shadow: 0 10px 24px rgba(231, 76, 60, 0.36);
}

.pay-btn:disabled {
  background: #d1d5db;
  color: #9ca3af;
  box-shadow: none;
  cursor: not-allowed;
}

.pay-hint {
  margin: 12px 0 0;
  font-size: 0.75rem;
  color: #9ca3af;
  text-align: center;
}

/* ── Loading / error states ── */
.loading-state {
  text-align: center;
  padding: 40px;
  color: #888;
  font-size: 0.95rem;
}

.spin {
  display: inline-block;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-box {
  background: #fee2e2;
  color: #991b1b;
  padding: 14px 18px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.9rem;
  margin-bottom: 16px;
}

/* ── Responsive ── */
@media (max-width: 768px) {
  .content-grid {
    grid-template-columns: 1fr;
  }

  .summary-sticky {
    position: static;
  }

  .passenger-row {
    flex-direction: column;
    align-items: flex-start;
    gap: 14px;
  }

  .passenger-right {
    width: 100%;
    justify-content: space-between;
  }

  .flight-banner {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
