<template>
  <div class="waiting-outer">
    <div class="waiting-container">
      <!-- Title -->
      <header class="waiting-header">
        <h1>等待开始</h1>
      </header>

      <!-- Room Info -->
      <section class="room-info">
        <span>房间号：{{ room?.id || '-' }}</span>
        <span>房主：{{ room?.host?.username || '-' }}</span>
      </section>

      <!-- Divider -->
      <div class="divider"></div>

      <!-- Player List -->
      <section class="player-section">
        <h2>玩家列表：</h2>
        <ol class="player-list">
          <li v-for="(p, i) in players" :key="p.id">
            {{ i + 1 }}. {{ p.username }} <span v-if="p.id === room.host.id">（房主）</span>
          </li>
          <li v-if="players.length < minPlayers">...</li>
        </ol>
      </section>

      <!-- Divider -->
      <div class="divider"></div>

      <!-- Actions -->
      <section class="waiting-actions">
        <button v-if="isHost" @click="startGame" class="btn primary">开始游戏</button>
        <button @click="leaveRoom" class="btn secondary">返回大厅</button>
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

    const fetchUser = async () => {
      const res = await fetch('/api/user/profile')
      user.value = await res.json()
    }
    const fetchRoom = async () => {
      const res = await fetch(`/api/rooms/${route.params.id}`)
      const data = await res.json()
      room.value = data
      players.value = data.players
    }
    const initSignalR = async () => {
      const conn = new HubConnectionBuilder()
        .withUrl('/gameHub')
        .build()
      conn.on('PlayerJoined', p => players.value.push(p))
      conn.on('PlayerLeft', id => players.value = players.value.filter(x => x.id !== id))
      conn.on('GameStarted', () => router.push(`/game/${room.value.id}`))
      await conn.start()
      connection.value = conn
    }
    const startGame = () => connection.value.invoke('StartGame', room.value.id)
    const leaveRoom = () => {
      connection.value.invoke('LeaveRoom', room.value.id)
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
.waiting-outer {
  display: flex; align-items: center; justify-content: center;
  width: 100vw; height: 100vh; background: #f5f5f5;
}
.waiting-container {
  width: 80vw; max-width: 1100px; aspect-ratio: 16/9;
  background: #fff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  padding: 32px; box-sizing: border-box;
  display: flex; flex-direction: column; justify-content: space-between;
}
.waiting-header h1 {
  text-align: center; font-size: 2.2rem; color: #333;
}
.room-info {
  display: flex; justify-content: center; gap: 48px;
  font-size: 1.3rem; color: #555;
}
.divider {
  height: 2px; background: #eee; width: 100%; margin: 16px 0;
}
.player-section h2 {
  font-size: 1.5rem; margin-bottom: 12px; color: #333;
  text-align: center;
}
.player-list {
  list-style: decimal inside; max-width: 400px;
  margin: 0 auto; font-size: 1.2rem; color: #444;
}
.waiting-actions {
  display: flex; justify-content: center; gap: 32px;
}
.btn {
  padding: 12px 24px; font-size: 1.2rem; border-radius: 6px;
  cursor: pointer; border: none; transition: background .2s;
}
.primary { background: #e60000; color: #fff; }
.primary:hover { background: #b80000; }
.secondary { background: #f0f0f0; color: #333; }
.secondary:hover { background: #ddd; }
@media (max-width: 900px) {
  .waiting-container { padding: 16px; }
  .room-info { font-size: 1rem; gap: 24px; }
  .player-list { font-size: 1rem; }
  .btn { font-size: 1rem; padding: 8px 16px; }
}
</style>
