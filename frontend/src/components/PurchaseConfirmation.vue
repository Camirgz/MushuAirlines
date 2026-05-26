<template>
  <div class="confirmation-page">
    <div class="confirmation-card">
      <div class="success-banner">
        <div class="success-icon">
          ✓
        </div>
        <h1>
          ¡Compra realizada con éxito!
        </h1>
        <p>Hemos enviado un correo de confirmacion con toda la informacion de tu reservación.</p>
      </div>

      <div class="email-bar">
        ✉ Enviado a: <strong>{{ purchase.email }}</strong>
      </div>

      <div class="details-card">
        <h3>✈ Detalles de tu vuelo</h3>
        <div class="details-grid">
          <div class="detail-item">
            <span>Código de reserva</span>
            <strong class="reservation-code">{{ purchase.reservationCode }}</strong>
          </div>

          <div class="detail-item">
            <span>Número de vuelo</span><strong>{{ purchase.flightNumber }}</strong>
          </div>

          <div class="detail-item">
            <span>Ruta</span> 
            <strong>{{ purchase.originAirport }} → {{ purchase.destinationAirport }}</strong>
          </div>

          <div class="detail-item">
            <span>Fecha de salida</span>
            <strong>{{ purchase.departureDate }}</strong>
          </div>
        </div>
      </div>

      <div v-if="message" :class="isError ? 'error-box' : 'success-box'">{{ message }}</div>
      <div class="bottom-box">
        <h4>¿No recibiste el correo?</h4>
        <p>Revisa tu bandeja de spam o solicita un reenvío.</p>
        <div class="buttons-container">
          <button class="btn-outline-custom" @click="resendEmail" :disabled="loading">
            {{loading? "Reenviando...": "Reenviar correo"}}
          </button>

          <RouterLink to="/" class="btn-gradient-custom"> Ir al menú → </RouterLink>
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

<style scoped>

.confirmation-page {
    min-height: 100vh;
    background: #f4f4f4;
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 40px 20px;
}

.confirmation-card {
    width: 100%;
    max-width: 760px;
    background: white;
    border-radius: 18px;
    overflow: hidden;
    box-shadow: 0 10px 40px rgba(0,0,0,0.15);
}

.success-banner {
    background: linear-gradient(90deg, #e60000, #f0a500);
    color: white;
    text-align: center;
    padding: 50px 30px;
}

.success-icon {
    width: 80px;
    height: 80px;
    background: white;
    color: #22c55e;
    border-radius: 50%;
    margin: 0 auto 20px;
    font-size: 2.5rem;
    font-weight: bold;

    display: flex;
    align-items: center;
    justify-content: center;
}

.success-banner h1 {
    font-size: 2rem;
    font-weight: 800;
    margin-bottom: 10px;
}

.success-banner p {
    margin: 0;
    opacity: 0.95;
}

.email-bar {
    background: #f5f5f5;
    padding: 16px 30px;
    text-align: center;
    font-size: 0.95rem;
}

.details-card {
    margin: 30px;
    border: 1px solid #ececec;
    border-radius: 16px;
    padding: 24px;
}

.details-card h3 {
    margin-bottom: 24px;
    font-size: 1.2rem;
    font-weight: 700;
}

.details-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 24px;
}

.detail-item {
    display: flex;
    flex-direction: column;
}

.detail-item span {
    font-size: 0.85rem;
    color: #777;
    margin-bottom: 5px;
}

.detail-item strong {
    font-size: 1.15rem;
    color: #222;
}

.reservation-code {
    color: #e60000;
    font-weight: 900;
}

.bottom-box {
    background: #eef5ff;
    margin: 30px;
    border-radius: 16px;
    padding: 30px;
    text-align: center;
}

.bottom-box h4 {
    font-weight: 800;
    margin-bottom: 10px;
}

.bottom-box p {
    color: #666;
    margin-bottom: 24px;
}

.buttons-container {
    display: flex;
    gap: 16px;
    justify-content: center;
}

.btn-outline-custom {
    border: 1px solid #e74c3c;
    background: white;
    color: #e74c3c;
    padding: 12px 24px;
    border-radius: 10px;
    font-weight: 700;
    cursor: pointer;
}

.btn-gradient-custom {
    background: linear-gradient(to right, #e74c3c, #f39c12);
    color: white;
    padding: 12px 28px;
    border-radius: 10px;
    font-weight: 700;
    text-decoration: none;
}

.success-box {
    margin: 0 30px;
    padding: 12px;
    border-radius: 10px;
    background: #dcfce7;
    color: #166534;
    text-align: center;
}

.error-box {
    margin: 0 30px;
    padding: 12px;
    border-radius: 10px;
    background: #fee2e2;
    color: #991b1b;
    text-align: center;
}

</style>