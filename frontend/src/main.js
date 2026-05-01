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
        { path : "/admin", name: "Admin", component: LandingPageInter, meta: { requiresAuth: true }  },
    ],
});

router.beforeEach((to, from, next) => {
    // we check if the route requires authentication and if we have a token in localStorage
    const token = localStorage.getItem("token");

    if (to.meta.requiresAuth && !token) {
        next("/login");
    } else {
        next();
    }
});

createApp(App).use(router).mount('#app')
