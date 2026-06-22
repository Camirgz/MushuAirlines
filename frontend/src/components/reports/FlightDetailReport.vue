<template>
  <AdminPageLayout>
    <AdminHero
      title="Vuelo Detallado"
      subtitle="Reporte de detalle de vuelos por fecha, ruta y clase"
      icon="bi bi-airplane-fill"
      back-to="/admin"
      back-text="Volver al panel"
    />

    <AdminCard>
      <h2><i class="bi bi-funnel"></i> Filtros</h2>
      <div class="filters-grid">

        <div class="form-group">
          <label>Origen</label>
          <div class="autocomplete-wrapper">
            <input
              v-model="origenQuery"
              type="text"
              placeholder="Código de aeropuerto"
              autocomplete="off"
              @input="onOrigenInput"
              @blur="onOrigenBlur"
            />
            <ul v-if="showOrigenDropdown && origenSuggestions.length" class="dropdown">
              <li
                v-for="s in origenSuggestions"
                :key="s.value"
                @mousedown.prevent="selectOrigen(s)"
              >
                <span class="suggestion-type airport">IATA</span>
                {{ s.label }}
              </li>
            </ul>
          </div>
        </div>

        <div class="form-group">
          <label>Destino</label>
          <div class="autocomplete-wrapper">
            <input
              v-model="destinoQuery"
              type="text"
              placeholder="Código de aeropuerto"
              autocomplete="off"
              @input="onDestinoInput"
              @blur="onDestinoBlur"
            />
            <ul v-if="showDestinoDropdown && destinoSuggestions.length" class="dropdown">
              <li
                v-for="s in destinoSuggestions"
                :key="s.value"
                @mousedown.prevent="selectDestino(s)"
              >
                <span class="suggestion-type airport">IATA</span>
                {{ s.label }}
              </li>
            </ul>
          </div>
        </div>

        <div class="form-group">
          <label>Clase</label>
          <select v-model="filtros.clase">
            <option value="">Todos</option>
            <option value="FirstClass">Primera Clase</option>
            <option value="Economy">Clase Turista</option>
          </select>
        </div>

        <div class="form-group">
          <label>Fecha desde</label>
          <input v-model="filtros.fechaDesde" type="date" />
        </div>

        <div class="form-group">
          <label>Fecha hasta</label>
          <input v-model="filtros.fechaHasta" type="date" />
        </div>

      </div>

      <div class="filter-actions">
        <button class="btn-primary" :disabled="loading" @click="cargarReporte">
          <i class="bi bi-search"></i>
          {{ loading ? 'Cargando...' : 'Buscar' }}
        </button>
        <button class="btn-secondary" @click="limpiarFiltros">
          <i class="bi bi-x-circle"></i>
          Limpiar
        </button>
      </div>
    </AdminCard>

    <div v-if="loadError" class="error-banner">
      <i class="bi bi-exclamation-circle-fill"></i> {{ loadError }}
    </div>

    <AdminCard v-if="rows.length > 0" class="results-card">
      <div class="table-header">
        <h2><i class="bi bi-table"></i> Resultados ({{ dataRows.length }} vuelos)</h2>
        <div class="export-actions">
          <button class="btn-excel" :disabled="exporting" @click="exportarExcel">
            <i class="bi bi-file-earmark-excel"></i>
            {{ exporting ? 'Exportando...' : 'Exportar a Excel' }}
          </button>
          <button class="btn-pdf" :disabled="exportingPdf" @click="exportarPdf">
            <i class="bi bi-file-earmark-pdf"></i>
            {{ exportingPdf ? 'Exportando...' : 'Exportar a PDF' }}
          </button>
        </div>
      </div>

      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Fecha</th>
              <th>Origen</th>
              <th>Destino</th>
              <th>N° Vuelo</th>
              <th>1ª Clase</th>
              <th>Economía</th>
              <th>Aerolínea</th>
              <th>Venta Pasajeros</th>
              <th>Venta Equipajes</th>
              <th>Total Venta</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(row, idx) in dataRows" :key="idx">
              <td>{{ formatDate(row.fecha) }}</td>
              <td><span class="airport-badge">{{ row.origen }}</span></td>
              <td><span class="airport-badge">{{ row.destino }}</span></td>
              <td>{{ row.numeroVuelo }}</td>
              <td>{{ row.pasajerosPrimeraClase }}</td>
              <td>{{ row.pasajerosEconomia }}</td>
              <td>{{ row.aerolinea }}</td>
              <td>{{ formatMoney(row.ventaPasajeros) }}</td>
              <td>{{ formatMoney(row.ventaEquipajes) }}</td>
              <td>{{ formatMoney(row.totalVenta) }}</td>
            </tr>
          </tbody>
          <tfoot v-if="totalsRow">
            <tr class="totals-row">
              <td colspan="4"><strong>TOTALES</strong></td>
              <td><strong>{{ totalsRow.pasajerosPrimeraClase }}</strong></td>
              <td><strong>{{ totalsRow.pasajerosEconomia }}</strong></td>
              <td>{{ totalsRow.aerolinea }}</td>
              <td><strong>{{ formatMoney(totalsRow.ventaPasajeros) }}</strong></td>
              <td><strong>{{ formatMoney(totalsRow.ventaEquipajes) }}</strong></td>
              <td><strong>{{ formatMoney(totalsRow.totalVenta) }}</strong></td>
            </tr>
          </tfoot>
        </table>
      </div>
    </AdminCard>

    <AdminCard v-else-if="searched && !loading && rows.length === 0" class="results-card">
      <div class="empty-state">
        <i class="bi bi-inbox"></i>
        <p>No se encontraron vuelos con los filtros aplicados.</p>
      </div>
    </AdminCard>
  </AdminPageLayout>
</template>

<script>
import AdminPageLayout from '@/components/layout/AdminPageLayout.vue';
import AdminHero       from '@/components/admin/ui/AdminHero.vue';
import AdminCard       from '@/components/admin/ui/AdminCard.vue';
import { getFlightDetailReport, downloadFlightDetailExcel, downloadFlightDetailPdf } from '@/services/FlightReportsService';
import API_BASE_URL from '@/config/api';

export default {
  name: 'FlightDetailReport',
  components: { AdminPageLayout, AdminHero, AdminCard },

  data() {
    return {
      filtros: { clase: '', fechaDesde: '', fechaHasta: '' },
      origenQuery: '',
      selectedOrigen: null,
      origenSuggestions: [],
      showOrigenDropdown: false,
      destinoQuery: '',
      selectedDestino: null,
      destinoSuggestions: [],
      showDestinoDropdown: false,
      rows: [],
      loading: false,
      exporting: false,
      exportingPdf: false,
      loadError: null,
      searched: false,
    };
  },

  computed: {
    dataRows() {
      return this.rows.filter(r => r.fecha !== null);
    },
    totalsRow() {
      return this.rows.find(r => r.fecha === null) ?? null;
    },
  },

  methods: {
    async fetchAirportSuggestions(q) {
      const res = await fetch(`${API_BASE_URL}/api/airport/suggestions?q=${encodeURIComponent(q)}`);
      const all = await res.json();
      return all.filter(s => s.type === 'airport');
    },

    async onOrigenInput() {
      this.selectedOrigen = null;
      this.showOrigenDropdown = true;
      const q = this.origenQuery.trim();
      if (!q) { this.origenSuggestions = []; return; }
      try {
        this.origenSuggestions = await this.fetchAirportSuggestions(q);
      } catch { this.origenSuggestions = []; }
    },
    onOrigenBlur() { setTimeout(() => { this.showOrigenDropdown = false; }, 200); },
    selectOrigen(s) {
      this.selectedOrigen = s.value;
      this.origenQuery = s.label;
      this.origenSuggestions = [];
      this.showOrigenDropdown = false;
    },

    async onDestinoInput() {
      this.selectedDestino = null;
      this.showDestinoDropdown = true;
      const q = this.destinoQuery.trim();
      if (!q) { this.destinoSuggestions = []; return; }
      try {
        this.destinoSuggestions = await this.fetchAirportSuggestions(q);
      } catch { this.destinoSuggestions = []; }
    },
    onDestinoBlur() { setTimeout(() => { this.showDestinoDropdown = false; }, 200); },
    selectDestino(s) {
      this.selectedDestino = s.value;
      this.destinoQuery = s.label;
      this.destinoSuggestions = [];
      this.showDestinoDropdown = false;
    },

    buildFilters() {
      return {
        origen:     this.selectedOrigen  ?? (this.origenQuery.trim()  || undefined),
        destino:    this.selectedDestino ?? (this.destinoQuery.trim() || undefined),
        clase:      this.filtros.clase      || undefined,
        fechaDesde: this.filtros.fechaDesde || undefined,
        fechaHasta: this.filtros.fechaHasta || undefined,
      };
    },

    handleError(err) {
      if (err.status === 401) {
        localStorage.removeItem('token');
        sessionStorage.setItem('authMessage', 'Su sesión expiró. Por favor inicie sesión nuevamente.');
        this.$router.push('/login');
        return true;
      }
      return false;
    },

    async cargarReporte() {
      this.loading   = true;
      this.loadError = null;
      this.searched  = true;
      try {
        this.rows = await getFlightDetailReport(this.buildFilters());
      } catch (err) {
        if (!this.handleError(err)) {
          this.loadError = err.message;
          this.rows = [];
        }
      } finally {
        this.loading = false;
      }
    },

    async exportarExcel() {
      this.exporting = true;
      try {
        const blob = await downloadFlightDetailExcel(this.buildFilters());
        this.triggerDownload(blob, 'ReporteVueloDetallado.xlsx');
      } catch (err) {
        if (!this.handleError(err)) this.loadError = err.message;
      } finally {
        this.exporting = false;
      }
    },

    async exportarPdf() {
      this.exportingPdf = true;
      try {
        const blob = await downloadFlightDetailPdf(this.buildFilters());
        this.triggerDownload(blob, 'ReporteVueloDetallado.pdf');
      } catch (err) {
        if (!this.handleError(err)) this.loadError = err.message;
      } finally {
        this.exportingPdf = false;
      }
    },

    triggerDownload(blob, filename) {
      const url  = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    },

    limpiarFiltros() {
      this.filtros         = { clase: '', fechaDesde: '', fechaHasta: '' };
      this.origenQuery     = '';
      this.selectedOrigen  = null;
      this.destinoQuery    = '';
      this.selectedDestino = null;
      this.rows            = [];
      this.loadError       = null;
      this.searched        = false;
    },

    formatDate(fecha) {
      if (!fecha) return '';
      const d = new Date(fecha);
      return d.toLocaleDateString('es-CR', { day: '2-digit', month: '2-digit', year: 'numeric' });
    },

    formatMoney(value) {
      if (value === null || value === undefined) return '';
      return new Intl.NumberFormat('es-CR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);
    },
  },
};
</script>

<style scoped>
.filters-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.form-group label {
  font-size: 0.8rem;
  font-weight: 700;
  color: #666f83;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.form-group input,
.form-group select {
  height: 40px;
  border: 1.5px solid #d1d5db;
  border-radius: 8px;
  padding: 0 12px;
  font-size: 0.9rem;
  color: #001233;
  outline: none;
  transition: border-color 0.2s;
}

.form-group input:focus,
.form-group select:focus {
  border-color: #e8631a;
  box-shadow: 0 0 0 3px rgba(232, 99, 26, 0.12);
}

.autocomplete-wrapper {
  position: relative;
}

.autocomplete-wrapper input {
  width: 100%;
}

.dropdown {
  position: absolute;
  top: calc(100% + 4px);
  left: 0;
  right: 0;
  background: #fff;
  border: 1.5px solid #d1d5db;
  border-radius: 8px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
  max-height: 200px;
  overflow-y: auto;
  list-style: none;
  padding: 4px 0;
  margin: 0;
  z-index: 100;
}

.dropdown li {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 9px 14px;
  font-size: 0.88rem;
  color: #333;
  cursor: pointer;
}

.dropdown li:hover {
  background: #fff5f0;
  color: #e8631a;
}

.suggestion-type {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
  flex-shrink: 0;
}

.suggestion-type.airport {
  background: #fff0e5;
  color: #e8631a;
}

.filter-actions {
  display: flex;
  gap: 12px;
}

.btn-primary {
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 10px 20px;
  font-size: 0.9rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: opacity 0.2s;
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-secondary {
  background: #f3f4f6;
  color: #1f2937;
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  font-size: 0.9rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: background 0.2s;
}

.btn-secondary:hover {
  background: #e5e7eb;
}

.results-card {
  margin-top: 12px;
}

.error-banner {
  background: #fef2f2;
  color: #991b1b;
  border: 1px solid #fecaca;
  border-radius: 12px;
  padding: 14px 16px;
  margin-top: 16px;
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 0.92rem;
  font-weight: 700;
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.table-header h2 {
  margin: 0 !important;
}

.export-actions {
  display: flex;
  gap: 10px;
}

.btn-excel {
  background: #217346;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  font-size: 0.88rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: opacity 0.2s;
}

.btn-excel:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-excel:hover:not(:disabled) {
  opacity: 0.88;
}

.btn-pdf {
  background: #c0392b;
  color: #fff;
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  font-size: 0.88rem;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  transition: opacity 0.2s;
}

.btn-pdf:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-pdf:hover:not(:disabled) {
  opacity: 0.88;
}

.table-wrapper {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.87rem;
}

th {
  text-align: left;
  padding: 10px 14px;
  font-size: 0.72rem;
  color: #999;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  border-bottom: 2px solid #f0f0f0;
  white-space: nowrap;
}

td {
  padding: 12px 14px;
  color: #333;
  border-bottom: 1px solid #f5f5f5;
}

tr:hover td {
  background: #fafafa;
}

.totals-row td {
  border-top: 2px solid #f0f0f0;
  border-bottom: none;
  background: #f8f9fa;
  color: #e8631a;
}

.airport-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: #fff0ee;
  color: #e74c3c;
  border-radius: 6px;
  padding: 3px 8px;
  font-size: 0.78rem;
  font-weight: 700;
}

.empty-state {
  text-align: center;
  padding: 40px 0;
  color: #aaa;
}

.empty-state i {
  font-size: 2.5rem;
  display: block;
  margin-bottom: 10px;
}

@media (max-width: 768px) {
  .filters-grid {
    grid-template-columns: 1fr;
  }
}
</style>
