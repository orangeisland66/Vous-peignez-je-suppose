<template>
    <div class="guess-word-container">
        <!-- Word display for drawer -->
        <div v-if="isDrawer" class="word-display">
            <h3>Your word to draw:</h3>
            <div class="current-word">{{ currentWord }}</div>
        </div>

        <!-- Guess input for guessers -->
        <div v-else class="guess-input-container">
            <div class="guess-hint">
                <span>Word length: {{ wordLength }}</span>
                <span class="hint" v-if="hint">Hint: {{ hint }}</span>
            </div>
            
            <div class="input-wrapper">
                <input
                    type="text"
                    v-model="guessInput"
                    :disabled="!canGuess"
                    @keyup.enter="submitGuess"
                    placeholder="Type your guess here..."
                    class="guess-input"
                />
                <button 
                    @click="submitGuess" 
                    :disabled="!canGuess || !guessInput.trim()"
                    class="submit-button"
                >
                    Guess
                </button>
            </div>

            <!-- Previous guesses display -->
            <div class="previous-guesses">
                <h4>Previous Guesses:</h4>
                <ul>
                    <li v-for="(guess, index) in previousGuesses" :key="index">
                        {{ guess.player }}: {{ guess.word }}
                    </li>
                </ul>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    name: 'GuessWord',
    
    props: {
        isDrawer: {
            type: Boolean,
            default: false
        },
        currentWord: {
            type: String,
            default: ''
        },
        canGuess: {
            type: Boolean,
            default: true
        },
        hint: {
            type: String,
            default: ''
        }
    },

    data() {
        return {
            guessInput: '',
            previousGuesses: []
        }
    },

    computed: {
        wordLength() {
            return this.currentWord.length
        }
    },

    methods: {
        submitGuess() {
            if (!this.canGuess || !this.guessInput.trim()) return

            const guess = {
                word: this.guessInput.trim().toLowerCase(),
                player: this.$store.state.user.username,
                timestamp: new Date()
            }

            // Emit the guess to parent component
            this.$emit('guess-submitted', guess)

            // Add to previous guesses
            this.previousGuesses.unshift(guess)

            // Clear input
            this.guessInput = ''
        },

        // Method to be called when a new round starts
        resetGuesses() {
            this.previousGuesses = []
            this.guessInput = ''
        }
    }
}
</script>

<style scoped>
.guess-word-container {
    padding: 1rem;
    border-radius: 8px;
    background-color: #f5f5f5;
    max-width: 600px;
    margin: 0 auto;
}

.word-display {
    text-align: center;
    padding: 1rem;
}

.current-word {
    font-size: 1.5rem;
    font-weight: bold;
    color: #2c3e50;
    margin: 0.5rem 0;
}

.guess-input-container {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.guess-hint {
    display: flex;
    justify-content: space-between;
    color: #666;
    font-size: 0.9rem;
}

.input-wrapper {
    display: flex;
    gap: 0.5rem;
}

.guess-input {
    flex: 1;
    padding: 0.5rem;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 1rem;
}

.submit-button {
    padding: 0.5rem 1rem;
    background-color: #42b983;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}

.submit-button:disabled {
    background-color: #a8a8a8;
    cursor: not-allowed;
}

.previous-guesses {
    margin-top: 1rem;
}

.previous-guesses ul {
    list-style: none;
    padding: 0;
    margin: 0;
    max-height: 200px;
    overflow-y: auto;
}

.previous-guesses li {
    padding: 0.3rem 0;
    border-bottom: 1px solid #eee;
    color: #666;
}

.hint {
    color: #42b983;
    font-style: italic;
}
</style>