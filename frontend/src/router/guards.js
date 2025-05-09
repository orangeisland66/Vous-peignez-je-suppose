//guard.js
// frontend/src/router/guards.js
// 定义路由守卫函数，负责在路由导航时进行权限控制，访问验证，导航管理
import store from '../store/index.js'

// export function authGuard(to, from, next) {
//     const isAuthenticated = localStorage.getItem('token')
    
//     if (to.meta.requiresAuth && !isAuthenticated) {
//       next({ name: 'Login' })
//     } else if ((to.name === 'Login' || to.name === 'Register') && isAuthenticated) {
//       next({ name: 'Home' })
//     } else {
//       next()
//     }
//   }

/**
 * 检查用户认证状态
 * @param {Object} to - 目标路由对象
 * @param {Object} from - 源路由对象
 * @param {Function} next - 导航守卫的下一个钩子函数（路由控制函数）
 */
export function checkUserAuth(to, from, next)
{
    // 判断路由是否需要登录权限
    const requiresAuth = to.matched.some(record=> record.meta.requiresAuth)

    //获取当前登录状态
    const isLoggedIn = store.getters['auth/isLoggedIn']

    if(requiresAuth && !isLoggedIn)
    {
      // 需要登陆权限但是用户未登陆，重定向到登陆页面
      next({
        path:'/Login',
        query:{redirect: to.fullPath} //保存原本要访问的页面，便于后续的跳转
      })
    }
    else if(to.path=='/Login' && isLoggedIn)
    {
      //用户已经登陆，访问登陆页面，重定向到首页
      next:({
        path:'/Home'
      })
    }
    else
    {
      //其他情况，正常访问
      next()
    }
}