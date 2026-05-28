<template>
  <div class="checkout-page-container">
    
    <!-- Banner de Encabezado Superior (Fiel a Mockup-2.png) -->
    <header class="checkout-header-banner">
      <div class="banner-content">
        <h1 class="banner-title">
          <span class="card-icon"></span> Pago y Confirmación
        </h1>
        <p class="banner-subtitle">Revise su información y complete el pago</p>
      </div>
    </header>

    <main class="checkout-main-content">
      
      <section class="left-info-column">
        
        <article class="info-card">
          <h2 class="card-section-title text-orange">
            <span class="icon">✈</span> Información del Vuelo
          </h2>
          <div class="flight-details-list">
            <div class="detail-item">
              <span class="detail-label">Número de vuelo</span>
                <!-- quemado de momento mientras conecta con la compra -->
              <strong class="detail-value">MU016</strong>
            </div>
            <div class="detail-item">
              <span class="detail-label">Origen</span>
                <!-- quemado de momento mientras conecta con la compra -->
              <span class="detail-value"> San José (SJO)</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Destino</span>
                <!-- quemado de momento mientras conecta con la compra -->
              <span class="detail-value"> Panamá (PTY)</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Salida</span>
                <!-- quemado de momento mientras conecta con la compra -->
              <span class="detail-value"> 12:30</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Llegada</span>
                <!-- quemado de momento mientras conecta con la compra -->
              <span class="detail-value"> 13:45</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Escalas</span>
                <!-- quemado de momento mientras conecta con la compra -->
              <strong class="detail-value">Vuelo directo</strong>
            </div>
          </div>
        </article>

        <article class="info-card">
          <h2 class="card-section-title text-orange">
            <span class="icon">👤</span> Pasajeros (1)
          </h2>
          <div class="passenger-item">
            <strong>Pasajero 1</strong>
            <span class="passenger-item-info">118940189</span>
          </div>
        </article>

        <article class="info-card">
          <h2 class="card-section-title no-icon">Resumen de Precio</h2>
          <div class="price-row">
            <span>Precio por pasajero</span>
              <!-- quemado de momento mientras conecta con la compra -->
            <strong>₡85 000</strong>
          </div>
          <div class="price-row border-bottom">
            <span>Número de pasajeros</span>
            <strong>1</strong>
          </div>
          <div class="price-total-row">
            <span>Total a pagar</span>
              <!-- quemado de momento mientras conecta con la compra -->
            <span class="total-amount-highlight">₡85 000</span>
          </div>
        </article>

      </section>

      <section class="right-form-column">
        <div class="payment-form-card">
          <h2 class="payment-card-title">Información de Pago</h2>
          
          <form @submit.prevent="confirmPayment" class="airline-payment-form">
            
            <div class="form-group-wrapper">
              <label class="custom-form-label">Nombre del Titular <span>*</span></label>
              <input 
                v-model="payment.holder" 
                type="text" 
                class="custom-form-input"
                :class="getFieldClass('holder')"
                placeholder="Nombre del titular en la tarjeta"
                @blur="validateField('holder')"
              />
              <span v-if="errors.holder" class="error-msg-text">{{ errors.holder }}</span>
            </div>

            <div class="form-group-wrapper">
              <label class="custom-form-label">Número de Tarjeta <span>*</span></label>
              <div class="input-with-brand">
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
                <span v-if="cardBrand" class="floating-brand-badge">{{ cardBrand }}</span>
              </div>
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

            <div class="secure-transaction-box">
              <div class="secure-icon-col">
                <span class="blue-check-icon">✓</span>
              </div>
              <div class="secure-text-col">
                <strong class="secure-title">Transacción segura</strong>
                <p class="secure-description">Sus datos de pago están protegidos</p>
              </div>
            </div>

            <div class="form-actions-grid">
              <button type="button" class="btn-airline-back" @click="goBack">Volver</button>
              <button 
                type="submit" 
                class="btn-airline-submit" 
                :disabled="!isFormValid || isProcessing"
              >
                <span v-if="isProcessing" class="spinner-inline"></span>
                <span>{{ isProcessing ? 'Procesando...' : 'Confirmar y Pagar ₡85 000' }}</span>
              </button>
            </div>

          </form>
        </div>
      </section>

    </main>
  </div>
</template>

<script>
    export default {
        name: 'PaymentForm',
        data() {
            return {
                isProcessing: false,
                payment: {
                holder: '',
                cardNumber: '',
                expiry: '',
                cvv: ''
            },
            errors: {},
            validFields: {}
        }
    },
    computed: {
        cardBrand() {
            const sanitized = this.payment.cardNumber.replace(/\s/g, '')
                if (sanitized.startsWith('4')) return 'Visa'
                if (sanitized.startsWith('5')) return 'Mastercard'
                    return ''
        },
        isFormValid() {
          const requiredFields = ['holder', 'cardNumber', 'expiry', 'cvv']
          return requiredFields.every(field => this.validFields[field] && !this.errors[field])
        }
      },
      methods: {
        getFieldClass(field) {
          if (this.errors[field]) return 'input-has-error'
          if (this.validFields[field]) return 'input-has-success'
          return ''
        },
        validateField(field) {
          const errs = { ...this.errors }
          const valids = { ...this.validFields }
          const data = this.payment

          const validationRules = {
            holder: () => !data.holder.trim() || data.holder.trim().length < 3 ? 'Ingrese el nombre del titular' : null,
            cardNumber: () => data.cardNumber.replace(/\s/g, '').length !== 16 ? 'Número de tarjeta inválido' : null,
            expiry: () => {
              const match = data.expiry.match(/^(\d{2})\/(\d{2})$/)
              if (!match) return 'Ingrese MM/AA.'
              const m = parseInt(match[1])
              if (m < 1 || m > 12) return 'Mes inválido'
              return null
            },
            cvv: () => !/^\d{3,4}$/.test(data.cvv) ? 'CVV inválido' : null
          }

          const errorResult = validationRules[field]?.()
          if (errorResult) {
            errs[field] = errorResult
            delete valids[field]
          } else {
            delete errs[field]
            valids[field] = true
          }

          this.errors = errs
          this.validFields = valids
        },
        formatCardNumber(e) {
          let value = e.target.value.replace(/\D/g, '').slice(0, 16)
          this.payment.cardNumber = value.replace(/(.{4})/g, '$1 ').trim()
        },
        formatExpiry(e) {
          let value = e.target.value.replace(/\D/g, '').slice(0, 4)
          if (value.length >= 3) {
            value = value.slice(0, 2) + '/' + value.slice(2)
          }
          this.payment.expiry = value
        },
        async confirmPayment() {
          if (!this.isFormValid) return
          this.isProcessing = true
          await new Promise(resolve => setTimeout(resolve, 2000))
          this.isProcessing = false
          alert('¡Pago procesado con éxito!')
        },
        goBack() {
          this.$router.push('/')
        }
      }
    }
</script>

<style scoped>
    .checkout-page-container {
      font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
      background-color: #f3f4f6;
      min-height: 100vh;
      padding: 0 0 40px 0;
      box-sizing: border-box;
    }

    .checkout-header-banner {
            background: linear-gradient(90deg, #e60000, #f0a500);
            padding: 20px;
            max-width: 1164px;
            margin: 0 auto 30px auto;
            border-radius: 0 0 8px 8px;
            color: white;
            box-sizing: border-box;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
    }

    .banner-content {
      max-width: 1200px;
      margin: 0 auto;
    }

    .banner-title {
      font-size: 1.8rem;
      font-weight: 700;
      margin: 0 0 6px 0;
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .banner-subtitle {
      font-size: 0.95rem;
      margin: 0;
      opacity: 0.9;
    }

    .checkout-main-content {
      display: grid;
      grid-template-columns: 340px 1fr;
      gap: 30px;
      max-width: 1200px;
      margin: 0 auto;
      padding: 0 20px;
      box-sizing: border-box;
    }

    .left-info-column {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .info-card {
      background-color: #ffffff;
      border-radius: 8px;
      padding: 24px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
    }

    .card-section-title {
      font-size: 1.05rem;
      font-weight: 700;
      color: #1f2937;
      margin: 0 0 16px 0;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .text-orange {
      color: #d93800;
    }

    .flight-details-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .detail-item {
      display: flex;
      flex-direction: column;
      font-size: 0.88rem;
    }

    .detail-label {
      color: #6b7280;
      margin-bottom: 2px;
    }

    .detail-value {
      color: #1f2937;
      font-weight: 500;
    }

    strong.detail-value {
      font-weight: 700;
    }

    .passenger-item {
      display: flex;
      flex-direction: column;
      font-size: 0.9rem;
      color: #1f2937;
    }

    .passenger-item-info {
      color: #6b7280;
      font-size: 0.82rem;
      margin-top: 2px;
    }

    .price-row {
      display: flex;
      justify-content: space-between;
      font-size: 0.88rem;
      color: #4b5563;
      margin-bottom: 10px;
    }

    .border-bottom {
      border-bottom: 1px solid #e5e7eb;
      padding-bottom: 14px;
      margin-bottom: 14px;
    }

    .price-total-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-weight: 700;
      font-size: 1rem;
      color: #1f2937;
      padding-top: 4px;
    }

    .total-amount-highlight {
      color: #d93800;
      font-size: 1.3rem;
      font-weight: 800;
    }

    .right-form-column {
      display: flex;
      flex-direction: column;
    }

    .payment-form-card {
      background-color: #ffffff;
      border-radius: 8px;
      padding: 32px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
    }

    .payment-card-title {
      font-size: 1.3rem;
      font-weight: 700;
      color: #1f2937;
      margin: 0 0 24px 0;
    }

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
      font-size: 0.9rem;
      font-weight: 600;
      color: #374151;
    }

    .custom-form-input {
      width: 100%;
      padding: 12px 14px;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      font-size: 0.95rem;
      color: #1f2937;
      background-color: #ffffff;
      box-sizing: border-box;
      outline: none;
      transition: border-color 0.15s, box-shadow 0.15s;
    }

    .custom-form-input:focus {
      border-color: #e63900;
      box-shadow: 0 0 0 3px rgba(230, 57, 0, 0.12);
    }

    .form-grid-half {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 20px;
    }

    .input-with-brand {
      position: relative;
      display: flex;
      align-items: center;
    }

    .floating-brand-badge {
      position: absolute;
      right: 14px;
      background-color: #f3f4f6;
      color: #e63900;
      font-weight: 700;
      font-size: 0.75rem;
      padding: 3px 8px;
      border-radius: 4px;
      border: 1px solid #e5e7eb;
    }

    .input-has-error { border-color: #ef4444 !important; }
    .input-has-success { border-color: #10b981; }
    .error-msg-text { color: #ef4444; font-size: 0.8rem; margin-top: 2px; }

    .secure-transaction-box {
      display: flex;
      gap: 12px;
      background-color: #eff6ff;
      border: 1px solid #bfdbfe;
      border-radius: 6px;
      padding: 14px 18px;
      margin-top: 5px;
    }

    .blue-check-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 18px;
      height: 18px;
      border: 2px solid #2563eb;
      border-radius: 50%;
      color: #2563eb;
      font-size: 0.75rem;
      font-weight: 900;
    }

    .secure-text-col {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .secure-title {
      color: #1e40af;
      font-size: 0.88rem;
      font-weight: 700;
    }

    .secure-description {
      color: #2563eb;
      font-size: 0.82rem;
      margin: 0;
    }

    .form-actions-grid {
      display: grid;
      grid-template-columns: 1fr 1.5fr;
      gap: 16px;
      margin-top: 10px;
    }

    .btn-airline-back {
      background-color: #ffffff;
      color: #4b5563;
      border: 1px solid #d1d5db;
      padding: 12px;
      border-radius: 6px;
      font-size: 0.95rem;
      font-weight: 500;
      cursor: pointer;
      transition: background-color 0.15s;
    }

    .btn-airline-back:hover {
      background-color: #f9fafb;
    }

    .btn-airline-submit {
      background: linear-gradient(90deg, #e63900 0%, #ff9e00 100%);
      color: #ffffff;
      border: none;
      padding: 12px;
      border-radius: 6px;
      font-size: 0.95rem;
      font-weight: 700;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      box-shadow: 0 2px 6px rgba(230, 57, 0, 0.2);
      transition: opacity 0.15s;
    }

    .btn-airline-submit:hover:not(:disabled) {
      opacity: 0.95;
    }

    .btn-airline-submit:disabled {
      opacity: 0.6;
      cursor: not-allowed;
      box-shadow: none;
    }

    .spinner-inline {
      width: 16px;
      height: 16px;
      border: 2px solid rgba(255,255,255,0.3);
      border-top-color: #ffffff;
      border-radius: 50%;
      animation: spin 0.6s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }

    @media (max-width: 850px) {
      .checkout-main-content {
        grid-template-columns: 1fr;
      }
      .form-actions-grid {
        grid-template-columns: 1fr;
      }
      .btn-airline-back {
        order: 2;
      }
    }
</style>