import { createApp } from 'vue'
import App from './App.vue'
import {createRouter, createWebHistory} from "vue-router";
import LandingPage from "./components/LandingPage.vue";
import LoginForm from './components/LoginForm.vue';
import LandingPageInter from './components/LandingPageInter.vue';
import AircraftTypesPage from './components/admin/AircraftTypesPage.vue';
import RoutesPage from './components/admin/RoutesPage.vue';
<<<<<<< Feature6_RouteCreation
import RouteCreationForm from './components/admin/RouteCreationForm.vue';
=======
>>>>>>> dev
import AirportsPage from './components/admin/AirportsPage.vue';
import UsersPage from './components/admin/UsersPage.vue';

const router = createRouter({
    history: createWebHistory(),
    routes: [
<<<<<<< Feature6_RouteCreation
        { path: "/", name: "Landing", component: LandingPage },
        { path: "/login", name: "Login", component: LoginForm },
        { path: "/admin", name: "Admin", component: LandingPageInter, meta: { requiresAuth: true } },
        { path: "/admin/aircraft-types", name: "AircraftTypes", component: AircraftTypesPage, meta: { requiresAuth: true } },
        { path: "/admin/routes", name: "Routes", component: RoutesPage, meta: { requiresAuth: true } },
        { path: "/admin/routes/create-route", name: "RouteCreation", component: RouteCreationForm, meta: { requiresAuth: true } },
        { path: "/admin/airports", name: "Airports", component: AirportsPage, meta: { requiresAuth: true } },
        { path: "/admin/users", name: "Users", component: UsersPage, meta: { requiresAuth: true } },
=======
        { path : "/", name: "Landing", component: LandingPage },
        { path : "/login", name: "Login", component: LoginForm },
        { path : "/admin", name: "Admin", component: LandingPageInter, meta: { requiresAuth: true }  },

        { path: "/admin/aircraft-types", name: "AircraftTypes", component: AircraftTypesPage },
        { path: "/admin/routes", name: "Routes", component: RoutesPage },       
        { path: "/admin/airports", name: "Airports", component: AirportsPage },
        { path: "/admin/users", name: "Users", component: UsersPage },
>>>>>>> dev
    ],
});

router.beforeEach((to, from, next) => {
    const token = localStorage.getItem("token");
    if (to.meta.requiresAuth && !token) {
        next("/login");
    } else {
        next();
    }
});

createApp(App).use(router).mount('#app')