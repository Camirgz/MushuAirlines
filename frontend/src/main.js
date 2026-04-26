import { createApp } from 'vue'
import App from './App.vue'
import {createRouter, createWebHistory} from "vue-router";
import LandingPage from "./components/LandingPage.vue";
import LoginForm from './components/LoginForm.vue';

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path : "/", name: "Landing", component: LandingPage },
        { path : "/login", name: "Login", component: LoginForm },
    ],
});

createApp(App).use(router).mount('#app')
