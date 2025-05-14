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
  --dai-green: #426666;
  --dai-green-light: #587878;
  --dai-green-lighter: #e8f0f0;
  --dai-green-dark: #304d4d;
  --dai-green-pale: #f0f5f5;
  --text-dark: #2c3e50;
}

.lobby-outer {
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background-color: var(--dai-green-pale);
}

.lobby-container {
  width: 75%;
  aspect-ratio: 16 / 9;
  max-width: 1400px;
  background: white;
  border-radius: 16px;
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.06);
  padding: 28px 36px;
  display: flex;
  flex-direction: column;
  border: 1px solid #e0e6e6;
  overflow: hidden;
}

.lobby-header {
  text-align: center;
  margin-bottom: 24px;
}

.lobby-header h1 {
  font-size: 2.8rem;
  color: var(--dai-green);
  font-weight: 600;
  letter-spacing: 4px;
  text-shadow: 1px 1px 0 rgba(0, 0, 0, 0.05);
  margin: 0;
  position: relative;
  display: inline-block;
}

.lobby-header h1::after {
  content: "";
  position: absolute;
  bottom: -8px;
  left: 50%;
  transform: translateX(-50%);
  width: 60px;
  height: 3px;
  background-color: var(--dai-green);
  border-radius: 3px;
}

.lobby-actions {
  display: flex;
  justify-content: space-around;
  margin-bottom: 28px;
}

.action-btn {
  flex: 1;
  margin: 0 14px;
  padding: 14px 0;
  font-size: 1.3rem;
  color: var(--dai-green);
  background: white;
  border: 2px solid var(--dai-green);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
  letter-spacing: 1px;
  box-shadow: 0 2px 4px rgba(66, 102, 102, 0.1);
}

.action-btn:hover {
  background: var(--dai-green);
  color: #fff;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(66, 102, 102, 0.2);
}

.lobby-room-list {
  flex: 1;
  overflow-y: auto;
  background-color: var(--dai-green-lighter);
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 20px;
  box-shadow: inset 0 2px 6px rgba(66, 102, 102, 0.08);
}

.list-title {
  font-size: 1.6rem;
  font-weight: 500;
  margin-bottom: 16px;
  color: var(--dai-green-dark);
  padding-left: 8px;
  border-left: 4px solid var(--dai-green);
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
  padding: 14px 20px;
  margin-bottom: 10px;
  border-radius: 8px;
  background-color: white;
  box-shadow: 0 2px 6px rgba(66, 102, 102, 0.08);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.room-row:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 10px rgba(66, 102, 102, 0.12);
}

.room-info {
  color: var(--text-dark);
  font-size: 1.2rem;
  font-weight: 500;
}

.room-btn {
  padding: 8px 20px;
  font-size: 1.05rem;
  border: none;
  border-radius: 6px;
  font-weight: 500;
  letter-spacing: 1px;
  transition: all 0.2s ease;
}

.room-btn.join {
  background: var(--dai-green);
  color: white;
  cursor: pointer;
}

.room-btn.full {
  background: #e0e0e0;
  color: #777;
}

.lobby-footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  padding-top: 14px;
  border-top: 1px solid var(--dai-green-lighter);
  font-size: 1.15rem;
  color: var(--text-dark);
}

.lobby-footer strong {
  color: var(--dai-green);
  margin-left: 4px;
}

.edit-nick-btn {
  margin-left: 12px;
  padding: 6px 12px;
  font-size: 1rem;
  color: var(--dai-green);
  background: transparent;
  border: 1px solid var(--dai-green);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.edit-nick-btn:hover {
  background-color: var(--dai-green);
  color: white;
}
</style>