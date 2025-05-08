<template>
  <div class="lobby-outer">
    <div class="lobby-container">
      <!-- Header -->
      <header class="lobby-header">
        <h1>大厅</h1>
      </header>

      <!-- Actions -->
      <section class="lobby-actions">
        <button @click="createRoom" class="action-btn">创建房间</button>
        <button @click="refreshRooms" class="action-btn">刷新列表</button>
        <button @click="goToSettings" class="action-btn">设置</button>
      </section>

      <!-- Room List -->
      <section class="lobby-room-list">
        <div class="list-title">房间列表：</div>
        <ul class="room-table">
          <li v-for="room in rooms" :key="room.id" class="room-row">
            <span class="room-info">房间 {{ room.id }} ({{ room.playerCount }}/{{ room.maxPlayers }})</span>
            <button
              v-if="room.playerCount < room.maxPlayers"
              @click="joinRoom(room.id)"
              class="room-btn join"
            >加入</button>
            <span v-else class="room-btn full">已满</span>
          </li>
        </ul>
      </section>

      <!-- Footer -->
      <footer class="lobby-footer">
        <span>昵称：<strong>{{ user?.username || '———' }}</strong></span>
        <button @click="editNickname" class="edit-nick-btn">修改昵称</button>
      </footer>
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
        const res = await fetch('/api/rooms')
        if (!res.ok) throw new Error('获取房间列表失败')
        this.rooms = await res.json()
      } catch (e) {
        console.error(e)
      }
    },
    async fetchUserInfo() {
      try {
        const res = await fetch('/api/user/profile')
        if (!res.ok) throw new Error('获取用户信息失败')
        this.user = await res.json()
      } catch (e) {
        console.error(e)
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
.lobby-outer {
  width: 100vw;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f5f5f5;
}
.lobby-container {
  width: 85vw;
  max-width: 1600px;
  aspect-ratio: 16 / 9;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  padding: 24px 32px;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
}

/* Header */
.lobby-header {
  text-align: center;
  margin-bottom: 20px;
}
.lobby-header h1 {
  font-size: 2.5rem;
  color: #333;
}

/* Actions */
.lobby-actions {
  display: flex;
  justify-content: space-around;
  margin-bottom: 24px;
}
.action-btn {
  flex: 1;
  margin: 0 12px;
  padding: 12px 0;
  font-size: 1.2rem;
  color: #e60000;
  background: transparent;
  border: 2px solid #e60000;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}
.action-btn:hover {
  background: #e60000;
  color: #fff;
}

/* Room List */
.lobby-room-list {
  flex: 1;
  overflow-y: auto;
}
.list-title {
  font-size: 1.5rem;
  font-weight: 500;
  margin-bottom: 12px;
}
.room-table {
  list-style: none;
  padding: 0;
  margin: 0;
}
.room-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 16px;
  border-bottom: 1px solid #eee;
  font-size: 1.2rem;
}
.room-info {
  color: #555;
}
.room-btn {
  padding: 6px 14px;
  font-size: 1rem;
  border: none;
  border-radius: 4px;
  cursor: default;
}
.room-btn.join {
  background: #e60000;
  color: #fff;
  cursor: pointer;
}
.room-btn.join:hover {
  background: #b80000;
}
.room-btn.full {
  background: #ccc;
  color: #777;
}

/* Footer */
.lobby-footer {
  text-align: right;
  padding-top: 12px;
  border-top: 1px solid #eee;
  font-size: 1.1rem;
  color: #333;
}
.edit-nick-btn {
  margin-left: 8px;
  font-size: 1rem;
  color: #e60000;
  background: none;
  border: none;
  cursor: pointer;
  text-decoration: underline;
}
.edit-nick-btn:hover {
  color: #b80000;
}

/* Responsive */
@media (max-width: 900px) {
  .lobby-container { padding: 16px; }
  .action-btn { font-size: 1rem; margin: 0 6px; }
  .room-row { font-size: 1rem; padding: 8px 12px; }
  .lobby-header h1 { font-size: 2rem; }
}
</style>
