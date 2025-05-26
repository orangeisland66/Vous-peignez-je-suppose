<template>
  <div class="game-background">
    <div class="game-container">
      <!-- Header: Round & Painter & Timer -->
      <div class="header-info">
        <div class="round-badge">
          <span class="round-label">当前回合</span>
          <span class="round-number">第 {{ currentRound }} 轮</span>
        </div>

        <div class="timer-badge">
          <div class="timer-icon">⏱</div>
          <span class="timer-text">{{ formatTime(timer) }}</span>
        </div>

        <!-- 玩家状态显示 -->
        <div class="players-status">
          <div class="player-item" v-for="player in players" :key="player.id"
            :class="{ active: player.id === currentPainterId }">
            <div class="player-avatar">
              <span>{{ player.username?.charAt(0).toUpperCase() || 'P' }}</span>
            </div>
            <div class="player-info">
              <span class="player-name">{{ player.username }}</span>
              <span class="player-role" v-if="player.id === currentPainterId">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
                  <!-- ...svg path... -->
                  <path
                    d="M12 2C13.1 2 14 2.9 14 4V5.18C17.06 5.6 19.4 8.27 19.4 11.4C19.4 13.5 18.1 15.36 16.17 16.19L15.5 16.47V18C15.5 19.1 14.6 20 13.5 20H10.5C9.4 20 8.5 19.1 8.5 18V16.47L7.83 16.19C5.9 15.36 4.6 13.5 4.6 11.4C4.6 8.27 6.94 5.6 10 5.18V4C10 2.9 10.9 2 12 2ZM12 7C9.24 7 7 9.24 7 12C7 13.66 8.34 15 10 15H14C15.66 15 17 13.66 17 12C17 9.24 14.76 7 12 7Z" />
                </svg>
                画家
              </span>
            </div>
          </div>
        </div>

        <!-- 当前词汇显示 -->
        <div class="current-word-badge" v-if="targetWord && !showWordSelection">
          <div class="word-icon">📝</div>
          <div class="word-content">
            <span class="word-label">{{ isPainter ? '绘画词汇' : '词汇提示' }}</span>
            <span class="word-text">{{ isPainter ? targetWord : getWordHint(targetWord) }}</span>
          </div>
        </div>
      </div>

      <!-- Main Game Area -->
      <div class="main-content">
        <!-- Left Panel - Canvas -->
        <section class="canvas-panel">
          <div class="canvas-container">
            <drawing-board ref="DrawingBoard" :readonly="!isPainter" :tool="currentTool" :color="currentColor"
              :size="currentSize" @stroke-completed="onStrokeCompleted" @canvas-cleared="onCanvasCleared" />
          </div>
        </section>

        <!-- Right Panel - Chat -->
        <section class="guess-panel">
          <div class="panel-header">
            <h2>游戏聊天</h2>
          </div>

          <Chat :isPainter="isPainter" :targetWord="targetWord" @guess-correct="handleCorrectGuess" />
        </section>
      </div>
    </div>

    <!-- 词汇选择弹窗 -->
    <div class="word-selection-overlay" v-if="isPainter && showWordSelection">
      <div class="word-selection-modal">
        <div class="modal-header">
          <h3>选择你要画的词汇</h3>
          <p class="selection-tip">弹窗，作画的显示四个词汇四选一，猜词者显示"作画者正在选词"</p>
        </div>

        <div class="word-options">
          <div class="word-option" v-for="(word, index) in wordOptions" :key="index" @click="selectWord(word)">
            <div class="word-text">{{ word.text }}</div>
            <div class="word-difficulty">{{ word.difficulty }}星</div>
          </div>
        </div>

        <div class="selection-timer">
          <div class="timer-bar">
            <div class="timer-progress" :style="{ width: selectionProgress + '%' }"></div>
          </div>
          <p class="timer-text">选择时间：{{ selectionTimer }}秒</p>
        </div>
      </div>
    </div>

    <!-- 等待画家选词的提示（给猜词者看） -->
    <div class="waiting-overlay" v-if="!isPainter && showWordSelection">
      <div class="waiting-modal">
        <div class="waiting-content">
          <div class="waiting-icon">🎨</div>
          <h3>作画者正在选词</h3>
          <p>请耐心等待...</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import DrawingBoard from '@/components/game/DrawingBoard.vue';
import Chat from '@/components/gameRoom/Chat.vue';
import signalRService from '@/services/signalRService';
import apiService from '@/services/apiService'; // 确保你已经引入了 apiService

export default {
  name: 'GameRoom',
  components: {
    DrawingBoard,
    Chat
  },
  props: {
    playerId: {
      type: Number,
      required: false
    }
  },
  data() {
    return {
      currentRound: 1,
      currentPainter: '',
      players: [],
      currentPainterId: null,
      timer: 60,
      targetWord: '',
      isPainter: true,
      isGameActive: true,
      showWordSelection: true,
      selectionTimer: 15,
      selectionProgress: 100,
      wordOptions: [
        { text: '苹果', difficulty: 1 },
        { text: '汽车', difficulty: 2 },
        { text: '彩虹', difficulty: 3 },
        { text: '飞机', difficulty: 2 }
      ],
      currentTool: 'pen',
      currentColor: '#000000',
      currentSize: 5,
      colors: [
        '#000000', '#FF0000', '#00FF00', '#0000FF',
        '#FFFF00', '#FF00FF', '#00FFFF', '#FFA500',
        '#800080', '#FFC0CB', '#A52A2A', '#808080'
      ],
      brushSizes: [2, 5, 10, 15, 20]
    };
  },
  computed: {
    getWordHint() {
      return (word) => {
        if (!word) return '';
        return word.split('').map((char, index) => {
          if (index === 0 || index === word.length - 1) {
            return char;
          }
          return '_';
        }).join(' ');
      };
    },
    currentPainterObj() {
      return this.players.find(p => p.id === this.currentPainterId) || {};
    }
  },
  mounted() {
    this.startTimer();
    //this.initializeGame();
    this.setupSignalR();
  },
  beforeDestroy() {
    console.log('[GameRoom] 组件销毁，准备断开SignalR连接');
    this.disconnectSignalR();
  },
  beforeRouteLeave(to, from, next) {
    console.log('[GameRoom] 路由离开，准备断开SignalR连接');
    this.disconnectSignalR();
    next();
  },
  methods: {
    initializeGame() {
      const localPlayerId = parseInt(localStorage.getItem('userId')) || this.playerId;
      this.isPainter = this.currentPainterId === localPlayerId;

      if (this.isPainter) {
        this.showWordSelectionModal();
      } else {
        this.startTimer();
      }
    },
    setupSignalR() {
      const roomId = this.$route.params.roomId;
      const playerId = parseInt(localStorage.getItem('userId')) || this.playerId;

      if (!roomId || !playerId) {
        console.error('[SignalR] 房间ID或玩家ID缺失');
        this.$router.push('/lobby');
        return;
      }

      signalRService.initialize(roomId).then(() => {
        signalRService.joinGroup(roomId, playerId)
          .then(() => {
            console.log(`[SignalR] 加入房间: ${roomId}, 玩家ID: ${playerId}`);
            this.fetchRoomPlayers();
          })
          .catch(err => {
            console.error(`[SignalR] 加入房间失败: ${err.message}`);
            this.$router.push('/lobby');
          });
      });
    },
    async fetchRoomPlayers() {
      try {
        const roomId = this.$route.params.roomId;
        const res = await apiService.getRoomDetails(roomId);
        if (res && res.room && res.room.players) {
          this.players = res.room.players.map(p => p.user);
          this.currentPainterId = res.room.currentPainterId || this.players[0]?.id || null;
          // 关键：获取到玩家和画家后再初始化游戏
          this.initializeGame();
        }
      } catch (e) {
        console.error('获取玩家列表失败', e);
      }
    },
    disconnectSignalR() {
      if (signalRService.hubConnection && signalRService.isConnected.value) {
        signalRService.hubConnection.stop()
          .then(() => {
            signalRService.isConnected.value = false;
            console.log('[GameRoom] SignalR连接已断开');
          })
          .catch(err => {
            console.error('[GameRoom] SignalR断开失败:', err);
          });
      }
    },
    showWordSelectionModal() {
      this.showWordSelection = true;
      this.selectionTimer = 15;
      this.selectionProgress = 100;
      this.startSelectionTimer();
    },
    startSelectionTimer() {
      const interval = setInterval(() => {
        this.selectionTimer--;
        this.selectionProgress = (this.selectionTimer / 15) * 100;

        if (this.selectionTimer <= 0) {
          clearInterval(interval);
          this.selectWord(this.wordOptions[0]);
        }
      }, 1000);
    },
    selectWord(word) {
      this.targetWord = word.text;
      this.showWordSelection = false;
      this.isGameActive = true;
      this.timer = 60;
      this.startTimer();

      console.log('画家选择了词汇:', word.text);
    },
    formatTime(sec) {
      const m = String(Math.floor(sec / 60)).padStart(2, '0');
      const s = String(sec % 60).padStart(2, '0');
      return `${m}:${s}`;
    },
    handleCorrectGuess() {
      this.isGameActive = false;
      this.currentRound++;
      this.resetGame();

      const localPlayerId = parseInt(localStorage.getItem('userId')) || this.playerId;
      const nextPainterIndex = (this.players.findIndex(p => p.id === this.currentPainterId) + 1) % this.players.length;
      this.currentPainterId = this.players[nextPainterIndex].id;
      this.isPainter = this.currentPainterId === localPlayerId;

      setTimeout(() => {
        this.initializeGame();
      }, 2000);
    },
    startTimer() {
      const timerInterval = setInterval(() => {
        if (this.isGameActive && this.timer > 0) {
          this.timer--;
        } else {
          clearInterval(timerInterval);
          if (this.timer === 0 && this.isGameActive) {
            this.isGameActive = false;
            this.handleTimeUp();
          }
        }
      }, 1000);
    },
    handleTimeUp() {
      console.log('时间到！');
      this.handleCorrectGuess();
      const roomId = this.$route.params.roomId;
      this.$router.push({ name: 'RoundResult', params: { roomId } });
      //this.$router.push({ name: 'FinalScore', params: { roomId: this.roomId } })
    },
    resetGame() {
      if (this.$refs.drawingBoard && this.$refs.drawingBoard.clearCanvas) {
        this.$refs.drawingBoard.clearCanvas();
      }
      this.targetWord = '';
      this.timer = 60;
    }
  }
};
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
  padding: 24px;
}

.game-container {
  width: 90%;
  height: calc(100vh - 100px);
  max-width: 1400px;
  background: white;
  border-radius: 24px;
  box-shadow: 0 10px 30px rgba(79, 70, 229, 0.1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

/* Header Layout */
.header-info {
  display: flex;
  align-items: center;
  gap: 20px;
  background: white;
  border-bottom: 1px solid var(--gray-light);
  flex-wrap: wrap;
}

/* 通用徽章统一大小 */
.info-badge {
  min-width: 160px;
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 8px 16px;
  border-radius: 12px;
  flex-shrink: 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

/* 当前回合 */
.round-badge {
  background: var(--primary-lightest);
  box-shadow: 0 2px 8px rgba(79, 70, 229, 0.1);
  flex-direction: column;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 160px;
  height: 64px;
  padding: 8px 16px;
  border-radius: 12px;
  flex-shrink: 0;
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

/* 计时器 */
.timer-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  background: #FEF2F2;
  min-width: 160px;
  height: 64px;
  padding: 8px 16px;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(239, 68, 68, 0.1);
  gap: 8px;
  flex-shrink: 0;
}

.timer-icon {
  font-size: 20px;
  color: var(--danger);
}

.timer-text {
  font-weight: 600;
  color: var(--danger);
  font-size: 20px;
}

/* 玩家状态 */
.players-status {
  display: flex;
  gap: 15px;
}

.player-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 12px;
  background: var(--light);
  transition: all 0.3s ease;
}

.player-item.active {
  background: linear-gradient(135deg, var(--primary-lightest) 0%, #ffffff 100%);
  border: 2px solid var(--primary-light);
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.15);
}

.player-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--primary-dark);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 14px;
}

.player-item.active .player-avatar {
  background: var(--primary-dark);
  color: white;
}

.player-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.player-name {
  font-weight: 500;
  color: var(--dark);
  font-size: 14px;
}

.player-role {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: var(--primary);
  font-weight: 500;
}

/* 当前词汇 */
.current-word-badge {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  background: linear-gradient(135deg, var(--secondary) 0%, #34D399 100%);
  color: black;
  padding: 8px 16px;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.2);
  min-width: 160px;
  height: 64px;
  margin-left: auto;
  flex-shrink: 0;
}

.word-icon {
  font-size: 18px;
}

.word-content {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.word-label {
  font-size: 12px;
  color: black;
  opacity: 0.9;
  font-weight: 600;
}

.word-text {
  font-size: 16px;
  font-weight: 600;
  opacity: 0.9;
  letter-spacing: 3px;
  color: black;
}

/* Main Content Layout */
.main-content {
  display: flex;
  height: auto;
  gap: 24px;
  flex: 1;
}

/* Canvas Panel Styles */
.canvas-panel {
  flex: 2;
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* 绘画工具栏样式 */
.drawing-toolbar {
  background: white;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-wrap: wrap;
  gap: 20px;
  align-items: center;
}

.tool-group {
  display: flex;
  gap: 8px;
}

.tool-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 8px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
  background: var(--light);
  border: 2px solid transparent;
}

.tool-item:hover {
  background: var(--primary-lightest);
}

.tool-item.active {
  background: var(--primary-lightest);
  border-color: var(--primary);
  color: var(--primary);
}

.tool-item span {
  font-size: 12px;
  font-weight: 500;
}

.color-group {
  display: flex;
  align-items: center;
  gap: 10px;
}

.color-label {
  font-size: 14px;
  font-weight: 500;
  color: var(--dark);
}

.color-palette {
  display: flex;
  gap: 6px;
}

.color-item {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  cursor: pointer;
  border: 2px solid white;
  box-shadow: 0 0 0 1px rgba(0, 0, 0, 0.1);
  transition: all 0.2s ease;
}

.color-item:hover {
  transform: scale(1.1);
}

.color-item.active {
  box-shadow: 0 0 0 3px var(--primary);
  transform: scale(1.1);
}

.size-group {
  display: flex;
  align-items: center;
  gap: 10px;
}

.size-label {
  font-size: 14px;
  font-weight: 500;
  color: var(--dark);
}

.size-options {
  display: flex;
  gap: 8px;
}

.size-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 6px 8px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
  background: var(--light);
  border: 2px solid transparent;
}

.size-item:hover {
  background: var(--primary-lightest);
}

.size-item.active {
  background: var(--primary-lightest);
  border-color: var(--primary);
}

.size-dot {
  background: var(--dark);
  border-radius: 50%;
}

.size-item span {
  font-size: 10px;
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
  min-height: 400px;
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
  padding: 20px 20px;
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

/* 词汇选择弹窗样式 */
.word-selection-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.word-selection-modal {
  background: white;
  border-radius: 20px;
  padding: 30px;
  width: 90%;
  max-width: 500px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
}

.modal-header {
  text-align: center;
  margin-bottom: 30px;
}

.modal-header h3 {
  font-size: 24px;
  font-weight: 600;
  color: var(--dark);
  margin: 0 0 10px 0;
}

.selection-tip {
  color: var(--gray);
  font-size: 14px;
  margin: 0;
  line-height: 1.5;
}

.word-options {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 15px;
  margin-bottom: 25px;
}

.word-option {
  background: linear-gradient(135deg, var(--primary-lightest) 0%, #ffffff 100%);
  border: 2px solid var(--primary-light);
  border-radius: 12px;
  padding: 20px;
  text-align: center;
  cursor: pointer;
  transition: all 0.3s ease;
}

.word-option:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 20px rgba(79, 70, 229, 0.2);
  border-color: var(--primary);
}

.word-selection-modal .word-text {
  font-size: 18px;
  font-weight: 600;
  color: var(--dark);
  margin-bottom: 8px;
  letter-spacing: 1px;
}

.word-difficulty {
  font-size: 14px;
  color: var(--warning);
  font-weight: 500;
}

.selection-timer {
  text-align: center;
}

.timer-bar {
  width: 100%;
  height: 6px;
  background: var(--gray-light);
  border-radius: 3px;
  overflow: hidden;
  margin-bottom: 10px;
}

.timer-progress {
  height: 100%;
  background: linear-gradient(90deg, var(--primary) 0%, var(--primary-light) 100%);
  transition: width 1s linear;
}

/* 等待弹窗样式 */
.waiting-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.waiting-modal {
  background: white;
  border-radius: 20px;
  padding: 40px;
  text-align: center;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
}

.waiting-content {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.waiting-icon {
  font-size: 48px;
  margin-bottom: 20px;
  animation: pulse 2s infinite;
}

@keyframes pulse {

  0%,
  100% {
    opacity: 1;
  }

  50% {
    opacity: 0.5;
  }
}

.waiting-content h3 {
  font-size: 20px;
  font-weight: 600;
  color: var(--dark);
  margin: 0 0 10px 0;
}

.waiting-content p {
  color: var(--gray);
  margin: 0;
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

  .header-info {
    flex-direction: column;
    gap: 12px;
    height: auto;
    padding: 16px;
  }

  .players-status {
    width: 100%;
    justify-content: center;
  }

  .current-word-badge {
    margin-left: 0;
  }

  .drawing-toolbar {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
  }

  .tool-group,
  .color-group,
  .size-group {
    width: 100%;
    justify-content: flex-start;
  }

  .word-options {
    grid-template-columns: 1fr;
  }
}
</style>