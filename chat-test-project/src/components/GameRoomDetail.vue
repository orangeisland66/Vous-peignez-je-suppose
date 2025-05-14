<template>
    <div class="game-room-detail">
        <div class="room-header">
            <h2>{{ room.name }}</h2>
            <div class="room-info">
                <span>房主: {{ room.owner }}</span>
                <span>房间状态: {{ room.status }}</span>
                <span>玩家数量: {{ room.players.length }}/{{ room.maxPlayers }}</span>
            </div>
        </div>

        <div class="room-content">
            <div class="players-list">
                <h3>玩家列表</h3>
                <ul>
                    <li v-for="player in room.players" :key="player.id" 
                            :class="{ 'current-player': player.id === currentUserId }">
                        <span class="player-name">{{ player.username }}</span>
                        <span class="player-status" :class="player.status">
                            {{ player.status }}
                        </span>
                        <span v-if="player.isOwner" class="owner-badge">房主</span>
                    </li>
                </ul>
            </div>

            <div class="room-settings">
                <h3>房间设置</h3>
                <div class="setting-item">
                    <span>回合数：</span>
                    <span>{{ room.rounds }}</span>
                </div>
                <div class="setting-item">
                    <span>绘画时间：</span>
                    <span>{{ room.drawTime }}秒</span>
                </div>
            </div>

            <div class="room-actions">
                <button 
                    v-if="isOwner"
                    :disabled="!canStartGame"
                    @click="startGame"
                    class="start-button">
                    开始游戏
                </button>
                <button 
                    v-if="!isOwner" 
                    @click="leaveRoom"
                    class="leave-button">
                    离开房间
                </button>
            </div>
        </div>

        <Chat 
        :roomId="room.id" 
        :playerId="currentUserId" 
        class="room-chat" 
        />
    </div>
</template>

<script>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useStore } from 'vuex'
import Chat from './Chat.vue'

export default {
    name: 'GameRoomDetail',
    components: {
        Chat
    },
    setup() {
        const store = useStore()
        const room = ref({
            id: '',
            name: '',
            owner: '',
            status: '等待中',
            players: [],
            maxPlayers: 8,
            rounds: 3,
            drawTime: 60
        })

        const currentUserId = computed(() => store.state.user.id)
        const isOwner = computed(() => room.value.owner === currentUserId.value)
        const canStartGame = computed(() => 
            isOwner.value && 
            room.value.players.length >= 2 && 
            room.value.status === '等待中'
        )

        const startGame = async () => {
            try {
                await store.dispatch('game/startGame', room.value.id)
            } catch (error) {
                console.error('Failed to start game:', error)
            }
        }

        const leaveRoom = async () => {
            try {
                await store.dispatch('gameRoom/leaveRoom', room.value.id)
            } catch (error) {
                console.error('Failed to leave room:', error)
            }
        }

        onMounted(async () => {
            // 获取房间详情
            const roomId = 'your-room-id'; // 硬编码房间 ID
            try {
                const roomData = await store.dispatch('gameRoom/getRoomDetails', roomId)
                room.value = roomData
            } catch (error) {
                console.error('Failed to fetch room details:', error)
            }
        })

        return {
            room,
            currentUserId,
            isOwner,
            canStartGame,
            startGame,
            leaveRoom
        }
    }
}
</script>

<style scoped>
.game-room-detail {
    padding: 20px;
    max-width: 1200px;
    margin: 0 auto;
}

.room-header {
    margin-bottom: 20px;
    padding-bottom: 10px;
    border-bottom: 1px solid #eee;
}

.room-info {
    display: flex;
    gap: 20px;
    margin-top: 10px;
}

.players-list {
    margin-bottom: 20px;
}

.players-list ul {
    list-style: none;
    padding: 0;
}

.players-list li {
    display: flex;
    align-items: center;
    padding: 10px;
    margin-bottom: 5px;
    background-color: #f5f5f5;
    border-radius: 4px;
}

.current-player {
    background-color: #e3f2fd;
}

.player-name {
    flex: 1;
}

.player-status {
    margin: 0 10px;
}

.owner-badge {
    background-color: #ffd700;
    padding: 2px 8px;
    border-radius: 12px;
    font-size: 12px;
}

.room-settings {
    margin-bottom: 20px;
}

.setting-item {
    margin: 10px 0;
}

.room-actions {
    margin-top: 20px;
}

button {
    padding: 10px 20px;
    border-radius: 4px;
    border: none;
    cursor: pointer;
    font-size: 16px;
}

.start-button {
    background-color: #4caf50;
    color: white;
}

.start-button:disabled {
    background-color: #cccccc;
    cursor: not-allowed;
}

.leave-button {
    background-color: #f44336;
    color: white;
}

.room-chat {
    margin-top: 20px;
    border-top: 1px solid #eee;
    padding-top: 20px;
}
</style>