// frontend/src/store/modules/gameRoom.js
const state = {
    rooms: [],
    currentRoom: null
  }
  
  const mutations = {
    SET_ROOMS(state, rooms) {
      state.rooms = rooms
    },
    SET_CURRENT_ROOM(state, room) {
      state.currentRoom = room
    },
    ADD_ROOM(state, room) {
      state.rooms.push(room)
    },
    UPDATE_ROOM(state, updatedRoom) {
      const index = state.rooms.findIndex(r => r.id === updatedRoom.id)
      if (index !== -1) {
        state.rooms.splice(index, 1, updatedRoom)
      }
    }
  }
  
  const actions = {
    async fetchRooms({ commit }) {
      try {
        const response = await apiService.get('/rooms')
        commit('SET_ROOMS', response.data)
      } catch (error) {
        throw error
      }
    },
  
    async createRoom({ commit }, roomData) {
      try {
        const response = await apiService.post('/rooms', roomData)
        commit('ADD_ROOM', response.data)
        return response.data
      } catch (error) {
        throw error
      }
    },
  
    async joinRoom({ commit }, roomId) {
      try {
        const response = await apiService.post(`/rooms/${roomId}/join`)
        commit('SET_CURRENT_ROOM', response.data)
        socketService.joinRoom(roomId)
      } catch (error) {
        throw error
      }
    }
  }
  
  export default {
    namespaced: true,
    state,
    mutations,
    actions
  }