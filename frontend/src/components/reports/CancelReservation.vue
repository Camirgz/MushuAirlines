<template>
  <div class="page">

    <div class="card">

      <template v-if="!finished">

        <h2>Cancelar reserva</h2>

        <p>
          ¿Está seguro que desea cancelar esta reserva?
        </p>

        <p>
          Esta acción no se puede deshacer.
        </p>

        <div class="buttons">

          <button
            class="secondary"
            @click="$router.push('/')">

            Volver al inicio

          </button>

          <button
            class="primary"
            @click="confirmCancellation"
            :disabled="loading">

            {{ loading ? "Cancelando..." : "Cancelar reserva" }}

          </button>

        </div>

      </template>

      <template v-else>

        <h2>{{ error ? "Error" : "Reserva cancelada" }}</h2>

        <p>{{ message }}</p>

        <button
          class="primary"
          @click="$router.push('/')">

          Ir al inicio

        </button>

      </template>

    </div>

  </div>
</template>

<script>
import { cancelReservation } from "@/services/CancelReservation";

export default {

    name: "CancelReservation",

    data() {
        return {
            loading: false,
            finished: false,
            error: false,
            message: ""
        };
    },

    methods: {

        async confirmCancellation() {

            this.loading = true;

            try {

                const token = this.$route.params.token;

                const response = await cancelReservation(token);

                this.message = response.message;

            }
            catch (err) {

                this.error = true;
                this.message = err.message;

            }
            finally {

                this.loading = false;
                this.finished = true;

            }

        }

    }

}
</script>

<style scoped>

.page{
    min-height:100vh;
    display:flex;
    justify-content:center;
    align-items:center;
    background:#f5f5f5;
}

.card{
    width:430px;
    background:white;
    padding:40px;
    border-radius:14px;
    text-align:center;
    box-shadow:0 8px 20px rgba(0,0,0,.08);
}

.buttons{
    display:flex;
    gap:12px;
    margin-top:30px;
}

.buttons button{
    flex:1;
}

.primary{
    background:#d62828;
    color:white;
}

.secondary{
    background:white;
    border:1px solid #ccc;
}

button{
    padding:12px;
    border-radius:8px;
    cursor:pointer;
}

</style>