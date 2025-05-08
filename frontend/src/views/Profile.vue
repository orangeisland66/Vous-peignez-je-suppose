<template>
  <div class="profile-outer">
    <div class="profile-container">
      <!-- Header -->
      <header class="profile-header">
        <h2>个人中心</h2>
      </header>

      <!-- Content -->
      <section class="profile-content">
        <!-- 个人信息 -->
        <div class="profile-section personal-info">
          <h3>个人信息</h3>
          <div class="info-grid">
            <div class="info-item">
              <span class="label">用户名</span>
              <span class="value">{{ user.username }}</span>
            </div>
            <div class="info-item">
              <span class="label">邮箱</span>
              <span class="value">{{ user.email }}</span>
            </div>
            <div class="info-item">
              <span class="label">注册时间</span>
              <span class="value">{{ formatDate(user.createdAt) }}</span>
            </div>
          </div>
        </div>

        <!-- 游戏统计 -->
        <div class="profile-section game-stats">
          <h3>游戏统计</h3>
          <div class="stats-grid">
            <div class="stat-card">
              <div class="stat-value">{{ stats.totalGames }}</div>
              <div class="stat-label">总游戏数</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{{ stats.wins }}</div>
              <div class="stat-label">胜利次数</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{{ stats.totalScore }}</div>
              <div class="stat-label">总得分</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{{ stats.rank }}</div>
              <div class="stat-label">排名</div>
            </div>
          </div>
        </div>

        <!-- 最近游戏记录 -->
        <div class="profile-section recent-games">
          <h3>最近游戏记录</h3>
          <div class="history-list">
            <div v-for="game in recentGames" :key="game.id" class="history-item">
              <div class="history-info">
                <span class="date">{{ formatDate(game.date) }}</span>
                <span class="result" :class="{ win: game.result==='win' }">
                  {{ game.result==='win' ? '胜利' : '失败' }}
                </span>
              </div>
              <div class="history-details">
                <span>得分: {{ game.score }}</span>
                <span>难度: {{ game.difficulty }}</span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Actions -->
      <footer class="profile-actions">
        <button class="btn primary" @click="$router.push('/lobby')">返回大厅</button>
        <button class="btn secondary" @click="$router.push('/settings')">设置</button>
      </footer>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Profile',
  data() {
    return {
      user: { username: '', email: '', createdAt: '' },
      stats: { totalGames: 0, wins: 0, totalScore: 0, rank: 0 },
      recentGames: []
    }
  },
  async created() {
    await Promise.all([this.fetchUser(), this.fetchStats(), this.fetchRecent()])
  },
  methods: {
    async fetchUser() {
      try {
        const res = await fetch('/api/user/profile')
        if (!res.ok) throw new Error()
        this.user = await res.json()
      } catch {}
    },
    async fetchStats() {
      try {
        const res = await fetch('/api/user/stats')
        if (!res.ok) throw new Error()
        this.stats = await res.json()
      } catch {}
    },
    async fetchRecent() {
      try {
        const res = await fetch('/api/user/games')
        if (!res.ok) throw new Error()
        this.recentGames = await res.json()
      } catch {}
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString('zh-CN', { year:'numeric', month:'2-digit', day:'2-digit' })
    }
  }
}
</script>

<style scoped>
.profile-outer {
  width:100vw; height:100vh;
  display:flex; align-items:center; justify-content:center;
  background:#f5f5f5;
}
.profile-container {
  width:85vw; max-width:1200px; aspect-ratio:16/9;
  background:#fff; border-radius:12px;
  box-shadow:0 4px 12px rgba(0,0,0,0.1);
  display:flex; flex-direction:column;
  padding:24px; box-sizing:border-box;
}
.profile-header h2 { text-align:center; font-size:2rem; color:#333; margin:0 }
.profile-content {
  flex:1; overflow-y:auto;
  display:flex; flex-direction:column; gap:24px;
}
.profile-section {
  background:#fff; border-radius:8px;
  box-shadow:0 2px 4px rgba(0,0,0,0.1);
  padding:16px;
}
.profile-section h3 { margin:0 0 12px; color:#e60000 }

/* Personal Info */
.info-grid { display:grid; grid-template-columns: repeat(auto-fit,minmax(200px,1fr)); gap:16px }
.info-item { display:flex; justify-content:space-between; padding:8px; background:#f9f9f9; border-radius:4px }
.info-item .label { font-weight:bold; color:#333 }
.info-item .value { color:#555 }

/* Game Stats */
.stats-grid { display:grid; grid-template-columns: repeat(auto-fit,minmax(150px,1fr)); gap:16px }
.stat-card { text-align:center; padding:16px; background:#f9f9f9; border-radius:6px }
.stat-value { font-size:1.5rem; font-weight:bold; color:#e60000 }
.stat-label { color:#555 }

/* History */
.history-list { display:grid; gap:12px }
.history-item { padding:12px; background:#f9f9f9; border-radius:4px }
.history-info { display:flex; justify-content:space-between; margin-bottom:8px }
.history-info .date { color:#555 }
.history-info .result { font-weight:bold }
.history-info .result.win { color:#28a745 }
.history-item .history-details { display:flex; gap:20px; color:#555 }

/* Actions */
.profile-actions { display:flex; justify-content:center; gap:24px; margin-top:16px }
.btn { padding:10px 24px; font-size:1rem; border:none; border-radius:4px; cursor:pointer }
.primary { background:#e60000; color:#fff }
.secondary { background:#f0f0f0; color:#333 }
.secondary:hover { background:#ddd }

@media(max-width:900px) {
  .profile-container { padding:16px }
  .profile-content { gap:16px }
  .profile-actions { flex-direction:column }
}
</style>
