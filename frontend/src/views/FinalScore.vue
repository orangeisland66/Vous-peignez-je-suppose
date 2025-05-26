<template>
  <div class="final-background">
    <div class="final-container">
      <!-- Header -->
      <header class="final-header">
        <div class="title-container">
          <div class="title-icon">🏆</div>
          <h1>游戏结束</h1>
        </div>
        <div class="game-info">
          <div class="info-card">
            <span class="info-label">房间号</span>
            <span class="info-value">{{ roomId }}</span>
          </div>
          <div class="info-card">
            <span class="info-label">总回合数</span>
            <span class="info-value">{{ totalRounds }}</span>
          </div>
        </div>
      </header>

      <!-- Main Content Area -->
      <div class="main-content">
        <!-- Left Panel - Ranking -->
        <section class="ranking-panel">
          <div class="panel-header">
            <h2>🏅 最终排名</h2>
            <span class="players-count">{{ players.length }} 位玩家</span>
          </div>

          <div class="ranking-container">
            <div v-if="players.length === 0" class="no-players">
              <div class="empty-icon">👥</div>
              <p>暂无玩家数据</p>
            </div>

            <ul v-else class="ranking-list">
              <li v-for="(player, idx) in sortedPlayers" :key="player.id" :class="['ranking-item', getRankClass(idx)]">
                <div class="rank-badge">
                  <div class="rank-number">{{ idx + 1 }}</div>
                  <div v-if="idx < 3" class="rank-crown">{{ getCrownIcon(idx) }}</div>
                </div>

                <div class="player-avatar">
                  <img v-if="player.avatarUrl" :src="player.avatarUrl" alt="avatar" class="avatar-img" />
                  <div v-else class="avatar-placeholder">{{ player.username?.charAt(0) || '?' }}</div>
                </div>

                <div class="player-details">
                  <div class="player-name">{{ player.username }}</div>
                  <div class="player-stats">
                    <span class="stat-item">总分: {{ player.totalScore }}</span>
                    <span class="stat-item">猜中: {{ player.correctGuesses }}</span>
                  </div>
                </div>

                <div class="achievements">
                  <span v-for="ach in player.achievements" :key="ach.id" class="achievement-badge"
                    :title="ach.description">
                    {{ ach.name }}
                  </span>
                </div>
              </li>
            </ul>
          </div>
        </section>

        <!-- Right Panel - Statistics -->
        <section class="stats-panel">
          <div class="panel-header">
            <h2>📊 游戏统计</h2>
          </div>

          <div class="stats-container">
            <div class="stats-grid">
              <div class="stat-card fastest">
                <div class="stat-icon">⚡</div>
                <div class="stat-content">
                  <div class="stat-title">最快猜对</div>
                  <div class="stat-value">{{ stats.fastestGuess }}秒</div>
                  <div class="stat-subtitle">{{ stats.fastestGuessPlayer }}</div>
                </div>
              </div>

              <div class="stat-card popular">
                <div class="stat-icon">🔥</div>
                <div class="stat-content">
                  <div class="stat-title">最受欢迎词</div>
                  <div class="stat-value">{{ stats.mostPopularWord }}</div>
                  <div class="stat-subtitle">出现 {{ stats.mostPopularWordCount }} 次</div>
                </div>
              </div>

              <div class="stat-card artist">
                <div class="stat-icon">🎨</div>
                <div class="stat-content">
                  <div class="stat-title">最佳画手</div>
                  <div class="stat-value">{{ stats.bestArtist }}</div>
                  <div class="stat-subtitle">得分 {{ stats.bestArtistScore }}</div>
                </div>
              </div>

              <div class="stat-card duration">
                <div class="stat-icon">⏱️</div>
                <div class="stat-content">
                  <div class="stat-title">游戏时长</div>
                  <div class="stat-value">{{ stats.gameDuration }}分钟</div>
                  <div class="stat-subtitle">精彩对局</div>
                </div>
              </div>
            </div>
          </div>
        </section>
      </div>

      <!-- Action Buttons -->
      <footer class="action-footer">
        <!-- <button @click="playAgain" class="action-btn primary">
          <span class="btn-icon">🎮</span>
          <span class="btn-text">再来一局</span>
        </button> -->
        <button @click="backToLobby" class="action-btn secondary">
          <span class="btn-icon">🏠</span>
          <span class="btn-text">返回大厅</span>
        </button>
      </footer>
    </div>
  </div>
</template>

<script>
export default {
  name: 'FinalScore',
  data() {
    return {
      roomId: this.$route.params.id,
      totalRounds: 0,
      players: [],
      stats: {
        fastestGuess: 0,
        fastestGuessPlayer: '',
        mostPopularWord: '',
        mostPopularWordCount: 0,
        bestArtist: '',
        bestArtistScore: 0,
        gameDuration: 0
      }
    }
  },
  computed: {
    sortedPlayers() {
      return [...this.players].sort((a, b) => b.totalScore - a.totalScore)
    }
  },
  async created() {
    await this.fetchFinalData()
  },
  methods: {
    async fetchFinalData() {
      try {
        const res = await fetch(`/api/game/final/${this.roomId}`)
        if (!res.ok) throw new Error('获取数据失败')
        const data = await res.json()
        this.totalRounds = data.totalRounds
        this.players = data.players
        this.stats = data.stats
      } catch (e) {
        console.error(e)
      }
    },
    getRankClass(index) {
      if (index === 0) return 'first-place'
      if (index === 1) return 'second-place'
      if (index === 2) return 'third-place'
      return ''
    },
    getCrownIcon(index) {
      const crowns = ['👑', '🥈', '🥉']
      return crowns[index] || ''
    },
    playAgain() {
      this.$router.push('/lobby')
    },
    backToLobby() {
      this.$router.push('/lobby')
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
  --gold: #FFD700;
  --silver: #C0C0C0;
  --bronze: #CD7F32;
}

/* Base and Layout Styles */
.final-background {
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

.final-container {
  width: 90%;
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
.final-header {
  padding: 24px 32px;
  background: white;
  border-bottom: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.title-container {
  display: flex;
  align-items: center;
}

.title-icon {
  width: 48px;
  height: 48px;
  background: linear-gradient(135deg, var(--gold) 0%, #FFE55C 100%);
  border-radius: 12px;
  margin-right: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  box-shadow: 0 4px 12px rgba(255, 215, 0, 0.3);
}

.final-header h1 {
  font-size: 2rem;
  font-weight: 600;
  color: var(--primary);
  margin: 0;
}

.game-info {
  display: flex;
  gap: 16px;
}

.info-card {
  background: var(--primary-lightest);
  padding: 12px 16px;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 100px;
}

.info-label {
  font-size: 12px;
  color: var(--gray);
  margin-bottom: 4px;
}

.info-value {
  font-size: 16px;
  font-weight: 600;
  color: var(--primary-dark);
}

/* Main Content Layout */
.main-content {
  display: flex;
  flex: 1;
  padding: 24px;
  gap: 24px;
  overflow: hidden;
}

/* Panel Base Styles */
.ranking-panel,
.stats-panel {
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.ranking-panel {
  flex: 3;
}

.stats-panel {
  flex: 2;
}

.panel-header {
  padding: 20px 24px;
  border-bottom: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-shrink: 0;
}

.panel-header h2 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--dark);
  margin: 0;
}

.players-count {
  background: var(--primary-lightest);
  color: var(--primary);
  font-size: 14px;
  font-weight: 500;
  padding: 4px 12px;
  border-radius: 50px;
}

/* Ranking Styles */
.ranking-container {
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

.ranking-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ranking-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  border-radius: 12px;
  background: white;
  border: 1px solid var(--gray-light);
  transition: all 0.3s ease;
}

.ranking-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

.ranking-item.first-place {
  background: linear-gradient(135deg, #FFF8DC 0%, #FFE55C 100%);
  border-color: var(--gold);
}

.ranking-item.second-place {
  background: linear-gradient(135deg, #F8F8FF 0%, #E6E6FA 100%);
  border-color: var(--silver);
}

.ranking-item.third-place {
  background: linear-gradient(135deg, #FFF8DC 0%, #DEB887 100%);
  border-color: var(--bronze);
}

.rank-badge {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  min-width: 60px;
}

.rank-number {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  font-weight: 600;
}

.first-place .rank-number {
  background: var(--gold);
  color: var(--dark);
}

.second-place .rank-number {
  background: var(--silver);
  color: var(--dark);
}

.third-place .rank-number {
  background: var(--bronze);
  color: white;
}

.rank-crown {
  font-size: 16px;
  margin-top: 4px;
}

.player-avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  overflow: hidden;
  border: 2px solid var(--gray-light);
}

.avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.avatar-placeholder {
  width: 100%;
  height: 100%;
  background: var(--primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 18px;
}

.player-details {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.player-name {
  font-size: 18px;
  font-weight: 600;
  color: var(--dark);
}

.player-stats {
  display: flex;
  gap: 16px;
}

.stat-item {
  font-size: 14px;
  color: var(--gray);
}

.achievements {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.achievement-badge {
  background: var(--primary);
  color: white;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
}

/* Statistics Styles */
.stats-container {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
  height: 100%;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  border: 1px solid var(--gray-light);
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

.stat-card.fastest {
  background: linear-gradient(135deg, #FFF3CD 0%, #FFE69C 100%);
  border-color: var(--warning);
}

.stat-card.popular {
  background: linear-gradient(135deg, #FFF0F0 0%, #FFE1E1 100%);
  border-color: var(--danger);
}

.stat-card.artist {
  background: linear-gradient(135deg, #F0F9FF 0%, #DBEAFE 100%);
  border-color: var(--primary-light);
}

.stat-card.duration {
  background: linear-gradient(135deg, #F0FDF4 0%, #DCFCE7 100%);
  border-color: var(--success);
}

.stat-icon {
  font-size: 32px;
  margin-bottom: 12px;
}

.stat-content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-title {
  font-size: 14px;
  color: var(--gray);
  margin-bottom: 8px;
}

.stat-value {
  font-size: 24px;
  font-weight: 600;
  color: var(--dark);
}

.stat-subtitle {
  font-size: 12px;
  color: var(--gray);
}

/* Action Footer */
.action-footer {
  padding: 24px 32px;
  border-top: 1px solid var(--gray-light);
  display: flex;
  justify-content: center;
  gap: 20px;
}

.action-btn {
  background: white;
  border: none;
  border-radius: 12px;
  padding: 12px 24px;
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 16px;
  font-weight: 500;
  min-width: 140px;
  justify-content: center;
}

.action-btn.primary {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
}

.action-btn.primary:hover {
  background: linear-gradient(135deg, var(--primary-dark) 0%, var(--primary) 100%);
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(79, 70, 229, 0.4);
}

.action-btn.secondary {
  background: white;
  color: var(--dark);
  border: 1px solid var(--gray-light);
}

.action-btn.secondary:hover {
  background: var(--primary-lightest);
  border-color: var(--primary-light);
  transform: translateY(-2px);
}

.btn-icon {
  font-size: 18px;
}

/* Responsive Design */
@media (max-width: 1200px) {
  .main-content {
    flex-direction: column;
    height: auto;
  }

  .ranking-panel,
  .stats-panel {
    flex: none;
    min-height: 300px;
  }

  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .final-container {
    width: 95%;
    height: auto;
    max-height: none;
    min-height: 90vh;
  }

  .final-header {
    flex-direction: column;
    gap: 16px;
    text-align: center;
  }

  .game-info {
    flex-direction: row;
    justify-content: center;
  }

  .main-content {
    padding: 16px;
  }

  .stats-grid {
    grid-template-columns: 1fr;
    gap: 12px;
  }

  .action-footer {
    flex-direction: column;
    align-items: center;
  }

  .action-btn {
    width: 100%;
    max-width: 200px;
  }

  .ranking-item {
    flex-wrap: wrap;
    gap: 12px;
  }

  .achievements {
    width: 100%;
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .final-header h1 {
    font-size: 1.5rem;
  }

  .title-icon {
    width: 40px;
    height: 40px;
    font-size: 20px;
  }

  .player-stats {
    flex-direction: column;
    gap: 4px;
  }

  .stat-value {
    font-size: 20px;
  }
}
</style>