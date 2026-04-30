<template>
  <div class="page">
    <h1>Lista de aeronaves</h1>
    <p>Aquí se administrarán los aeronaves de Mushu Airlines.</p>

    <RouterLink to="/admin" class="back-btn">
      Volver al panel
    </RouterLink>
  </div>
</template>

<script>
export default {
  name: "AircraftTypesPage",
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
