<template>
    <div class="game-result">
        <div class="result-container">
            <h2 class="result-title">Game Over!</h2>

            <!-- Correct Word Section -->
            <div class="word-section">
                <h3>The word was:</h3>
                <p class="correct-word">{{ word }}</p>
            </div>

            <!-- Winner Section -->
            <div class="winner-section" v-if="winner">
                <h3>Winner:</h3>
                <p class="winner-name">{{ winner.username }}</p>
                <div class="winner-points">+{{ pointsEarned }} points</div>
            </div>

            <!-- Player Scores -->
            <div class="scores-section">
                <h3>Final Scores:</h3>
                <div class="player-scores">
                    <div v-for="player in players" :key="player.id" class="player-score">
                        <span class="player-name">{{ player.username }}</span>
                        <span class="score">{{ player.score }}</span>
                    </div>
                </div>
            </div>

            <!-- Action Buttons -->
            <div class="action-buttons">
                <button @click="playAgain" class="btn play-again">Play Again</button>
                <button @click="backToLobby" class="btn back-lobby">Back to Lobby</button>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    name: 'GameResult',
    props: {
        word: {
            type: String,
            required: true
        },
        winner: {
            type: Object,
            default: null
        },
        players: {
            type: Array,
            required: true
        },
        pointsEarned: {
            type: Number,
            default: 0
        }
    },
    methods: {
        playAgain() {
            this.$emit('play-again');
        },
        backToLobby() {
            this.$emit('back-to-lobby');
        }
    }
}
</script>

<style scoped>
.game-result {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 400px;
    padding: 20px;
}

.result-container {
    background-color: white;
    border-radius: 8px;
    padding: 24px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
    max-width: 500px;
    width: 100%;
    text-align: center;
}

.result-title {
    color: #2c3e50;
    margin-bottom: 24px;
    font-size: 28px;
}

.word-section,
.winner-section,
.scores-section {
    margin-bottom: 24px;
}

.correct-word {
    font-size: 24px;
    color: #42b983;
    font-weight: bold;
    margin: 8px 0;
}

.winner-name {
    font-size: 20px;
    color: #2c3e50;
    margin: 8px 0;
}

.winner-points {
    color: #42b983;
    font-weight: bold;
}

.player-scores {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-top: 12px;
}

.player-score {
    display: flex;
    justify-content: space-between;
    padding: 8px 16px;
    background-color: #f8f9fa;
    border-radius: 4px;
}

.player-name {
    color: #2c3e50;
}

.score {
    font-weight: bold;
    color: #42b983;
}

.action-buttons {
    display: flex;
    gap: 16px;
    justify-content: center;
    margin-top: 24px;
}

.btn {
    padding: 10px 20px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 16px;
    transition: opacity 0.3s;
}

.btn:hover {
    opacity: 0.8;
}

.play-again {
    background-color: #42b983;
    color: white;
}

.back-lobby {
    background-color: #6c757d;
    color: white;
}
</style>