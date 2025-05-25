// frontend/src/store/index.js
import { createStore } from 'vuex'
import user from './modules/user'
import gameRoom from './modules/gameRoom'
import game from './modules/game'
import router from '../router'

const store = createStore({
  modules: {
    user,
    gameRoom,
    game
  },
  actions: {
    'user/logout': (context) => {
      localStorage.removeItem('token')
      localStorage.removeItem('userInfo')
      router.push('/login') // 使用注入的 router 进行跳转
    }
  }
})

export default store