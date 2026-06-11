<template>
  <section class="admin-card">
    <slot></slot>
  </section>
</template>

<script>
export default {
  name: "AdminCard",
};
</script>

<style scoped>
.admin-card {
  background: #ffffff;
  border-radius: 16px;
  padding: 28px 32px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.12);
}

.admin-card :deep(h2) {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 18px;
}

@media (max-width: 768px) {
  .admin-card {
    padding: 24px 20px;
  }
}
</style>
