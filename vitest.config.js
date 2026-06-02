import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    dedupe: ['vue'],
    alias: {
      '@': fileURLToPath(new URL('./frontend/src', import.meta.url)),
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    include: ['tests/**/*.test.js'],
    reporter: 'verbose',
  },
})
