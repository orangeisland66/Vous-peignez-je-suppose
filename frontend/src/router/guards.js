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
// 用于 创建房间/加入房间 后进入等待游戏开始的界面
export function checkRoomPermission(to, from, next)
{
  const isLoggedIn = !!localStorage.getItem('token')
  if(!isLoggedIn)
  {
    // 未登陆，跳转到登陆页面
    return next({name:'Login'})
  }
  // 获取信息 （为什么这里得到的全是Undefined?)
  const roomId = to.params.id
  const userId = store.state.user?.id
  const rooms = store.state.gameRoom.rooms
  const room = rooms.find(r => String(r.id) === String(roomId))

  // 调试输出
  console.log('checkRoomPermission 调试:')
  console.log('roomId:', roomId)
  console.log('userId',userId)
  console.log('room:', room)

  if(!room || !room.members.includes(userId))
  {
    //用户不是该房间成员
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
  const roomId = to.params.roomId
  const userId = store.state.user.id
  const room = store.state.rooms[roomId]
  if (!room || !room.members.includes(userId))
  {
    return next({name:'Lobby'})
  }

  // 检查游戏是否开始
  if(!room.gameStarted)
  {
    // 游戏未开始，不能进入
    return next({name:'WaitingRoom', params:{id:roomId}})
  }
  return next()
}

// 检查用户是否已登陆，用户是否是该房间成员，游戏是否结束
export function checkGameEnded(to,from,next)
{
  const isLoggedIn = !!localStorage.getItem('token')
  if(!isLoggedIn)
  {
    return next({name:'Login'})
  }
  const roomId = to.params.roomId
  const userId = store.state.user.id
  const room = store.state.rooms[roomId]
  if(!room || !room.members.includes(userId))
  {
    return next({name:'Lobby'})
  }
  // 检查游戏是否已经结束
  if(room.gameStatus !== 'ended')
  {
    // 游戏未结束，不能访问最终分数界面
    return next({name:'GameRoom', params:{id:roomId}})
  }

  return next()
}

