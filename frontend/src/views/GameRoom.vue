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
            <!-- 使用DrawingBoard组件替换原始Canvas -->
            <drawing-board
              ref="drawingBoard"
              :readonly="!isPainter"
              @stroke-completed="onStrokeCompleted"
              @canvas-cleared="onCanvasCleared"
            />
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
// 导入DrawingBoard组件
import DrawingBoard from '@/components/game/DrawingBoard.vue';

export default {
  name: 'GameRoom',
  components: {
    DrawingBoard // 注册DrawingBoard组件
  },
  data() {
    return {
      currentRound: 2,
      currentPainter: 'Bob',
      timer: 45,
      targetWord: 'apple',
      isPainter: true, // 根据实际逻辑设置
      guessInput: '',
      guessList: [
        { username: 'Alice', word: 'apple', correct: false },
        { username: 'Charlie', word: 'banana', correct: false }
      ]
    }
  },
  mounted() {
    // TODO: 初始化从后端获取 isPainter、currentPainter、targetWord 等
  },
  methods: {
    formatTime(sec) {
      const m = String(Math.floor(sec / 60)).padStart(2, '0')
      const s = String(sec % 60).padStart(2, '0')
      return `${m}:${s}`
    },
    sendGuess() {
      if (!this.guessInput) return
      // TODO: 发送猜词到后端
      this.guessList.push({ username: '你', word: this.guessInput, correct: null })
      this.guessInput = ''
    },
    // 处理来自DrawingBoard的事件
    onStrokeCompleted(stroke) {
      // TODO: 如果需要，可以将笔画数据发送到后端，以便同步给其他玩家
      console.log('笔画完成:', stroke);
    },
    onCanvasCleared() {
      // TODO: 通知后端画布已清空
      console.log('画布已清空');
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
}
</style>