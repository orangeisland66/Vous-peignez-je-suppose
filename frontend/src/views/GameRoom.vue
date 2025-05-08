<template>
  <div class="game-outer">
    <div class="game-container">
      <!-- Header: Round & Painter & Timer -->
      <header class="game-header">
        <div class="info-row">
          <span>当前回合：第 {{ currentRound }} 轮</span>
          <span class="painter">Painter: {{ currentPainter }}</span>
        </div>
        <div class="timer">倒计时：{{ formatTime(timer) }}</div>
      </header>

      <!-- Divider -->
      <div class="divider"></div>

      <!-- Canvas Section -->
      <section class="canvas-section">
        <canvas
          ref="canvas"
          class="drawing-canvas"
          @mousedown="onCanvasDown"
          @mousemove="onCanvasMove"
          @mouseup="onCanvasUp"
          :class="{ 'readonly': !isPainter }"
        ></canvas>

        <!-- Painter Tools -->
        <div v-if="isPainter" class="tool-bar">
          <button @click="selectTool('pen')">画笔</button>
          <button @click="selectTool('eraser')">橡皮</button>
          <button @click="clearCanvas">清除</button>
        </div>

        <!-- Word Hint for Painter -->
        <div v-if="isPainter" class="word-hint">
          当前词语：<strong>{{ targetWord }}</strong>
        </div>
      </section>

      <!-- Guess Section -->
      <section v-if="!isPainter" class="guess-section">
        <h3>猜词区域：</h3>
        <div class="guess-input-row">
          <span>&gt;</span>
          <input
            v-model.trim="guessInput"
            @keyup.enter="sendGuess"
            placeholder="输入你的猜词"
          />
          <button @click="sendGuess">发送</button>
        </div>
      </section>

      <!-- Divider -->
      <div class="divider"></div>

      <!-- Guess List -->
      <section class="guess-list-section">
        <ul>
          <li v-for="item in guessList" :key="item.username">
            {{ item.username }}：{{ item.word }}
            <span v-if="item.correct === true" class="correct">✔</span>
            <span v-else-if="item.correct === false" class="wrong">✖</span>
          </li>
        </ul>
      </section>
    </div>
  </div>
</template>

<script>
export default {
  name: 'GameRoom',
  data() {
    return {
      currentRound: 2,
      currentPainter: 'Bob',
      timer: 45,
      targetWord: 'apple',
      isPainter: false, // 根据实际逻辑设置
      guessInput: '',
      guessList: [
        { username: 'Alice', word: 'apple', correct: false },
        { username: 'Charlie', word: 'banana', correct: false }
      ],
      // Canvas Drawing State
      drawing: false,
      ctx: null,
      tool: 'pen'
    }
  },
  mounted() {
    const canvas = this.$refs.canvas
    this.ctx = canvas.getContext('2d')
    canvas.width = canvas.clientWidth
    canvas.height = canvas.clientHeight
    // TODO: 初始化从后端获取 isPainter、currentPainter、targetWord 等
  },
  methods: {
    formatTime(sec) {
      const m = String(Math.floor(sec / 60)).padStart(2, '0')
      const s = String(sec % 60).padStart(2, '0')
      return `${m}:${s}`
    },
    selectTool(tool) {
      this.tool = tool
      if (tool === 'eraser') this.ctx.globalCompositeOperation = 'destination-out'
      else this.ctx.globalCompositeOperation = 'source-over'
    },
    clearCanvas() {
      const canvas = this.$refs.canvas
      this.ctx.clearRect(0, 0, canvas.width, canvas.height)
    },
    onCanvasDown(e) {
      if (!this.isPainter) return
      this.drawing = true
      this.ctx.beginPath()
      this.ctx.lineWidth = this.tool === 'pen' ? 2 : 10
      this.ctx.moveTo(e.offsetX, e.offsetY)
    },
    onCanvasMove(e) {
      if (this.drawing) {
        this.ctx.lineTo(e.offsetX, e.offsetY)
        this.ctx.stroke()
      }
    },
    onCanvasUp() {
      if (this.drawing) this.drawing = false
    },
    sendGuess() {
      if (!this.guessInput) return
      // TODO: 发送猜词到后端
      this.guessList.push({ username: '你', word: this.guessInput, correct: null })
      this.guessInput = ''
    }
  }
}
</script>

<style scoped>
.game-outer {
  width: 100vw;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f5f5;
}
.game-container {
  width: 85vw;
  max-width: 1200px;
  aspect-ratio: 16/9;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  padding: 24px;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
}

/* Header */
.game-header .info-row {
  display: flex;
  justify-content: center;
  gap: 48px;
  font-size: 1.3rem;
  color: #333;
}
.game-header .timer {
  text-align: center;
  margin-top: 8px;
  font-size: 1.2rem;
  color: #e60000;
}

.divider {
  height: 2px;
  background: #eee;
  margin: 16px 0;
}

/* Canvas Section */
.canvas-section {
  flex: 1;
  position: relative;
  display: flex;
  justify-content: center;
  align-items: center;
}
.drawing-canvas {
  width: 80%;
  height: 100%;
  border: 2px solid #ccc;
  border-radius: 6px;
  background: #fafafa;
}
.drawing-canvas.readonly {
  cursor: not-allowed;
}
.tool-bar {
  position: absolute;
  top: 8px;
  right: 8px;
  display: flex;
  gap: 8px;
}
.tool-bar button {
  padding: 6px 12px;
  border: none;
  background: #e60000;
  color: #fff;
  border-radius: 4px;
  cursor: pointer;
}
.word-hint {
  position: absolute;
  bottom: 8px;
  left: 8px;
  font-size: 1.1rem;
  color: #333;
}

/* Guess Section */
.guess-section {
  margin-top: 16px;
  text-align: center;
}
.guess-input-row {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  margin-top: 8px;
}
.guess-input-row input {
  padding: 6px 12px;
  font-size: 1rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}
.guess-input-row button {
  padding: 6px 12px;
  font-size: 1rem;
  border: none;
  background: #e60000;
  color: #fff;
  border-radius: 4px;
  cursor: pointer;
}

/* Guess List */
.guess-list-section {
  margin-top: 16px;
  font-size: 1rem;
  color: #444;
}
.guess-list-section ul {
  list-style: none;
  padding: 0;
}
.guess-list-section li {
  margin: 4px 0;
}
.correct {
  color: #28a745;
  margin-left: 4px;
}
.wrong {
  color: #e60000;
  margin-left: 4px;
}

@media (max-width: 900px) {
  .game-container { padding: 16px; }
  .game-header .info-row { font-size: 1rem; gap: 24px; }
  .game-header .timer { font-size: 1rem; }
  .drawing-canvas { width: 98%; }
  .guess-input-row input { width: 120px; }
}
</style>
