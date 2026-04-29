import { createApp } from 'vue'
import App from './App.vue'
import {createRouter, createWebHistory} from "vue-router";
import LandingPage from "./components/LandingPage.vue";
import LoginForm from './components/LoginForm.vue';
import LandingPageInter from './components/LandingPageInter.vue';

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path : "/", name: "Landing", component: LandingPage },
        { path : "/login", name: "Login", component: LoginForm },
        { path : "/admin", name: "Admin", component: LandingPageInter },
    ],
});

createApp(App).use(router).mount('#app')
