// frontend/src/store/index.js
import { createStore } from 'vuex'
import user from './modules/user'
import gameRoom from './modules/gameRoom'
import game from './modules/game'

export default createStore({
  modules: {
    user,
    gameRoom,
    game
  }
})