// frontend/src/router/index.js
import { createRouter, createWebHistory } from 'vue-router'
import { authGuard } from './guards'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/LoginPage.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('../views/RegisterPage.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/HomePage.vue'),
    meta: { requiresAuth: true }
  },
  {
    path: '/game/:roomId',
    name: 'Game',
    component: () => import('../views/GamePage.vue'),
    meta: { requiresAuth: true },
    props: true
  }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
})

router.beforeEach(authGuard)

export default router