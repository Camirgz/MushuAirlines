<template>
  <div>
    <!-- Navbar -->
    <nav class="navbar bg-white shadow-sm px-4 py-2">
      <a class="navbar-brand d-flex align-items-center gap-2" href="#">
        <img src="@/assets/logo.png" alt="Logo" width="42" height="42" class="rounded-2" />
        <div>
          <div class="brand-name">Mushu Airlines</div>
          <div class="brand-tagline">Vuela con el dragón</div>
        </div>
      </a>
      <div class="d-flex align-items-center gap-4">
        <a href="#" class="nav-link-item">
          <i class="bi bi-briefcase me-1"></i>Mis vuelos
        </a>
        <a href="#" class="nav-link-item">
          <i class="bi bi-calendar-check me-1"></i>Check-in
        </a>
        <a href="/login" class="btn btn-outline-danger rounded-pill px-3 py-1 admin-btn">
          <i class="bi bi-person me-1"></i>Admin Login
        </a>
      </div>
    </nav>

    <!-- Hero Section -->
    <div class="hero-section">
      <div class="hero-plane-bg">✈</div>
      <div class="hero-content">
        <h1 class="hero-title">Encuentra tu vuelo perfecto</h1>
        <p class="hero-subtitle">Viaja con Mushu Airlines a más de 100 destinos alrededor del mundo</p>

        <!-- Search Card -->
        <div class="search-card">
          <!-- Trip type -->
          <div class="trip-type mb-3">
            <label class="trip-option">
              <input type="radio" name="tripType" value="ida-vuelta" v-model="tripType" />
              <span>Ida y vuelta</span>
            </label>
            <label class="trip-option">
              <input type="radio" name="tripType" value="solo-ida" v-model="tripType" />
              <span>Solo ida</span>
            </label>
            <label class="trip-option">
              <input type="radio" name="tripType" value="multiciudad" v-model="tripType" />
              <span>Multiciudad</span>
            </label>
          </div>

          <!-- Search fields -->
          <div class="search-fields" :class="{ 'three-cols': tripType === 'solo-ida' }">
            <div class="field-group">
              <label class="field-label">Origen</label>
              <div class="input-box">
                <i class="bi bi-geo-alt field-icon"></i>
                <input type="text" placeholder="Ciudad o aeropuerto" />
              </div>
            </div>
            <div class="field-group">
              <label class="field-label">Destino</label>
              <div class="input-box">
                <i class="bi bi-geo-alt field-icon"></i>
                <input type="text" placeholder="Ciudad o aeropuerto" />
              </div>
            </div>
            <div class="field-group">
              <label class="field-label">Fecha de ida</label>
              <div class="input-box">
                <input type="date" v-model="fechaIda" />
              </div>
            </div>
            <div class="field-group" v-if="tripType !== 'solo-ida'">
              <label class="field-label">Fecha de regreso</label>
              <div class="input-box">
                <input type="date" v-model="fechaRegreso" />
              </div>
            </div>
          </div>

          <button class="search-btn">
            <i class="bi bi-search me-2"></i>Buscar vuelos
          </button>
        </div>
      </div>
    </div>

    <!-- Adventure Section -->
    <div class="adventure-section">
      <h2 class="adventure-title">¿Listo para tu próxima aventura?</h2>
      <p class="adventure-subtitle">Comienza tu búsqueda arriba y descubre nuestras mejores ofertas</p>
    </div>
  </div>
</template>

<script>
export default {
  name: "LandingPage",
  data() {
    return {
      tripType: "ida-vuelta",
      fechaIda: "",
      fechaRegreso: "",
    };
  },
};
</script>

<style scoped>
/* Navbar */
.navbar {
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.brand-name {
  font-weight: 700;
  font-size: 1.05rem;
  color: #1a1a1a;
  line-height: 1.2;
}

.brand-tagline {
  font-size: 0.7rem;
  color: #888;
}

.nav-link-item {
  text-decoration: none;
  color: #333;
  font-size: 0.92rem;
  display: flex;
  align-items: center;
}

.nav-link-item:hover {
  color: #e74c3c;
}

.admin-btn {
  font-size: 0.88rem;
  border-color: #e74c3c;
  color: #e74c3c;
}

.admin-btn:hover {
  background: #e74c3c;
  color: white;
}

/* Hero */
.hero-section {
  background: linear-gradient(135deg, #c0392b 0%, #e74c3c 35%, #e67e22 70%, #f0a500 100%);
  min-height: 460px;
  display: flex;
  align-items: center;
  padding: 56px 24px 72px;
  position: relative;
  overflow: hidden;
}

.hero-plane-bg {
  position: absolute;
  right: 8%;
  top: 50%;
  transform: translateY(-60%) rotate(-15deg);
  font-size: 220px;
  opacity: 0.12;
  color: white;
  pointer-events: none;
  user-select: none;
}

.hero-content {
  max-width: 920px;
  margin: 0 auto;
  width: 100%;
  position: relative;
  z-index: 1;
}

.hero-title {
  color: white;
  font-size: 2.8rem;
  font-weight: 800;
  margin-bottom: 10px;
}

.hero-subtitle {
  color: rgba(255, 255, 255, 0.9);
  font-size: 1.05rem;
  margin-bottom: 28px;
}

/* Search Card */
.search-card {
  background: white;
  border-radius: 16px;
  padding: 28px 32px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.18);
}

/* Trip type */
.trip-type {
  display: flex;
  gap: 24px;
  align-items: center;
}

.trip-option {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  font-size: 0.92rem;
  color: #555;
  margin: 0;
}

.trip-option input[type="radio"] {
  accent-color: #1a1a1a;
  width: 14px;
  height: 14px;
  cursor: pointer;
}

/* Search fields */
.search-fields {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 20px;
}

.search-fields.three-cols {
  grid-template-columns: repeat(3, 1fr);
}

.field-label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: #333;
  margin-bottom: 6px;
}

.input-box {
  display: flex;
  align-items: center;
  border: 1.5px solid #e0e0e0;
  border-radius: 8px;
  padding: 10px 12px;
  gap: 8px;
  background: white;
  transition: border-color 0.2s;
}

.input-box:focus-within {
  border-color: #e74c3c;
}

.field-icon {
  color: #bbb;
  font-size: 0.95rem;
  flex-shrink: 0;
}

.input-box input[type="text"] {
  border: none;
  outline: none;
  width: 100%;
  font-size: 0.88rem;
  color: #333;
}

.input-box input[type="text"]::placeholder {
  color: #bbb;
}

.input-box input[type="date"] {
  border: none;
  outline: none;
  width: 100%;
  font-size: 0.88rem;
  color: #999;
  cursor: pointer;
}

.input-box input[type="date"]:valid:not([value=""]) {
  color: #333;
}

/* Search button */
.search-btn {
  width: 100%;
  padding: 14px;
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: white;
  font-size: 1rem;
  font-weight: 600;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: opacity 0.2s;
}

.search-btn:hover {
  opacity: 0.9;
}

/* Adventure section */
.adventure-section {
  text-align: center;
  padding: 64px 24px;
  background: #f8f9fa;
}

.adventure-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1a1a1a;
  margin-bottom: 10px;
}

.adventure-subtitle {
  color: #888;
  font-size: 0.95rem;
}
</style>
