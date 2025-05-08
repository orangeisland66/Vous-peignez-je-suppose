<template>
  <div class="round-outer">
    <div class="round-container">
      <!-- Header -->
      <header class="round-header">
        <h2>第 {{ currentRound }} 回合结果</h2>
        <div class="round-info">
          <span>总回合数：{{ totalRounds }}</span>
          <span>正确答案：{{ correctWord }}</span>
        </div>
      </header>

      <!-- Content -->
      <section class="round-content">
        <!-- Preview Drawing -->
        <div class="preview-section">
          <h3>最终画作</h3>
          <canvas ref="previewCanvas" class="preview-canvas"></canvas>
        </div>

        <!-- Players Results -->
        <div class="players-section">
          <h3>玩家表现</h3>
          <div class="players-list">
            <div
              v-for="player in players"
              :key="player.id"
              class="player-card"
              :class="{ drawing: player.isDrawing }"
            >
              <div class="player-header">
                <img :src="player.avatarUrl || '/default-avatar.png'" alt="avatar" class="avatar" />
                <div class="info">
                  <span class="name">{{ player.username }}</span>
                  <span class="role">{{ player.isDrawing ? '画手' : '猜词者' }}</span>
                </div>
              </div>
              <div class="scores">
                <div><span>本回合：</span><strong>{{ player.roundScore }}</strong></div>
                <div><span>总积分：</span><strong>{{ player.totalScore }}</strong></div>
              </div>
              <div v-if="!player.isDrawing" class="guess-info">
                <span>猜词：</span>
                <strong :class="{ correct: player.hasGuessedCorrectly }">
                  {{ player.guessedWord || '未猜出' }}
                </strong>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Actions -->
      <footer class="round-actions">
        <button
          v-if="isHost"
          class="btn primary"
          @click="proceed"
          :disabled="!canProceed"
        >
          {{ isLastRound ? '查看总分榜' : '下一回合' }}
        </button>
        <button class="btn secondary" @click="leave">退出游戏</button>
      </footer>
    </div>
  </div>
</template>

<script>
export default {
  name: 'RoundResult',
  data() {
    return {
      roomId: this.$route.params.id,
      currentRound: 1,
      totalRounds: 1,
      correctWord: '',
      players: [],
      isHost: false,
      isLastRound: false,
      canProceed: false,
      countdown: 3
    }
  },
  async created() {
    await this.loadResult()
    this.startCountdown()
  },
  mounted() {
    this.renderCanvas()
  },
  methods: {
    async loadResult() {
      const res = await fetch(`/api/game/round/${this.roomId}/result`)
      const data = await res.json()
      this.currentRound = data.currentRound
      this.totalRounds = data.totalRounds
      this.correctWord = data.correctWord
      this.players = data.players
      this.isHost = data.isHost
      this.isLastRound = this.currentRound === this.totalRounds
    },
    renderCanvas() {
      const canvas = this.$refs.previewCanvas
      canvas.width = canvas.clientWidth
      canvas.height = canvas.clientHeight
      const ctx = canvas.getContext('2d')
      // TODO: 从后端获取并渲染绘图数据
      // e.g. ctx.drawImage(...)
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
      if (this.isLastRound) {
        this.$router.push(`/room/${this.roomId}/final`)
      } else {
        await fetch(`/api/game/round/${this.roomId}/next`, { method: 'POST' })
        this.$router.push(`/room/${this.roomId}/game`)
      }
    },
    async leave() {
      await fetch(`/api/game/${this.roomId}/leave`, { method: 'POST' })
      this.$router.push('/lobby')
    }
  }
}
</script>

<style scoped>
.round-outer {
  width: 100vw;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  background: #f5f5f5;
}
.round-container {
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
.round-header h2 {
  text-align: center;
  font-size: 2rem;
  margin: 0;
}
.round-info {
  display: flex;
  justify-content: center;
  gap: 32px;
  margin-top: 8px;
  color: #555;
}

.round-content {
  flex: 1;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  margin: 24px 0;
}
.preview-section,
.players-section {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  padding: 16px;
}
.preview-section h3,
.players-section h3 {
  margin: 0 0 12px;
  font-size: 1.3rem;
}
.preview-canvas {
  width: 100%;
  height: 100%;
  border: 1px solid #ddd;
  border-radius: 4px;
}
.players-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.player-card {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px;
  border-radius: 6px;
  background: #f9f9f9;
}
.player-card.drawing {
  background: #ffece6;
}
.player-header {
  display: flex;
  align-items: center;
  gap: 12px;
}
.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
}
.info .name {
  font-weight: bold;
  font-size: 1.1rem;
}
.info .role {
  font-size: 0.9rem;
  color: #777;
}
.scores {
  display: flex;
  justify-content: space-between;
  font-size: 1rem;
  color: #333;
}
.guess-info {
  font-size: 1rem;
}
.guess-info .correct {
  color: #28a745;
}

.round-actions {
  display: flex;
  justify-content: center;
  gap: 24px;
}
.btn {
  padding: 10px 24px;
  font-size: 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
.primary {
  background: #e60000;
  color: #fff;
}
.primary:disabled {
  background: #ccc;
  cursor: not-allowed;
}
.secondary {
  background: #f0f0f0;
  color: #333;
}
.secondary:hover {
  background: #ddd;
}

@media (max-width: 900px) {
  .round-container { padding: 16px; }
  .round-info { flex-direction: column; gap: 12px; }
  .round-content { grid-template-columns: 1fr; }
  .preview-canvas { height: 200px; }
  .scores { flex-direction: column; gap: 4px; }
  .round-actions { flex-direction: column; gap: 12px; }
}
</style>
