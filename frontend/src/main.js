import { createApp } from 'vue'
import App from './App.vue'
import './assets/styles/main.css'
import {createRouter, createWebHistory} from "vue-router";
import LandingPage from "./components/LandingPage.vue";
import LoginForm from './components/login/LoginForm.vue';
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
import PassengerInfoPage from './components/PassengerInfoPage.vue';
import PurchaseConfirmation from './components/PurchaseConfirmation.vue';
import ProfilePage from './components/admin/ProfilePage.vue';
import PaymentForm from './components/PaymentForm.vue';
import ReservationLogin from "./components/login/ReservationLogin.vue";
import ReservationReport from "./components/reports/ReservationReport.vue";
import FlightDetailReport from "./components/reports/FlightDetailReport.vue";
import MonthlyIncomeReport from "./components/reports/MonthlyIncomeReport.vue";

import "@/assets/styles/admin-shared.css";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", name: "Landing", component: LandingPage },
        { path: "/login", name: "Login", component: LoginForm },
        { path: "/admin", name: "Admin", component: LandingPageInter, meta: { requiresAuth: true, allowedRoles: ["Administrator", "Operator"] } },
        { path: "/create-profile", name: "createProfile", component: AdminCreateEmployee, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/complete-register", name: "CompleteRegister", component: CompleteRegister },
        { path: "/admin/aircraft-types", name: "AircraftTypes", component: AircraftTypesPage, meta: { requiresAuth: true, allowedRoles: ["Administrator", "Operator"] } },
        { path: "/admin/aircraft-types/create", name: "CreateAircraftType", component: CreateAircraftType, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/admin/routes", name: "Routes", component: RoutesPage, meta: { requiresAuth: true, allowedRoles: ["Administrator", "Operator"] } },
        { path: "/admin/routes/create-route", name: "RouteCreation", component: RouteCreationForm, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/admin/airports", name: "Airports", component: AirportsPage, meta: { requiresAuth: true, allowedRoles: ["Administrator", "Operator"]} },
        { path: "/admin/airports/create-airport", name: "AirportCreation", component: AirportCreationForm, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/admin/users", name: "Users", component: UsersPage, meta: { requiresAuth: true, allowedRoles: ["Administrator"] } },
        { path: "/purchase/passengers", name: "PassengerInfo", component: PassengerInfoPage },
        { path: "/purchase-confirmation/:id", name: "PurchaseConfirmation", component: PurchaseConfirmation },
        { path: "/admin/profile", name: "ProfilePage", component: ProfilePage, meta: { requiresAuth: true, allowedRoles: ["Administrator", "Operator"] } },
        { path: "/payment", name: "Payment", component: PaymentForm },
        { path: "/my-reservation", name: "ReservationLogin",component: ReservationLogin},
        { path: "/my-reservation/report", name: "ReservationReport",component: ReservationReport, meta:{requiresAuth:true}},
        { path: "/admin/reports/flight-detail", name: "FlightDetailReport", component: FlightDetailReport, meta: { requiresAuth: true, allowedRoles: ["Administrator", "Operator"] } },
        { path: "/admin/reports/monthly-income", name: "MonthlyIncomeReport", component: MonthlyIncomeReport, meta: { requiresAuth: true, allowedRoles: ["Administrator", "Operator"] } },
    ],
});

function getRoleFromToken() {
    const token = localStorage.getItem("token");
    if (!token) return null;
    try {
        const payload = JSON.parse(atob(token.split(".")[1]));
        let role =
            payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ||
            payload.role ||
            payload.Role ||
            null;
        if (role === "Administrador") {
            role = "Administrator";
        }
        if (role === "Operador") {
            role = "Operator";
        }
        return role;
    } catch (error) {
        console.error("Error leyendo el token:", error);
        return null;
    }
}
router.beforeEach((to, from, next) => {

    if (to.path.startsWith("/my-reservation")) {

        const reservationToken = localStorage.getItem("reservationToken");

        if (!reservationToken && to.meta.requiresAuth) {
            return next("/my-reservation");
        }

        return next();
    }

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