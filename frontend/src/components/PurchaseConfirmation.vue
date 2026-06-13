<template>
  <div class="confirmation-root">

  <canvas ref="confettiCanvas" class="confetti-canvas" />
  <AdminPageLayout>

    <AdminHero
      title="¡Compra realizada con éxito!"
      subtitle="Hemos enviado un correo de confirmación con toda la información de tu reservación."
      icon="bi bi-check-circle-fill"
    />

    <div v-if="loading" class="loading-state">
      <i class="bi bi-arrow-repeat spin me-2"></i> Cargando información de la compra...
    </div>

    <div v-else-if="loadError" class="error-box" style="margin-bottom: 16px;">
      <i class="bi bi-exclamation-circle-fill me-2"></i>{{ loadError }}
    </div>

    <template v-else-if="purchase">

      <AdminCard>

        <div class="email-bar">
          <i class="bi bi-envelope-check-fill me-2"></i>
          Confirmación enviada a: <strong>{{ purchase.email }}</strong>
        </div>

        <div class="reservation-block">
          <span class="reservation-label">Código de Reserva</span>
          <span class="reservation-code">{{ purchase.reservationCode }}</span>
        </div>

        <div class="details-grid">

          <div class="detail-item">
            <span class="detail-label"><i class="bi bi-receipt me-1"></i>Número de Factura</span>
            <strong class="detail-value">{{ purchase.invoiceNumber }}</strong>
          </div>

          <div class="detail-item">
            <span class="detail-label"><i class="bi bi-credit-card me-1"></i>Método de Pago</span>
            <strong class="detail-value">{{ purchase.paymentMethod }}</strong>
          </div>

          <div class="detail-item">
            <span class="detail-label"><i class="bi bi-person me-1"></i>Titular</span>
            <strong class="detail-value">{{ purchase.fullName }}</strong>
          </div>

        </div>

        <!-- Leg 1 -->
        <div class="flight-leg-block">
          <div class="flight-leg-title">
            <i class="bi bi-airplane-fill me-2"></i>
            {{ purchase.flightNumber2 ? 'Vuelo 1' : 'Vuelo' }}
          </div>
          <div class="details-grid">
            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-geo-alt me-1"></i>Ruta</span>
              <strong class="detail-value">{{ purchase.originAirport }} → {{ purchase.destinationAirport }}</strong>
            </div>
            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-calendar3 me-1"></i>Fecha de salida</span>
              <strong class="detail-value">{{ formatDate(purchase.departureDate) }}</strong>
            </div>
            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-airplane me-1"></i>Aeronave</span>
              <strong class="detail-value">{{ purchase.aircraftType }}</strong>
            </div>
            <div class="detail-item" v-if="purchase.aircraftModel">
              <span class="detail-label"><i class="bi bi-tools me-1"></i>Modelo</span>
              <strong class="detail-value">{{ purchase.aircraftModel }}</strong>
            </div>
          </div>
        </div>

        <!-- Leg 2 (stopover only) -->
        <div class="flight-leg-block flight-leg-block--stopover" v-if="purchase.flightNumber2">
          <div class="flight-leg-title">
            <i class="bi bi-airplane-fill me-2"></i>
            Vuelo 2
          </div>
          <div class="details-grid">
            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-geo-alt me-1"></i>Ruta</span>
              <strong class="detail-value">{{ purchase.originAirport2 }} → {{ purchase.destinationAirport2 }}</strong>
            </div>
            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-calendar3 me-1"></i>Fecha de salida</span>
              <strong class="detail-value">{{ formatDate(purchase.departureDate2) }}</strong>
            </div>
            <div class="detail-item" v-if="purchase.aircraftType2">
              <span class="detail-label"><i class="bi bi-airplane me-1"></i>Aeronave</span>
              <strong class="detail-value">{{ purchase.aircraftType2 }}</strong>
            </div>
            <div class="detail-item" v-if="purchase.aircraftModel2">
              <span class="detail-label"><i class="bi bi-tools me-1"></i>Modelo</span>
              <strong class="detail-value">{{ purchase.aircraftModel2 }}</strong>
            </div>
          </div>
        </div>

        <div class="breakdown-section" v-if="purchase.details && purchase.details.length">
          <div class="breakdown-title">Desglose de asientos</div>
          <div class="breakdown-row breakdown-row--header">
            <span>Clase</span>
            <span>Asientos</span>
            <span>Subtotal</span>
          </div>
          <div
            class="breakdown-row"
            v-for="detail in purchase.details"
            :key="detail.seatClass"
          >
            <span>{{ translateClass(detail.seatClass) }}</span>
            <span>{{ detail.seatCount }}</span>
            <span>${{ detail.subtotal.toLocaleString() }}</span>
          </div>
        </div>

        <div class="breakdown-section" v-if="purchase.passengerBaggageDetails && purchase.passengerBaggageDetails.length">
          <div class="breakdown-title">Equipaje por pasajero</div>
          <div class="bpp-table">
            <div class="bpp-row bpp-row--header">
              <span class="bpp-col-name">Pasajero</span>
              <span class="bpp-col-num">Mano</span>
              <span class="bpp-col-num">Documentado</span>
              <span class="bpp-col-amount">Subtotal</span>
            </div>
            <div
              class="bpp-row"
              v-for="pb in purchase.passengerBaggageDetails"
              :key="pb.passengerFullName"
            >
              <span class="bpp-col-name">{{ pb.passengerFullName }}</span>
              <span class="bpp-col-num">{{ pb.handBagCount }}</span>
              <span class="bpp-col-num">{{ pb.checkedBagCount }}</span>
              <span class="bpp-col-amount">${{ pb.baggageSubtotal.toLocaleString() }}</span>
            </div>
            <div class="bpp-row bpp-row--total">
              <span class="bpp-col-name">Total equipaje</span>
              <span class="bpp-col-num">{{ totalHandBags }}</span>
              <span class="bpp-col-num">{{ totalCheckedBags }}</span>
              <span class="bpp-col-amount">${{ totalBaggageSubtotal.toLocaleString() }}</span>
            </div>
          </div>
        </div>

        <div class="breakdown-section">
          <div class="breakdown-row breakdown-row--total">
            <span>Total pagado</span>
            <span class="total-amount">${{ purchase.totalPaid.toLocaleString() }}</span>
          </div>
        </div>

      </AdminCard>

      <AdminCard>

        <div
          v-if="emailMessage"
          :class="emailIsError ? 'alert-box alert-box--error' : 'alert-box alert-box--success'"
        >
          <i :class="emailIsError ? 'bi bi-x-circle-fill' : 'bi bi-check-circle-fill'" class="me-2"></i>
          {{ emailMessage }}
        </div>

        <div class="bottom-box">
          <h4>¿No recibiste el correo?</h4>
          <p>Revisa tu carpeta de spam o solicita un reenvío.</p>

          <div class="buttons-container">
            <button
              class="btn-outline-custom"
              @click="resendEmail"
              :disabled="resending"
            >
              <i class="bi bi-send me-1"></i>
              {{ resending ? 'Reenviando...' : 'Reenviar correo' }}
            </button>

            <RouterLink to="/" class="btn-gradient-custom">
              <i class="bi bi-house me-1"></i>Volver al inicio
            </RouterLink>
          </div>
        </div>

      </AdminCard>

    </template>

  </AdminPageLayout>

  </div>
</template>

<script>
import AdminPageLayout from '@/components/layout/AdminPageLayout.vue';
import AdminHero       from '@/components/admin/ui/AdminHero.vue';
import AdminCard       from '@/components/admin/ui/AdminCard.vue';
import { getPurchaseData, sendConfirmation } from '@/services/PurchaseService';

export default {
  name: 'PurchaseConfirmation',

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
      loading:      true,
      loadError:    null,
      purchase:     null,
      resending:    false,
      emailMessage: null,
      emailIsError: false,
    };
  },

  computed: {
    totalHandBags() {
      return (this.purchase?.passengerBaggageDetails || [])
        .reduce((sum, p) => sum + (p.handBagCount || 0), 0);
    },
    totalCheckedBags() {
      return (this.purchase?.passengerBaggageDetails || [])
        .reduce((sum, p) => sum + (p.checkedBagCount || 0), 0);
    },
    totalBaggageSubtotal() {
      return (this.purchase?.passengerBaggageDetails || [])
        .reduce((sum, p) => sum + (p.baggageSubtotal || 0), 0);
    },
  },

  beforeUnmount() {
    if (this._cancelConfetti) this._cancelConfetti();
  },

  async mounted() {
    this.launchCelebration();

    const purchaseId = parseInt(this.$route.params.id);
    try {
      this.purchase = await getPurchaseData(purchaseId);
    } catch (err) {
      this.loadError = err.message ?? 'Error al cargar la información de la compra.';
    } finally {
      this.loading = false;
    }
  },

  methods: {
    formatDate(isoString) {
      if (!isoString) return '';
      const d = new Date(isoString);
      return d.toLocaleDateString('es-CR', { year: 'numeric', month: 'long', day: 'numeric' });
    },

    translateClass(seatClass) {
      if (seatClass === 'FirstClass') return 'Primera Clase';
      if (seatClass === 'Economy')    return 'Clase Turista';
      return seatClass;
    },

    translateBaggageType(baggageType) {
      if (baggageType === 'HandBaggage')    return 'Equipaje de Mano';
      if (baggageType === 'CheckedBaggage') return 'Equipaje Documentado';
      return baggageType;
    },

    launchCelebration() {
      this.$nextTick(() => {
        this.launchConfetti();
      });
    },

    launchConfetti() {
      const canvas = this.$refs.confettiCanvas;
      if (!canvas) return;
      const ctx = canvas.getContext('2d');

      const resize = () => {
        canvas.width  = window.innerWidth;
        canvas.height = window.innerHeight;
      };
      resize();

      const COLORS = [
        '#e74c3c', '#f39c12', '#3498db', '#2ecc71',
        '#9b59b6', '#e67e22', '#f1c40f', '#1abc9c',
        '#e91e8c', '#ffffff',
      ];

      const rand = (min, max) => Math.random() * (max - min) + min;

      const pieces = Array.from({ length: 160 }, () => ({
        x:     rand(0, window.innerWidth),
        y:     rand(-window.innerHeight * 0.8, -10),
        w:     rand(6, 14),
        h:     rand(3, 8),
        color: COLORS[Math.floor(Math.random() * COLORS.length)],
        angle: rand(0, Math.PI * 2),
        spin:  rand(-0.12, 0.12),
        vx:    rand(-2, 2),
        vy:    rand(1.5, 4),
        shape: Math.random() > 0.5 ? 'rect' : 'circle',
      }));

      const ACTIVE_MS = 4000;
      const FADE_MS   = 1500;
      const start     = Date.now();
      let   rafId     = null;

      const draw = () => {
        const elapsed = Date.now() - start;

        if (elapsed > ACTIVE_MS + FADE_MS) {
          ctx.clearRect(0, 0, canvas.width, canvas.height);
          canvas.style.display = 'none';
          return;
        }

        ctx.clearRect(0, 0, canvas.width, canvas.height);
        const alpha = elapsed > ACTIVE_MS
          ? 1 - (elapsed - ACTIVE_MS) / FADE_MS
          : 1;

        pieces.forEach(p => {
          p.x     += p.vx;
          p.y     += p.vy;
          p.angle += p.spin;

          if (p.y > canvas.height + 20 && elapsed < ACTIVE_MS) {
            p.y  = rand(-80, -10);
            p.x  = rand(0, canvas.width);
            p.vy = rand(1.5, 4);
          }

          ctx.save();
          ctx.globalAlpha = alpha;
          ctx.translate(p.x, p.y);
          ctx.rotate(p.angle);
          ctx.fillStyle = p.color;

          if (p.shape === 'circle') {
            ctx.beginPath();
            ctx.arc(0, 0, p.w / 2, 0, Math.PI * 2);
            ctx.fill();
          } else {
            ctx.fillRect(-p.w / 2, -p.h / 2, p.w, p.h);
          }

          ctx.restore();
        });

        rafId = requestAnimationFrame(draw);
      };

      rafId = requestAnimationFrame(draw);

      this._cancelConfetti = () => {
        if (rafId) cancelAnimationFrame(rafId);
      };
    },

    async resendEmail() {
      if (this.resending) return;
      this.resending    = true;
      this.emailMessage = null;
      try {
        await sendConfirmation(this.purchase.purchaseId);
        this.emailMessage = 'Correo reenviado correctamente.';
        this.emailIsError = false;
      } catch {
        this.emailMessage = 'No se pudo reenviar el correo. Intente nuevamente.';
        this.emailIsError = true;
      } finally {
        this.resending = false;
      }
    },
  },
};
</script>

<style scoped>

.confirmation-root {
  position: relative;
}

.confetti-canvas {
  position: fixed;
  inset: 0;
  width: 100%;
  height: 100%;
  pointer-events: none;
  z-index: 9998;
}

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

.email-bar {
  background: #f5f5f5;
  padding: 14px 24px;
  text-align: center;
  font-size: 0.9rem;
  border-radius: 10px;
  margin-bottom: 24px;
  color: #555;
}

.reservation-block {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  margin-bottom: 28px;
}

.reservation-label {
  font-size: 0.78rem;
  color: #aaa;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.reservation-code {
  font-size: 2.2rem;
  font-weight: 900;
  letter-spacing: 0.12em;
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.details-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  margin-bottom: 28px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.detail-label {
  font-size: 0.78rem;
  color: #999;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.detail-value {
  font-size: 1rem;
  color: #1a1a1a;
  font-weight: 700;
}

.flight-leg-block {
  border-top: 1.5px solid #fcd9a4;
  padding-top: 18px;
  margin-bottom: 4px;
}

.flight-leg-block--stopover {
  border-top-color: #fcd9a4;
}

.flight-leg-title {
  font-size: 0.82rem;
  font-weight: 800;
  color: #e67e22;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  margin-bottom: 14px;
}

.flight-leg-block--stopover .flight-leg-title {
  color: #e67e22;
}

.breakdown-section {
  border-top: 1.5px solid #f0f0f0;
  padding-top: 20px;
}

.breakdown-title {
  font-size: 0.82rem;
  font-weight: 700;
  color: #555;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  margin-bottom: 10px;
}

.breakdown-row {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: 12px;
  padding: 8px 0;
  font-size: 0.9rem;
  color: #333;
  border-bottom: 1px solid #f8f8f8;
}

.breakdown-row--header {
  font-size: 0.75rem;
  color: #aaa;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  padding-bottom: 6px;
}

.breakdown-row--header span:not(:first-child),
.breakdown-row span:not(:first-child) {
  text-align: right;
}

/* ── Per-passenger baggage table ── */
.bpp-table {
  display: flex;
  flex-direction: column;
  font-size: 0.9rem;
}

.bpp-row {
  display: grid;
  grid-template-columns: 1fr 56px 120px 110px;
  gap: 8px;
  padding: 8px 0;
  color: #333;
  border-bottom: 1px solid #f8f8f8;
  align-items: center;
}

.bpp-row--header {
  font-size: 0.75rem;
  color: #aaa;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  padding-bottom: 6px;
}

.bpp-row--total {
  font-weight: 700;
  color: #1a1a1a;
  border-bottom: none;
  border-top: 1px solid #e8e8e8;
  margin-top: 2px;
}

.bpp-col-name  { text-align: left; }
.bpp-col-num   { text-align: right; }
.bpp-col-amount { text-align: right; }

.breakdown-row--total {
  grid-template-columns: 1fr auto;
  border-bottom: none;
  border-top: 1.5px solid #e0e0e0;
  margin-top: 4px;
  font-weight: 700;
}

.total-amount {
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  font-size: 1.05rem;
  font-weight: 900;
}

.alert-box {
  padding: 12px 16px;
  border-radius: 10px;
  font-size: 0.88rem;
  font-weight: 600;
  margin-bottom: 16px;
}

.alert-box--success {
  background: #dcfce7;
  color: #166534;
}

.alert-box--error {
  background: #fee2e2;
  color: #991b1b;
}

.error-box {
  background: #fee2e2;
  color: #991b1b;
  padding: 14px 18px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.9rem;
}

.bottom-box {
  background: #eef5ff;
  border-radius: 16px;
  padding: 28px 30px;
  text-align: center;
}

.bottom-box h4 {
  font-weight: 800;
  margin-bottom: 8px;
  color: #1a1a1a;
}

.bottom-box p {
  color: #666;
  margin-bottom: 22px;
  font-size: 0.9rem;
}

.buttons-container {
  display: flex;
  gap: 14px;
  justify-content: center;
  flex-wrap: wrap;
}

.btn-outline-custom {
  border: 1.5px solid #e74c3c;
  background: white;
  color: #e74c3c;
  padding: 11px 22px;
  border-radius: 10px;
  font-weight: 700;
  font-size: 0.9rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: background 0.2s, color 0.2s;
}

.btn-outline-custom:hover:not(:disabled) {
  background: #fff5f5;
}

.btn-outline-custom:disabled {
  opacity: 0.55;
  cursor: not-allowed;
}

.btn-gradient-custom {
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  color: white;
  padding: 11px 22px;
  border-radius: 10px;
  font-weight: 700;
  font-size: 0.9rem;
  text-decoration: none;
  display: flex;
  align-items: center;
  transition: opacity 0.2s;
}

.btn-gradient-custom:hover {
  opacity: 0.88;
}

@media (max-width: 560px) {
  .details-grid {
    grid-template-columns: 1fr;
  }

  .reservation-code {
    font-size: 1.7rem;
  }
}
</style>
