<template>
  <RouterView />

  <div v-if="popupMessage" class="popup-overlay">
    <div class="popup-card">
      <div class="popup-icon">
        <i class="bi bi-exclamation-triangle"></i>
      </div>

      <h3>Aviso</h3>

      <p>{{ popupMessage }}</p>

      <button @click="closePopup">
        Aceptar
      </button>
    </div>
  </div>
</template>

<script>
export default {
  name: "App",

  data() {
    return {
      popupMessage: null,
    };
  },

  mounted() {
    this.showAuthMessage();
  },

  watch: {
    $route() {
      this.showAuthMessage();
    },
  },

  methods: {
    showAuthMessage() {
      const message = sessionStorage.getItem("authMessage");

      if (message) {
        this.popupMessage = message;
        sessionStorage.removeItem("authMessage");
      }
    },

    closePopup() {
      this.popupMessage = null;
    },
  },
};
</script>

<style scoped>
.popup-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(15, 23, 42, 0.45);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
}

.popup-card {
  width: 360px;
  background: #ffffff;
  border-radius: 16px;
  padding: 28px;
  text-align: center;
  box-shadow: 0 18px 40px rgba(15, 23, 42, 0.25);
}

.popup-icon {
  width: 58px;
  height: 58px;
  margin: 0 auto 14px;
  border-radius: 50%;
  background: #fff4ed;
  color: #f01818;
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 1.6rem;
}

.popup-card h3 {
  margin: 0 0 10px;
  font-size: 1.3rem;
  font-weight: 800;
  color: #111827;
}

.popup-card p {
  margin: 0 0 22px;
  color: #4b5563;
  font-size: 0.95rem;
}

.popup-card button {
  border: none;
  background: linear-gradient(135deg, #f01818 0%, #ff5a00 45%, #ffc400 100%);
  color: white;
  padding: 10px 24px;
  border-radius: 10px;
  font-weight: 800;
  cursor: pointer;
}

.popup-card button:hover {
  opacity: 0.92;
}
</style>
