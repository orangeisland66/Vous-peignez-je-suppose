// frontend/src/router/guards.js
export function authGuard(to, from, next) {
    const isAuthenticated = localStorage.getItem('token')
    
    if (to.meta.requiresAuth && !isAuthenticated) {
      next({ name: 'Login' })
    } else if ((to.name === 'Login' || to.name === 'Register') && isAuthenticated) {
      next({ name: 'Home' })
    } else {
      next()
    }
  }