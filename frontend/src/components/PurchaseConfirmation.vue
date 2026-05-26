<template>
  <div class="page-container">

    <div class="container mt-5 mb-5">

      <div class="confirmation-card">

        <div class="success-icon">
          ✓
        </div>

        <h1 class="title">
          ¡Compra realizada con éxito!
        </h1>

        <p class="subtitle">
          Se envió un correo de confirmación con la información de su reservación.
        </p>

        <div class="reservation-code-box">

          <span class="label">
            Codigo de reserva
          </span>

          <h2>
            {{ purchase.reservationCode }}
          </h2>

        </div>

        <div class="purchase-details">

          <div class="detail-row">
            <strong>Cliente:</strong>
            {{ purchase.fullName }}
          </div>

          <div class="detail-row">
            <strong>Pasaporte:</strong>
            {{ purchase.passportNumber }}
          </div>

          <div class="detail-row">
            <strong>Vuelo:</strong>
            {{ purchase.flightNumber }}
          </div>

          <div class="detail-row">
            <strong>Origen:</strong>
            {{ purchase.originAirport }}
          </div>

          <div class="detail-row">
            <strong>Destino:</strong>
            {{ purchase.destinationAirport }}
          </div>

          <div class="detail-row">
            <strong>Total pagado:</strong>
            ${{ purchase.totalPaid }}
          </div>

        </div>

        <div
          v-if="message"
          :class="isError
            ? 'alert-error-custom'
            : 'alert-success-custom'"
        >
          {{ message }}
        </div>

        <div class="buttons-container">

          <button
            class="search-btn"
            @click="resendEmail"
            :disabled="loading"
          >
            {{
              loading
                ? "Reenviando..."
                : "Reenviar correo"
            }}
          </button>

          <RouterLink
            to="/"
            class="btn btn-outline-danger rounded-pill px-4"
          >
            Volver al menú
          </RouterLink>

        </div>

      </div>

    </div>

  </div>
</template>

<script>
import axios from "axios";

export default {
  data() {
    return {
      loading: false,
      purchase: {},
      message: "",
      isError: false
    };
  },

  async mounted() {
    await this.loadPurchase();
  },

  methods: {

    async loadPurchase() {
      try {

        const purchaseId = this.$route.params.id;

        const response = await axios.post(
          `http://localhost:5103/api/purchaseConfirmation/send/${purchaseId}`
        );

        this.purchase = response.data;

        this.message =
          "Correo enviado correctamente.";

        this.isError = false;
      }
      catch (error) {

        this.message =
          error.response?.data ||
          "Error al enviar correo.";

        this.isError = true;
      }
    },

    async resendEmail() {

      if (this.loading) return;

      this.loading = true;

      try {

        const response = await axios.post(
          `http://localhost:5103/api/purchaseConfirmation/resend/${this.purchase.purchaseId}`
        );

        this.message = response.data;
        this.isError = false;
      }
      catch (error) {

        this.message =
          error.response?.data ||
          "No se pudo reenviar el correo.";

        this.isError = true;
      }
      finally {

        this.loading = false;
      }
    }
  }
};
</script>