<template>
  <AdminPageLayout>
    <AdminHero
      title="Crear Aeronave"
      subtitle="Panel de administración para operadores de Mushu Airlines"
      icon="bi bi-airplane"
      back-to="/admin/aircraft-types"
      back-text="Volver a la lista"
    />

    <AdminCard class="create-aircraft-card">
      <h2>Crear Aeronave</h2>

      <form @submit.prevent="Submit">
        <div class="form-group">
          <label class="form-label">
            Modelo <span class="required">*</span>
          </label>

          <input
            v-model="Form.Model"
            type="text"
            class="form-input"
            placeholder="Ej: 737-800"
            required
          />
        </div>

        <div class="form-group">
          <label class="form-label">
            Tipo de aeronave <span class="required">*</span>
          </label>

          <input
            v-model="Form.Type"
            list="AircraftTypesList"
            type="text"
            class="form-input"
            placeholder="Ej: Avión comercial, Helicóptero..."
            @input="OnTypeNameChanged"
            required
          />

          <datalist id="AircraftTypesList">
            <option
              v-for="Option in AircraftTypeOptions"
              :key="Option.id ?? Option.Id"
              :value="Option.name ?? Option.Name"
            />
          </datalist>
        </div>

        <div class="form-group">
          <label class="form-label">
            Peso soportado (kg) <span class="required">*</span>
          </label>

          <input
            v-model.number="Form.WeightKg"
            type="number"
            class="form-input"
            placeholder="Ej: 75000"
            min="1"
            required
          />
        </div>

        <hr class="section-divider" />

        <h3 class="section-title">Primera Clase</h3>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">
              Cantidad de filas <span class="required">*</span>
            </label>

            <input
              v-model.number="Form.FirstClass.RowCount"
              type="number"
              class="form-input"
              placeholder="Ej: 4"
              min="1"
              required
            />
          </div>

          <div class="form-group">
            <label class="form-label">
              Asientos por fila <span class="required">*</span>
            </label>

            <input
              v-model.number="Form.FirstClass.SeatsPerRow"
              type="number"
              class="form-input"
              placeholder="Ej: 4"
              min="1"
              required
            />
          </div>
        </div>

        <hr class="section-divider" />

        <h3 class="section-title">Clase Turista</h3>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">
              Cantidad de filas <span class="required">*</span>
            </label>

            <input
              v-model.number="Form.EconomyClass.RowCount"
              type="number"
              class="form-input"
              placeholder="Ej: 20"
              min="0"
              required
            />
          </div>

          <div class="form-group">
            <label class="form-label">
              Asientos por fila <span class="required">*</span>
            </label>

            <input
              v-model.number="Form.EconomyClass.SeatsPerRow"
              type="number"
              class="form-input"
              placeholder="Ej: 6"
              min="0"
              required
            />
          </div>
        </div>

        <div
          class="seat-counter"
          :class="{ 'seat-counter--danger': TotalSeats >= 1000 }"
        >
          <i class="bi bi-person-fill me-2"></i>

          Capacidad total:
          <strong>{{ TotalSeats }} asientos</strong>

          <span v-if="TotalSeats >= 1000" class="seat-limit-msg">
            — máximo permitido: 999
          </span>
        </div>

        <div v-if="ErrorMessage" class="error-message">
          <i class="bi bi-exclamation-circle-fill"></i>
          <span>{{ ErrorMessage }}</span>
        </div>

        <div class="form-actions">
          <button
            type="button"
            class="cancel-btn"
            :disabled="IsSubmitting"
            @click="Cancel"
          >
            <i class="bi bi-x-lg me-2"></i>
            Cancelar
          </button>

          <button
            type="submit"
            class="submit-btn"
            :disabled="IsSubmitting || TotalSeats >= 1000"
          >
            <i class="bi bi-floppy me-2"></i>
            {{ IsSubmitting ? "Guardando..." : "Guardar" }}
          </button>
        </div>
      </form>
    </AdminCard>
  </AdminPageLayout>
</template>

<script>
import AdminPageLayout from "@/components/layout/AdminPageLayout.vue";
import AdminHero from "@/components/admin/ui/AdminHero.vue";
import AdminCard from "@/components/admin/ui/AdminCard.vue";

import {
  CreateAircraftType,
  GetAircraftTypeOptions,
} from "../../services/AircraftTypesService";

export default {
  name: "CreateAircraftType",

  components: {
    AdminPageLayout,
    AdminHero,
    AdminCard,
  },

  data() {
    return {
      IsSubmitting: false,
      ErrorMessage: "",
      AircraftTypeOptions: [],
      Form: {
        Model: "",
        Type: "",
        WeightKg: null,
        EconomyClass: {
          RowCount: null,
          SeatsPerRow: null,
        },
        FirstClass: {
          RowCount: null,
          SeatsPerRow: null,
        },
      },
    };
  },

  computed: {
    TotalSeats() {
      const Economy =
        (this.Form.EconomyClass.RowCount || 0) *
        (this.Form.EconomyClass.SeatsPerRow || 0);

      const FirstClass =
        (this.Form.FirstClass.RowCount || 0) *
        (this.Form.FirstClass.SeatsPerRow || 0);

      return Economy + FirstClass;
    },
  },

  mounted() {
    this.LoadAircraftTypeOptions();
  },

  methods: {
    LoadAircraftTypeOptions() {
      GetAircraftTypeOptions()
        .then((Response) => {
          this.AircraftTypeOptions = Response.data;
        })
        .catch(() => {});
    },

    OnTypeNameChanged() {
      const Option = this.AircraftTypeOptions.find((AircraftOption) => {
        return (
          (AircraftOption.Name ?? AircraftOption.name) === this.Form.Type
        );
      });

      if (!Option) {
        return;
      }

      const DefaultModel = Option.DefaultModel ?? Option.defaultModel;
      const DefaultWeightKg =
        Option.DefaultWeightKg ?? Option.defaultWeightKg;
      const DefaultEconomyRows =
        Option.DefaultEconomyRows ?? Option.defaultEconomyRows;
      const DefaultEconomySeatsPerRow =
        Option.DefaultEconomySeatsPerRow ??
        Option.defaultEconomySeatsPerRow;
      const DefaultFirstClassRows =
        Option.DefaultFirstClassRows ?? Option.defaultFirstClassRows;
      const DefaultFirstClassSeatsPerRow =
        Option.DefaultFirstClassSeatsPerRow ??
        Option.defaultFirstClassSeatsPerRow;

      if (DefaultModel) {
        this.Form.Model = DefaultModel;
      }

      if (DefaultWeightKg) {
        this.Form.WeightKg = DefaultWeightKg;
      }

      if (DefaultEconomyRows) {
        this.Form.EconomyClass.RowCount = DefaultEconomyRows;
      }

      if (DefaultEconomySeatsPerRow) {
        this.Form.EconomyClass.SeatsPerRow = DefaultEconomySeatsPerRow;
      }

      if (DefaultFirstClassRows) {
        this.Form.FirstClass.RowCount = DefaultFirstClassRows;
      }

      if (DefaultFirstClassSeatsPerRow) {
        this.Form.FirstClass.SeatsPerRow = DefaultFirstClassSeatsPerRow;
      }
    },

    Submit() {
      this.IsSubmitting = true;
      this.ErrorMessage = "";

      const Payload = {
        Model: this.Form.Model,
        Type: this.Form.Type,
        WeightKg: this.Form.WeightKg,
        EconomyRows: this.Form.EconomyClass.RowCount || 0,
        EconomySeatsPerRow: this.Form.EconomyClass.SeatsPerRow || 0,
        FirstClassRows: this.Form.FirstClass.RowCount,
        FirstClassSeatsPerRow: this.Form.FirstClass.SeatsPerRow,
      };

      CreateAircraftType(Payload)
        .then(() => {
          this.$router.push("/admin/aircraft-types");
        })
        .catch((Err) => {
          this.ErrorMessage = this.ParseError(Err);
        })
        .finally(() => {
          this.IsSubmitting = false;
        });
    },

    ParseError(Err) {
      if (!Err.response) {
        return "No se pudo conectar con el servidor. Verifique su conexión e intente de nuevo.";
      }

      const Data = Err.response.data;

      if (Data && typeof Data === "object") {
        if (Data.errors) {
          const Messages = Object.values(Data.errors).flat();

          if (Messages.length) {
            return Messages.join(" ");
          }
        }

        if (Data.title) {
          return Data.title;
        }

        return "Ocurrió un error inesperado. Intente de nuevo.";
      }

      if (typeof Data === "string") {
        if (Data.startsWith("Ya existe")) {
          return Data;
        }

        if (Data.startsWith("La capacidad total")) {
          return Data;
        }

        if (Data.includes("CHK_SeatsPerRow")) {
          return "El número de asientos por fila no está dentro del rango permitido para este tipo de aeronave.";
        }

        if (Data.includes("CHECK constraint")) {
          return "Los datos ingresados no cumplen las restricciones de la aeronave. Verifique los valores e intente de nuevo.";
        }

        if (Data.includes("PRIMARY KEY") || Data.includes("UNIQUE KEY")) {
          return "Ya existe una aeronave registrada con esos datos.";
        }

        if (Data.includes("FOREIGN KEY")) {
          return "Uno de los valores ingresados no corresponde a un registro existente.";
        }
      }

      return "Ocurrió un error al guardar la aeronave. Verifique los datos e intente de nuevo.";
    },

    Cancel() {
      this.$router.push("/admin/aircraft-types");
    },
  },
};
</script>

<style scoped>
.create-aircraft-card {
  padding: 36px;
}

.create-aircraft-card h2 {
  font-size: 1.35rem;
  font-weight: 900;
  color: #07172c;
  margin: 0 0 28px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 7px;
  margin-bottom: 20px;
}

.form-label {
  font-size: 0.9rem;
  font-weight: 700;
  color: #374151;
}

.required {
  color: #e74c3c;
}

.form-input {
  width: 100%;
  padding: 12px 16px;
  border: 1.5px solid #e5e7eb;
  border-radius: 10px;
  font-size: 0.95rem;
  color: #111827;
  background: #ffffff;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
  box-sizing: border-box;
}

.form-input::placeholder {
  color: #9ca3af;
}

.form-input:focus {
  outline: none;
  border-color: #ff5f00;
  box-shadow: 0 0 0 3px rgba(255, 95, 0, 0.12);
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.section-divider {
  border: none;
  border-top: 1.5px solid #e5e7eb;
  margin: 8px 0 24px;
}

.section-title {
  font-size: 1rem;
  font-weight: 800;
  color: #07172c;
  margin: 0 0 18px;
}

.seat-counter {
  margin-top: 20px;
  padding: 12px 18px;
  background: #f0fdf4;
  color: #166534;
  border: 1px solid #bbf7d0;
  border-radius: 10px;
  font-size: 0.92rem;
  display: flex;
  align-items: center;
  gap: 4px;
  transition: 0.2s ease;
}

.seat-counter--danger {
  background: #fff1f1;
  color: #b91c1c;
  border-color: #fecaca;
}

.seat-limit-msg {
  margin-left: 4px;
  font-weight: 700;
}

.error-message {
  display: flex;
  align-items: center;
  gap: 10px;
  border-radius: 12px;
  padding: 14px 16px;
  margin-top: 20px;
  font-size: 0.92rem;
  font-weight: 700;
  background: #fef2f2;
  color: #991b1b;
  border: 1px solid #fecaca;
}

.error-message i {
  font-size: 1.1rem;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 32px;
  padding-top: 24px;
  border-top: 1.5px solid #e5e7eb;
}

.cancel-btn,
.submit-btn {
  border: none;
  border-radius: 8px;
  padding: 12px 24px;
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  display: flex;
  align-items: center;
  transition: opacity 0.2s ease, transform 0.2s ease, background 0.2s ease;
}

.cancel-btn {
  background: #f3f4f6;
  color: #1f2937;
}

.cancel-btn:hover:not(:disabled) {
  background: #e5e7eb;
  transform: translateY(-1px);
}

.submit-btn {
  background: linear-gradient(to right, #e74c3c, #f39c12);
  color: #ffffff;
}

.submit-btn:hover:not(:disabled) {
  opacity: 0.9;
  transform: translateY(-1px);
}

.submit-btn:disabled,
.cancel-btn:disabled {
  opacity: 0.55;
  cursor: not-allowed;
  transform: none;
}

@media (max-width: 768px) {
  .create-aircraft-card {
    padding: 24px;
  }

  .form-row {
    grid-template-columns: 1fr;
    gap: 0;
  }

  .form-actions {
    flex-direction: column-reverse;
  }

  .cancel-btn,
  .submit-btn {
    width: 100%;
    justify-content: center;
  }

  .seat-counter {
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
