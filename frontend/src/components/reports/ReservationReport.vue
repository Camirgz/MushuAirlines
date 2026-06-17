<template>
  <div class="page">
    <nav class="navbar">
      <RouterLink to="/">
        <img src="@/assets/logo.png" alt="Logo" class="logo" />
      </RouterLink>
    </nav>
    <div v-if="loading" class="state">
      <i class="bi bi-arrow-repeat spin"></i> Cargando...
    </div>
    <div v-else-if="loadError" class="error">
      <i class="bi bi-exclamation-circle-fill"></i> {{ loadError }}
    </div>
    <template v-else-if="reservation">
      <div class="page-content">
        <AdminHero
          back-to="/"
          back-text="Volver"
          icon="bi bi-airplane-fill"
          :title="`${reservation.originAirport} → ${reservation.destinationAirport} · ${daysRemaining(reservation.departureDate)} ${daysRemaining(reservation.departureDate) === 1 ? 'día restante' : 'días restantes'}`"
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

                <div class="info-item" v-if="reservation.details?.length">
                  <span class="label"><i class="bi bi-ticket-perforated"></i> Clase</span>
                  <strong>{{ translateClass(reservation.details[0].seatClass) }}</strong>
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
                <i class="bi bi-airplane"></i> Aeronave: <strong>{{ reservation.aircraftType }}</strong>
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
                <i class="bi bi-airplane"></i> Aeronave: <strong>{{ reservation.aircraftType2 }}</strong>
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
                <span class="summary-value">{{ reservation.originAirport }} → {{ reservation.destinationAirport }}</span>
              </div>
              <div class="summary-row">
                <span class="summary-label">Tramos</span>
                <span class="summary-value">2 vuelos</span>
              </div>
              <div class="summary-row">
                <span class="summary-label">Pasajeros</span>
                <span class="summary-value">{{ reservation.totalSeats }}</span>
              </div>
              <div class="summary-row" v-if="reservation.details?.length">
                <span class="summary-label">Clase</span>
                <span class="summary-value">{{ translateClass(reservation.details[0].seatClass) }}</span>
              </div>
            </AdminCard>
          </aside>
        </div>
      </div>
    </template>
  </div>
</template>

<script>
import { getReservationReport } from '@/services/ReservationService';
import AdminHero       from '@/components/admin/ui/AdminHero.vue';
import AdminCard       from '@/components/admin/ui/AdminCard.vue';

export default {
  name: 'ReservationReport',
  components: { AdminCard, AdminHero },

  data() {
    return { loading: true, loadError: null, reservation: null };
  },

  computed: {
    totalHandBags() {
      return (this.reservation?.passengerBaggageDetails || [])
        .reduce((s, p) => s + (p.handBagCount || 0), 0);
    },
    totalCheckedBags() {
      return (this.reservation?.passengerBaggageDetails || [])
        .reduce((s, p) => s + (p.checkedBagCount || 0), 0);
    },
  },

  async mounted() {
    try {
      this.reservation = await getReservationReport();
    } catch (e) {
      this.loadError = e.message ?? 'No se pudo cargar la información del vuelo.';
    } finally {
      this.loading = false;
    }
  },

  methods: {
    formatDateShort(d) {
      return d ? new Date(d).toLocaleDateString('es-CR', { day: '2-digit', month: 'short', year: 'numeric' }) : '';
    },
    formatTime(d) {
      return d ? new Date(d).toLocaleTimeString('es-CR', { hour: '2-digit', minute: '2-digit' }) : '';
    },
    daysRemaining(d) {
      if (!d) return 0;
      return Math.max(0, Math.ceil((new Date(d) - new Date()) / 86400000));
    },
    duration(s, e) {
      if (!s || !e) return '';
      const ms = new Date(e) - new Date(s);
      return `${Math.floor(ms / 3600000)}h ${Math.floor((ms % 3600000) / 60000)}m`;
    },
    translateClass(c) {
      return c === 'FirstClass' ? 'Primera Clase' : c === 'Economy' ? 'Económica' : c;
    },
    goBaggage()         { this.$router.push('/reservation/baggage'); },
    cancelReservation() { this.$router.push('/reservation/cancel'); },
    printItinerary()    { alert('Pendiente'); },
  },
};
</script>

<style scoped>
.page         { min-height: 100vh; background: #f4f6f8; font-family: 'Segoe UI', sans-serif; }
.page-content { padding: 28px 32px 40px; }

.navbar { display: flex; align-items: center; height: 64px; padding: 0 32px; background: #fff; border-bottom: 1px solid #e8e8e8; }
.logo   { height: 40px; }

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