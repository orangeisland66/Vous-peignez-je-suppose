<template>
    <div class="game-room-create">
        <h2>Create New Game Room</h2>
        <form @submit.prevent="createRoom" class="create-room-form">
            <div class="form-group">
                <label for="roomName">Room Name:</label>
                <input 
                    type="text" 
                    id="roomName" 
                    v-model="roomData.name" 
                    required
                    placeholder="Enter room name"
                >
            </div>

            <div class="form-group">
                <label for="maxPlayers">Max Players:</label>
                <select id="maxPlayers" v-model="roomData.maxPlayers">
                    <option value="2">2 Players</option>
                    <option value="4">4 Players</option>
                    <option value="6">6 Players</option>
                    <option value="8">8 Players</option>
                </select>
            </div>

            <div class="form-group">
                <label for="roundTime">Round Time (seconds):</label>
                <select id="roundTime" v-model="roomData.roundTime">
                    <option value="60">60 seconds</option>
                    <option value="90">90 seconds</option>
                    <option value="120">120 seconds</option>
                </select>
            </div>

            <div class="form-group">
                <label for="rounds">Number of Rounds:</label>
                <select id="rounds" v-model="roomData.rounds">
                    <option value="3">3 Rounds</option>
                    <option value="5">5 Rounds</option>
                    <option value="7">7 Rounds</option>
                </select>
            </div>

            <div class="form-group">
                <label>
                    <input 
                        type="checkbox" 
                        v-model="roomData.isPrivate"
                    > Private Room
                </label>
            </div>

            <div class="form-actions">
                <button type="submit" class="btn-create">Create Room</button>
                <button type="button" class="btn-cancel" @click="$router.back()">Cancel</button>
            </div>
        </form>
    </div>
</template>

<script>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useStore } from 'vuex'

export default {
    name: 'GameRoomCreate',
    setup() {
        const router = useRouter()
        const store = useStore()

        const roomData = ref({
            name: '',
            maxPlayers: 4,
            roundTime: 90,
            rounds: 5,
            isPrivate: false
        })

        const createRoom = async () => {
            try {
                const response = await store.dispatch('gameRoom/createRoom', roomData.value)
                router.push(`/game-room/${response.id}`)
            } catch (error) {
                console.error('Failed to create room:', error)
                // Handle error (you might want to add error handling UI)
            }
        }

        return {
            roomData,
            createRoom
        }
    }
}
</script>

<style scoped>
.game-room-create {
    max-width: 500px;
    margin: 0 auto;
    padding: 20px;
}

.create-room-form {
    background: #fff;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.form-group {
    margin-bottom: 15px;
}

label {
    display: block;
    margin-bottom: 5px;
    font-weight: 600;
}

input[type="text"],
select {
    width: 100%;
    padding: 8px;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 14px;
}

.form-actions {
    display: flex;
    gap: 10px;
    margin-top: 20px;
}

button {
    padding: 10px 20px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-weight: 600;
}

.btn-create {
    background-color: #4CAF50;
    color: white;
}

.btn-cancel {
    background-color: #f44336;
    color: white;
}

button:hover {
    opacity: 0.9;
}
</style>