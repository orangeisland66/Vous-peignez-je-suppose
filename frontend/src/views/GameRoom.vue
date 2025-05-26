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
       
        <div class="painter-info">
          <!-- <div class="painter-avatar">{{ currentPainter.charAt(0) }}</div> -->
          <span class="painter-name">{{ currentPainter }}</span>
        </div>
      </div>

      <!-- Main Game Area -->
      <div class="main-content">
        <!-- Left Panel - Canvas -->
        <section class="canvas-panel">
          <div class="canvas-container">
            <drawing-board
              ref="drawingBoard"
              :readonly="!isPainter"
              @stroke-completed="onStrokeCompleted"
              @canvas-cleared="onCanvasCleared"
            />
          </div>
          

        </section>

        <!-- Right Panel - Chat -->
        <section class="guess-panel">
          <div class="panel-header">
            <h2>游戏聊天</h2>
          </div>
          
          <Chat 
            :isPainter="isPainter"
            :targetWord="targetWord"
            @guess-correct="handleCorrectGuess"
          />
        </section>
      </div>
    </div>
  </div>
</template>

<script>
import DrawingBoard from '@/components/game/DrawingBoard.vue';
import Chat from '@/components/gameRoom/Chat.vue';
import signalRService from '@/services/signalRService.js'; // 引入 signalRService

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
      currentRound: 1,          // 当前回合数
      currentPainter: 'Alice',   // 当前画师
      timer: 60,                // 倒计时（秒）
      targetWord: 'umbrella',   // 目标词语（实际从后端获取）
      isPainter: true,         // 是否为画师（需根据业务逻辑动态设置）
      isGameActive: true,        // 游戏是否进行中
    };
  },
  // 添加路由离开守卫
  beforeRouteLeave(to, from, next) {
    console.log('[GameRoom] 路由离开，准备断开SignalR连接');
    this.disconnectSignalR();
    next();
  },
  beforeDestroy() {
    console.log('[GameRoom] 组件销毁，准备断开SignalR连接');
    this.disconnectSignalR();
  },
  mounted() {
    this.startTimer();
    const roomId = this.$route.params.roomId;
    // 优先从 localStorage 获取 userId
    const playerId = parseInt(localStorage.getItem('userId')) || this.playerId;

    console.log('[GameRoom] Joining room with details:', {
      roomId,
      playerId,
      propsPlayerId: this.playerId,
      localStorageUserId: localStorage.getItem('userId')
    });

    if (roomId && playerId) {
      signalRService.initialize(roomId).then(() => {
        signalRService.joinGroup(roomId, playerId)
          .then(() => {
            console.log(`[SignalR] 成功加入房间: ${roomId}, 玩家ID: ${playerId}`);
          })
          .catch(err => {
            console.error(`[SignalR] 加入房间失败: ${err.message}`);
            this.$router.push('/lobby');
          });
      });
    } else {
      console.error('[SignalR] 房间ID或玩家ID缺失，无法加入房间');
      // 重定向到大厅
      this.$router.push('/lobby');
    }
  },
  methods: {
    // 断开SignalR连接的方法
    disconnectSignalR() {
      if (signalRService.hubConnection && signalRService.isConnected.value) {
        signalRService.hubConnection.stop()
          .then(() => {
            signalRService.isConnected.value = false;
            console.log('[GameRoom] SignalR连接已断开');
          })
          .catch(err => {
            console.error('[GameRoom] 断开SignalR连接失败:', err);
          });
      }
    },
    // 格式化时间
    formatTime(sec) {
      const m = String(Math.floor(sec / 60)).padStart(2, '0');
      const s = String(sec % 60).padStart(2, '0');
      return `${m}:${s}`;
    },

    // 处理猜对事件
    handleCorrectGuess() {
      this.isGameActive = false;
      this.currentRound++; // 进入下一回合
      this.resetGame();    // 重置游戏状态（示例逻辑）
      
      // 模拟切换画师（实际需与后端交互）
      this.currentPainter = this.currentPainter === 'Alice' ? 'Bob' : 'Alice';
      this.targetWord = this.getRandomWord(); // 随机新词语
      this.timer = 60;
      this.isPainter = !this.isPainter;
    },

    // 启动倒计时
    startTimer() {
      if (this.isGameActive && this.timer > 0) {
        setTimeout(() => {
          this.timer--;
          this.startTimer();
        }, 1000);
      } else if (this.timer === 0 && this.isGameActive) {
        this.isGameActive = false;
        // 处理倒计时结束逻辑（如平局、切换画师等）
      }
    },

    // 重置游戏状态
    resetGame() {
      this.$refs.drawingBoard.clearCanvas(); // 清空画布（需DrawingBoard组件支持）
      // 其他重置逻辑...
    },

    // 随机词语示例（实际从词库获取）
    getRandomWord() {
      const words = ['apple', 'banana', 'umbrella', 'computer', 'flower'];
      return words[Math.floor(Math.random() * words.length)];
    },

    // 绘图相关事件处理
    // 在DrawingBoard组件中一笔绘制完成会调用这个函数
    // 在这里调用我的signalR中的函数，发送消息到后端
    onStrokeCompleted(stroke) {

      // 调试信息
      console.log('在GameRoom.vue的onStrokeCompleted函数中收到笔画:',stroke);

      // 如需同步到其他玩家，此处发送WebSocket消息
      if(!this.isPainter || !stroke) return;
      
      // 调试信息
      console.log('在GameRoom.vue的onStrokeCompleted中开始调用SignalR发送消息');
      // 前端不做连接判断，等到signalR中判断
      // //通过signalR发送消息到后端
      // if(!this.signalRService.isConnected.value){
      //   console.warn('SignalR未连接,无法发送笔画');
      //   return;
      // }
      //await signalRService.sendStroke(stroke);
    },
    onCanvasCleared() {
      console.log('画布已清空');
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
  /* margin-top: 5%; */
  background: linear-gradient(135deg, #F9FAFB 0%, #EEF2FF 100%);
  /* height: calc(100vh - 300px); */
  width: 100vw;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  color: var(--dark);
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

/* Header Styles */
.game-header {
  height: 60px; /* 原高度可能为 80px+，调小至 60px 或更小 */
  padding: 0 20px; /* 减少左右内边距 */
  /* 可选：压缩文字大小 */
  font-size: 14px; 
  
}

.header-info {
  display: flex;
  height: 0px;
  align-items: center;
  gap: 20px;
  /* margin-right: auto; */
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
  justify-content: center;
  align-items: center;
  background: var(--primary-lightest);
  padding: 8px 30px;
  border-radius: 50px;
  box-shadow: 0 2px 10px rgba(79, 70, 229, 0.1);
  margin-left: auto;
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
  margin-right: 8px; 
}

.painter-name {
  font-weight: 500;
  color: var(--primary-dark);
  text-align: center; 
}

/* Main Content Layout */
.main-content {
  display: flex;
  height: auto;
  /* min-height: 600px; */
  padding: 20px 0px;

  gap: 24px;
}

/* Canvas Panel Styles */
.canvas-panel {
  height: auto;
  flex: 2;
  position: relative;
  display: flex;
  flex-direction: column;
}

.canvas-container {
  height: auto;
  flex: 1;
  display: flex;
  justify-content: center;
  align-items: center;
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  /* margin-bottom: 16px; */
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
  height: var(--panel-height);
  /* max-height: 100vh;  */
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

@media (max-width: 992px) {
  .main-content {
    flex-direction: column;
    height: auto;
    padding: 16px;
  }

  .canvas-panel {
    margin-bottom: 24px;
  }
}
</style>