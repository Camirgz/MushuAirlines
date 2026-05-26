import { createApp } from 'vue'
import App from './App.vue'
import {createRouter, createWebHistory} from "vue-router";
import LandingPage from "./components/LandingPage.vue";
import LoginForm from './components/LoginForm.vue';
import LandingPageInter from './components/LandingPageInter.vue';
import AdminCreateEmployee from './components/CreateProfile.vue';
import CompleteRegister from './components/CompleteRegister.vue';
import AircraftTypesPage from './components/admin/AircraftTypesPage.vue';
import CreateAircraftType from './components/admin/CreateAircraftType.vue';
import RoutesPage from './components/admin/RoutesPage.vue';
import RouteCreationForm from './components/admin/RouteCreationForm.vue';
import AirportsPage from './components/admin/AirportsPage.vue';
import AirportCreationForm from './components/admin/AirportCreationForm.vue';
import UsersPage from './components/admin/UsersPage.vue';
import PurchaseConfirmation from './components/PurchaseConfirmation.vue';

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", name: "Landing", component: LandingPage },
        { path: "/login", name: "Login", component: LoginForm },
        { path: "/admin", name: "Admin", component: LandingPageInter, meta: { requiresAuth: true, allowedRoles: ["Administrator", "Operator"] } },
        { path: "/create-profile", name: "createProfile", component: AdminCreateEmployee, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/complete-register", name: "CompleteRegister", component: CompleteRegister },
        { path: "/admin/aircraft-types", name: "AircraftTypes", component: AircraftTypesPage, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/admin/aircraft-types/create", name: "CreateAircraftType", component: CreateAircraftType, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/admin/routes", name: "Routes", component: RoutesPage, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/admin/routes/create-route", name: "RouteCreation", component: RouteCreationForm, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/admin/airports", name: "Airports", component: AirportsPage, meta: { requiresAuth: true, allowedRoles: ["Administrator"]} },
        { path: "/admin/airports/create-airport", name: "AirportCreation", component: AirportCreationForm, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/admin/users", name: "Users", component: UsersPage, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/purchase-confirmation/:id", name: "PurchaseConfirmation", component: PurchaseConfirmation }
    ],
});

function getRoleFromToken() {
    const token = localStorage.getItem("token");
    if (!token) return null;
    try {
        const payload = JSON.parse(atob(token.split(".")[1]));
        console.log("Payload del token:", payload);
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
}

router.beforeEach((to, from, next) => {
    const token = localStorage.getItem("token");

    const requiresAuth = to.matched.some(route => route.meta.requiresAuth);

    const allowedRoles = to.matched.flatMap(route => route.meta.allowedRoles || []);

    if (requiresAuth && !token) {
        sessionStorage.setItem(
            "authMessage",
            "Debe iniciar sesión para acceder a esta página."
        );

        return next("/");
    }

    const role = getRoleFromToken();

    if (allowedRoles.length > 0) {
        if (!role || !allowedRoles.includes(role)) {
            sessionStorage.setItem(
                "authMessage",
                "Usuario no autorizado."
            );

            if (role === "Operator") {
                return next("/admin");
            }

            localStorage.removeItem("token");
            return next("/");
        }
    }

    next();
});
createApp(App).use(router).mount('#app')