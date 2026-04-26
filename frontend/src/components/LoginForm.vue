<template>
    <div>
    <h1>Login del dragón</h1>
        <div class="d-flex justify-content-center align-items-center vh-100">
            <div class="card p-4 shadow" style="max-width: 400px; width: 100%">
                <h3 class="text-center">Login administrativo</h3>
                <form @submit.prevent="saveLogin">
                    <div class="form-group">
                        <label for="username">Username:</label>
                        <input
                        v-model="form.username"
                        type="username"
                        id="username"
                        class="form-control"
                        required
                        />
                    </div>
                    <div class="form-group">
                        <label for="password">Password:</label>
                        <input
                        v-model="form.password"
                        type="password"
                        id="password"
                        class="form-control"
                        required
                        />
                    </div>
                    <div>
                        <button type="submit" class="btn btn-success btn-block">
                            Acceder
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>

<script>
import axios from "axios";
export default {
    data() {
        return {
            form: { username: "", password: "", },
        };
    },
    methods: {
        saveLogin() {
            console.log("Datos a guardar:", this.form);
            axios.post("http://localhost:5103/api/login", {
                username: this.form.username,
                password: this.form.password,
            })
            .then(function(response) {
            if (response.data === true) {
                alert("Login correcto");
                window.location.href = "/";
            }})
            .catch(function(error) {
                alert("Credenciales incorrectas");
                console.log(error);
            });
        },
    },
};
</script>

<style>

</style>