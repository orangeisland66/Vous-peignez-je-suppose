<template>
  <div class="result-background">
    <div class="result-container">
      <!-- Header -->
      <header class="result-header">
        <div class="round-info-container">
          <div class="round-badge">
            <span class="round-number">第 {{ currentRound }} 回合</span>
            <span class="round-status">结果</span>
          </div>
          <div class="game-progress">
            <div class="progress-bar">
              <div class="progress-fill" :style="{ width: (currentRound / totalRounds) * 100 + '%' }"></div>
            </div>
            <span class="progress-text">{{ currentRound }}/{{ totalRounds }} 回合</span>
          </div>
        </div>
        <div class="answer-display">
          <span class="answer-label">本轮正确答案</span>
          <div class="answer-badge">{{ correctWord }}</div>
        </div>
      </header>

      <!-- Main Content Area -->
      <div class="main-content">
        <!-- Left Panel - Final Drawing -->
        <section class="drawing-panel">
          <div class="panel-header">
            <h3>最终画作</h3>
          </div>
          <div class="canvas-container">
            <canvas ref="previewCanvas" class="preview-canvas"></canvas>
            <div class="canvas-overlay" v-if="!canvasLoaded">
              <div class="loading-spinner"></div>
              <p>加载画作中...</p>
            </div>
          </div>
        </section>

        <!-- Right Panel - Players Results -->
        <section class="players-panel">
          <div class="panel-header">
            <h3>玩家表现</h3>
            <div class="sort-controls">
              <button class="sort-btn" :class="{ active: sortBy === 'round' }" @click="sortBy = 'round'">
                本回合
              </button>
              <button class="sort-btn" :class="{ active: sortBy === 'total' }" @click="sortBy = 'total'">
                总分
              </button>
            </div>
          </div>

          <div class="players-container">
            <div v-if="sortedPlayers.length === 0" class="no-players">
              <div class="empty-icon">👥</div>
              <p>暂无玩家数据</p>
            </div>

            <div v-else class="players-grid">
              <div v-for="(player, index) in sortedPlayers" :key="player.id" class="player-card" :class="{
                drawing: player.isDrawing,
                winner: index === 0 && sortBy === 'round',
                'top-scorer': index === 0 && sortBy === 'total'
              }">
                <div class="player-rank" v-if="sortBy === 'total'">
                  <span class="rank-number">#{{ index + 1 }}</span>
                </div>

                <div class="player-header">
                  <div class="avatar-container">
                    <span v-if="!player.avatarUrl || hasError">{{ player.username?.charAt(0).toUpperCase() || 'P' }}</span>
                    <img v-else :src="player.avatarUrl" alt="avatar" class="avatar" @error="handleImageError" />
                    <div class="role-badge" :class="player.isDrawing ? 'drawer' : 'guesser'">
                      {{ player.isDrawing ? '🎨' : '🤔' }}
                    </div>
                  </div>
                  <div class="player-info">
                    <span class="player-name">{{ player.username }}</span>
                    <span class="player-role">{{ player.isDrawing ? '画手' : '猜词者' }}</span>
                  </div>
                </div>

                <div class="score-section">
                  <div class="score-item round-score">
                    <span class="score-label">本回合</span>
                    <div class="score-value">
                      <span class="score-number">+{{ player.roundScore }}</span>
                      <div class="score-bar">
                        <div class="score-fill"
                          :style="{ width: Math.min((player.roundScore / 100) * 100, 100) + '%' }"></div>
                      </div>
                    </div>
                  </div>
                  <div class="score-item total-score">
                    <span class="score-label">总积分</span>
                    <span class="total-number">{{ player.totalScore }}</span>
                  </div>
                </div>

                <div v-if="!player.isDrawing" class="guess-section">
                  <div class="guess-result">
                    <span class="guess-label">猜词结果</span>
                    <div class="guess-badge"
                      :class="{ correct: player.hasGuessedCorrectly, incorrect: !player.hasGuessedCorrectly }">
                      <span class="guess-icon">{{ player.hasGuessedCorrectly ? '✓' : '✗' }}</span>
                      <span class="guess-text">{{ player.guessedWord || '未猜出' }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>
      </div>

      <!-- Footer Actions -->
      <footer class="result-actions">
        <div class="countdown-display" v-if="countdown > 0">
          <div class="countdown-circle">
            <span class="countdown-number">{{ countdown }}</span>
          </div>
          <span class="countdown-text">秒后可继续</span>
        </div>

        <div class="action-buttons">
          <button class="action-button secondary" @click="leave">
            <span class="btn-icon">🚪</span>
            退出游戏
          </button>
          <button v-if="isHost" class="action-button primary" @click="proceed" :disabled="!canProceed">
            <span class="btn-icon">{{ isLastRound ? '🏆' : '▶️' }}</span>
            {{ isLastRound ? '查看总分榜' : '下一回合' }}
          </button>
        </div>
      </footer>
    </div>
  </div>
</template>

<script>
import { HubConnectionBuilder } from '@microsoft/signalr'
import apiService from '@/services/apiService';
import signalRService from '@/services/signalRService';
export default {
  name: 'RoundResult',
  data() {
      return {
      roomId: this.$route.params.roomId, // 从路由获取 roomId
      currentRound: 1,
      totalRounds: 1,
      correctWord: '',
      players: [], // 存储玩家列表（包含分数和用户信息）
      isHost: false,
      isLastRound: false,
      canProceed: false,
      countdown: 3,
      canvasLoaded: false,
      sortBy: 'round'
    };
  },
  computed: {
    sortedPlayers() {
      // 按当前排序规则对玩家数组排序
      if (this.sortBy === 'round') {
        return [...this.players].sort((a, b) => b.roundScore - a.roundScore);
      } else {
        return [...this.players].sort((a, b) => b.totalScore - a.totalScore);
      }
    }
  },
  async created() {
    // 初始化时加载房间信息和玩家数据
    await this.loadRoundInfo();
    await this.fetchPlayers(); // 新增：获取玩家信息
    this.startCountdown();
    // this.initSignalR();
  },
  methods: {
    async loadRoundInfo() {
      try {
        const res = await fetch(`/api/game/round/${this.roomId}/result`);
        if (!res.ok) throw new Error('获取回合信息失败');
        const data = await res.json();
        this.currentRound = data.currentRound;
        this.totalRounds = data.totalRounds;
        this.correctWord = data.correctWord;
        this.isHost = data.isHost;
        this.isLastRound = data.currentRound === data.totalRounds;
      } catch (error) {
        console.error('Load round info error:', error);
      }
    },
    async fetchPlayers() {
      console.log('[fetchRoomPlayers] 调用来源：', new Error().stack); // 打印调用栈
      try {
        // 调用与之前 `fetchRoomPlayers` 类似的逻辑获取玩家数据
        const roomId = this.$route.params.roomId;
        const res = await apiService.getRoomDetails(roomId); // 使用原有的 API 方法
        if (res && res.room && res.room.players) {
            const scoreMap = new Map();
            signalRService.chatMessages.value.forEach(msg => {
              if (msg.playerId !== undefined && msg.score !== undefined) {
                scoreMap.set(msg.playerId, msg.score);
              }
            });
          // 处理玩家数据（假设每个玩家包含分数字段）
          this.players = res.room.players.map(player => ({
            id: player.user.id,
            username: player.user.username,
            avatarUrl: player.user.avatarUrl, // 假设用户信息包含头像 URL
            isDrawing: player.id === res.room.currentPainterId, // 判断是否为当前画手
            roundScore: 1, // 本回合得分
            totalScore: player.Score || 0, // 总分
            hasGuessedCorrectly: player.HasGuessed || false, // 是否猜对
            guessedWord: player.guessedWord || '' // 猜测的词汇
          }));
        }
      } catch (error) {
        console.error('Fetch players error:', error);
      }
    },
    async initSignalR() {
      this.connection = new HubConnectionBuilder()
        .withUrl(`/hub/game?roomId=${this.roomId}`)
        .withAutomaticReconnect()
        .build()

      this.connection.on('PlayersUpdated', updatedPlayers => {
      this.players = updatedPlayers.map(player => ({
        id: player.id,
        username: player.username,
        avatarUrl: player.avatarUrl || '',
        isDrawing: player.isDrawing || false,
        roundScore: player.roundScore || 0,
        totalScore: player.totalScore || 0,
        hasGuessedCorrectly: player.hasGuessedCorrectly || false,
        guessedWord: player.guessedWord || ''
      }))
    })

      this.connection.on('RoundUpdated', roundData => {
        this.currentRound = roundData.currentRound
        this.correctWord = roundData.correctWord
      })

      try {
        await this.connection.start()
        console.log('SignalR connected')
        // 主动拉一次
        await this.fetchPlayers()
      } catch (err) {
        console.error('SignalR connection error:', err)
        // fallback: 轮询
        this.polling = setInterval(this.fetchPlayers, 3000)
      }
    },
    renderCanvas() {
      const canvas = this.$refs.previewCanvas
      if (!canvas) return
      canvas.width = canvas.clientWidth
      canvas.height = canvas.clientHeight
      const ctx = canvas.getContext('2d')

      // 模拟绘图加载
      setTimeout(() => {
        // TODO: 从后端拉取画布图像数据并渲染
        this.canvasLoaded = true
      }, 1500)
    },
    startCountdown() {
      const timer = setInterval(() => {
        this.countdown--
        if (this.countdown <= 0) {
          clearInterval(timer)
          this.canProceed = true
        }
      }, 1000)
    },
    async proceed() {
      try {
        if (this.isLastRound) {
          this.$router.push(`/room/${this.roomId}/final`)
        } else {
          await fetch(`/api/game/round/${this.roomId}/next`, { method: 'POST' })
          this.$router.push(`/room/${this.roomId}/game`)
        }
      } catch (error) {
        console.error('Proceed error:', error)
      }
    },
    async leave() {
      try {
        await fetch(`/api/game/${this.roomId}/leave`, { method: 'POST' })
        this.$router.push('/lobby')
      } catch (error) {
        console.error('Leave error:', error)
        this.$router.push('/lobby')
      }
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

/* Base Layout */
.result-background {
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

.result-container {
  width: 95%;
  max-width: 1400px;
  background: white;
  border-radius: 24px;
  box-shadow: 0 10px 30px rgba(79, 70, 229, 0.1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  height: 90vh;
  max-height: 900px;
}

/* Header Styles */
.result-header {
  padding: 24px 24px;
  background: white;
  border-bottom: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 24px;
}

.round-info-container {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

/* 回合徽章样式统一改为圆角矩形，字体清晰 */
.round-badge {
  display: inline-flex;
  align-items: center;
  gap: 12px;
  background: var(--primary-lightest);
  padding: 10px 20px;
  border-radius: 16px;
  border: 1.5px solid var(--primary-light);
  box-shadow: 0 2px 6px rgba(79, 70, 229, 0.08);
}

.round-number {
  font-size: 1rem;
  font-weight: 600;
  color: var(--primary);
}

/* 
.round-status {
  font-size: 0.875rem;
  color: var(--primary-dark);
  background: white;
  padding: 4px 10px;
  border-radius: 12px;
  box-shadow: inset 0 0 0 1px var(--gray-light);
} */

/* 进度条组 */
.game-progress {
  display: flex;
  align-items: center;
  gap: 12px;
}

.progress-bar {
  width: 140px;
  height: 10px;
  background: var(--gray-lighter);
  border-radius: 6px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(to right, var(--primary), var(--primary-light));
  border-radius: 6px;
  transition: width 0.4s ease;
}

.progress-text {
  font-size: 0.85rem;
  color: var(--gray);
  font-weight: 500;
}

/* 答案显示 */
.answer-display {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 8px;
  min-width: 180px;
}

.answer-label {
  font-size: 0.95rem;
  color: var(--gray);
  font-weight: 500;
}

.answer-badge {
  background: linear-gradient(135deg, var(--secondary), var(--success));
  color: white;
  padding: 12px 20px;
  border-radius: 8px;
  font-size: 1.15rem;
  font-weight: 700;
  white-space: nowrap;
}

/* Main Content Layout */
.main-content {
  display: flex;
  flex: 1;
  padding: 0 24px;
  gap: 24px;
  overflow: hidden;
}

/* Panel Styles */
.drawing-panel,
.players-panel {
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.drawing-panel {
  flex: 1;
  min-width: 400px;
}

.players-panel {
  flex: 1.2;
  min-width: 500px;
}

.panel-header {
  padding: 20px 24px;
  border-bottom: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
  background: var(--light);
}

.panel-header h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--dark);
  margin: 0;
}

.drawing-actions {
  display: flex;
  gap: 8px;
}

/* .action-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  border: none;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-btn.save {
  background: var(--primary-lightest);
  color: var(--primary);
}

.action-btn.share {
  background: var(--primary-lightest);
  color: var(--primary);
} */

/* .action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
} */

.btn-icon {
  font-size: 14px;
}

/* Canvas Styles */
.canvas-container {
  flex: 1;
  padding: 20px;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}

.preview-canvas {
  width: 100%;
  height: 100%;
  max-height: 400px;
  border: 2px solid var(--gray-light);
  border-radius: 12px;
  background: white;
  box-shadow: inset 0 2px 4px rgba(0, 0, 0, 0.05);
}

.canvas-overlay {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  color: var(--gray);
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid var(--gray-light);
  border-top: 3px solid var(--primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}

/* Sort Controls */
.sort-controls {
  display: flex;
  background: var(--gray-light);
  border-radius: 8px;
  padding: 2px;
}

.sort-btn {
  padding: 6px 12px;
  border: none;
  background: transparent;
  color: var(--gray);
  font-size: 0.85rem;
  font-weight: 500;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.sort-btn.active {
  background: var(--primary);
  color: white;
}

/* Players Section */
.players-container {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
}

.no-players {
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: var(--gray);
  text-align: center;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.5;
}

.players-grid {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* Player Card Styles */
.player-card {
  background: white;
  border: 2px solid var(--gray-light);
  border-radius: 16px;
  padding: 20px;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.player-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: var(--gray-light);
  transition: all 0.3s ease;
}

.player-card.drawing::before {
  background: linear-gradient(to right, var(--accent), var(--primary-light));
}

.player-card.winner::before {
  background: linear-gradient(to right, var(--warning), var(--accent));
}

.player-card.top-scorer::before {
  background: linear-gradient(to right, var(--success), var(--secondary));
}

.player-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(79, 70, 229, 0.1);
  border-color: var(--primary-light);
}

.player-rank {
  position: absolute;
  top: 16px;
  right: 16px;
  background: var(--primary);
  color: white;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 0.9rem;
}

.player-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 16px;
}

.avatar-container {
  position: relative;
}

.avatar {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid var(--gray-light);
}

.role-badge {
  position: absolute;
  bottom: -4px;
  right: -4px;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  border: 2px solid white;
}

.role-badge.drawer {
  background: var(--accent);
}

.role-badge.guesser {
  background: var(--primary-light);
}

.player-info {
  flex: 1;
}

.player-name {
  display: block;
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--dark);
  margin-bottom: 2px;
}

.player-role {
  font-size: 0.9rem;
  color: var(--gray);
}

/* Score Section */
.score-section {
  display: flex;
  gap: 20px;
  margin-bottom: 12px;
}

.score-item {
  flex: 1;
}

.score-label {
  display: block;
  font-size: 0.8rem;
  color: var(--gray);
  margin-bottom: 4px;
  font-weight: 500;
}

.score-value {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.score-number {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--success);
}

.score-bar {
  height: 4px;
  background: var(--gray-light);
  border-radius: 2px;
  overflow: hidden;
}

.score-fill {
  height: 100%;
  background: linear-gradient(to right, var(--success), var(--secondary));
  border-radius: 2px;
  transition: width 0.3s ease;
}

.total-number {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--primary);
}

/* Guess Section */
.guess-section {
  border-top: 1px solid var(--gray-light);
  padding-top: 12px;
}

.guess-result {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.guess-label {
  font-size: 0.9rem;
  color: var(--gray);
  font-weight: 500;
}

.guess-badge {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 0.9rem;
  font-weight: 500;
}

.guess-badge.correct {
  background: rgba(34, 197, 94, 0.1);
  color: var(--success);
  border: 1px solid rgba(34, 197, 94, 0.2);
}

.guess-badge.incorrect {
  background: rgba(239, 68, 68, 0.1);
  color: var(--danger);
  border: 1px solid rgba(239, 68, 68, 0.2);
}

.guess-icon {
  font-size: 14px;
}

/* Footer Styles */
.result-actions {
  padding: 24px;
  background: var(--light);
  border-top: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
}

.countdown-display {
  display: flex;
  align-items: center;
  gap: 4px;
  color: var(--gray);
}

.countdown-circle {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--primary-lightest);
  border: 2px solid var(--primary-light);
  display: flex;
  align-items: center;
  justify-content: center;
  animation: pulse 1s ease-in-out infinite;
}

@keyframes pulse {
  0% {
    transform: scale(1);
  }

  50% {
    transform: scale(1.05);
  }

  100% {
    transform: scale(1);
  }
}

.countdown-number {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--primary);
}

.countdown-text {
  font-size: 0.9rem;
  font-weight: 500;
}

.action-buttons {
  display: flex;
  align-items: center;
  gap: 16px;
}

.action-button {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  border: none;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  min-width: 140px;
  justify-content: center;
}

.action-button.primary {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.2);
}

.action-button.primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(79, 70, 229, 0.3);
}

.action-button.primary:disabled {
  background: var(--gray-light);
  color: var(--gray);
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.action-button.secondary {
  background: white;
  color: var(--gray);
  border: 2px solid var(--gray-light);
}

.action-button.secondary:hover {
  background: var(--danger);
  color: white;
  border-color: var(--danger);
  transform: translateY(-2px);
}

/* Responsive Design */
@media (max-width: 1200px) {
  .main-content {
    flex-direction: column;
    overflow-y: auto;
  }

  .drawing-panel,
  .players-panel {
    min-width: unset;
    flex: none;
  }

  .drawing-panel {
    height: 300px;
  }
}

@media (max-width: 768px) {
  .result-container {
    width: 100%;
    height: 100vh;
    border-radius: 0;
  }

  .result-header {
    flex-direction: column;
    align-items: stretch;
    gap: 16px;
  }

  .round-info-container,
  .answer-display {
    align-items: center;
  }

  .main-content {
    padding: 16px;
  }

  .score-section {
    flex-direction: column;
    gap: 12px;
  }

  .result-actions {
    flex-direction: column;
    gap: 16px;
  }

  .action-buttons {
    width: 100%;
    justify-content: stretch;
  }

  .action-button {
    flex: 1;
  }
}
</style>