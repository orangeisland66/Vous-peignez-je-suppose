<template>
  <div class="lobby-background">
    <div class="lobby-container">
      <!-- Header -->
      <header class="lobby-header">
        <div class="logo-container">
          <div class="logo-icon"></div>
          <h1>游戏大厅</h1>
        </div>
        <div class="user-info">
          <div class="avatar">{{ user?.username?.charAt(0) || '?' }}</div>
          <span class="username">{{ user?.username || '未登录' }}</span>
          <button @click="editNickname" class="edit-nick-btn">
            <span class="btn-icon">✎</span>
          </button>
        </div>
      </header>

      <!-- Main Content Area -->
      <div class="main-content">
        <!-- Left Panel - Actions -->
        <aside class="action-panel">
          <button @click="createRoom" class="action-card create">
            <div class="action-icon">+</div>
            <span class="action-text">创建房间</span>
          </button>
          <button @click="refreshRooms" class="action-card refresh">
            <div class="action-icon">↻</div>
            <span class="action-text">刷新列表</span>
          </button>
          <button @click="goToSettings" class="action-card settings">
            <div class="action-icon">⚙</div>
            <span class="action-text">设置</span>
          </button>
        </aside>

        <!-- Right Panel - Room List -->
        <section class="room-panel">
          <div class="panel-header">
            <h2>可用房间</h2>
            <span class="rooms-count">{{ rooms.length }} 个房间</span>
          </div>
          
          <div class="room-list-container">
            <div v-if="rooms.length === 0" class="no-rooms">
              <div class="empty-icon">🏠</div>
              <p>暂无可用房间</p>
              <button @click="createRoom" class="create-now-btn">立即创建</button>
            </div>
            
            <ul v-else class="room-list">
              <li v-for="room in rooms" :key="room.id" class="room-card">
                <div class="room-card-content">
                  <div class="room-name">{{ room.name }}</div> <!-- 修改这里，显示房间名称 -->
                  <div class="room-code">房间号: {{ room.roomId }}</div>
                  <div class="room-capacity">
                    <div class="capacity-bar">
                      <div class="capacity-fill" :style="{width: (room.players.length/room.maxPlayers)*100 + '%'}"></div>
                    </div>
                    <div class="capacity-text">人数:{{ room.players.length }}/{{ room.maxPlayers }}</div>
                  </div>
                  <button
                    v-if="room.players.length < room.maxPlayers"
                    @click="joinRoom(room.id)"
                    class="join-btn"
                  >加入游戏</button>
                  <div v-else class="full-badge">房间已满</div>
                </div>
              </li>
            </ul>
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Lobby',
  data() {
    return {
      rooms: [],
      user: null
    }
  },
  async created() {
    await this.fetchUserInfo()
    await this.fetchRooms()
  },
  methods: {
    async fetchRooms() {
      try {
        const res = await fetch('/api/rooms/list')
        if (!res.ok) throw new Error('获取房间列表失败')
        this.rooms = await res.json()
      } catch (e) {
        console.error(e)
        this.$toast?.error('获取房间列表失败')
      }
    },
    async fetchUserInfo() {
      try {
        const res = await fetch('/api/user/profile')
        if (!res.ok) throw new Error('获取用户信息失败')
        this.user = await res.json()
      } catch (e) {
        console.error(e)
        this.$toast?.error('获取用户信息失败')
      }
    },
    createRoom() {
      this.$router.push('/room/create')
    },
    refreshRooms() {
      this.fetchRooms()
    },
    goToSettings() {
      this.$router.push('/settings')
    },
    joinRoom(id) {
      this.$router.push(`/room/${id}/waiting`)
    },
    editNickname() {
      this.$router.push('/profile')
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
.lobby-background {
  background: linear-gradient(135deg, #F9FAFB 0%, #EEF2FF 100%);
  min-height: 100vh;
  width: 100vw;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  color: var(--dark);
}

.lobby-container {
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
.lobby-header {
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

.lobby-header h1 {
  font-size: 1.75rem;
  font-weight: 600;
  color: var(--primary);
  margin: 0;
}

.user-info {
  display: flex;
  align-items: center;
  background: var(--primary-lightest);
  padding: 8px 16px;
  border-radius: 50px;
  box-shadow: 0 2px 10px rgba(79, 70, 229, 0.1);
}

.avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 16px;
  margin-right: 10px;
}

.username {
  font-weight: 500;
  color: var(--primary-dark);
  margin-right: 10px;
}

.edit-nick-btn {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  border: none;
  background: white;
  color: var(--primary);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
  transition: all 0.2s ease;
}

.edit-nick-btn:hover {
  background: var(--primary);
  color: white;
  transform: scale(1.05);
}

.btn-icon {
  font-size: 14px;
}

/* Main Content Layout */
.main-content {
  display: flex;
  height: 70vh;
  min-height: 500px;
  padding: 24px;
  gap: 24px;
}

/* Action Panel Styles */
.action-panel {
  width: 240px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.action-card {
  background: white;
  border: none;
  border-radius: 16px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  flex: 1;
}

.action-card.create {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: var(--dark);
}

.action-card.refresh {
  background: white;
  color: var(--dark);
  border: 1px solid var(--gray-light);
}

.action-card.settings {
  background: var(--primary-lightest);
  color: var(--primary-dark);
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

.create .action-icon {
  background: rgba(255, 255, 255, 0.2);
}

.refresh .action-icon {
  background: var(--primary-lightest);
  color: var(--primary);
}

.settings .action-icon {
  background: white;
  color: var(--primary);
}

.action-text {
  font-weight: 600;
  font-size: 16px;
}

/* Room Panel Styles */
.room-panel {
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

.rooms-count {
  background: var(--primary-lightest);
  color: var(--primary);
  font-size: 14px;
  font-weight: 500;
  padding: 4px 12px;
  border-radius: 50px;
}

.room-list-container {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
}

.room-name {
  font-size: 20px; /* 调整字体大小，使其更大 */
  font-weight: 600;
  color: var(--dark);
  margin-bottom: 4px; /* 添加底部外边距，与 RoomId 分隔 */
}

.room-code {
  font-size: 14px; /* 房间 RoomId 的字体大小，比 18px 小 */
  color: var(--gray); /* 使用灰色，使其不那么醒目 */
  margin-top: 4px; /* 在房间名称下方留一点空间 */
  font-weight: 400; /* 可以设置为普通字体粗细 */
}


.no-rooms {
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

.no-rooms p {
  font-size: 16px;
  margin-bottom: 20px;
}

.create-now-btn {
  background: var(--primary);
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.create-now-btn:hover {
  background: var(--primary-dark);
}

.room-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 16px;
}

.room-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  transition: all 0.2s ease;
  border: 1px solid var(--gray-light);
}

.room-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 20px rgba(79, 70, 229, 0.15);
  border-color: var(--primary-light);
}

.room-card-content {
  padding: 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.room-id {
  font-size: 18px;
  font-weight: 600;
  color: var(--dark);
}

.room-capacity {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.capacity-bar {
  height: 8px;
  background: var(--gray-light);
  border-radius: 4px;
  overflow: hidden;
}

.capacity-fill {
  height: 100%;
  background: linear-gradient(to right, var(--primary), var(--primary-light));
  border-radius: 4px;
  transition: width 0.3s ease;
}

.capacity-text {
  font-size: 14px;
  color: var(--gray);
  text-align: right;
}

.join-btn {
  margin-top: 10px;
  background: var(--primary);
  color: white;
  border: none;
  padding: 12px;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.join-btn:hover {
  background: var(--primary-dark);
  transform: translateY(-2px);
}

.full-badge {
  margin-top: 10px;
  background: var(--gray-light);
  color: var(--gray);
  text-align: center;
  padding: 12px;
  border-radius: 8px;
  font-weight: 500;
}

/* Responsive Adjustments */
@media (max-width: 992px) {
  .main-content {
    flex-direction: column;
    height: auto;
  }
  
  .action-panel {
    width: 100%;
    flex-direction: row;
  }
  
  .room-list {
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  }
}

@media (max-width: 768px) {
  .lobby-container {
    width: 95%;
  }
  
  .lobby-header {
    flex-direction: column;
    gap: 16px;
  }
  
  .action-panel {
    flex-direction: column;
  }
}
</style>