// src/router/index.js
import { createRouter, createWebHistory } from 'vue-router'
import Login from '@/views/Login.vue'
import Register from '@/views/Register.vue'
import Lobby from '@/views/Lobby.vue'
import CreateRoom from '@/views/CreateRoom.vue'
import WaitingRoom from '@/views/WaitingRoom.vue'
import GameRoom from '@/views/GameRoom.vue'
import RoundResult from '@/views/RoundResult.vue'
import FinalScore from '@/views/FinalScore.vue'
import Settings from '@/views/Settings.vue'
import Profile from '@/views/Profile.vue'

const routes = [
  { path: '/', redirect: '/login' },
  { path: '/login', name: 'Login', component: Login, meta: { guest: true } },
  { path: '/register', name: 'Register', component: Register, meta: { guest: true } },
  { path: '/lobby', name: 'Lobby', component: Lobby, meta: { requiresAuth: true } },
  { path: '/room/create', name: 'CreateRoom', component: CreateRoom, meta: { requiresAuth: true } },
  { path: '/room/:id/waiting', name: 'WaitingRoom', component: WaitingRoom, meta: { requiresAuth: true } },
  { path: '/room/:id/game', name: 'GameRoom', component: GameRoom, meta: { requiresAuth: true } },
  { path: '/room/:id/round-result', name: 'RoundResult', component: RoundResult, meta: { requiresAuth: true } },
  { path: '/room/:id/final', name: 'FinalScore', component: FinalScore, meta: { requiresAuth: true } },
  { path: '/settings', name: 'Settings', component: Settings, meta: { requiresAuth: true } },
  { path: '/profile', name: 'Profile', component: Profile, meta: { requiresAuth: true } },
  { path: '/:catchAll(.*)', redirect: '/login' }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 }
  }
})

// 全局导航守卫
router.beforeEach((to, from, next) => {
  const isLoggedIn = !!localStorage.getItem('token')
  if (to.meta.requiresAuth && !isLoggedIn) {
    return next({ name: 'Login' })
  }
  if (to.meta.guest && isLoggedIn) {
    return next({ name: 'Lobby' })
  }
  next()
})

export default router