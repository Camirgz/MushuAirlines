<template>
  <AdminPageLayout>

    <AdminHero
      title="Información de Pago"
      subtitle="Complete los datos de su tarjeta para finalizar la compra."
      icon="bi bi-credit-card-fill"
    />

    <AdminCard>

      <form
        @submit.prevent="confirmPayment"
        class="airline-payment-form"
      >

        <div class="form-group-wrapper">
          <label class="custom-form-label">
            Método de Pago <span>*</span>
          </label>

          <div class="payment-methods">

            <button
              type="button"
              class="payment-method-btn"
              :class="{ selected: payment.paymentMethod === 'Visa' }"
              @click="payment.paymentMethod = 'Visa'"
            >
              Visa
            </button>

            <button
              type="button"
              class="payment-method-btn"
              :class="{ selected: payment.paymentMethod === 'Mastercard' }"
              @click="payment.paymentMethod = 'Mastercard'"
            >
              Mastercard
            </button>

            <button
              type="button"
              class="payment-method-btn"
              :class="{ selected: payment.paymentMethod === 'Amex' }"
              @click="payment.paymentMethod = 'Amex'"
            >
              Amex
            </button>

          </div>
        </div>

        <div class="form-group-wrapper">
          <label class="custom-form-label">
            Nombre del Titular <span>*</span>
          </label>

          <input
            v-model="payment.holder"
            type="text"
            class="custom-form-input"
            :class="getFieldClass('holder')"
            placeholder="Nombre del titular"
            @blur="validateField('holder')"
          />

          <span
            v-if="errors.holder"
            class="error-msg-text"
          >
            {{ errors.holder }}
          </span>
        </div>

        <div class="form-group-wrapper">
          <label class="custom-form-label">
            Número de Tarjeta <span>*</span>
          </label>

          <input
            v-model="payment.cardNumber"
            type="text"
            class="custom-form-input"
            :class="getFieldClass('cardNumber')"
            maxlength="19"
            placeholder="1234 5678 9012 3456"
            @input="formatCardNumber"
            @blur="validateField('cardNumber')"
          />

          <span
            v-if="errors.cardNumber"
            class="error-msg-text"
          >
            {{ errors.cardNumber }}
          </span>
        </div>

        <div class="form-grid-half">

          <div class="form-group-wrapper">
            <label class="custom-form-label">
              Fecha de Expiración <span>*</span>
            </label>

            <input
              v-model="payment.expiry"
              type="text"
              class="custom-form-input"
              :class="getFieldClass('expiry')"
              maxlength="5"
              placeholder="MM/AA"
              @input="formatExpiry"
              @blur="validateField('expiry')"
            />

            <span
              v-if="errors.expiry"
              class="error-msg-text"
            >
              {{ errors.expiry }}
            </span>
          </div>

          <div class="form-group-wrapper">
            <label class="custom-form-label">
              CVV <span>*</span>
            </label>

            <input
              v-model="payment.cvv"
              type="password"
              class="custom-form-input"
              :class="getFieldClass('cvv')"
              maxlength="4"
              placeholder="123"
              @input="payment.cvv = payment.cvv.replace(/\D/g, '')"
              @blur="validateField('cvv')"
            />

            <span
              v-if="errors.cvv"
              class="error-msg-text"
            >
              {{ errors.cvv }}
            </span>
          </div>

        </div>

        <div class="bottom-box">

          <h4>Pago seguro</h4>

          <p>Sus datos están protegidos y serán procesados de forma segura.</p>

          <div class="buttons-container">

            <button
              type="button"
              class="btn-outline-custom"
              @click="goBack"
            >
              Volver
            </button>

            <button
              type="submit"
              class="btn-gradient-custom"
              :disabled="!isFormValid || isProcessing"
            >
              {{ isProcessing ? 'Procesando...' : 'Confirmar y Pagar' }}
            </button>

          </div>

        </div>

      </form>

    </AdminCard>

  </AdminPageLayout>
</template>

<script>
import axios from 'axios';
import AdminPageLayout from '@/components/layout/AdminPageLayout.vue';
import AdminHero from '@/components/admin/ui/AdminHero.vue';
import AdminCard from '@/components/admin/ui/AdminCard.vue';

export default {
  name: 'PaymentForm',

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
      isProcessing: false,

      payment: {
        paymentMethod: '',
        holder: '',
        cardNumber: '',
        expiry: '',
        cvv: '',
      },

      errors: {},
      validFields: {},
    };
  },

  computed: {
    isFormValid() {
      return (
        !!this.payment.paymentMethod &&
        this.payment.holder.trim().length >= 3 &&
        this.payment.cardNumber.replace(/\s/g, '').length === 16 &&
        /^(\d{2})\/(\d{2})$/.test(this.payment.expiry) &&
        parseInt(this.payment.expiry.slice(0, 2)) >= 1 &&
        parseInt(this.payment.expiry.slice(0, 2)) <= 12 &&
        /^\d{3,4}$/.test(this.payment.cvv)
      );
    },
  },

  methods: {
    getFieldClass(field) {
      if (this.errors[field]) return 'input-has-error';
      if (this.validFields[field]) return 'input-has-success';
      return '';
    },

    validateField(field) {
      const errs = { ...this.errors };
      const valids = { ...this.validFields };
      const data = this.payment;

      const validationRules = {
        holder: () =>
          data.holder.trim().length < 3
            ? 'Ingrese el nombre del titular'
            : null,

        cardNumber: () =>
          data.cardNumber.replace(/\s/g, '').length !== 16
            ? 'Número de tarjeta inválido'
            : null,

        expiry: () => {
          const match = data.expiry.match(/^(\d{2})\/(\d{2})$/);
          if (!match) return 'Ingrese MM/AA.';
          const month = parseInt(match[1]);
          if (month < 1 || month > 12) return 'Mes inválido';
          return null;
        },

        cvv: () =>
          !/^\d{3,4}$/.test(data.cvv)
            ? 'CVV inválido'
            : null,
      };

      const errorResult = validationRules[field]?.();

      if (errorResult) {
        errs[field] = errorResult;
        delete valids[field];
      } else {
        delete errs[field];
        valids[field] = true;
      }

      this.errors = errs;
      this.validFields = valids;
    },

    formatCardNumber(e) {
      const value = e.target.value
        .replace(/\D/g, '')
        .slice(0, 16);

      this.payment.cardNumber = value.replace(/(.{4})/g, '$1 ').trim();
    },

    formatExpiry(e) {
      let value = e.target.value
        .replace(/\D/g, '')
        .slice(0, 4);

      if (value.length >= 3) {
        value = value.slice(0, 2) + '/' + value.slice(2);
      }

      this.payment.expiry = value;
    },

    async confirmPayment() {
      if (!this.isFormValid) return;

      this.isProcessing = true;

      try {
        const response = await axios.post(
          'http://localhost:5103/api/payment/approve',
          {
            holder: this.payment.holder,
            cardNumber: this.payment.cardNumber,
            expiry: this.payment.expiry,
            cvv: this.payment.cvv,
            paymentMethod: this.payment.paymentMethod,
          }
        );

        const purchaseId =
          response.data.purchaseId ?? response.data.PurchaseId;

        this.$router.push(`/purchase-confirmation/${purchaseId}`);
      } catch (error) {
        console.error(error);
        alert(error.response?.data || 'Error procesando el pago');
      } finally {
        this.isProcessing = false;
      }
    },

    goBack() {
      this.$router.push('/');
    },
  },
};
</script>

<style scoped>

.airline-payment-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group-wrapper {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.custom-form-label {
  font-weight: 600;
}

.custom-form-input {
  width: 100%;
  padding: 12px;
  border: 1px solid #d1d5db;
  border-radius: 10px;
}

.custom-form-input:focus {
  outline: none;
  border-color: #e74c3c;
}

.form-grid-half {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.payment-methods {
  display: flex;
  gap: 12px;
}

.payment-method-btn {
  border: 1px solid #d1d5db;
  background: white;
  padding: 10px 18px;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 700;
}

.payment-method-btn.selected {
  border-color: #e74c3c;
  color: #e74c3c;
}

.error-msg-text {
  color: #dc2626;
  font-size: 0.85rem;
}

.input-has-error {
  border-color: #dc2626;
}

.input-has-success {
  border-color: #16a34a;
}

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
  justify-content: center;
  gap: 16px;
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
  border: none;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: white;
  padding: 12px 28px;
  border-radius: 10px;
  font-weight: 700;
  cursor: pointer;
}

.btn-gradient-custom:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

</style>