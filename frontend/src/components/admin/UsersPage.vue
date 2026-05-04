<template>
  <div class="page">
    <h1>Lista de usuarios</h1>
    <p>Aquí se administrarán los usuarios de Mushu Airlines.</p>

    <RouterLink to="/admin" class="back-btn">
      Volver al panel
    </RouterLink>
  </div>
</template>

<script>
export default {
  name: "UsersPage",
};
</script>

<style scoped>
.page {
  padding: 40px;
}

.back-btn {
  color: #f01818;
  font-weight: 700;
}
</style>
