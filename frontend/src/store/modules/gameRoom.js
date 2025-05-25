// frontend/src/store/modules/gameRoom.js

/* 该文件缺乏相关API接口的调用，待API完成后补充 */
import apiService from '@/services/apiService';

// state还在想，之后需要修改
const state = {
  // 房间列表
  rooms: [],
  // 当前房间
  currentRoom: null,
  // 当前房间的成员列表
  players: [],
  // 房间聊天消息
  messages: [],
  // 房间状态 (如:公开、私密)
  roomStatus: 'public',
  // 房间最大人数
  maxPlayers: 0,
  // 房主
  owner: null
}
  
const mutations = 
{
  SET_ROOMS(state, rooms) 
  {
    state.rooms = rooms
  },
  ADD_ROOM(state, room) 
  {
    state.rooms.push(room)
  },
  REMOVE_ROOM(state, roomId)
  {
    state.rooms = state.rooms.filter(room=>room.id !== roomId)
  },
  SET_CURRENT_ROOM(state, room) 
  {
    state.currentRoom = room
  },
  SET_PLAYERS(state, players)
  {
    state.players = players
  },
  ADD_PLAYER(state, player)
  {
    const existingPlayer = state.players.find(p => p.username === player.username);
    console.log('existingPlayer:', existingPlayer)
    if (!existingPlayer) {
      state.players.push(player);
    }
  },
  REMOVE_PLAYER(state, playerId)
  {
    state.players = state.players.filter(player=>player.id !== playerId)
  },
  ADD_MESSAGE(state, message)
  {
    state.messages.push(message)
  },
  CLEAR_MESSAGES(state)
  {
    state.messages = []
  },
  CLEAR_ROOM_DATA(state)
  {
    state.currentRoom = null
    state.players = []
    state.messages = []
  },
  SET_ROOM_ERROR(state, error)
  {
    state.roomError = error
  },
  SET_ROOM_STATUS(state, status)
  {
    state.roomStatus = status
  },
  SET_MAX_PLAYERS(state, maxPlayers)
  {
    state.maxPlayers = maxPlayers
  }
}
  
const actions = {
  // 获取房间列表
  async fetchRooms({ commit }) 
  {
    try 
    {
      // 这里需要调用API来获取房间列表，先使用注释占位
      //const response = await apiService.get('/rooms')
      //const rooms = response.data

      commit('SET_ROOMS', response.data)
    } 
    catch (error) 
    {
      throw error
    }
  },
  // 创建房间
  async createRoom({ commit }, roomData) 
  {
    try 
    {
      // 这里需要调用API来创建房间，先使用注释占位
      //const response = await apiService.post('/rooms', roomData)
      //const room = response.data

      commit('ADD_ROOM', response.data)
      return response.data
    } 
    catch (error) 
    {
      throw error
    }
  },
  // 删除房间
  async deleteRoom({commit}, roomId)
  {
    try
    {
      // 这里需要调用API来删除房间，先使用注释
      // const response = await apiService.delete('/rooms/${roomId}')
      
      commit('REMOVE_ROOM', roomId)
    }
    catch (error)
    {
      throw error
    }
  },
  // 加入房间
  async joinRoom({commit}, { roomId, userId, player })
  {
    try
    {
      // 这里需要调用API来加入房间
      const response = await apiService.joinRoom(roomId, userId, player)

      commit('SET_CURRENT_ROOM', response.data)
      commit('ADD_PLAYER', player) // 添加玩家到玩家列表

      // socketService.joinRoom(roomId)
    }
    catch(error)
    {
      throw error
    }
  },
  // 离开房间
  async leaveRoom({commit}, roomId)
  {
    try
    {
      // await apiService.post('/rooms/${roomId}/leave')
      commit('CLEAR_ROOM_DATA')
      // socketService.leaveRoom(roomId)
    }
    catch(error)
    {
      throw error
    }
  },
  // 获取成员列表
  async fetchPlayers({commit}, roomId)
  {
    try
    {
      // 这里需要调用API来获取房间成员列表
      // const response = await apiService.get('/rooms/${roomId}/players')
      // commit('SET_PLAYERS', response.data)

    }
    catch(error)
    {
      throw error
    }
  },
  // 发送消息
  async sendMessage({commit}, {roomId, message})
  {
    try
    {
      // const response = await apiService.post('/rooms/${roomId}/messages', {message})
      // commit('ADD_MESSAGE', response.data)
    }
    catch(error)
    {
      throw error
    }
  },
  // 获取历史消息
  async fetchMessages({commit}, room)
  {
    try
    {
      // const response = await apiService.get('/rooms/${room.id}/messages')
      // commit('SET_MESSAGES', response.data)
      // response.data.forEach(msg=>commit('ADD_MESSAGE',msg))
    }
    catch(error)
    {
      throw error
    }
  },
  // 修改房间状态
  async updateRoomStatus({commit},{roomId, status})
  {
    try
    {
      // const response = await apiService.put('/rooms/${roomId}/status', {status})
      // commit('SET_ROOM_STATUS', response.data.status)
    }
    catch(error)
    {
      throw error
    }
  },
  // 设置最大的人数
  async setMaxPlayers({commit},{roomId, maxPlayers})
  {
    try
    {
      // const response = await apiService.put('/rooms/${roomId}/max-players', {maxPlayers})
      commit('SET_MAX_PLAYERS', response.data.maxPlayers)
    }
    catch(error)
    {
      throw error
    }
  }
};

const getters = 
{
  allRooms: state => state.rooms,
  activeRooms: state => state.rooms.filter(room => room.states = "waiting"),
  currentRoom: state => state.currentRoom,
  players: state => state.players,
  messages: state => state.messages,
  isRoomOwner: (state, getters, rootState) => {
    if (!state.currentRoom || !state.players.length) return false;
    const userId = rootState.user.userInfo.id;
    const owner = state.players.find(p => p.isOwner);
    return owner && owner.id === userId;
  },
  canStartGame: (state, getters) => {
  if (!getters.isRoomOwner) return false;
    
  // 至少需要2名玩家，并且所有玩家都已准备
  return state.players.length >= 2 && 
          state.players.filter(p => !p.isOwner).every(p => p.isReady);
  }
};

export default
{
  namespaced: true,
  state,
  mutations,
  actions,
  getters
}