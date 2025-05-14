// vite.config.js
import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': '/src'
    }
  },
  server: {
    proxy: {
      '/gameHub': {
        target: 'http://localhost:5076', // 后端地址
        ws: true, // 启用 WebSocket 代理
        changeOrigin: true
      }
    }
  }
});