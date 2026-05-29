<template>
  <section class="admin-hero">
    <RouterLink
      v-if="backTo"
      :to="backTo"
      class="hero-back-link"
    >
      <i class="bi bi-arrow-left"></i>
      {{ backText }}
    </RouterLink>

    <div class="hero-content">
      <i :class="icon" class="hero-icon"></i>

      <div>
        <h1>{{ title }}</h1>
        <p>{{ subtitle }}</p>
      </div>
    </div>
  </section>
</template>

<script>
export default {
  name: "AdminHero",

  props: {
    title: {
      type: String,
      required: true,
    },

    subtitle: {
      type: String,
      required: true,
    },

    icon: {
      type: String,
      default: "bi bi-airplane",
    },

    backTo: {
      type: String,
      default: "",
    },

    backText: {
      type: String,
      default: "Volver",
    },
  },
};
</script>

<style scoped>
.admin-hero {
  background: linear-gradient(135deg, #c0392b 0%, #e74c3c 35%, #e67e22 70%, #f0a500 100%);
  color: #ffffff;
  border-radius: 16px;
  padding: 40px 42px;
  display: flex;
  flex-direction: column;
  gap: 18px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.18);
  margin-bottom: 28px;
  position: relative;
  overflow: hidden;
}

.admin-hero::after {
  position: absolute;
  right: 32px;
  top: 50%;
  transform: translateY(-50%) rotate(-15deg);
  font-size: 120px;
  opacity: 0.12;
  color: white;
  pointer-events: none;
  user-select: none;
}

.hero-content {
  display: flex;
  align-items: center;
  gap: 22px;
  position: relative;
  z-index: 1;
}

.hero-icon {
  font-size: 2.5rem;
  position: relative;
  z-index: 1;
}

.admin-hero h1 {
  color: white;
  font-size: 2.4rem;
  font-weight: 800;
  margin: 0 0 10px;
}

.admin-hero p {
  color: rgba(255, 255, 255, 0.9);
  font-size: 1.05rem;
  margin: 0;
}

.hero-back-link {
  width: fit-content;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  color: #ffffff;
  text-decoration: none;
  font-weight: 700;
  font-size: 0.92rem;
  position: relative;
  z-index: 1;
}

.hero-back-link:hover {
  color: #ffffff;
  opacity: 0.88;
}

@media (max-width: 768px) {
  .admin-hero {
    padding: 30px 26px;
  }

  .hero-content {
    align-items: flex-start;
  }

  .hero-icon {
    font-size: 2.1rem;
  }

  .admin-hero h1 {
    font-size: 1.75rem;
  }
}
</style>
