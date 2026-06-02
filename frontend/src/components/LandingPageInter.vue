<template>
  <AdminPageLayout>
    <AdminHero
      title="Panel de administración"
      subtitle="Panel de administración para operadores de Mushu Airlines"
      icon="bi bi-airplane"
    />

    <AdminCard v-if="isInternalUser">
      <h2>Administración de vuelos de la aerolínea</h2>

      <div class="option-list">
        <RouterLink to="/admin/aircraft-types" class="admin-option">
          <div class="option-left">
            <div class="option-icon">
              <i class="bi bi-airplane"></i>
            </div>
            <span>Tipos de aeronaves</span>
          </div>

          <i class="bi bi-chevron-right option-arrow"></i>
        </RouterLink>

        <RouterLink to="/admin/routes" class="admin-option">
          <div class="option-left">
            <div class="option-icon">
              <i class="bi bi-geo-alt"></i>
            </div>
            <span>Rutas</span>
          </div>

          <i class="bi bi-chevron-right option-arrow"></i>
        </RouterLink>

        <RouterLink to="/admin/airports" class="admin-option">
          <div class="option-left">
            <div class="option-icon">
              <i class="bi bi-airplane-engines"></i>
            </div>
            <span>Aeropuertos</span>
          </div>

          <i class="bi bi-chevron-right option-arrow"></i>
        </RouterLink>
      </div>

      <hr class="section-line" />

      <div v-if="isAdmin">
        <h2>Administración de usuarios de la aerolínea</h2>
  
        <div class="option-list">
          <RouterLink to="/admin/users" class="admin-option">
            <div class="option-left">
              <div class="option-icon">
                <i class="bi bi-people"></i>
              </div>
              <span>Usuarios administradores y operarios</span>
            </div>
  
            <i class="bi bi-chevron-right option-arrow"></i>
          </RouterLink>
        </div>
      </div>

      <hr class="section-line" />

      <h2>Sistema de reportes</h2>

      <div class="empty-reports">
        <div>
          <h3>Reportes no disponibles por el momento</h3>
          <p>
            Esta sección estará disponible en una futura actualización del sistema.
          </p>
        </div>
      </div>
    </AdminCard>
  </AdminPageLayout>
</template>

<script>
import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue"

export default {
  name: "LandingPageInter",

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
      userRole: null,
    };
  },

  computed: {
    isAdmin() {
      return this.userRole === "Administrator";
    },

    isOperator() {
      return this.userRole === "Operator";
    },

    isInternalUser() {
      return this.isAdmin || this.isOperator;
    },
  },

  mounted() {
    this.userRole = this.getRoleFromToken();
    console.log("Rol actual:", this.userRole);
  },

  methods: {
    getRoleFromToken() {
      const token = localStorage.getItem("token");

      if (!token) return null;

      try {
        const payload = JSON.parse(atob(token.split(".")[1]));

        return (
          payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
          payload.role ||
          payload.Role ||
          null
        );
      } catch (error) {
        console.error("Error leyendo el token:", error);
        return null;
      }
    },
  },
};
</script>

<style scoped>
.admin-page {
  min-height: 100vh;
  background: #f8f9fa;
  color: #1a1a1a;
}

.admin-main {
  max-width: 940px;
  margin: 0 auto;
  padding: 56px 24px 72px;
}

.admin-hero {
  background: linear-gradient(135deg, #c0392b 0%, #e74c3c 35%, #e67e22 70%, #f0a500 100%);
  color: #ffffff;
  border-radius: 16px;
  padding: 40px 42px;
  display: flex;
  align-items: center;
  gap: 22px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.18);
  margin-bottom: 28px;
  position: relative;
  overflow: hidden;
}

.admin-hero::after {
  content: '';
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

.hero-icon {
  font-size: 2.5rem;
  position: relative;
  z-index: 1;
}

.admin-hero div {
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

.admin-card {
  background: #ffffff;
  border-radius: 16px;
  padding: 28px 32px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.12);
}

.admin-card h2 {
  font-size: 1rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 18px;
}

.option-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.admin-option {
  text-decoration: none;
  color: #333;
  border: 1.5px solid #e0e0e0;
  border-radius: 8px;
  padding: 16px 18px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #ffffff;
  transition: border-color 0.2s ease, background 0.2s ease, box-shadow 0.2s ease;
}

.admin-option:hover {
  border-color: #e74c3c;
  background: #fff5f5;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.admin-option.router-link-active,
.admin-option.router-link-exact-active {
  border-color: #e74c3c;
  background: #fff5f5;
}

.option-left {
  display: flex;
  align-items: center;
  gap: 13px;
  font-size: 0.95rem;
  font-weight: 600;
}

.option-icon {
  width: 38px;
  height: 38px;
  background: #fff0ee;
  color: #e74c3c;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1rem;
  flex-shrink: 0;
}

.option-arrow {
  color: #bbb;
  transition: color 0.2s ease, transform 0.2s ease;
}

.admin-option:hover .option-arrow {
  color: #e74c3c;
  transform: translateX(3px);
}

.section-line {
  border: none;
  border-top: 1px solid #f0f0f0;
  margin: 26px 0;
}

.empty-reports {
  border: 1.5px dashed #e0e0e0;
  background: #f8f9fa;
  border-radius: 12px;
  padding: 22px 24px;
}

.empty-reports h3 {
  margin: 0 0 6px;
  font-size: 0.95rem;
  font-weight: 700;
  color: #333;
}

.empty-reports p {
  margin: 0;
  color: #888;
  font-size: 0.92rem;
  line-height: 1.5;
}
</style>
