<template>
<AdminPageLayout>
  <AdminHero
    title="¡Compra realizada con éxito!"
    subtitle="Hemos enviado un correo de confirmación con toda la información de tu reservación."
    icon="bi bi-check-circle-fill"
  />
  <AdminCard>

    <div class="email-bar">
      Enviado a:
      <strong>{{ purchase.email }}</strong>
    </div>

    <div class="details-grid">

      <div class="detail-item">
        <span>Código de reserva</span>

        <strong class="reservation-code">
          {{ purchase.reservationCode }}
        </strong>
      </div>

      <div class="detail-item">
        <span>Número de vuelo</span>

        <strong>
          {{ purchase.flightNumber }}
        </strong>
      </div>

      <div class="detail-item">
        <span>Ruta</span>

        <strong>
          {{ purchase.originAirport }}
          →
          {{ purchase.destinationAirport }}
        </strong>
      </div>

      <div class="detail-item">
        <span>Fecha de salida</span>

        <strong>
          {{ purchase.departureDate }}
        </strong>
      </div>

    </div>

  </AdminCard>

  <AdminCard>

    <div
      v-if="message"
      :class="isError ? 'error-box' : 'success-box'"
    >
      {{ message }}
    </div>

    <div class="bottom-box">

      <h4>
        ¿No recibiste el correo?
      </h4>

      <p>
        Revisa spam o solicita un reenvío.
      </p>

      <div class="buttons-container">

        <button
          class="btn-outline-custom"
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
          class="btn-gradient-custom"
        >
          Ir al menú →
        </RouterLink>
      </div>
    </div>
  </AdminCard>
</AdminPageLayout>
</template>

<script>
import axios from "axios";

import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";
export default {
  data() {
    return {
      loading: false,
      purchase: {},
      message: "",
      isError: false
    };
  },
  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
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

.email-bar {
    background: #f5f5f5;
    padding: 16px 30px;
    text-align: center;
    font-size: 0.95rem;
    border-radius: 12px;
    margin-bottom: 24px;
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

/* PARTE DE ABAJO */

.bottom-box {
    background: #eef5ff;
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

/* BOTONES */

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

/* ALERTAS */

.success-box {
    margin-bottom: 20px;
    padding: 12px;
    border-radius: 10px;
    background: #dcfce7;
    color: #166534;
    text-align: center;
}

.error-box {
    margin-bottom: 20px;
    padding: 12px;
    border-radius: 10px;
    background: #fee2e2;
    color: #991b1b;
    text-align: center;
}

</style>