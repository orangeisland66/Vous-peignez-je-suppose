// src/router/index.js
import { createRouter, createWebHistory } from 'vue-router';
import App from '../App.vue';

const routes = [
  {
    path: '/',
    name: 'App',
    component: App
  }
  // 可以根据需求添加更多路由
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router;