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
          <button @click="goToProfile" class="action-card profile">
            <div class="action-icon">⚙</div>
            <span class="action-text">个人资料</span>
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
                  <div class="room-name">{{ room.name }}</div>
                  <div class="room-code">房间号: {{ room.roomId }}</div>
                  <div class="room-capacity">
                    <div class="capacity-bar">
                      <div class="capacity-fill"
                        :style="{ width: (room.players.length / room.maxPlayers) * 100 + '%' }">
                      </div>
                    </div>
                    <div class="capacity-text">人数:{{ room.players.length }}/{{ room.maxPlayers }}</div>
                  </div>
                  <button v-if="room.players.length < room.maxPlayers" @click="joinRoom(room.roomId, user.userId, user)"
                    class="join-btn">加入游戏</button>
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
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useStore } from 'vuex';

export default {
  name: 'Lobby',
  setup() {
    const router = useRouter();
    const store = useStore();
    const rooms = ref([]);
    const user = ref(null);
    const currentUserId = ref(null);

    const fetchRooms = async () => {
      try {
        console.log('Fetching rooms list...');
        const res = await fetch('/api/rooms/list');
        if (!res.ok) throw new Error('获取房间列表失败');
        rooms.value = await res.json();
      } catch (e) {
        console.error(e);
        this.$toast?.error('获取房间列表失败');
      }
    };

    const fetchUserInfo = async (userId) => {
      try {
        console.log('Fetching user info for userId:', userId);
        const res = await fetch(`/api/users/profile?userId=${userId}`);
        console.log('Fetching user info from URL:', `/api/users/profile?userId=${userId}`);

        if (!res.ok) {
          const errorText = await res.text();
          throw new Error(`获取用户信息失败: 状态码 ${res.status}, 响应: ${errorText}`);
        }
        user.value = await res.json();
        console.log('User info fetched:', user.value);
      } catch (e) {
        console.error('获取用户信息失败:', e);
        user.value = null;
        this.$toast?.error(e.message || '获取用户信息失败');
      }
    };

    const createRoom = () => {
      router.push('/room/create');
    };

    const refreshRooms = () => {
      fetchRooms();
    };

    const goToProfile = () => {
      router.push('/profile');
    };

    const joinRoom = async (roomId, userId, user) => {
      try {
        console.log('Joining room with ID:', roomId);
        console.log('User info:', user);
        const player = {
          Username: user.username,
          // UserId: user.id,
          // GameRoom:null,
          Score: 0,
          Status: 1,
          // IsHost: false,
          // HasDrawn: false,
          // LeftAt: null,
          // LastDrawingTime: null,
          // HasGuessed: false,
          JoinedAt: new Date().toISOString()
        };
        await store.dispatch('gameRoom/joinRoom', { roomId, userId, player: player });
        router.push(`/room/join/${roomId}`);
      } catch (error) {
        console.error('Failed to join room:', error);
      }
    };

    const editNickname = () => {
      router.push('/profile');
    };

    onMounted(async () => {
      console.log('Lobby created hook called.');
      const userIdString = localStorage.getItem('userId');
      console.log('Lobby.vue - userIdString from local storage:', userIdString);

      if (userIdString) {
        currentUserId.value = parseInt(userIdString);
        console.log('Lobby.vue - Parsed currentUserId:', currentUserId.value);

        if (isNaN(currentUserId.value)) {
          console.error('Lobby.vue - Parsed userId is NaN. Redirecting to login.');
          this.$toast?.error('用户ID无效，请重新登录');
          localStorage.removeItem('userId');
          router.push('/login');
          return;
        }

        await fetchUserInfo(currentUserId.value);
        await fetchRooms();
      } else {
        console.error('Lobby.vue - userId not found in local storage. Redirecting to login.');
        this.$toast?.error('未检测到登录用户，请重新登录');
        router.push('/login');
      }
    });

    return {
      rooms,
      user,
      createRoom,
      refreshRooms,
      goToProfile,
      joinRoom,
      editNickname
    };
  }
};
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
  width: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  color: var(--dark);
  padding: 20px 0;
  overflow: hidden;
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
  height: 90vh;
  max-height: 800px;
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
  color: var(--primary-dark);
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
  color: black;
  transform: scale(1.05);
}

.btn-icon {
  font-size: 14px;
}

/* Main Content Layout */
.main-content {
  display: flex;
  flex: 1;
  padding: 24px;
  gap: 24px;
  overflow: hidden;
}

/* Action Panel Styles */
.action-panel {
  width: 240px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  flex-shrink: 0;
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

.action-card.profile {
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
  flex-shrink: 0;
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
  height: 100%;
  scrollbar-width: thin;
  scrollbar-color: var(--primary-light) var(--gray-light);
}

/* Custom scrollbar styles */
.room-list-container::-webkit-scrollbar {
  width: 8px;
}

.room-list-container::-webkit-scrollbar-track {
  background: var(--gray-light);
  border-radius: 4px;
}

.room-list-container::-webkit-scrollbar-thumb {
  background: var(--primary-light);
  border-radius: 4px;
}

.room-list-container::-webkit-scrollbar-thumb:hover {
  background: var(--primary);
}

.room-name {
  font-size: 20px;
  font-weight: 600;
  color: var(--dark);
  margin-bottom: 4px;
}

.room-code {
  font-size: 14px;
  color: var(--gray);
  margin-top: 4px;
  font-weight: 400;
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
  color: black;
  border: none;
  padding: 12px;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.join-btn:hover {
  background: var(--primary-dark);
  color: white;
  transform: translateY(-2px);
}

.full-badge {
  margin-top: 10px;
  background: var(--primary);
  color: black;
  text-align: center;
  padding: 12px;
  border-radius: 8px;
  font-size: small;
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
    flex-shrink: 0;
  }

  .room-list {
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  }

  .room-panel {
    flex: 1;
    min-height: 400px;
  }

  .lobby-container {
    height: auto;
    max-height: none;
    min-height: 90vh;
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