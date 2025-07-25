<template>
  <div class="waiting-background">
    <div class="waiting-container">
      <!-- Header -->
      <header class="waiting-header">
        <div class="logo-container">
          <div class="logo-icon"></div>
          <h1>等待游戏开始</h1>
        </div>
        <div class="room-badge">
          <span class="room-label">房间号</span>
          <!-- 显示从后端获取的字符串 roomId -->
          <span class="room-id">#{{ room?.roomId || '-' }}</span>
        </div>
      </header>

      <!-- Main Content Area -->
      <!-- 使用 v-if 控制，在加载完成且没有错误时显示 -->
      <div class="main-content" v-if="!isLoading && room && !errorMessage">
        <!-- Left Panel - Host & Actions -->
        <aside class="host-panel">
          <div class="host-info">
            <!-- 显示房主信息 -->
            <div class="avatar">{{ hostPlayer?.userName?.charAt(0)?.toUpperCase() || '?' }}</div>
            <div class="host-details">
              <div class="host-label">房主</div>
              <div class="host-name">{{ hostPlayer?.username || '加载中...' }}</div>
            </div>
          </div>

          <div class="action-cards">
            <!-- 判断当前用户是否为房主 -->
            <button v-if="isCurrentUserHost" @click="startGame" class="action-card start">
              <div class="action-icon">▶</div>
              <span class="action-text">开始游戏</span>
            </button>
            <button @click="leaveRoom" class="action-card leave">
              <div class="action-icon">←</div>
              <span class="action-text">返回大厅</span>
            </button>
          </div>

          <div class="game-rules">
            <h3>游戏规则</h3>
            <ul>
              <li>每轮有一名玩家担任画师</li>
              <li>画师根据提示词进行绘画</li>
              <li>其他玩家猜测画师正在画的内容</li>
              <li>猜对的玩家获得积分</li>
            </ul>
          </div>
        </aside>

        <!-- Right Panel - Player List -->
        <section class="players-panel">
          <div class="panel-header">
            <h2>玩家列表</h2>
            <!-- 显示真实玩家数量 -->
            <span class="player-count">{{ actualPlayers.length }} 名玩家</span>
          </div>

          <div class="player-list-container">
            <div v-if="actualPlayers.length === 0 && !isLoading" class="no-players">
              <div class="empty-icon">👤</div>
              <p>暂无玩家，等待加入...</p> <!-- 或者你之前的 "暂无玩家" -->
            </div>


            <ul v-else class="player-list">
              <!-- 遍历从后端获取的 players -->
              <li v-for="(player, index) in actualPlayers" :key="player.id" class="player-card">
                <!-- 假设 Player 对象有 user.username -->
                <div class="player-avatar">{{ player.user?.userName?.charAt(0)?.toUpperCase() || 'P' }}</div>
                <div class="player-details">
                  <div class="player-name">{{ player.user?.username || '玩家加载中' }}</div>
                  <div class="player-id">玩家 #{{ index + 1 }}</div>
                </div>
                <!-- 判断是否为房主 -->
                <div v-if="player.isHost" class="host-badge">房主</div>
              </li>
            </ul>
          </div>
        </section>
      </div>
      <!-- 加载状态 -->
      <div v-if="isLoading" class="loading-indicator">
        <p>正在加载房间信息...</p>
      </div>
      <!-- 错误信息 -->
      <div v-if="!isLoading && errorMessage" class="error-message">
        <p>{{ errorMessage }}</p>
        <button @click="leaveRoom">返回大厅</button>
      </div>
    </div>
  </div>
</template>

<script>
import apiService from '@/services/apiService'
import signalRService from '../services/signalRService';

export default {
  name: 'WaitingRoom',
  data() {
    return {
      room: null,
      currentUser: null,
      isLoading: true,
      errorMessage: '',
      pollInterval: null, // 轮询定时器
      pollIntervalMs: 3000, // 轮询间隔（3秒）
      isLoadingStartGame: false,
    };
  },
  computed: {
    // 从路由参数获取房间的字符串ID
    roomIdFromRoute() {
      return this.$route.params.roomId;
    },
    // 从 room.players 中找到房主玩家对象 (Player 对象)
    hostPlayerRecord() {
      if (this.room && this.room.players) {
        return this.room.players.find(p => p.isHost === true);
      }
      return null;
    },
    // 获取房主的 User 对象 (用于显示用户名等)
    hostPlayer() {
      return this.hostPlayerRecord?.user || null;
    },
    // 判断当前登录用户是否为房主
    isCurrentUserHost() {
      if (this.currentUser && this.hostPlayer) {
        return this.currentUser.id === this.hostPlayer.id; // 比较 User ID
      }
      return false;
    },
    // 实际的玩家列表 (Player 对象列表)
    actualPlayers() {
      return this.room?.players || [];
    },
    // isHost() {
    //   return this.room.host.id === this.user.id
    // }
  },
  async created() {
    console.log('[WaitingRoom] Created hook started.');
    // 1. 获取当前登录用户信息
    const userIdString = localStorage.getItem('userId');
    const userName = localStorage.getItem('userName');
    console.log(`[WaitingRoom] localStorage - userId: ${userIdString}, userName: ${userName}`);
    if (userIdString && userName) {
      this.currentUser = {
        id: parseInt(userIdString),
        userName: userName,
      };
    } else {
      this.errorMessage = "用户未登录，请先登录。";
      this.isLoading = false;
      this.$router.push('/login'); // 跳转到登录页
      return;
    }

    // 2. 检查路由中是否有 roomId
    if (!this.roomIdFromRoute) {
      this.errorMessage = "未找到房间ID，无法加载房间信息。";
      this.isLoading = false;
      // 可以考虑跳转回大厅或显示更友好的错误页
      this.$router.push('/lobby');
      return;

    }

    // 3. 调用 API 获取房间详情
    await this.fetchRoomDetails();

    if (this.roomIdFromRoute && this.currentUser) {
      this.startPolling();
    }
    await signalRService.initialize(this.roomIdFromRoute)
      .then(() => {
        console.log(`[WaitingRoom] SignalR initialized for room: ${this.roomIdFromRoute}`);
        // 3.1 加入房间组
        return signalRService.joinGroup(this.roomIdFromRoute, this.currentUser.id);
      })
      .then(() => {
        console.log(`[WaitingRoom] Successfully joined SignalR group for room: ${this.roomIdFromRoute}`);
        // 3.2 注册SignalR事件监听器
       // signalRService.registerRoomEvents(this);
      })
      .catch(err => {
        console.error('[WaitingRoom] Error initializing SignalR:', err);
        this.errorMessage = '无法连接到游戏服务器，请稍后重试。';
      });
  },
  beforeUnmount() {
    this.stopPolling(); // 组件销毁时停止轮询
  },
  // mounted() {
  //   // 为了测试，直接使用静态数据而不是从API获取
  //   console.log('WaitingRoom mounted, using mock data for testing')
  // },
  methods: {
    startPolling() {
      this.stopPolling(); // 防止重复启动
      this.pollInterval = setInterval(() => {
        this.fetchRoomDetails(false); // 轮询调用获取房间数据的方法
      }, this.pollIntervalMs);
    },

    stopPolling() {
      if (this.pollInterval) {
        clearInterval(this.pollInterval);
        this.pollInterval = null;
      }
    },

    async fetchRoomDetails(if_loading = true) {
      this.isLoading = if_loading;
      this.errorMessage = ''; // 重置错误信息
      try {
        console.log(`WaitingRoom: 正在获取房间 ${this.roomIdFromRoute} 的详细信息...`);
        const response = await apiService.getRoomDetails(this.roomIdFromRoute);
        console.log('WaitingRoom: 获取房间详情的响应:', response);
        if (response && response.success && response.room) {
          this.room = response.room;
          console.log('WaitingRoom: 成功获取房间数据:', this.room);
          // 检查游戏状态 (RoomStatus.Playing 在后端枚举中是 1)
          if (this.room.status === 1) { // 1 代表游戏进行中
            console.log('[WaitingRoom] Game has started (Status=1, detected via polling). Navigating to game page.');
            this.stopPolling(); // 游戏开始，停止轮询
            this.$router.push(`/room/${this.room.roomId}/game`); // 跳转到游戏页面
            return; // 已跳转，无需进一步处理
          }
          // 可选：处理其他状态，例如房间关闭或游戏结束
          else if (this.room.Status === 2 || this.room.Status === 3) { // 2: Completed, 3: Closed
            const message = this.room.Status === 2 ? "游戏已结束。" : "房间已关闭。";
            console.log(`[WaitingRoom] Room status is ${this.room.Status} (${message}). Navigating to lobby.`);
            this.stopPolling();
            alert(message); // 提示用户
            this.$router.push('/lobby'); // 跳转回大厅
            return;
          }
          // 验证数据结构 (可选，用于调试)
          if (!this.room.roomId) console.warn("后端返回的 room 对象缺少 roomId 字符串");
          if (!this.room.players) console.warn("后端返回的 room 对象缺少 players 列表");
          else {
            this.room.players.forEach(p => {
              if (p.user === undefined) console.warn(`玩家 ID ${p.id} (后端Player.Id) 缺少 user 对象`);
              if (p.isHost === undefined) console.warn(`玩家 ID ${p.id} 缺少 isHost 标志`);
            });
          }
          if (!this.hostPlayer) {
            console.warn("无法从房间数据中确定房主信息。请检查后端是否正确设置了 Player.isHost 和 Player.user。");
            // 检查 this.room.creator 是否可用作为备选
            if (this.room.creator) {
              console.log("尝试使用 room.creator 作为房主信息:", this.room.creator);
              // 如果 hostPlayer 逻辑依赖于 players 列表中的 isHost，
              // 而 creator 是直接挂在 room 上的，需要调整 hostPlayer 计算属性
            }
          }

        } else {
          this.errorMessage = response?.message || '无法加载房间信息，房间可能不存在或已关闭。';
          this.room = null; // 清空房间数据
          console.error('WaitingRoom: 获取房间信息失败:', response?.message);
          // 如果房间不存在或加载失败，应该停止轮询
          this.stopPolling(); // <--- 确保在这里也停止轮询
        }
      } catch (error) {
        console.error('WaitingRoom: 获取房间详情时发生网络或API错误:', error);
        if (error.response && error.response.status === 404) {
          this.errorMessage = '房间不存在或已被关闭。';
        } else {
          this.errorMessage = '加载房间信息失败，请检查网络连接或稍后重试。';
        }
        this.room = null; // 清空房间数据
      } finally {
        this.isLoading = false;
      }
    },
    async startGame() {
      if (!this.isCurrentUserHost) {
        alert("只有房主才能开始游戏。");
        return;
      }
      if (!this.room || !this.currentUser || !this.currentUser.id) {
        alert("无法开始游戏：房间或当前用户信息不完整。");
        return;
      }

      // 可选：在这里添加其他开始游戏的逻辑，例如检查玩家人数
      if (this.actualPlayers.length < 2) { // 假设最少需要2名玩家
        alert("玩家人数不足（至少需要2人），无法开始游戏。");
        return;
      }

      this.isLoadingStartGame = true; // 设置按钮加载状态
      this.errorMessage = ''; // 清除之前的错误信息

      console.log(`[WaitingRoom] Host (User ID: ${this.currentUser.id}) is attempting to start game for room: ${this.room.roomId}`);
      try {
        // 调用SignalR方法而非API
        await signalRService.hubConnection.invoke(
          "StartRoomGame", 
          this.room.roomId, 
          this.currentUser.id
        );
        
        // 成功调用SignalR方法，等待服务器推送游戏开始消息
        console.log("[WaitingRoom] SignalR call to start game was successful. Waiting for game start notification.");
      } catch (error) {
        // 处理SignalR调用错误
        this.errorMessage = `开始游戏时发生错误: ${error?.message || '未知错误，请检查控制台。'}`;
        alert(this.errorMessage);
        console.error('[WaitingRoom] Error calling StartRoomGame via SignalR:', error);
      } finally {
        this.isLoadingStartGame = false;
      }
    },
    // 修改 leaveRoom 方法
    async leaveRoom() {
      console.log('[WaitingRoom] User clicked "返回大厅" button.');

      if (!this.room || !this.currentUser || !this.currentUser.id) {
        console.warn('[WaitingRoom] Room or currentUser data is missing. Redirecting to lobby.');
        this.$router.push('/lobby');
        return;
      }

      const roomIdString = this.room.roomId;
      const userId = this.currentUser.id;

      if (this.isCurrentUserHost) {
        // 房主离开，弹出确认框
        if (!confirm("您是房主，离开将会解散当前房间。确定要离开吗？")) {
          console.log('[WaitingRoom] Host cancelled disband room operation.');
          return; // 用户取消操作
        }
        console.log(`[WaitingRoom] Host (ID: ${userId}) confirmed to disband/exit room (ID: ${roomIdString}).`);
      } else {
        console.log(`[WaitingRoom] Player (ID: ${userId}) is leaving room (ID: ${roomIdString}).`);
      }

      try {
        // 调用 apiService 中的 exitRoom 方法
        const response = await apiService.exitRoom(roomIdString, userId);

        if (response && response.success) {
          console.log(`[WaitingRoom] Exit room operation successful: ${response.message}`);
          if (response.roomDisbanded) {
            alert('房间已成功解散。'); // 或者使用更美观的通知
          } else {
            alert('您已成功离开房间。'); // 或者使用更美观的通知
          }
        } else {
          // API 调用成功返回，但业务逻辑上失败 (e.g., backend returns { success: false, message: "..." })
          const failureMessage = response?.message || '操作失败，服务器返回未知错误。';
          console.error(`[WaitingRoom] Failed to exit room ${roomIdString}: ${failureMessage}`);
          alert(`操作失败: ${failureMessage}`);
        }
      } catch (error) {
        // API 调用本身发生错误 (网络错误, HTTP 500, 后端抛出的未处理异常等)
        // error 对象可能是后端直接抛出的 { success: false, message: "..." } (如果 apiService 这样处理了)
        // 或者是一个更通用的 Error 对象
        const errorMessage = error?.message || '无法连接到服务器或发生未知网络错误。';
        console.error(`[WaitingRoom] Error calling exitRoom API for room ${roomIdString}:`, error);
        alert(`操作失败: ${errorMessage}`);
      } finally {
        // 无论API调用结果如何，都将用户导航回大厅
        console.log('[WaitingRoom] Navigating to /lobby.');
        this.$router.push('/lobby');
      }
    }
  }
}
</script>

<style scoped>
:root {
  --primary: #4F46E5;
  --primary-dark: #4338CA;
  --primary-light: #818CF8;
  --primary-lightest: #EEF2FF;
  --secondary: #10B981;
  --secondary-dark: #059669;
  --accent: #F472B6;
  --dark: #1F2937;
  --light: #F9FAFB;
  --gray: #6B7280;
  --gray-light: #E5E7EB;
  --success: #22C55E;
  --warning: #F59E0B;
  --danger: #EF4444;
}

/* Base and Layout Styles */
.waiting-background {
  /* background: linear-gradient(135deg, #3153a9 0%, #15788c 100%); */
  min-height: 100vh;
  width: 100vw;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  color: var(--dark);
}

.waiting-container {
  width: 90%;
  max-width: 1200px;
  background: white;
  border-radius: 24px;
  box-shadow: 0 10px 30px rgba(79, 70, 229, 0.1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

/* Header Styles */
.waiting-header {
  padding: 20px 32px;
  background: white;
  border-bottom: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo-container {
  display: flex;
  align-items: center;
}

.logo-icon {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  border-radius: 10px;
  margin-right: 12px;
  position: relative;
}

.logo-icon::before {
  content: "🎮";
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 20px;
}

.waiting-header h1 {
  font-size: 1.75rem;
  font-weight: 600;
  color: var(--primary);
  margin: 0;
}

.room-badge {
  background: var(--primary-lightest);
  padding: 8px 16px;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  box-shadow: 0 2px 8px rgba(79, 70, 229, 0.1);
}

.room-label {
  font-size: 12px;
  color: var(--primary);
  margin-bottom: 2px;
}

.room-id {
  font-size: 16px;
  font-weight: 600;
  color: var(--primary-dark);
}

/* Main Content Layout */
.main-content {
  display: flex;
  height: 70vh;
  min-height: 500px;
  padding: 24px;
  gap: 24px;
}

/* Host Panel Styles */
.host-panel {
  width: 240px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.host-info {
  background: var(--primary-lightest);
  border-radius: 16px;
  padding: 20px;
  display: flex;
  align-items: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  background: var(--primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 22px;
  margin-right: 16px;
}

.host-details {
  display: flex;
  flex-direction: column;
}

.host-label {
  font-size: 14px;
  color: var(--primary);
  margin-bottom: 4px;
}

.host-name {
  font-size: 18px;
  font-weight: 600;
  color: var(--primary-dark);
}

.action-cards {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.action-card {
  background: white;
  border: none;
  border-radius: 16px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.action-card.start {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: white;
}

.action-card.leave {
  background: white;
  color: var(--dark);
  border: 1px solid var(--gray-light);
}

.action-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

.action-icon {
  font-size: 24px;
  margin-bottom: 12px;
  height: 40px;
  width: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 12px;
  color: #4F46E5;
}

.start .action-icon {
  background: rgba(255, 255, 255, 0.2);
}

.leave .action-icon {
  background: var(--primary-lightest);
  color: var(--primary);
}

.action-text {
  font-weight: 600;
  font-size: 16px;
  color: black
}

.leave .action-text {
  color: var(--dark);
}

.game-rules {
  background: white;
  border-radius: 16px;
  padding: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid var(--gray-light);
}

.game-rules h3 {
  margin: 0 0 12px 0;
  font-size: 18px;
  color: var(--dark);
}

.game-rules ul {
  margin: 0;
  padding-left: 20px;
  color: var(--gray);
}

.game-rules li {
  margin-bottom: 8px;
  font-size: 14px;
}

/* Players Panel Styles */
.players-panel {
  flex: 1;
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.panel-header {
  padding: 20px 24px;
  border-bottom: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.panel-header h2 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--dark);
  margin: 0;
}

.player-count {
  background: var(--primary-lightest);
  color: var(--primary);
  font-size: 14px;
  font-weight: 500;
  padding: 4px 12px;
  border-radius: 50px;
}

.player-list-container {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
}

.no-players {
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: var(--gray);
  text-align: center;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.5;
}

.no-players p {
  font-size: 16px;
  margin-bottom: 20px;
}

.player-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.player-card {
  background: white;
  border-radius: 12px;
  padding: 16px;
  display: flex;
  align-items: center;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  transition: all 0.2s ease;
  border: 1px solid var(--gray-light);
}

.player-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 20px rgba(79, 70, 229, 0.15);
  border-color: var(--primary-light);
}

.player-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--primary-lightest);
  color: var(--primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 18px;
  margin-right: 16px;
}

.player-details {
  flex: 1;
}

.player-name {
  font-size: 16px;
  font-weight: 600;
  color: var(--dark);
  margin-bottom: 4px;
}

.player-id {
  font-size: 14px;
  color: var(--gray);
}

.host-badge {
  background: var(--primary-lightest);
  color: var(--primary);
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
}

/* 移除了玩家占位符样式 */

/* Responsive Adjustments */
@media (max-width: 992px) {
  .main-content {
    flex-direction: column;
    height: auto;
  }

  .host-panel {
    width: 100%;
  }

  .action-cards {
    flex-direction: row;
    gap: 16px;
  }

  .action-card {
    flex: 1;
  }
}

@media (max-width: 768px) {
  .waiting-container {
    width: 95%;
  }

  .waiting-header {
    flex-direction: column;
    gap: 16px;
  }

  .action-cards {
    flex-direction: column;
  }
}
</style>