//guard.js
// frontend/src/router/guards.js
// 定义路由守卫函数，负责在路由导航时进行权限控制，访问验证，导航管理
import store from '../store/index.js'

/**
 * 检查用户认证状态
 * @param {Object} to - 目标路由对象
 * @param {Object} from - 源路由对象
 * @param {Function} next - 导航守卫的下一个钩子函数（路由控制函数）
 */

// 检查用户是否已登陆
export function checkUserAuth(to, from, next)
{
  // 判断路由是否需要登陆权限
  const requiresAuth = to.matched.some(record=>record.meta.requiresAuth)
  const isLoggedIn = !!localStorage.getItem('token')

  if(requiresAuth && !isLoggedIn)
  {
    // 需要登陆权限但未登陆
    // 跳转到登陆页面
    return next({name:'Login'})
  }
  if(to.meta.guest && isLoggedIn)  
  {
    // 已经登陆的用户访问 只允许未登陆用户 访问的页面
    // 跳转到大厅页面
    return next({name:'Lobby'})
  }
  //其余情况，正常访问
  return next()
}

// 检查用户是否已登陆，是否在该房间成员列表中
// 用于 加入房间 后进入等待游戏开始的界面
export function checkRoomPermission(to, from, next)
{
  const isLoggedIn = !!localStorage.getItem('token')
  if(!isLoggedIn)
  {
    // 未登陆，跳转到登陆页面
    return next({name:'Login'})
  }
  // 获取信息 
  const roomId = to.params.id
  const userId = store.state.user.userInfo.id
  const rooms = store.state.gameRoom.rooms
  const room = rooms.find(r => String(r.id) === String(roomId))
  if(!room || !room.players.some(p=>String(p.id) == String(userId)))
  {
    return next({name:'Lobby'})
  }
  // 其余正常前进
  return next()
}

// 检查用户是否已登陆，用户是否是该房间成员，游戏是否已经开始
export function checkGamePermission(to,from,next)
{
  const isLoggedIn = !!localStorage.getItem('token')
  if(!isLoggedIn)
  {
    return next({name:'Login'})
  }
  const roomId = to.params.id
  const userId = store.state.userInfo.id
  const rooms = store.state.gameRoom.rooms
  const room = rooms.find(r=>String(r.id) === String(roomId))
  if (!room || !room.players.some(p=>String(p.id) === String(userId)))
  {
    return next({name:'Lobby'})
  }
  // 检查游戏是否开始
  const isGameStarted = store.state.game.isGameStarted
  if(!isGameStarted)
  {
    return next({name: 'WaitingRoom', params:{id:roomId}})
  }
  return next()
}

// 检查用户是否为房间成员且当前为单轮结束阶段
export function checkRoundResultPermission(to, from, next) {
  const isLoggedIn = !!localStorage.getItem('token')
  if (!isLoggedIn) {
    return next({ name: 'Login' })
  }
  const roomId = to.params.id
  const userId = store.state.user.userInfo.id
  const rooms = store.state.gameRoom.rooms
  const room = rooms.find(r => String(r.id) === String(roomId))
  if (!room || !room.players.some(p => String(p.id) === String(userId))) {
    return next({ name: 'Lobby' })
  }
  // 检查是否为单轮结束状态
  const gameStatus = store.state.game.gameStatus
  if (gameStatus !== 'roundEnd') {
    // 如果不是单轮结束，跳转回游戏房间
    return next({ name: 'GameRoom', params: { id: roomId } })
  }
  return next()
}

// 检查用户是否为房间成员且游戏已结束（用于最终分数页面）
export function checkGameEnded(to,from,next)
{
  const isLoggedIn = !!localStorage.getItem('token')
  if(!isLoggedIn)
  {
    return next({name:'Login'})
  }
  const roomId = to.params.id
  const userId = store.state.user.userInfo.id
  const rooms = store.state.gameRoom.rooms
  const room = rooms.find(r=>String(r.id) === String(roomId))
  if(!room || !room.players.some(p=>String(p.id)===String(userId)))
  {
    return next({name:'Lobby'})
  }
  // 检查游戏是否已经结束
  const gameStatus = store.state.game.gameStatus
  if(gameStatus !== 'gameEnd')
  {
    return next({name:'GameRoom', params:{id:roomId}})
  }
  return next()
}

