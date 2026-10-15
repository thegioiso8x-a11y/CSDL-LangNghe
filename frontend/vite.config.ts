import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    chunkSizeWarningLimit: 1500,
    rolldownOptions: {
      output: {
        manualChunks: (id) => {
          const moduleId = id.replaceAll('\\', '/')
          if (!moduleId.includes('node_modules/')) return undefined
          if (moduleId.includes('recharts')) return 'vendor-recharts'
          if (
            moduleId.includes('react-router') ||
            moduleId.includes('react-dom') ||
            moduleId.match(/node_modules\/react\//)
          ) {
            return 'vendor-react'
          }
          return undefined
        },
      },
    },
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      '/uploads': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
})
