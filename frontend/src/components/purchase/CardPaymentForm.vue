<template>
  <div class="card-payment-form">

    <div class="form-group-wrapper">
      <label class="custom-form-label">Método de Pago <span>*</span></label>
      <div class="payment-methods">
        <button
          v-for="method in ['Visa', 'Mastercard', 'Amex']"
          :key="method"
          type="button"
          class="payment-method-btn"
          :class="{ selected: payment.paymentMethod === method }"
          @click="payment.paymentMethod = method"
        >
          {{ method }}
        </button>
      </div>
    </div>

    <div class="form-group-wrapper">
      <label class="custom-form-label">Nombre del Titular <span>*</span></label>
      <input
        v-model="payment.holder"
        type="text"
        class="custom-form-input"
        :class="getFieldClass('holder')"
        placeholder="Nombre del titular"
        @blur="validateField('holder')"
      />
      <span v-if="errors.holder" class="error-msg-text">{{ errors.holder }}</span>
    </div>

    <div class="form-group-wrapper">
      <label class="custom-form-label">Número de Tarjeta <span>*</span></label>
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
      <span v-if="errors.cardNumber" class="error-msg-text">{{ errors.cardNumber }}</span>
    </div>

    <div class="form-grid-half">
      <div class="form-group-wrapper">
        <label class="custom-form-label">Fecha de Expiración <span>*</span></label>
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
        <span v-if="errors.expiry" class="error-msg-text">{{ errors.expiry }}</span>
      </div>

      <div class="form-group-wrapper">
        <label class="custom-form-label">CVV <span>*</span></label>
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
        <span v-if="errors.cvv" class="error-msg-text">{{ errors.cvv }}</span>
      </div>
    </div>

    <div class="api-error" v-if="apiError">
      <i class="bi bi-exclamation-circle-fill me-2"></i>{{ apiError }}
    </div>

    <div class="total-preview">
      <span class="total-preview-label">Total a cobrar</span>
      <span class="total-preview-amount">${{ total.toLocaleString() }}</span>
    </div>

    <div class="form-actions">
      <button type="button" class="btn-cancel" @click="$emit('cancel')" :disabled="paying">
        Cancelar
      </button>
      <button
        type="button"
        class="btn-pay"
        :disabled="!isFormValid || paying"
        @click="handleSubmit"
      >
        <i class="bi bi-credit-card-fill"></i>
        {{ paying ? 'Procesando...' : 'Confirmar y Pagar' }}
      </button>
    </div>

  </div>
</template>

<script>
export default {
  name: 'CardPaymentForm',

  props: {
    total:    { type: Number,  required: true },
    paying:   { type: Boolean, default: false },
    apiError: { type: String,  default: null },
  },

  emits: ['submit', 'cancel'],

  data() {
    return {
      payment: {
        paymentMethod: '',
        holder:        '',
        cardNumber:    '',
        expiry:        '',
        cvv:           '',
      },
      errors:      {},
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
    handleSubmit() {
      if (!this.isFormValid) return;
      this.$emit('submit', { ...this.payment });
    },

    getFieldClass(field) {
      if (this.errors[field])      return 'input-has-error';
      if (this.validFields[field]) return 'input-has-success';
      return '';
    },

    validateField(field) {
      const errs   = { ...this.errors };
      const valids = { ...this.validFields };
      const d      = this.payment;

      const rules = {
        holder:     () => d.holder.trim().length < 3 ? 'Ingrese el nombre del titular' : null,
        cardNumber: () => d.cardNumber.replace(/\s/g, '').length !== 16 ? 'Número de tarjeta inválido' : null,
        expiry: () => {
          const m = d.expiry.match(/^(\d{2})\/(\d{2})$/);
          if (!m) return 'Ingrese MM/AA.';
          return parseInt(m[1]) < 1 || parseInt(m[1]) > 12 ? 'Mes inválido' : null;
        },
        cvv: () => !/^\d{3,4}$/.test(d.cvv) ? 'CVV inválido' : null,
      };

      const result = rules[field]?.();
      if (result) {
        errs[field] = result;
        delete valids[field];
      } else {
        delete errs[field];
        valids[field] = true;
      }

      this.errors      = errs;
      this.validFields = valids;
    },

    formatCardNumber(e) {
      const value = e.target.value.replace(/\D/g, '').slice(0, 16);
      this.payment.cardNumber = value.replace(/(.{4})/g, '$1 ').trim();
    },

    formatExpiry(e) {
      let value = e.target.value.replace(/\D/g, '').slice(0, 4);
      if (value.length >= 3) value = value.slice(0, 2) + '/' + value.slice(2);
      this.payment.expiry = value;
    },
  },
};
</script>

<style scoped>
.card-payment-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-group-wrapper {
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.custom-form-label {
  font-size: 0.82rem;
  font-weight: 700;
  color: #374151;
}

.custom-form-label span {
  color: #e74c3c;
}

.custom-form-input {
  width: 100%;
  padding: 10px 12px;
  border: 1.5px solid #e5e7eb;
  border-radius: 8px;
  font-size: 0.9rem;
  transition: border-color 0.15s;
  box-sizing: border-box;
}

.custom-form-input:focus {
  outline: none;
  border-color: #e74c3c;
}

.input-has-error   { border-color: #dc2626; }
.input-has-success { border-color: #16a34a; }

.form-grid-half {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.payment-methods {
  display: flex;
  gap: 8px;
}

.payment-method-btn {
  flex: 1;
  border: 1.5px solid #e5e7eb;
  background: #fff;
  padding: 8px 0;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 700;
  font-size: 0.82rem;
  transition: border-color 0.15s, color 0.15s;
}

.payment-method-btn.selected {
  border-color: #e74c3c;
  color: #e74c3c;
  background: #fff9f9;
}

.error-msg-text {
  color: #dc2626;
  font-size: 0.78rem;
}

.api-error {
  background: #fff5f5;
  border: 1.5px solid #fca5a5;
  color: #b91c1c;
  border-radius: 8px;
  padding: 10px 14px;
  font-size: 0.82rem;
  font-weight: 600;
}

.total-preview {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  padding: 12px 0 4px;
  border-top: 1.5px solid #f0f0f0;
}

.total-preview-label {
  font-size: 0.72rem;
  font-weight: 800;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

.total-preview-amount {
  font-size: 1.6rem;
  font-weight: 900;
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.form-actions {
  display: flex;
  gap: 10px;
}

.btn-cancel {
  flex: 0 0 auto;
  padding: 12px 16px;
  border: 1.5px solid #e5e7eb;
  border-radius: 8px;
  background: #fff;
  color: #6b7280;
  font-weight: 700;
  font-size: 0.88rem;
  cursor: pointer;
  transition: border-color 0.15s;
}

.btn-cancel:hover:not(:disabled) {
  border-color: #d1d5db;
  color: #374151;
}

.btn-cancel:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-pay {
  flex: 1;
  padding: 12px 16px;
  border: none;
  border-radius: 8px;
  background: linear-gradient(135deg, #e74c3c 0%, #f39c12 100%);
  color: #fff;
  font-size: 0.88rem;
  font-weight: 800;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  box-shadow: 0 4px 14px rgba(231, 76, 60, 0.28);
  transition: opacity 0.15s, transform 0.15s;
}

.btn-pay:hover:not(:disabled) {
  opacity: 0.92;
  transform: translateY(-1px);
}

.btn-pay:disabled {
  background: #d1d5db;
  color: #9ca3af;
  box-shadow: none;
  cursor: not-allowed;
}
</style>
