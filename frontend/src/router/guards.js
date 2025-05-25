// src/router/guards.js
import store from '../store/index.js'

export function checkUserAuth(to, from, next) {
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)
  const isLoggedIn = !!localStorage.getItem('token')

    // 调试日志，非常重要！
  console.log(`[AuthGuard] Navigating to: ${to.path}`);
  console.log(`[AuthGuard] Route meta:`, to.meta);
  console.log(`[AuthGuard] requiresAuth: ${requiresAuth}`);
  console.log(`[AuthGuard] isLoggedIn: ${isLoggedIn}`);

  if (requiresAuth && !isLoggedIn) {
    return next({ name: 'Login' })
  }

  if (to.meta.guest && isLoggedIn) {
    return next({ name: 'Lobby' })
  }

  return next()
}
export function checkRoundResultPermission(to, from, next) {
  const isLoggedIn = !!localStorage.getItem('token')
  if (!isLoggedIn) {
    return next({ name: 'Login' })
  }

  const roomId = to.params.id
  const userId = store.state.user?.id
  const rooms = store.state.gameRoom?.rooms || []
  const room = rooms.find(r => String(r.id) === String(roomId))

  if (!room || !room.members.includes(userId)) {
    return next({ name: 'Lobby' })
  }

  // 添加具体的回合结果权限检查逻辑
  if (!room.showRoundResult) { // 假设存在 showRoundResult 标志
    return next({ name: 'GameRoom', params: { id: roomId } })
  }

  return next()
}
export function checkRoomPermission(to, from, next) {
  const isLoggedIn = !!localStorage.getItem('token')
  if (!isLoggedIn) {
    return next({ name: 'Login' })
  }

  const roomId = to.params.id
  const userId = store.state.user?.id
  const rooms = store.state.gameRoom?.rooms || []
  const room = rooms.find(r => String(r.id) === String(roomId))

  console.log('checkRoomPermission 调试:')
  console.log('roomId:', roomId)
  console.log('userId:', userId)
  console.log('room:', room)

  if (!room || !room.members.includes(userId)) {
    return next({ name: 'Lobby' })
  }

  return next()
}

export function checkGamePermission(to, from, next) {
  // const isLoggedIn = !!localStorage.getItem('token')
  // if (!isLoggedIn) {
  //   return next({ name: 'Login' })
  // }

  // const roomId = to.params.id
  // const userId = store.state.user?.id
  // const rooms = store.state.gameRoom?.rooms || []
  // const room = rooms.find(r => String(r.id) === String(roomId))

  // if (!room || !room.members.includes(userId)) {
  //   return next({ name: 'Lobby' })
  // }

  // if (!room.gameStarted) {
  //   return next({ name: 'WaitingRoom', params: { id: roomId } })
  // }

  return next()
}

export function checkGameEnded(to, from, next) {
  const isLoggedIn = !!localStorage.getItem('token')
  if (!isLoggedIn) {
    return next({ name: 'Login' })
  }

  const roomId = to.params.id
  const userId = store.state.user?.id
  const rooms = store.state.gameRoom?.rooms || []
  const room = rooms.find(r => String(r.id) === String(roomId))

  if (!room || !room.members.includes(userId)) {
    return next({ name: 'Lobby' })
  }

  if (room.gameStatus !== 'ended') {
    return next({ name: 'GameRoom', params: { id: roomId } })
  }
  return next()
}
