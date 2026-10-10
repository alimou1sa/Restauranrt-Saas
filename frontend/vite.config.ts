import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

// In development the browser talks to the Vite server (same origin) and Vite forwards /api to the
// backend. This avoids the backend CORS allow-list (which only lists the API's own origins).
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const target = env.VITE_DEV_PROXY_TARGET || 'https://localhost:7107';
  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: { '/api': { target, changeOrigin: true, secure: false } },
    },
  };
});
