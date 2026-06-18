<template>
  <AdminPageLayout> 
  <div class="page">
    <div v-if="loading" class="state">
      <i class="bi bi-arrow-repeat spin"></i> Cargando...
    </div>
    <div v-else-if="loadError" class="error">
      <i class="bi bi-exclamation-circle-fill"></i> {{ loadError }}
    </div>
    <template v-else-if="reservation">
      <div class="page-content">
        <AdminHero
          back-text="Volver"
          @back="goBack"
          icon="bi bi-airplane-fill"
          :title="`${reservation.originAirport} → ${finalDestination} · ${daysRemaining(reservation.departureDate)} ${daysRemaining(reservation.departureDate) === 1 ? 'día restante' : 'días restantes'}`"
          :subtitle="`Código: ${reservation.reservationCode} · Salida: ${formatDateShort(reservation.departureDate)}`"/>

        <div class="layout">
          <div class="main">
            <AdminCard>
              <h2><i class="bi bi-person-circle"></i> Información de la Reserva</h2>
              <div class="info-grid">

                <div class="info-item">
                  <span class="label"><i class="bi bi-qr-code"></i> Código de Reserva</span>
                  <strong>{{ reservation.reservationCode }}</strong>
                </div>

                <div class="info-item">
                  <span class="label"><i class="bi bi-person"></i> Pasajero Principal</span>
                  <strong>{{ reservation.fullName }}</strong>
                </div>

                <div class="info-item">
                  <span class="label"><i class="bi bi-envelope"></i> Correo</span>
                  <strong>{{ reservation.email }}</strong>
                </div>

                <div class="info-item">
                  <span class="label"><i class="bi bi-calendar3"></i> Fecha del vuelo</span>
                  <strong>{{ formatDateShort(reservation.departureDate) }}</strong>
                </div>

                <div class="info-item">
                  <span class="label"><i class="bi bi-clock"></i> Días restantes</span>
                  <strong>
                    {{ daysRemaining(reservation.departureDate) }}
                    {{ daysRemaining(reservation.departureDate) === 1 ? 'día' : 'días' }}
                  </strong>
                </div>

                <div class="info-item">
                  <span class="label"><i class="bi bi-people"></i> Pasajeros</span>
                  <strong>
                    {{ reservation.totalSeats }}
                    {{ reservation.totalSeats === 1 ? 'pasajero' : 'pasajeros' }}
                  </strong>
                </div>
                <div class="breakdown" v-if="reservation.details?.length">
                  <div class="breakdown-header">
                    <span>Clase</span>
                    <span>Asientos</span>
                  </div>
                  <div class="breakdown-row" v-for="detail in reservation.details" :key="detail.seatClass">
                    <span>{{ translateClass(detail.seatClass) }}</span>
                    <span>{{ detail.seatCount }}</span>
                  </div>
                </div>
              </div>
            </AdminCard>

            <AdminCard>
              <div class="leg-header">
                <div class="leg-title">
                  <div class="leg-icon"><i class="bi bi-airplane-fill"></i></div>
                  {{ reservation.flightNumber2 ? 'Tramo 1' : 'Vuelo' }}
                </div>
                <span class="leg-num">{{ reservation.flightNumber }}</span>
              </div>
              <div class="timeline">
                <div class="tl-end">
                  <div class="code">{{ reservation.originAirport }}</div>
                  <div class="city">{{ reservation.originCity }}</div>
                  <div class="time">{{ formatTime(reservation.departureDate) }}</div>
                </div>
                <div class="tl-line">
                  <div class="track"></div>
                  <div class="duration"><i class="bi bi-clock"></i> {{ duration(reservation.departureDate, reservation.arrivalDate) }}</div>
                  <i class="bi bi-airplane-fill plane-icon"></i>
                </div>
                <div class="tl-end right">
                  <div class="code">{{ reservation.destinationAirport }}</div>
                  <div class="city">{{ reservation.destinationCity }}</div>
                  <div class="time">{{ formatTime(reservation.arrivalDate) }}</div>
                </div>
              </div>
              <div class="aircraft">
                <i class="bi bi-airplane"></i> Aeronave: <strong>{{reservation.aircraftModel }}</strong>
              </div>
            </AdminCard>

            <AdminCard v-if="reservation.flightNumber2">
              <div class="leg-header">
                <div class="leg-title">
                  <div class="leg-icon"><i class="bi bi-airplane-fill"></i></div>
                  Tramo 2
                </div>
                <span class="leg-num">{{ reservation.flightNumber2 }}</span>
              </div>
              <div class="timeline">
                <div class="tl-end">
                  <div class="code">{{ reservation.originAirport2 }}</div>
                  <div class="city">{{ reservation.originCity2 }}</div>
                  <div class="time">{{ formatTime(reservation.departureDate2) }}</div>
                </div>
                <div class="tl-line">
                  <div class="track"></div>
                  <div class="duration"><i class="bi bi-clock"></i> {{ duration(reservation.departureDate2, reservation.arrivalDate2) }}</div>
                  <i class="bi bi-airplane-fill plane-icon"></i>
                </div>
                <div class="tl-end right">
                  <div class="code">{{ reservation.destinationAirport2 }}</div>
                  <div class="city">{{ reservation.destinationCity2 }}</div>
                  <div class="time">{{ formatTime(reservation.arrivalDate2) }}</div>
                </div>
              </div>
              <div class="aircraft">
                <i class="bi bi-airplane"></i> Aeronave: <strong>{{ reservation.aircraftModel2 }}</strong>
              </div>
            </AdminCard>

            <AdminCard v-if="reservation.passengerBaggageDetails?.length">
              <h2><i class="bi bi-luggage"></i> Equipaje por pasajero</h2>
              <table>
                <thead>
                  <tr>
                    <th>Pasajero</th>
                    <th>Equipaje de Mano</th>
                    <th>Equipaje Documentado</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="p in reservation.passengerBaggageDetails" :key="p.passengerFullName">
                    <td>{{ p.passengerFullName }}</td>
                    <td>{{ p.handBagCount }} </td>
                    <td>{{ p.checkedBagCount }} </td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr class="total">
                    <td>Total</td>
                    <td>{{ totalHandBags }} </td>
                    <td>{{ totalCheckedBags }} </td>
                  </tr>
                </tfoot>
              </table>
            </AdminCard>

          </div>

          <aside class="sidebar">

            <AdminCard>
              <h2><i class="bi bi-airplane-fill"></i> Opciones de viaje</h2>
              <button class="action" @click="goBaggage">
                <div class="action-icon orange"><i class="bi bi-suitcase-lg"></i></div>
                <div>
                  <div class="action-label">Maletas</div>
                  <div class="action-sub">Gestionar equipaje</div>
                </div>
              </button>

              <button class="action" @click="cancelReservation">
                <div class="action-icon red"><i class="bi bi-x-circle"></i></div>
                <div>
                  <div class="action-label">Cancelar reserva</div>
                  <div class="action-sub">Solicitar reembolso</div>
                </div>
              </button>

              <button class="action" @click="printItinerary">
                <div class="action-icon gray"><i class="bi bi-printer"></i></div>
                <div>
                  <div class="action-label">Imprimir itinerario</div>
                  <div class="action-sub">Print Itinerary</div>
                </div>
              </button>
            </AdminCard>

            <AdminCard v-if="reservation.flightNumber2">
              <p class="summary-title">RESUMEN</p>
              <div class="summary-row">
                <span class="summary-label">Ruta</span>
                <span class="summary-value">{{ reservation.originAirport }} → {{ finalDestination }}</span>
              </div>
              <div class="summary-row">
                <span class="summary-label">Tramos</span>
                <span class="summary-value">2 vuelos</span>
              </div>
              <div class="summary-row">
                <span class="summary-label">Pasajeros</span>
                <span class="summary-value">{{ reservation.totalSeats }}</span>
              </div>
              <div class="summary-row" v-for="detail in reservation.details" :key="detail.seatClass">
                <span class="summary-label">{{ translateClass(detail.seatClass) }}</span>
                <span class="summary-value">{{ detail.seatCount }} asiento{{ detail.seatCount === 1 ? '' : 's' }}</span>
              </div>
            </AdminCard>
          </aside>
        </div>
      </div>
    </template>
  </div>
  </AdminPageLayout>

</template>

<script>
import { getReservationReport } from '@/services/ReservationService';
import AdminHero       from '@/components/admin/ui/AdminHero.vue';
import AdminCard       from '@/components/admin/ui/AdminCard.vue';
import AdminPageLayout from '@/components/layout/AdminPageLayout.vue';
const MILLISECONDS_IN_A_MINUTE = 1000 * 60;
const MILLISECONDS_IN_AN_HOUR = MILLISECONDS_IN_A_MINUTE * 60;
const MILLISECONDS_IN_A_DAY = MILLISECONDS_IN_AN_HOUR * 24;

const FLIGHT_CLASS_TRANSLATIONS = {
  'FirstClass': 'Primera Clase',
  'Economy': 'Económica'
};
export default {
  name: 'ReservationReport',
  components: { AdminCard, AdminHero, AdminPageLayout },

  data() {
    return { loading: true, loadError: null, reservation: null };
  },
  emits: ["back"],
  computed: {
    totalHandBags() {
      return this.sumBaggageCount('handBagCount');
    },

    totalCheckedBags() {
      return this.sumBaggageCount('checkedBagCount');
    },
    finalDestination() {
      return this.reservation?.destinationAirport2 ??
            this.reservation?.destinationAirport;
    },
  },

  async mounted() {
    try {
      this.reservation = await getReservationReport();
    } catch (error) {
      this.loadError = error.message ?? 'No se pudo cargar la información del vuelo.';
    } finally {
      this.loading = false;
    }
  },
  
  methods: {
    goBack() {
      localStorage.removeItem("reservationToken");
      this.$router.push("/");
    },
    formatDateShort(dateString) {
      if (!dateString) return '';
      const date = new Date(dateString);
      return date.toLocaleDateString('es-CR', { day: '2-digit', month: 'short', year: 'numeric' });
    },

    formatTime(dateString) {
      if (!dateString) return '';
      const date = new Date(dateString);
      return date.toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit' });
    },

    daysRemaining(targetDateString) {
      if (!targetDateString) return 0;
      
      const targetDate = new Date(targetDateString);
      const currentDate = new Date();
      const timeDifference = targetDate - currentDate;
      
      const daysLeft = Math.ceil(timeDifference / MILLISECONDS_IN_A_DAY);
      return Math.max(0, daysLeft);
    },

    duration(startDateString, endDateString) {
      if (!startDateString || !endDateString) return '';
      const startDate = new Date(startDateString);
      let endDate = new Date(endDateString);
      let timeDifference = endDate - startDate;

      // Si la diferencia es negativa, el vuelo cruza la medianoche (o cambia de día)
      if (timeDifference < 0) {
        endDate = new Date(endDate.getTime() + MILLISECONDS_IN_A_DAY);
        timeDifference = endDate - startDate;
      }

      const hours = Math.floor(timeDifference / MILLISECONDS_IN_AN_HOUR);
      const minutes = Math.floor((timeDifference % MILLISECONDS_IN_AN_HOUR) / MILLISECONDS_IN_A_MINUTE);

      return `${hours}h ${minutes}m`;
    },

    translateClass(flightClass) {
      // if not found, return original value
      return FLIGHT_CLASS_TRANSLATIONS[flightClass] || flightClass;
    },
    sumBaggageCount(baggageType) {
      const passengerBaggageList = this.reservation?.passengerBaggageDetails || [];
      // sum the specified baggage type across all passengers
      return passengerBaggageList.reduce((totalAccumulator, passenger) => {
        const baggageCount = passenger[baggageType] || 0;
        return totalAccumulator + baggageCount;
      }, 0);
    },
  }
};
</script>

<style scoped>
/* Base */
.page         { min-height: 100vh; background: #f4f6f8; font-family: 'Segoe UI', sans-serif; }
.page-content { padding: 28px 32px 40px; }
 
/* Navbar */
.navbar { display: flex; align-items: center; height: 64px; padding: 0 32px; background: #fff; border-bottom: 1px solid #e8e8e8; }
.logo   { height: 40px; }
 
/* States */
.state { text-align: center; padding: 60px; color: #888; }
.spin  { display: inline-block; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
.error { margin: 24px 32px; padding: 14px 18px; background: #fff3f0; border: 1px solid #ffc5b0; border-radius: 10px; color: #c0392b; }
 
.layout  { display: grid; grid-template-columns: 1fr 300px; gap: 20px; align-items: start; }
.sidebar { position: sticky; top: 24px; display: flex; flex-direction: column; gap: 20px; }
.main    { display: flex; flex-direction: column; gap: 20px; }
 
.info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
.info-item { display: flex; flex-direction: column; gap: 4px; padding: 12px 14px; background: #fafafa; border: 1px solid #f0f0f0; border-radius: 10px; }
.label     { font-size: 0.73rem; color: #e8631a; font-weight: 500; display: flex; align-items: center; gap: 4px; }
.info-item strong { font-size: 0.9rem; color: #1a1a1a; }
 
.breakdown        { margin-top: 16px; border-top: 1px solid #f0f0f0; padding-top: 14px; }
.breakdown-header { display: flex; justify-content: space-between; font-size: 0.72rem; color: #999; text-transform: uppercase; letter-spacing: 0.5px; padding: 0 4px 8px; border-bottom: 1px solid #f0f0f0; }
.breakdown-row    { display: flex; justify-content: space-between; padding: 10px 4px; border-bottom: 1px solid #f5f5f5; font-size: 0.87rem; color: #333; }
.breakdown-row:last-child { border-bottom: none; }
 
.leg-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.leg-title  { display: flex; align-items: center; gap: 10px; font-weight: 700; }
.leg-icon   { width: 34px; height: 34px; border-radius: 50%; background: #e8631a; color: #fff; display: flex; align-items: center; justify-content: center; font-size: 0.8rem; }
.leg-num    { font-size: 0.8rem; color: #999; background: #f4f4f4; padding: 4px 10px; border-radius: 20px; }
 
.timeline   { display: flex; align-items: center; margin-bottom: 14px; }
.tl-end     { display: flex; flex-direction: column; min-width: 72px; }
.tl-end.right { align-items: flex-end; }
.code       { font-size: 1.8rem; font-weight: 800; color: #e8631a; line-height: 1; }
.city       { font-size: 0.73rem; color: #888; margin-top: 2px; }
.time       { font-size: 0.95rem; font-weight: 700; margin-top: 4px; }
.tl-line    { flex: 1; display: flex; flex-direction: column; align-items: center; gap: 4px; padding: 0 12px; }
.track      { width: 100%; height: 2px; background: #e8631a; border-radius: 2px; }
.duration   { font-size: 0.73rem; color: #888; border: 1px solid #e0e0e0; border-radius: 20px; padding: 2px 8px; white-space: nowrap; }
.plane-icon { font-size: 0.8rem; color: #e8631a; align-self: flex-end; }
.aircraft   { font-size: 0.82rem; color: #888; border-top: 1px solid #f0f0f0; padding-top: 12px; display: flex; align-items: center; gap: 6px; }
.aircraft strong { color: #444; }
 
table       { width: 100%; border-collapse: collapse; font-size: 0.87rem; }
th          { text-align: left; padding: 8px 12px; font-size: 0.72rem; color: #999; text-transform: uppercase; letter-spacing: 0.5px; border-bottom: 1px solid #f0f0f0; }
td          { padding: 12px; color: #333; border-bottom: 1px solid #f5f5f5; }
tr:hover td { background: #fafafa; }
.total td   { font-weight: 700; color: #e8631a; border-top: 2px solid #f0f0f0; border-bottom: none; }
 
.action       { display: flex; align-items: center; gap: 14px; width: 100%; padding: 12px 14px; border: 1px solid #f0f0f0; border-radius: 12px; background: #fff; cursor: pointer; text-align: left; margin-bottom: 10px; transition: box-shadow 0.15s; }
.action:last-child { margin-bottom: 0; }
.action:hover { box-shadow: 0 2px 8px rgba(0,0,0,0.08); border-color: #d0d0d0; }
.action-icon  { width: 36px; height: 36px; border-radius: 10px; display: flex; align-items: center; justify-content: center; font-size: 1rem; flex-shrink: 0; }
.action-icon.orange { background: #fff0e5; color: #e8631a; }
.action-icon.red    { background: #fff0f0; color: #e03e3e; }
.action-icon.gray   { background: #f4f4f4; color: #555; }
.action-label { font-size: 0.88rem; font-weight: 600; color: #1a1a1a; }
.action-sub   { font-size: 0.73rem; color: #aaa; }

.summary-title { font-size: 0.7rem; font-weight: 700; letter-spacing: 1.5px; color: #bbb; margin: 0 0 12px; }
.summary-row   { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #f5f5f5; font-size: 0.85rem; }
.summary-row:last-child { border-bottom: none; }
.summary-label { color: #999; }
.summary-value { font-weight: 600; }
</style>