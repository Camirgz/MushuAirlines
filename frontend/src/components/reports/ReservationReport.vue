<template>
  <div class="confirmation-root">

    <AdminPageLayout>

      <AdminHero
        title="Mi Reserva"
        subtitle="Información de su itinerario de vuelo."
        icon="bi bi-airplane-fill"
      />

      <div v-if="loading" class="loading-state">
        <i class="bi bi-arrow-repeat spin me-2"></i> Cargando información...
      </div>

      <div v-else-if="loadError" class="error-box" style="margin-bottom: 16px;">
        <i class="bi bi-exclamation-circle-fill me-2"></i>{{ loadError }}
      </div>

      <template v-else-if="reservation">

        <AdminCard>

          <div class="reservation-block">
            <span class="reservation-label">Código de Reserva</span>
            <span class="reservation-code">{{ reservation.reservationCode }}</span>
          </div>

          <div class="details-grid">

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-person me-1"></i>Pasajero Principal</span>
              <strong class="detail-value">{{ reservation.fullName }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-envelope me-1"></i>Correo</span>
              <strong class="detail-value">{{ reservation.email }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-calendar3 me-1"></i>Fecha del vuelo</span>
              <strong class="detail-value">{{ formatDate(reservation.departureDate) }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-clock me-1"></i>Días restantes</span>
              <strong class="detail-value">{{ daysRemaining(reservation.departureDate) }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-people me-1"></i>Cantidad de pasajeros</span>
              <strong class="detail-value">{{ reservation.totalSeats }}</strong>
            </div>

            <div class="detail-item" v-if="reservation.details && reservation.details.length">
              <span class="detail-label"><i class="bi bi-ticket-perforated me-1"></i>Clase</span>
              <strong class="detail-value">{{ translateClass(reservation.details[0].seatClass) }}</strong>
            </div>

          </div>

        </AdminCard>

        <AdminCard>

          <div class="flight-leg-title">
            <i class="bi bi-airplane-fill me-2"></i>
            {{ reservation.flightNumber2 ? 'Tramo 1' : 'Vuelo' }}
          </div>

          <div class="details-grid">

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-geo-alt me-1"></i>Origen</span>
              <strong class="detail-value">{{ reservation.originAirport }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-geo-alt-fill me-1"></i>Destino</span>
              <strong class="detail-value">{{ reservation.destinationAirport }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-airplane me-1"></i>Número de vuelo</span>
              <strong class="detail-value">{{ reservation.flightNumber }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-airplane me-1"></i>Aeronave</span>
              <strong class="detail-value">{{ reservation.aircraftType }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-clock me-1"></i>Salida</span>
              <strong class="detail-value">{{ formatDateTime(reservation.departureDate) }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-clock-fill me-1"></i>Llegada</span>
              <strong class="detail-value">{{ formatDateTime(reservation.arrivalDate) }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-stopwatch me-1"></i>Duración</span>
              <strong class="detail-value">{{ duration(reservation.departureDate, reservation.arrivalDate) }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-signpost-split me-1"></i>Escalas</span>
              <strong class="detail-value">
                {{ reservation.flightNumber2 ? reservation.originAirport2 : 'Vuelo directo' }}
              </strong>
            </div>

          </div>

        </AdminCard>

        <AdminCard v-if="reservation.flightNumber2">

          <div class="flight-leg-title">
            <i class="bi bi-airplane-fill me-2"></i>
            Tramo 2
          </div>

          <div class="details-grid">

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-geo-alt me-1"></i>Origen</span>
              <strong class="detail-value">{{ reservation.originAirport2 }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-geo-alt-fill me-1"></i>Destino</span>
              <strong class="detail-value">{{ reservation.destinationAirport2 }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-airplane me-1"></i>Número de vuelo</span>
              <strong class="detail-value">{{ reservation.flightNumber2 }}</strong>
            </div>

            <div class="detail-item" v-if="reservation.aircraftType2">
              <span class="detail-label"><i class="bi bi-airplane me-1"></i>Aeronave</span>
              <strong class="detail-value">{{ reservation.aircraftType2 }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-clock me-1"></i>Salida</span>
              <strong class="detail-value">{{ formatDateTime(reservation.departureDate2) }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-clock-fill me-1"></i>Llegada</span>
              <strong class="detail-value">{{ formatDateTime(reservation.arrivalDate2) }}</strong>
            </div>

            <div class="detail-item">
              <span class="detail-label"><i class="bi bi-stopwatch me-1"></i>Duración</span>
              <strong class="detail-value">{{ duration(reservation.departureDate2, reservation.arrivalDate2) }}</strong>
            </div>

          </div>

        </AdminCard>

        <AdminCard v-if="reservation.passengerBaggageDetails && reservation.passengerBaggageDetails.length">

          <div class="breakdown-title">Equipaje por pasajero</div>

          <div class="bpp-table">

            <div class="bpp-row bpp-row--header">
              <span class="bpp-col-name">Pasajero</span>
              <span class="bpp-col-num">Mano</span>
              <span class="bpp-col-num">Documentado</span>
            </div>

            <div
              class="bpp-row"
              v-for="passenger in reservation.passengerBaggageDetails"
              :key="passenger.passengerFullName"
            >
              <span class="bpp-col-name">{{ passenger.passengerFullName }}</span>
              <span class="bpp-col-num">{{ passenger.handBagCount }}</span>
              <span class="bpp-col-num">{{ passenger.checkedBagCount }}</span>
            </div>

            <div class="bpp-row bpp-row--total">
              <span class="bpp-col-name">Total</span>
              <span class="bpp-col-num">{{ totalHandBags }}</span>
              <span class="bpp-col-num">{{ totalCheckedBags }}</span>
            </div>

          </div>

        </AdminCard>

        <AdminCard>

          <div class="buttons-container">

            <button class="btn-outline-custom" @click="goBaggage">
              <i class="bi bi-suitcase-lg me-2"></i>Maletas
            </button>

            <button class="btn-outline-custom" @click="cancelReservation">
              <i class="bi bi-x-circle me-2"></i>Cancelar reserva
            </button>

            <button class="btn-gradient-custom" @click="printItinerary">
              <i class="bi bi-printer me-2"></i>Print Itinerary
            </button>

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
import { getReservationReport } from '@/services/ReservationService';

export default {
  name: 'ReservationReport',

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
      loading:   true,
      loadError: null,
      reservation: null,
    };
  },

  computed: {
    totalHandBags() {
      return (this.reservation?.passengerBaggageDetails || [])
        .reduce((sum, p) => sum + (p.handBagCount || 0), 0);
    },
    totalCheckedBags() {
      return (this.reservation?.passengerBaggageDetails || [])
        .reduce((sum, p) => sum + (p.checkedBagCount || 0), 0);
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

    formatDate(date) {
      if (!date) return '';
      return new Date(date).toLocaleDateString('es-CR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      });
    },

    formatDateTime(date) {
      if (!date) return '';
      return new Date(date).toLocaleString('es-CR');
    },

    daysRemaining(date) {
      if (!date) return '';
      const today     = new Date();
      const departure = new Date(date);
      const diff      = departure.getTime() - today.getTime();
      return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
    },

    duration(start, end) {
      if (!start || !end) return '';
      const diff    = new Date(end) - new Date(start);
      const hours   = Math.floor(diff / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      return `${hours} h ${minutes} min`;
    },

    translateClass(seatClass) {
      if (seatClass === 'FirstClass') return 'Primera Clase';
      if (seatClass === 'Economy')    return 'Clase Turista';
      return seatClass;
    },

    goBaggage() {
      this.$router.push('/reservation/baggage');
    },

    cancelReservation() {
      this.$router.push('/reservation/cancel');
    },

    printItinerary() {
      alert('Pendiente');
    },

  },
};
</script>