<template>
  <div class="final-outer">
    <div class="final-container">
      <!-- Header -->
      <header class="final-header">
        <h2>游戏结束</h2>
        <div class="game-info">
          <span>房间号：{{ roomId }}</span>
          <span>总回合数：{{ totalRounds }}</span>
        </div>
      </header>

      <!-- Content -->
      <section class="final-content">
        <!-- Ranking -->
        <div class="ranking-section">
          <h3>最终排名</h3>
          <ul class="ranking-list">
            <li v-for="(player, idx) in sortedPlayers" :key="player.id" :class="['ranking-item', { 'top-three': idx < 3 }]">
              <div class="rank-number">{{ idx + 1 }}</div>
              <img :src="player.avatarUrl || '/default-avatar.png'" alt="avatar" class="avatar" />
              <div class="player-info">
                <span class="username">{{ player.username }}</span>
                <span class="stats">总分: {{ player.totalScore }} | 猜中: {{ player.correctGuesses }}</span>
              </div>
              <div class="badges">
                <span v-for="ach in player.achievements" :key="ach.id" class="badge" :title="ach.description">
                  {{ ach.name }}
                </span>
              </div>
            </li>
          </ul>
        </div>

        <!-- Statistics -->
        <div class="stats-section">
          <h3>游戏统计</h3>
          <div class="stats-grid">
            <div class="stat-card">
              <h4>最快猜对</h4>
              <div class="value">{{ stats.fastestGuess }} 秒</div>
              <div class="label">{{ stats.fastestGuessPlayer }}</div>
            </div>
            <div class="stat-card">
              <h4>最受欢迎词</h4>
              <div class="value">{{ stats.mostPopularWord }}</div>
              <div class="label">出现 {{ stats.mostPopularWordCount }} 次</div>
            </div>
            <div class="stat-card">
              <h4>最佳画手</h4>
              <div class="value">{{ stats.bestArtist }}</div>
              <div class="label">得分 {{ stats.bestArtistScore }}</div>
            </div>
            <div class="stat-card">
              <h4>游戏时长</h4>
              <div class="value">{{ stats.gameDuration }} 分钟</div>
            </div>
          </div>
        </div>
      </section>

      <!-- Actions -->
      <footer class="action-buttons">
        <button class="btn primary" @click="playAgain">再来一局</button>
        <button class="btn secondary" @click="backToLobby">返回大厅</button>
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
.final-outer {
  width: 100vw;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f5f5;
}
.final-container {
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
.final-header h2 {
  text-align: center;
  font-size: 2rem;
  margin: 0;
}
.game-info {
  display: flex;
  justify-content: center;
  gap: 32px;
  margin-top: 8px;
  color: #555;
}

/* Content */
.final-content {
  flex: 1;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  margin: 24px 0;
}

/* Ranking Section */
.ranking-section {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  padding: 16px;
}
.ranking-list {
  list-style: none;
  margin: 0;
  padding: 0;
}
.ranking-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 12px;
  border-bottom: 1px solid #eee;
}
.ranking-item.top-three {
  background: #ffece6;
}
.rank-number {
  font-size: 1.5rem;
  color: #e60000;
  width: 40px;
  text-align: center;
}
.avatar {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  object-fit: cover;
}
.player-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.username {
  font-size: 1.2rem;
  font-weight: bold;
  color: #333;
}
.stats {
  font-size: 0.9rem;
  color: #777;
}
.badges {
  display: flex;
  gap: 8px;
}
.badge {
  background: #e60000;
  color: #fff;
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 0.8rem;
}

/* Stats Section */
.stats-section {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  padding: 16px;
}
.stats-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
  margin-top: 16px;
}
.stat-card {
  background: #f9f9f9;
  border-radius: 8px;
  padding: 12px;
  text-align: center;
}
.stat-card h4 {
  margin: 0;
  font-size: 1rem;
  color: #333;
}
.value {
  font-size: 1.2rem;
  font-weight: bold;
  margin: 8px 0;
  color: #e60000;
}
.label {
  font-size: 0.9rem;
  color: #555;
}

/* Actions */
.action-buttons {
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
.secondary {
  background: #f0f0f0;
  color: #333;
}
.secondary:hover {
  background: #ddd;
}

/* Responsive */
@media (max-width: 900px) {
  .final-container { padding: 16px; }
  .game-info { flex-direction: column; gap: 12px; }
  .final-content { grid-template-columns: 1fr; }
  .action-buttons { flex-direction: column; gap: 12px; }
}
</style>
