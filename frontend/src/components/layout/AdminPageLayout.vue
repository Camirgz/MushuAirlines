<template>
  <div class="admin-page">
    <AdminNavbar />

    <main class="admin-main">
      <slot></slot>
    </main>

    <AppFooter />
  </div>
</template>

<script>
import AdminNavbar from "@/components/layout/AdminNavbar.vue";
import AppFooter from "@/components/layout/AppFooter.vue";

export default {
  name: "AdminPageLayout",

  components: {
    AdminNavbar,
    AppFooter,
  },
};
</script>

<style scoped>
.admin-page {
  min-height: 100vh;
  background: #f8f9fa;
  color: #1a1a1a;
}

.admin-main {
  max-width: 940px;
  margin: 0 auto;
  padding: 56px 24px 72px;
}
</style>
