// frontend/src/store/modules/game.js
const state = {
    canvasData: [],
    chatMessages: [],
    currentWord: '',
    isDrawing: false,
    players: [],
    roundTime: 0,
    gameState: 'waiting' // waiting, drawing, guessing, finished
  }
  
  const mutations = {
    SET_CANVAS_DATA(state, data) {
      state.canvasData = data
    },
    ADD_STROKE(state, stroke) {
      state.canvasData.push(stroke)
    },
    ADD_MESSAGE(state, message) {
      state.chatMessages.push(message)
    },
    SET_CURRENT_WORD(state, word) {
      state.currentWord = word
    },
    SET_GAME_STATE(state, gameState) {
      state.gameState = gameState
    },
    SET_PLAYERS(state, players) {
      state.players = players
    },
    RESET_GAME(state) {
      state.canvasData = []
      state.chatMessages = []
      state.currentWord = ''
      state.isDrawing = false
      state.players = []
      state.roundTime = 0
      state.gameState = 'waiting'
    }
  }
  
  const actions = {
    sendDrawing({ commit }, stroke) {
      commit('ADD_STROKE', stroke)
      socketService.sendStroke(stroke)
    },
  
    sendGuess({ commit }, message) {
      commit('ADD_MESSAGE', message)
      socketService.sendMessage(message)
    },
  
    startGame({ commit }) {
      socketService.startGame()
      commit('SET_GAME_STATE', 'drawing')
    },
  
    updatePlayers({ commit }, players) {
      commit('SET_PLAYERS', players)
    }
  }
  
  export default {
    namespaced: true,
    state,
    mutations,
    actions
  }