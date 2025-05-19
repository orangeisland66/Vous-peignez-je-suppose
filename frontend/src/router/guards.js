// src/router/guards.js
import store from '../store/index.js'

export function checkUserAuth(to, from, next) {
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)
  const isLoggedIn = !!localStorage.getItem('token')

  if (requiresAuth && !isLoggedIn) {
    return next({ name: 'Login' })
  }

  if (to.meta.guest && isLoggedIn) {
    return next({ name: 'Lobby' })
  }

  return next()
}

export function checkRoomPermission(to, from, next) {
  // const isLoggedIn = !!localStorage.getItem('token')
  // if (!isLoggedIn) {
  //   return next({ name: 'Login' })
  // }

  // const roomId = to.params.id
  // const userId = store.state.user?.id
  // const rooms = store.state.gameRoom?.rooms || []
  // const room = rooms.find(r => String(r.id) === String(roomId))

  // console.log('checkRoomPermission 调试:')
  // console.log('roomId:', roomId)
  // console.log('userId:', userId)
  // console.log('room:', room)

  // if (!room || !room.members.includes(userId)) {
  //   return next({ name: 'Lobby' })
  // }

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
