<template>
    <div class="game-container">
        <!-- Game status and info section -->
        <div class="game-info">
            <h2>Round {{ currentRound }}/{{ totalRounds }}</h2>
            <div class="timer" v-if="isGameStarted">
                Time: {{ remainingTime }}s
            </div>
            <div class="current-word" v-if="isDrawing">
                Word to draw: {{ currentWord }}
            </div>
        </div>

        <!-- Main game area -->
        <div class="game-area">
            <!-- Drawing board component -->
            <DrawingBoard
                v-if="isGameStarted"
                :isDrawing="isDrawing"
                :strokes="strokes"
                @stroke-added="handleStrokeAdded"
            />

            <!-- Word guessing component -->
            <GuessWord
                v-if="isGameStarted && !isDrawing"
                :disabled="!canGuess"
                @submit-guess="handleGuess"
            />

            <!-- Player list and scores -->
            <div class="players-list">
                <h3>Players</h3>
                <div v-for="player in players" :key="player.id" class="player-item">
                    <span>{{ player.username }}</span>
                    <span>Score: {{ player.score }}</span>
                    <span v-if="player.id === currentDrawer">Drawing</span>
                </div>
            </div>
        </div>

        <!-- Game result component -->
        <GameResult
            v-if="showResults"
            :results="gameResults"
            @play-again="restartGame"
        />
    </div>
</template>

<script>
import { ref, onMounted, onUnmounted } from 'vue'
import DrawingBoard from './DrawingBoard.vue'
import GuessWord from './GuessWord.vue'
import GameResult from './GameResult.vue'
import { useStore } from 'vuex'
import socketService from '@/services/socketService'

export default {
    name: 'Game',
    components: {
        DrawingBoard,
        GuessWord,
        GameResult
    },

    setup() {
        const store = useStore()
        const currentRound = ref(1)
        const totalRounds = ref(3)
        const remainingTime = ref(60)
        const currentWord = ref('')
        const isGameStarted = ref(false)
        const isDrawing = ref(false)
        const canGuess = ref(true)
        const showResults = ref(false)
        const players = ref([])
        const currentDrawer = ref(null)
        const strokes = ref([])
        const gameResults = ref([])
        let timer = null

        // Game initialization
        const initGame = async () => {
            await socketService.connect()
            setupSocketListeners()
            isGameStarted.value = true
        }

        // Socket event handlers
        const setupSocketListeners = () => {
            socketService.on('gameStart', handleGameStart)
            socketService.on('newRound', handleNewRound)
            socketService.on('strokeReceived', handleStrokeReceived)
            socketService.on('correctGuess', handleCorrectGuess)
            socketService.on('gameEnd', handleGameEnd)
            socketService.on('updatePlayers', handleUpdatePlayers)
        }

        // Game event handlers
        const handleGameStart = (data) => {
            currentWord.value = data.word
            currentDrawer.value = data.drawerId
            isDrawing.value = store.state.user.id === data.drawerId
            startTimer()
        }

        const handleNewRound = (data) => {
            currentRound.value++
            currentWord.value = data.word
            currentDrawer.value = data.drawerId
            isDrawing.value = store.state.user.id === data.drawerId
            strokes.value = []
            startTimer()
        }

        const handleStrokeAdded = (stroke) => {
            if (isDrawing.value) {
                socketService.emit('newStroke', stroke)
            }
        }

        const handleStrokeReceived = (stroke) => {
            if (!isDrawing.value) {
                strokes.value.push(stroke)
            }
        }

        const handleGuess = (guess) => {
            socketService.emit('submitGuess', { guess })
        }

        const handleCorrectGuess = (data) => {
            // Handle correct guess logic
        }

        const handleGameEnd = (results) => {
            gameResults.value = results
            showResults.value = true
            isGameStarted.value = false
            clearTimer()
        }

        const handleUpdatePlayers = (updatedPlayers) => {
            players.value = updatedPlayers
        }

        // Timer functions
        const startTimer = () => {
            remainingTime.value = 60
            clearTimer()
            timer = setInterval(() => {
                remainingTime.value--
                if (remainingTime.value <= 0) {
                    clearTimer()
                    socketService.emit('timeUp')
                }
            }, 1000)
        }

        const clearTimer = () => {
            if (timer) {
                clearInterval(timer)
                timer = null
            }
        }

        const restartGame = () => {
            showResults.value = false
            currentRound.value = 1
            socketService.emit('restartGame')
        }

        // Lifecycle hooks
        onMounted(() => {
            initGame()
        })

        onUnmounted(() => {
            clearTimer()
            socketService.disconnect()
        })

        return {
            currentRound,
            totalRounds,
            remainingTime,
            currentWord,
            isGameStarted,
            isDrawing,
            canGuess,
            showResults,
            players,
            currentDrawer,
            strokes,
            gameResults,
            handleStrokeAdded,
            handleGuess,
            restartGame
        }
    }
}
</script>

<style scoped>
.game-container {
    padding: 20px;
    max-width: 1200px;
    margin: 0 auto;
}

.game-info {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
}

.game-area {
    display: grid;
    grid-template-columns: 3fr 1fr;
    gap: 20px;
}

.players-list {
    background-color: #f5f5f5;
    padding: 15px;
    border-radius: 8px;
}

.player-item {
    display: flex;
    justify-content: space-between;
    padding: 8px 0;
    border-bottom: 1px solid #ddd;
}

.timer {
    font-size: 1.2em;
    font-weight: bold;
}

.current-word {
    font-size: 1.2em;
    color: #2c3e50;
}
</style>