// vite.config.js
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

export default defineConfig({
  plugins: [vue()],
  server: {
    // 配置开发服务器
    proxy: {
      // 将所有以 /api 开头的请求代理到后端服务器
      '/api': {
        // **目标后端服务器地址和端口**
        target: 'http://localhost:5076', // <-- 指向你的后端地址和端口
        changeOrigin: true, // 改变源，以欺骗后端认为请求来自同一个域
        // **重要：重写路径，将前端请求的 /api 前缀去掉再转发给后端**
        // 因为后端 API 接口不包含 /api 前缀
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    }
  }
})
