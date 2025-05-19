// 引入 Axios (如果还没有安装，请运行 npm install axios)
import axios from 'axios';

// 假设你的后端API基础URL是 /api
const API_BASE_URL = '/api'; // 根据你的后端实际地址修改

const apiService = {
  // 示例：一个临时的 login 函数 (保留或根据需要修改)
  async login(credentials) {
    console.log('Placeholder API call: login', credentials);
    // 模拟一个成功的登录响应
    return Promise.resolve({
      data: {
        token: 'dummy-token-' + Math.random().toString(36).substr(2, 8), // 模拟一个随机token
        userInfo: { id: 1, username: credentials.username, avatar: '', score: 0, winCount: 0, totalGames: 0 }
      }
    });
  },

  // 示例：一个临时的 register 函数 (保留或根据需要修改)
   async register(userData) {
    //  try {
    //         const response = await axios.post('/api/user/register', userData);
    //         return response;
    //     } catch (error) {
    //         throw error;
    //     }
     console.log('Placeholder API call: register', userData);
      // 模拟一个成功的注册响应
     return Promise.resolve({
       data: {
         token: 'dummy-token-' + Math.random().toString(36).substr(2, 8),
         userInfo: { id: 2, username: userData.username, avatar: '', score: 0, winCount: 0, totalGames: 0 }
       }
      });
   },

   // 示例：一个临时的 updateGameStats 函数 (保留或根据需要修改)
    async updateGameStats(isWinner) {
       console.log('Placeholder API call: updateGameStats', isWinner);
       // 模拟一个成功的响应
       return Promise.resolve({ data: {} });
    },

  // **添加 createRoom 函数**
  async createRoom(roomData) {
    console.log('Attempting to create room with data:', roomData);
    try {
      // **使用 axios.post 发送请求到后端 API**
      // `${API_BASE_URL}/rooms/create` 是完整的 API 路径
      // roomData 是请求体，包含了房间的所有设置
      const response = await axios.post(`${API_BASE_URL}/rooms/create`, roomData);

      console.log('Backend response (createRoom):', response.data);

      // 假设后端成功时返回的数据结构是 { success: true, roomId: '...', ... }
      // 你需要根据后端实际返回的数据结构来调整这里的返回
      return response.data;

    } catch (error) {
      console.error('Error creating room:', error);
      // 抛出错误以便在调用方 (CreateRoom.vue) 捕获和处理
      // 可以根据 error.response 来获取后端返回的错误详情
      if (error.response) {
          // 后端返回了状态码不在 2xx 范围内的响应
          console.error('Error response data:', error.response.data);
          console.error('Error response status:', error.response.status);
          console.error('Error response headers:', error.response.headers);
          // 抛出包含后端错误信息的 Error
          throw new Error(error.response.data.message || `创建房间失败：状态码 ${error.response.status}`);
      } else if (error.request) {
          // 请求已发送，但没有收到响应 (例如，后端服务器未运行)
          console.error('Error request:', error.request);
          throw new Error('创建房间失败：无法连接到服务器');
      } else {
          // 在设置请求时触发了错误
          console.error('Error message:', error.message);
          throw new Error(`创建房间失败：${error.message}`);
      }
    }
  }
};

export default apiService;