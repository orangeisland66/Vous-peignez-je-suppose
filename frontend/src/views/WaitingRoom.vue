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
          <span class="room-id">#{{ room?.id || '-' }}</span>
        </div>
      </header>

      <!-- Main Content Area -->
      <div class="main-content">
        <!-- Left Panel - Host & Actions -->
        <aside class="host-panel">
          <div class="host-info">
            <div class="avatar">{{ room?.host?.username?.charAt(0) || '?' }}</div>
            <div class="host-details">
              <div class="host-label">房主</div>
              <div class="host-name">{{ room?.host?.username || '-' }}</div>
            </div>
          </div>
          
          <div class="action-cards">
            <button v-if="isHost" @click="startGame" class="action-card start">
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
            <span class="player-count">{{ players.length }} 名玩家</span>
          </div>
          
          <div class="player-list-container">
            <div v-if="players.length === 0" class="no-players">
              <div class="empty-icon">👤</div>
              <p>暂无玩家</p>
            </div>
            
            <ul v-else class="player-list">
              <li v-for="(p, i) in players" :key="p.id" class="player-card">
                <div class="player-avatar">{{ p.username.charAt(0) }}</div>
                <div class="player-details">
                  <div class="player-name">{{ p.username }}</div>
                  <div class="player-id">玩家 #{{ i + 1 }}</div>
                </div>
                <div v-if="p.id === room.host.id" class="host-badge">房主</div>
              </li>
              
              <!-- 移除了最小玩家人数提示 -->
            </ul>
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'WaitingRoom',
  data() {
    return {
      room: { id: 1, host: { id: 1, username: 'Alice' } },
      players: [
        { id: 1, username: 'Alice' },
        { id: 2, username: 'Bob' },
        { id: 3, username: 'Charlie' },
        { id: 4, username: 'David' },
        { id: 5, username: 'Eve' }
      ],
      user: { id: 1, username: 'Alice' },
      minPlayers: 4
    }
  },
  computed: {
    isHost() {
      return this.room.host.id === this.user.id
    }
  },
  mounted() {
    // 为了测试，直接使用静态数据而不是从API获取
    console.log('WaitingRoom mounted, using mock data for testing')
  },
  methods: {
    startGame() {
      // 直接跳转到游戏页面，不进行人数检查
      console.log('直接跳转到游戏页面')
      this.$router.push(`/room/${this.room.id}/game`)
    },
    leaveRoom() {
      console.log('返回大厅')
      this.$router.push('/lobby')
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
  background: linear-gradient(135deg, #F9FAFB 0%, #EEF2FF 100%);
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
  font-size: 28px;
  margin-bottom: 12px;
  height: 40px;
  width: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 12px;
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