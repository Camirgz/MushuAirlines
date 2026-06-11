<template>
  <footer class="app-footer">
    <div class="footer-content">
      <p class="footer-contact">
        Contacto:
        <a href="mailto:mushuairlines@gmail.com">
          mushuairlines@gmail.com
        </a>
      </p>

      <p class="footer-copy">
        © {{ currentYear }} Mushu Airlines. Todos los derechos reservados.
      </p>
    </div>
  </footer>
</template>

<script>
export default {
  name: "AppFooter",

  computed: {
    currentYear() {
      return new Date().getFullYear();
    },
  },
};
</script>

<style scoped>
.app-footer {
  background: #ffffff;
  border-top: 1px solid #f0f0f0;
  padding: 22px 24px;
  margin-top: 40px;
}

.footer-content {
  max-width: 940px;
  margin: 0 auto;
  text-align: center;
}

.footer-contact,
.footer-copy {
  margin: 0;
  font-size: 0.85rem;
  color: #888;
}

.footer-contact {
  margin-bottom: 5px;
}

.footer-contact a {
  color: #e74c3c;
  text-decoration: none;
  font-weight: 600;
}

.footer-contact a:hover {
  text-decoration: underline;
}

.footer-copy {
  color: #999;
}
</style>
