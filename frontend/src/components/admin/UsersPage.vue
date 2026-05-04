<template>
  <div class="login-page">
    <div class="login-card">
      
      <h2 class="login-title">Lista de Usuarios</h2>
      <p class="login-subtitle">
        Administra los usuarios de Mushu Airlines
      </p>

      <RouterLink to="/create-profile" class="login-btn create-btn">
        + Crear nuevo usuario
      </RouterLink>

      <div class="users-placeholder">
        <p>No hay usuarios cargados aún...</p>
      </div>

      <RouterLink to="/admin" class="back-btn">
        ← Volver al panel
      </RouterLink>

    </div>
  </div>
</template>

<script>
export default {
  name: "UsersPage",
};
</script>

<style scoped>
.login-page{
  min-height: 100vh;
  background: linear-gradient(#d63031, #f39c12);
  display: flex;
  justify-content: center;
  align-items: center;
}

.login-card{
  background: white;
  width: 700px;
  padding: 30px;
  border-radius: 15px;
}

.login-title{
  text-align: center;
  font-weight: bold;
}

.login-subtitle{
  text-align: center;
  color: gray;
  margin-bottom: 25px;
}

.login-btn{
  display: block;
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: 8px;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: white;
  font-weight: bold;
  text-align: center;
  text-decoration: none;
  margin-bottom: 20px;
}

.users-placeholder{
  border: 1px dashed lightgray;
  padding: 20px;
  text-align: center;
  border-radius: 10px;
  margin-bottom: 20px;
}

.back-btn{
  display: block;
  text-align: center;
  color: #e74c3c;
  font-weight: bold;
  text-decoration: none;
}
</style>