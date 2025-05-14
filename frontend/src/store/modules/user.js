// frontend/src/store/modules/user.js
import apiService from '@/services/apiService';

const state = {
    user: null,
    token: localStorage.getItem('token') || null
  }
  
  const mutations = {
    SET_USER(state, user) {
      state.user = user
    },
    SET_TOKEN(state, token) {
      state.token = token
      localStorage.setItem('token', token)
    },
    LOGOUT(state) {
      state.user = null
      state.token = null
      localStorage.removeItem('token')
    }
  }
  
  const actions = {
    async login({ commit }, credentials) {
      try {
        const response = await apiService.post('/auth/login', credentials)
        commit('SET_USER', response.data.user)
        commit('SET_TOKEN', response.data.token)
        return response
      } catch (error) {
        throw error
      }
    },
  
    async register({ commit }, userData) {
      try {
        const response = await apiService.post('/auth/register', userData)
        commit('SET_USER', response.data.user)
        commit('SET_TOKEN', response.data.token)
        return response
      } catch (error) {
        throw error
      }
    },
  
     logout({ commit }, router) {
        commit('LOGOUT');
        router.push('/login');
    }
  }
  
  const getters = {
    isAuthenticated: state => !!state.token,
    currentUser: state => state.user
  }
  
  export default {
    namespaced: true,
    state,
    mutations,
    actions,
    getters
  }