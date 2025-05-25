// 引入 Axios (如果还没有安装，请运行 npm install axios)
import axios from 'axios';

// 假设你的后端API基础URL是 /api
const API_BASE_URL = '/api'; // 根据你的后端实际地址修改

const apiService = {
  // 示例：一个临时的 login 函数 (保留或根据需要修改)
  async login(credentials) {
     try {
      const response = await axios.post(`${API_BASE_URL}/users/login`, credentials);
      return response;
    } catch (error) {
      console.error('登录请求出错:', error);
      throw error;
    }
  },

   async register(userData) {
     console.log('Attempting to register user with data:', userData);
     try {
            // **使用 axios.post 发送请求到后端 API**
            // 将 URL 修改为 /api/User/register
            // userData 应该是一个包含 username, email, 和 passwordHash (明文密码) 的对象
            const response = await axios.post(`${API_BASE_URL}/users/register`, userData);

            console.log('Backend response (register):', response.data);

            // axios 在状态码 2xx 时不会抛出错误，直接返回 response
            // 你可以根据需要检查 response.status，但通常直接返回 response.data 即可
            return response.data; // 返回后端响应的数据

        } catch (error) {
            console.error('Error during registration:', error);
            // 抛出错误以便在调用方 (Register.vue) 捕获和处理
            // axios 会在非 2xx 状态码时抛出错误，错误对象通常包含 error.response
            if (error.response) {
                // 后端返回了状态码不在 2xx 范围内的响应 (例如 400, 409, 500)
                console.error('Error response data:', error.response.data);
                console.error('Error response status:', error.response.status);
                // 抛出包含后端错误信息的 Error
                // 假设后端在错误时返回 { message: "错误信息" } 或标准的 validation 错误
                // 对于 validation 错误，error.response.data 可能是 { errors: { ... } } 结构
                let errorMessage = '注册失败，未知错误';
                if (error.response.data && error.response.data.message) {
                     errorMessage = error.response.data.message;
                } else if (error.response.data && error.response.data.errors) {
                    // 处理后端返回的 ModelState 验证错误
                    // 遍历 errors 对象，将所有错误消息拼接起来
                    const validationErrors = Object.values(error.response.data.errors).flat();
                    errorMessage = '注册失败：' + validationErrors.join('; ');
                } else {
                     errorMessage = `注册失败：状态码 ${error.response.status}`;
                }
                throw new Error(errorMessage);

            } else if (error.request) {
                // 请求已发送，但没有收到响应 (例如，后端服务器未运行)
                console.error('Error request:', error.request);
                throw new Error('注册失败：无法连接到服务器');
            } else {
                // 在设置请求时触发了错误
                console.error('Error message:', error.message);
                throw new Error(`注册失败：${error.message}`);
            }
        }
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
  },

    async getRoomDetails(roomIdString) {
    try {
        const url = `${API_BASE_URL}/rooms/details/by-string-id/${roomIdString}`;
        console.log('请求的 URL:', url);
        const response = await axios.get(url);
        return response.data;
    } catch (error) {
        console.error(`获取房间 ${roomIdString} 详情失败:`, error);
        throw error;
    }
  },

    // 添加 joinRoom 函数
  async joinRoom(roomId, userId, player) {
    console.log('Attempting to join room with ID:', roomId);
    try {
      console.log('Player data:', player);

      const response = await axios.post(
        `${API_BASE_URL}/rooms/join/${roomId}?userId=${userId}`,
        player // 作为 body 发送
      );
      // const response = await axios.post(`${API_BASE_URL}/rooms/join/${roomId}`);
      console.log('Backend response (joinRoom):', response.data);
      return response.data;
    } catch (error) {
      console.error('Error joining room:', error);
      if (error.response) {
        console.error('Error response data:', error.response.data);
        console.error('Error response status:', error.response.status);
        throw new Error(error.response.data.message || `加入房间失败：状态码 ${error.response.status}`);
      } else if (error.request) {
        console.error('Error request:', error.request);
        throw new Error('加入房间失败：无法连接到服务器');
      } else {
        console.error('Error message:', error.message);
        throw new Error(`加入房间失败：${error.message}`);
      }
    }
}
};

export default apiService;