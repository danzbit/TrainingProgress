import path from 'path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const BFF_URL = process.env.VITE_API_BASE_URL || 'http://localhost:5187';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    proxy: {
      '/auth-service': { target: BFF_URL, changeOrigin: true },
      '/training-service': { target: BFF_URL, changeOrigin: true },
      '/analytics-service': { target: BFF_URL, changeOrigin: true },
      '/notification-service': { target: BFF_URL, changeOrigin: true },
      '/ai-chat-service': { target: BFF_URL, changeOrigin: true },
    },
  },
})
