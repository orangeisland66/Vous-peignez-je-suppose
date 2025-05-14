<template>
  <div class="waiting-background">
    <div class="waiting-container">
      <!-- Header -->
      <header class="waiting-header">
        <h1>等待开始</h1>
      </header>

      <!-- Room Info -->
      <section class="waiting-info">
        <div class="info-item">房间号：<span>{{ room?.id || '-' }}</span></div>
        <div class="info-item">房主：<span>{{ room?.host?.username || '-' }}</span></div>
      </section>

      <!-- Player List -->
      <section class="waiting-players">
        <h2>玩家列表</h2>
        <ul class="player-list">
          <li v-for="(p, i) in players" :key="p.id">
            <span class="player-index">{{ i + 1 }}.</span>
            <span class="player-name">{{ p.username }}</span>
            <span v-if="p.id === room.host.id" class="badge">房主</span>
          </li>
          <li v-if="players.length < minPlayers" class="player-placeholder">等待更多玩家加入...</li>
        </ul>
      </section>

      <!-- Actions -->
      <section class="waiting-actions">
        <button v-if="isHost" @click="startGame" class="btn start">开始游戏</button>
        <button @click="leaveRoom" class="btn leave">返回大厅</button>
      </section>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { HubConnectionBuilder } from '@microsoft/signalr'

export default {
  name: 'WaitingRoom',
  setup() {
    const route = useRoute()
    const router = useRouter()
    const room = ref({ host: {} })
    const players = ref([])
    const user = ref({})
    const connection = ref(null)
    const minPlayers = 4

    const isHost = computed(() => room.value.host.id === user.value.id)

    const fetchJSON = async (url) => {
      const res = await fetch(url)
      if (!res.ok) throw new Error('请求失败')
      return res.json()
    }

    const fetchUser = async () => {
      try {
        const data = await fetchJSON('/api/user/profile')
        user.value = data
      } catch (e) {
        console.error('获取用户信息失败', e)
      }
    }

    const fetchRoom = async () => {
      try {
        const data = await fetchJSON(`/api/rooms/${route.params.id}`)
        room.value = data
        players.value = data.players
      } catch (e) {
        console.error('获取房间信息失败', e)
      }
    }

    const initSignalR = async () => {
      const conn = new HubConnectionBuilder()
        .withUrl('/gameHub')
        .build()
      conn.on('PlayerJoined', p => players.value.push(p))
      conn.on('PlayerLeft', id => players.value = players.value.filter(x => x.id !== id))
      conn.on('GameStarted', () => {
        // 服务端通知所有玩家游戏开始，当前客户端也跳转
        router.push(`/game/${room.value.id}`)
      })
      try {
        await conn.start()
        connection.value = conn
      } catch (e) {
        console.error('SignalR 连接失败', e)
      }
    }

    const startGame = async () => {
      if (!connection.value) return
      try {
        // 主动调用 StartGame，服务端逻辑应触发 GameStarted 广播
        await connection.value.invoke('StartGame', room.value.id)
        // 可选：立即跳转
        router.push(`/game/${room.value.id}`)
      } catch (e) {
        console.error('开始游戏失败', e)
      }
    }

    const leaveRoom = async () => {
      if (connection.value) {
        try {
          await connection.value.invoke('LeaveRoom', room.value.id)
        } catch (e) {
          console.error('离开房间请求失败', e)
        }
      }
      // 确保跳转无误
      router.push('/lobby')
    }

    onMounted(async () => {
      await fetchUser()
      await fetchRoom()
      await initSignalR()
    })

    return { room, players, isHost, startGame, leaveRoom, minPlayers }
  }
}
</script>

<style scoped>
:root {
  --primary: #4F46E5;
  --primary-light: #818CF8;
  --gray-light: #E5E7EB;
  --dark: #1F2937;
}

.waiting-background {
  background: linear-gradient(135deg, #F9FAFB 0%, #EEF2FF 100%);
  width: 100vw;
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
}

.waiting-container {
  display: grid;
  grid-template-rows: auto auto 1fr auto;
  width: 90%;
  max-width: 1200px;
  background: white;
  border-radius: 24px;
  box-shadow: 0 10px 30px rgba(79, 70, 229, 0.1);
  padding: 32px;
  box-sizing: border-box;
  gap: 24px;
}

.waiting-header h1 {
  margin: 0;
  text-align: center;
  font-size: 2rem;
  color: var(--primary);
}

.waiting-info {
  display: flex;
  justify-content: center;
  gap: 48px;
  font-size: 1.2rem;
  color: var(--dark);
}

.info-item span {
  font-weight: 600;
}

.waiting-players {
  overflow-y: auto;
}
.waiting-players h2 {
  margin: 0;
  font-size: 1.5rem;
  color: var(--dark);
  text-align: center;
}

.player-list {
  list-style: none;
  padding: 0;
  margin: 16px 0 0;
  max-height: 300px;
  overflow-y: auto;
}
.player-list li {
  display: flex;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid var(--gray-light);
}
.player-index {
  width: 24px;
  font-weight: 600;
  color: var(--primary);
}
.player-name {
  flex: 1;
  color: var(--dark);
}
.badge {
  background: var(--primary-light);
  color: var(--primary);
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 0.9rem;
}
.player-placeholder {
  text-align: center;
  color: var(--gray-light);
  padding: 12px 0;
}

.waiting-actions {
  display: flex;
  justify-content: center;
  gap: 24px;
}
.btn {
  padding: 12px 24px;
  font-size: 1.1rem;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
}
.btn.start {
  background: var(--primary);
  color: white;
}
.btn.start:hover {
  background: var(--primary-light);
}
.btn.leave {
  background: var(--gray-light);
  color: var(--dark);
}
.btn.leave:hover {
  background: #ddd;
}

/* Responsive */
@media (max-width: 768px) {
  .waiting-container {
    padding: 16px;
    gap: 16px;
  }
  .waiting-info {
    flex-direction: column;
    gap: 16px;
  }
  .player-list {
    max-height: 200px;
  }
}</style>
