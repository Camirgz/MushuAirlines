import { createApp } from 'vue'
import App from './App.vue'
import {createRouter, createWebHistory} from "vue-router";
import LandingPage from "./components/LandingPage.vue";
import LoginForm from './components/LoginForm.vue';
import LandingPageInter from './components/LandingPageInter.vue';

import AdminCreateEmployee from './components/CreateProfile.vue';
import CompleteRegister from './components/CompleteRegister.vue';
import AircraftTypesPage from './components/admin/AircraftTypesPage.vue';
import RoutesPage from './components/admin/RoutesPage.vue';
import RouteCreationForm from './components/admin/RouteCreationForm.vue';
import AirportsPage from './components/admin/AirportsPage.vue';
import UsersPage from './components/admin/UsersPage.vue';

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", name: "Landing", component: LandingPage },
        { path: "/login", name: "Login", component: LoginForm },
        { path: "/admin", name: "Admin", component: LandingPageInter, meta: { requiresAuth: true } },
        { path: "/create-profile", name: "createProfile", component: AdminCreateEmployee, meta: { requiresAuth: true, requiresAdmin: true } },
        { path: "/complete-register", name: "CompleteRegister", component: CompleteRegister },

        { path: "/admin/aircraft-types", name: "AircraftTypes", component: AircraftTypesPage, meta: { requiresAuth: true } },
        { path: "/admin/routes", name: "Routes", component: RoutesPage, meta: { requiresAuth: true } },
        { path: "/admin/routes/create-route", name: "RouteCreation", component: RouteCreationForm, meta: { requiresAuth: true } },
        { path: "/admin/airports", name: "Airports", component: AirportsPage, meta: { requiresAuth: true } },
        { path: "/admin/users", name: "Users", component: UsersPage, meta: { requiresAuth: true } },
    ],
});

function getRoleFromToken() {
    const token = localStorage.getItem("token");
    if (!token) return null;

    const payload = JSON.parse(atob(token.split(".")[1]));
    return payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
}

router.beforeEach((to, from, next) => {
    const token = localStorage.getItem("token");
    if (to.meta.requiresAuth && !token) {
        return next("/login");
    } 
    if (to.meta.requiresAdmin) {
        const role = getRoleFromToken();

        if (role !== "Administrator") {
            // to the intern landing page, only admins
            return next("/admin"); 
        }
    }

    next();
});

createApp(App).use(router).mount('#app')