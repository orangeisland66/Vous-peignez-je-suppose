// src/services/signalRService.js
import * as signalR from '@microsoft/signalr';
import { ref, computed, onMounted, onUnmounted } from 'vue';

class SignalRService {
  constructor() {
    this.hubConnection = null;
    this.chatMessages = ref([]);
    this.connectionId = ref('');
    this.isConnected = ref(false);
  }

  // 初始化并启动连接
  async initialize(roomId) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`/gameHub?roomId=${roomId}`) // 假设后端端点是 /gameHub
      .withAutomaticReconnect()
      .build();

    // 注册接收消息的处理函数
    this.hubConnection.on('ReceiveChatMessage', (playerId, message) => {
      this.chatMessages.value.push({
        playerId,
        message,
        timestamp: new Date().toISOString()
      });
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
      console.log('SignalR 连接已建立');
      return true;
    } catch (error) {
      console.error('SignalR 连接失败:', error);
      return false;
    }
  }

  // 发送聊天消息
  async sendChatMessage(roomId, message) {
    if (!this.isConnected.value || !this.hubConnection) {
      console.error('SignalR 连接未建立');
      return false;
    }

    try {
      await this.hubConnection.invoke('SendChatMessage', roomId, message);
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
}

// 创建单例实例
const signalRService = new SignalRService();
export default signalRService;