
// 默认连接到房间1！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
//！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
// src/services/signalRService.js
import * as signalR from '@microsoft/signalr';
import { ref, computed, onMounted, onUnmounted } from 'vue';

class SignalRService {
  constructor() {
    this.hubConnection = null;
    this.chatMessages = ref([]);
    this.connectionId = ref('');
    this.isConnected = ref(false);
    this.currentRoomId = ref(1); // 默认连接到房间1
  }

  // 初始化并启动连接
  async initialize(roomId = 1) { // 默认为房间1
    this.currentRoomId.value = roomId;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`/gameHub?roomId=${roomId}`)
      .withAutomaticReconnect()
      .build();

    // 注册接收消息的处理函数
    this.hubConnection.on('ReceiveChatMessage', (data) => {
      console.log('接收到消息:', data);
      
      // 验证数据格式
      if (!data || !data.content) {
        console.error('无效的消息格式:', data);
        return;
      }
      
      // 使用不可变方式更新数组（确保 Vue 检测到变化）
      this.chatMessages.value = [
        ...this.chatMessages.value,
        {
          playerId: data.playerId || 0,
          username: data.username || '匿名用户',
          content: data.content,  // 关键：使用 content 字段
          timestamp: data.timestamp || new Date().toISOString(),
          isSystem: false,
          isCorrect: false,
          isWrong: false
        }
      ];
    });

    // 处理连接状态变化
    this.hubConnection.onclose(() => {
      this.isConnected.value = false;
      console.log('SignalR 连接已断开');
    });

    this.hubConnection.onreconnected(() => {
      this.isConnected.value = true;
      this.connectionId.value = this.hubConnection.connectionId;
      console.log('SignalR 连接已重新建立');
    });

    this.hubConnection.onreconnecting((error) => {
      this.isConnected.value = false;
      console.log('SignalR 正在重连:', error);
    });

    try {
      await this.hubConnection.start();
      this.isConnected.value = true;
      this.connectionId.value = this.hubConnection.connectionId;
      console.log(`SignalR 已连接到房间 ${roomId}`);
      return true;
    } catch (error) {
      console.error('SignalR 连接失败:', error);
      return false;
    }
  }

  // 发送聊天消息
  async sendChatMessage(message) {
    if (!this.isConnected.value || !this.hubConnection) {
        console.error('SignalR 连接未建立');
        return false;
    }

    try {
        // 传递当前房间ID和消息内容（字符串）
        await this.hubConnection.invoke('SendChatMessage', this.currentRoomId.value, message);
        return true;
    } catch (error) {
        console.error('发送消息失败:', error);
        return false;
    }
  }
  // 断开连接
  async disconnect() {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.isConnected.value = false;
    }
  }

  // 切换房间
  async switchRoom(roomId) {
    if (this.isConnected.value) {
      await this.disconnect();
    }
    return await this.initialize(roomId);
  }
}

// 创建单例实例
const signalRService = new SignalRService();
export default signalRService;