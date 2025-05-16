<template>
  <div class="game-background">
    <div class="game-container">
      <!-- Header: Round & Painter & Timer -->
      <header class="game-header">
        <div class="header-info">
          <div class="round-badge">
            <span class="round-label">当前回合</span>
            <span class="round-number">第 {{ currentRound }} 轮</span>
          </div>
          <div class="timer-badge">
            <div class="timer-icon">⏱</div>
            <span class="timer-text">{{ formatTime(timer) }}</span>
          </div>
        </div>
        <div class="painter-info">
          <div class="painter-avatar">{{ currentPainter.charAt(0) }}</div>
          <span class="painter-name">{{ currentPainter }}</span>
        </div>
      </header>

      <!-- Main Game Area -->
      <div class="main-content">
        <!-- Left Panel - Canvas -->
        <section class="canvas-panel">
          <div class="canvas-container">
            <canvas
              ref="canvas"
              class="drawing-canvas"
              @mousedown="onCanvasDown"
              @mousemove="onCanvasMove"
              @mouseup="onCanvasUp"
              :class="{ 'readonly': !isPainter }"
            ></canvas>
          </div>
          
          <!-- Painter Tools -->
          <div v-if="isPainter" class="tool-bar">
            <button @click="selectTool('pen')" class="tool-btn" :class="{ 'active': tool === 'pen' }">
              <div class="tool-icon">✏️</div>
              <span class="tool-text">画笔</span>
            </button>
            <button @click="selectTool('eraser')" class="tool-btn" :class="{ 'active': tool === 'eraser' }">
              <div class="tool-icon">🧽</div>
              <span class="tool-text">橡皮</span>
            </button>
            <button @click="clearCanvas" class="tool-btn clear">
              <div class="tool-icon">🗑️</div>
              <span class="tool-text">清除</span>
            </button>
          </div>
          
          <!-- Word Hint for Painter -->
          <div v-if="isPainter" class="word-hint">
            <div class="hint-label">当前词语</div>
            <div class="target-word">{{ targetWord }}</div>
          </div>
        </section>

        <!-- Right Panel - Guesses -->
        <section class="guess-panel">
          <div class="panel-header">
            <h2>猜词记录</h2>
            <span class="guess-count">{{ guessList.length }} 条</span>
          </div>
          
          <!-- Guess Input -->
          <div v-if="!isPainter" class="guess-input-container">
            <div class="input-wrapper">
              <input
                v-model.trim="guessInput"
                @keyup.enter="sendGuess"
                placeholder="输入你的猜词..."
                class="guess-input"
              />
              <button @click="sendGuess" class="send-btn">
                <span class="send-icon">↗️</span>
              </button>
            </div>
          </div>
          
          <!-- Guess List -->
          <div class="guess-list-container">
            <ul class="guess-list">
              <li v-for="item in guessList" :key="item.username" class="guess-item">
                <div class="guesser-avatar">{{ item.username.charAt(0) }}</div>
                <div class="guess-content">
                  <div class="guesser-name">{{ item.username }}</div>
                  <div class="guess-word">{{ item.word }}</div>
                </div>
                <div class="guess-status">
                  <span v-if="item.correct === true" class="status-icon correct">✓</span>
                  <span v-else-if="item.correct === false" class="status-icon wrong">✗</span>
                </div>
              </li>
            </ul>
            
            <div v-if="guessList.length === 0" class="no-guesses">
              <div class="empty-icon">💭</div>
              <p>还没有人猜词</p>
            </div>
          </div>
        </section>
      </div>
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
:root {
  --primary: #4F46E5;
  --primary-dark: #4338CA;
  --primary-light: #818CF8;
  --primary-lightest: #EEF2FF;
  --secondary: #10B981;
  --secondary-dark: #059669;
  --accent: #F472B6;
  --dark: #1F2937;
  --light: #F9FAFB;
  --gray: #6B7280;
  --gray-light: #E5E7EB;
  --success: #22C55E;
  --warning: #F59E0B;
  --danger: #EF4444;
}

/* Base and Layout Styles */
.game-background {
  background: linear-gradient(135deg, #F9FAFB 0%, #EEF2FF 100%);
  min-height: 100vh;
  width: 100vw;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  color: var(--dark);
}

.game-container {
  width: 90%;
  max-width: 1200px;
  background: white;
  border-radius: 24px;
  box-shadow: 0 10px 30px rgba(79, 70, 229, 0.1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

/* Header Styles */
.game-header {
  padding: 20px 32px;
  background: white;
  border-bottom: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-info {
  display: flex;
  align-items: center;
  gap: 20px;
}

.round-badge {
  display: flex;
  flex-direction: column;
  align-items: center;
  background: var(--primary-lightest);
  padding: 8px 16px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(79, 70, 229, 0.1);
}

.round-label {
  font-size: 12px;
  color: var(--primary);
  margin-bottom: 2px;
}

.round-number {
  font-weight: 600;
  color: var(--primary-dark);
  font-size: 16px;
}

.timer-badge {
  display: flex;
  align-items: center;
  background: #FEF2F2;
  padding: 8px 16px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(239, 68, 68, 0.1);
  gap: 8px;
}

.timer-icon {
  font-size: 16px;
  color: var(--danger);
}

.timer-text {
  font-weight: 600;
  color: var(--danger);
  font-size: 16px;
}

.painter-info {
  display: flex;
  align-items: center;
  gap: 10px;
  background: var(--primary-lightest);
  padding: 8px 16px;
  border-radius: 50px;
  box-shadow: 0 2px 10px rgba(79, 70, 229, 0.1);
}

.painter-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 16px;
}

.painter-name {
  font-weight: 500;
  color: var(--primary-dark);
}

/* Main Content Layout */
.main-content {
  display: flex;
  height: 70vh;
  min-height: 500px;
  padding: 24px;
  gap: 24px;
}

/* Canvas Panel Styles */
.canvas-panel {
  flex: 2;
  position: relative;
  display: flex;
  flex-direction: column;
}

.canvas-container {
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  margin-bottom: 16px;
}

.drawing-canvas {
  width: 90%;
  height: 90%;
  border: 2px solid var(--gray-light);
  border-radius: 12px;
  background: white;
}

.drawing-canvas.readonly {
  cursor: not-allowed;
}

.tool-bar {
  display: flex;
  justify-content: center;
  gap: 16px;
  margin-bottom: 16px;
}

.tool-btn {
  background: white;
  border: 1px solid var(--gray-light);
  border-radius: 12px;
  padding: 12px 16px;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
}

.tool-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
}

.tool-btn.active {
  background: var(--primary-lightest);
  border-color: var(--primary-light);
  color: var(--primary);
}

.tool-btn.clear {
  background: #FEF2F2;
  border-color: #FECACA;
  color: var(--danger);
}

.tool-icon {
  font-size: 18px;
}

.word-hint {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: white;
  padding: 12px 16px;
  border-radius: 12px;
  text-align: center;
  display: flex;
  flex-direction: column;
}

.hint-label {
  font-size: 12px;
  opacity: 0.8;
  margin-bottom: 4px;
}

.target-word {
  font-size: 24px;
  font-weight: 600;
}

/* Guess Panel Styles */
.guess-panel {
  flex: 1;
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.panel-header {
  padding: 20px 24px;
  border-bottom: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.panel-header h2 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--dark);
  margin: 0;
}

.guess-count {
  background: var(--primary-lightest);
  color: var(--primary);
  font-size: 14px;
  font-weight: 500;
  padding: 4px 12px;
  border-radius: 50px;
}

.guess-input-container {
  padding: 16px 24px;
  border-bottom: 1px solid var(--gray-light);
}

.input-wrapper {
  display: flex;
  background: white;
  border: 2px solid var(--gray-light);
  border-radius: 10px;
  overflow: hidden;
  transition: all 0.2s ease;
}

.input-wrapper:focus-within {
  border-color: var(--primary-light);
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

.guess-input {
  flex: 1;
  border: none;
  padding: 12px 16px;
  font-size: 16px;
  outline: none;
}

.send-btn {
  background: var(--primary);
  color: white;
  border: none;
  padding: 0 16px;
  cursor: pointer;
  transition: background 0.2s ease;
}

.send-btn:hover {
  background: var(--primary-dark);
}

.send-icon {
  font-size: 18px;
}

.guess-list-container {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
}

.guess-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.guess-item {
  display: flex;
  align-items: center;
  padding: 12px;
  border-radius: 10px;
  margin-bottom: 8px;
  border: 1px solid var(--gray-light);
  transition: all 0.2s ease;
}

.guess-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.05);
  border-color: var(--primary-light);
}

.guesser-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--primary-lightest);
  color: var(--primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 16px;
  margin-right: 12px;
}

.guess-content {
  flex: 1;
}

.guesser-name {
  font-size: 14px;
  color: var(--gray);
  margin-bottom: 2px;
}

.guess-word {
  font-size: 16px;
  font-weight: 500;
  color: var(--dark);
}

.guess-status {
  margin-left: 8px;
}

.status-icon {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
}

.status-icon.correct {
  background: #DCFCE7;
  color: var(--success);
}

.status-icon.wrong {
  background: #FEF2F2;
  color: var(--danger);
}

.no-guesses {
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: var(--gray);
  text-align: center;
  padding: 20px;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.5;
}

.no-guesses p {
  font-size: 16px;
}

/* Responsive Adjustments */
@media (max-width: 992px) {
  .main-content {
    flex-direction: column;
    height: auto;
  }
  
  .canvas-panel {
    margin-bottom: 24px;
  }
  
  .game-header {
    flex-direction: column;
    gap: 12px;
  }
  
  .header-info {
    width: 100%;
    justify-content: space-between;
  }
}

@media (max-width: 768px) {
  .game-container {
    width: 95%;
    padding: 16px;
  }
  
  .tool-bar {
    flex-direction: column;
    position: absolute;
    right: 16px;
    top: 16px;
    z-index: 10;
  }
  
  .tool-btn {
    padding: 8px;
  }
  
  .tool-text {
    display: none;
  }
}
</style>