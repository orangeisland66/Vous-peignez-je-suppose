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

import {checkUserAuth,checkRoomPermission,checkGamePermission,checkGameEnded,checkRoundResultPermission} from './guards.js'

// 定义路由
const routes = [
  // 默认定义到登陆页面
  { 
    path: '/', 
    redirect: '/login',
    // beforeEnter:checkUserAuth
  },
  // 登陆页面
  { 
    path: '/login',
    name: 'Login', 
    component: Login, 
    // meta: { guest: true },
    // beforeEnter:checkUserAuth 
  },
  // 注册页面
  { 
    path: '/register', 
    name: 'Register', 
    component: Register, 
    // meta: { guest: true },
    // beforeEnter:checkUserAuth
  },
  // 大厅页面
  { 
    path: '/lobby', 
    name: 'Lobby', 
    component: Lobby, 
    // meta: { requiresAuth: true },
    // beforeEnter:checkUserAuth 
  },
  // 创建游戏房间页面
  { 
    path: '/room/create', 
    name: 'CreateRoom', 
    component: CreateRoom, 
    // meta: { requiresAuth: true },
    // beforeEnter:checkUserAuth 
  },
  //等待游戏开始页面
  { 
    path: '/room/join/:roomId', // <--- 确保这里的参数名是 'roomId'
    name: 'WaitingRoom',
    component: WaitingRoom,
    // meta: { requiresAuth: true } ,// 如果需要登录
    // beforeEnter:checkRoomPermission
  },
  // { 
  //   path: '/room/:roomId/waiting', // <--- 确保这里的参数名是 'roomId'
  //   name: 'WaitingRoom',
  //   component: () => import('../views/WaitingRoom.vue'), // 确保路径正确
  //   // meta: { requiresAuth: true } ,// 如果需要登录
  //   // beforeEnter:checkRoomPermission
  // },
  // 进入游戏页面
  { 
    path: '/room/:roomId/game', 
    name: 'GameRoom',
    component: GameRoom,
    // 修改 props 配置以支持多个来源的 playerId
    props: route => ({
      playerId: parseInt(localStorage.getItem('userId')) || parseInt(route.params.playerId) || null
    })
  },
  // 单轮游戏结束页面
  { 
    path: '/room/:roomId/round-result', 
    name: 'RoundResult', 
    component: RoundResult, 
    // meta: { requiresAuth: true },
    // beforeEnter:checkRoundResultPermission
  },
  // 游戏结束，显示最终分数页面
  {
    path: '/room/:roomId/final', 
    name: 'FinalScore', 
    component: FinalScore, 
    meta: { requiresAuth: true },
    // beforeEnter:checkGameEnded 
  },
  // 设置页面
  { 
    path: '/settings', 
    name: 'Settings', 
    component: Settings, 
    // meta: { requiresAuth: true },
    // beforeEnter:checkUserAuth 
  },
  // 用户个人资料页面
  { 
    path: '/profile', 
    name: 'Profile', 
    component: Profile, 
    // meta: { requiresAuth: true },
    // beforeEnter:checkUserAuth
  },
  // 404 页面
  { 
    path: '/:catchAll(.*)', 
    redirect: '/login' 
  }
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
  // const isLoggedIn = !!localStorage.getItem('token')
  // if (to.meta.requiresAuth && !isLoggedIn) {
  //   return next({ name: 'Login' })
  // }
  // if (to.meta.guest && isLoggedIn) {
  //   return next({ name: 'Lobby' })
  // }
  // next()

  // 调用自定义的路由守卫函数
  checkUserAuth(to,from,next)
})

export default router