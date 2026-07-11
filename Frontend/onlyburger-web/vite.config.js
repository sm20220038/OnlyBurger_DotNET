import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The dev server proxies /api to the ASP.NET Core backend so the browser makes
// same-origin requests (no CORS, no HTTPS certificate prompts during development).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5169',
        changeOrigin: true,
      },
      // SignalR hub for live order/delivery tracking. `ws: true` lets the WebSocket
      // upgrade pass through to the backend.
      '/hubs': {
        target: 'http://localhost:5169',
        changeOrigin: true,
        ws: true,
      },
    },
  },
})
