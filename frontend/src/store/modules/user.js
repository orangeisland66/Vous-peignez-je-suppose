// frontend/src/store/modules/user.js

import apiService from '@/services/apiService';

/*  此文件缺乏相关API接口的调用，待API完成后补充 */

// 需要管理的数据
const state = 
{
  isLoggedIn: false,
  // 认证令牌，用于与后端验证身份
  token: null, 
  userInfo:
  {
    id: null,
    username: '',
    // 用户头像路径
    avatar: '',
    // 用户积分(可用于成就排名什么的，后面用不到可以删去)
    score: 0,
    winCount: 0,
    totalGames:0
  },
  onlineStatus: 'online' //用户在线状态，没用也可以删去
}

// mutations 更改state的唯一方法
const mutations =
{
  SET_LOGIN_STATUS(state, isLoggedIn)
  {
    state.isLoggedIn = isLoggedIn
  },
  SET_TOKEN(state, token)
  {
    state.token = token
  },
  SET_USER_INFO(state, userInfo)
  {
    state.userInfo = userInfo
  },
  CLEAR_USER_DATA(state)
  {
    state.isLoggedIn = false;
    state.token = null;
    state.userInfo = 
    {
      id:null,
      username:'',
      avatar:'',
      score:0,
      winCount:0,
      totalGames:0
    };
  },
  UPDATE_SCORE(state, points)
  {
    state.userInfo.score += points;
  },
  UPDATE_GAME_COUNT(state, isWinner)
  {
    state.userInfo.totalGames += 1;
    if(isWinner)
    {
      state.userInfo.winCount += 1;
    }
  },
  SET_ONLINE_STATUS(state, status)
  {
    state.onlineStatus = status;
  }
};
  
// actions 用于处理异步操作，一般用于调用API
const actions = {
  // 登录操作
  async login({ commit }, credentials) 
  {
    try 
    {
      // 这里应该是API请求，暂时使用注释代替
      // const response = await apiService.login(credentials)
      // const {token, userInfo} = response.data

      commit('SET_USER_INFO', userInfo);
      commit('SET_TOKEN', token);
      commit('SET_LOGIN_STATUS', true);

      // 保存到LocalStorage以便页面刷新后保存登录状态
      localStorage.setItem('token', token);
      localStorage.setItem('userInfo', JSON.stringify(userInfo));

      return response;
    } 
    catch (error) 
    {
      throw error;
    }
  },
  
  // 注册操作
  async register({ commit }, userData) 
  {
    try 
    {
      // 这里应该是API请求，暂时使用注释代替
      // const response = await apiService.register(userData);
      // const {token, userInfo} = response.data;

      commit('SET_USER_INFO', userInfo);
      commit('SET_TOKEN', token);
      commit('SET_LOGIN_STATUS', true);

      // 保存到LocalStorage以便页面刷新后保存登录状态
      localStorage.setItem('token', token);
      localStorage.setItem('userInfo', JSON.stringify(userInfo));
      return response;
    } 
    catch (error) 
    {
      throw error;
    }
  },
  
  // 登出操作
  logout({ commit },router) 
  {
    // 清除状态
    commit('CLEAR_USER_DATA');
    // 清除LocalStorage
    localStorage.removeItem('token');
    localStorage.removeItem('userInfo');
    router.push('/login'); //跳转登录界面
  },

  // 初始化用户状态
  initUserState({commit})
  {
    const token = localStorage.getItem('token');
    const userInfo = JSON.parse(localStorage.getItem('userInfo'));

    if(token && userInfo)
    {
      commit('SET_TOKEN', token);
      commit('SET_USER_INFO', userInfo);
      commit('SET_LOGIN_STATUS', true);
    }
  },

  // 更新用户积分
  //（这个积分不是游戏内获得的分数，而是一种类似成就系统的点数，如果后续要做这一部分的内容再补充） 
  updateScore({commit}, points)
  {

  },

  // 更新游戏局数和胜利局数
  updateGameStats({commit}, isWinner)
  {
    try
    {
      // 这里应该是API请求，暂时使用注释代替
      // const response = await apiService.updateGameStats(isWinner);
      
      commit('UPDATE_GAME_COUNT', isWinner);
    }
    catch (error)
    {
      throw error;
    }
  }
};

// getters 用于获取state的值，用于派生状态或过滤数据
const getters = 
{
  isLoggedIn: state => state.isLoggedIn,
  userInfo: state => state.userInfo,
  username: state => state.userInfo.username,
}
  
// 模块导出
export default 
{
  namespaced: true,
  state,
  mutations,
  actions,
  getters
}