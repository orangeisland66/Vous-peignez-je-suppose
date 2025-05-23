<template>
  <div class="profile-background">
    <div class="profile-container">
      <!-- Header -->
      <header class="profile-header">
        <div class="logo-container">
          <div class="logo-icon"></div>
          <h1>个人中心</h1>
        </div>
        <div class="user-info">
          <div class="avatar">{{ user.username?.charAt(0) || '?' }}</div>
          <span class="username">{{ user.username || '未登录' }}</span>
        </div>
      </header>

      <!-- Main Content Area -->
      <div class="main-content">
        <!-- Personal Info Section -->
        <section class="info-section">
          <div class="section-header">
            <h2>个人信息</h2>
          </div>
          <div class="section-content info-grid">
            <div class="info-item">
              <span class="info-label">用户名</span>
              <span class="info-value">{{ user.username }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">邮箱</span>
              <span class="info-value">{{ user.email }}</span>
            </div>
            <div class="info-item">
              <span class="info-label">注册时间</span>
              <span class="info-value">{{ formatDate(user.createdAt) }}</span>
            </div>
          </div>
        </section>

        <!-- Game Stats Section -->
        <section class="stats-section">
          <div class="section-header">
            <h2>游戏统计</h2>
          </div>
          <div class="section-content stats-grid">
            <div class="stat-card">
              <div class="stat-icon">🎮</div>
              <div class="stat-value">{{ stats.totalGames }}</div>
              <div class="stat-label">总游戏数</div>
            </div>
            <div class="stat-card">
              <div class="stat-icon">🏆</div>
              <div class="stat-value">{{ stats.wins }}</div>
              <div class="stat-label">胜利次数</div>
            </div>
            <div class="stat-card">
              <div class="stat-icon">⭐</div>
              <div class="stat-value">{{ stats.totalScore }}</div>
              <div class="stat-label">总得分</div>
            </div>
            <div class="stat-card">
              <div class="stat-icon">📊</div>
              <div class="stat-value">{{ stats.rank }}</div>
              <div class="stat-label">排名</div>
            </div>
          </div>
        </section>

        <!-- Recent Games Section -->
        <section class="history-section">
          <div class="section-header">
            <h2>最近游戏记录</h2>
            <span class="games-count">{{ recentGames.length }} 场游戏</span>
          </div>
          <div class="section-content">
            <div v-if="recentGames.length === 0" class="no-history">
              <div class="empty-icon">🎲</div>
              <p>暂无游戏记录</p>
            </div>
            <ul v-else class="history-list">
              <li v-for="game in recentGames" :key="game.id" class="history-item">
                <div class="history-card">
                  <div class="history-header">
                    <span class="game-date">{{ formatDate(game.date) }}</span>
                    <span class="game-result" :class="{ win: game.result === 'win' }">
                      {{ game.result === 'win' ? '胜利' : '失败' }}
                    </span>
                  </div>
                  <div class="history-details">
                    <div class="detail-item">
                      <span class="detail-label">得分:</span>
                      <span class="detail-value">{{ game.score }}</span>
                    </div>
                    <div class="detail-item">
                      <span class="detail-label">难度:</span>
                      <span class="detail-value">{{ game.difficulty }}</span>
                    </div>
                  </div>
                </div>
              </li>
            </ul>
          </div>
        </section>
      </div>

      <!-- Footer Actions -->
      <footer class="profile-actions">
        <button @click="$router.push('/lobby')" class="action-btn primary">
          返回大厅
        </button>
        <button @click="$router.push('/settings')" class="action-btn secondary">
          设置
        </button>
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
      } catch (e) {
        console.error(e)
        this.$toast?.error('获取用户信息失败')
      }
    },
    async fetchStats() {
      try {
        const res = await fetch('/api/user/stats')
        if (!res.ok) throw new Error()
        this.stats = await res.json()
      } catch (e) {
        console.error(e)
        this.$toast?.error('获取统计信息失败')
      }
    },
    async fetchRecent() {
      try {
        const res = await fetch('/api/user/games')
        if (!res.ok) throw new Error()
        this.recentGames = await res.json()
      } catch (e) {
        console.error(e)
        this.$toast?.error('获取游戏记录失败')
      }
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' })
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
.profile-background {
  background: linear-gradient(135deg, #F9FAFB 0%, #EEF2FF 100%);
  min-height: 100vh;
  width: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  color: var(--dark);
  padding: 20px 0;
  overflow: hidden;
}

.profile-container {
  width: 90%;
  max-width: 1200px;
  background: white;
  border-radius: 24px;
  box-shadow: 0 10px 30px rgba(79, 70, 229, 0.1);
  overflow: hidden;
  display: flex;
  flex-direction: column;
  height: 90vh;
  max-height: 800px;
}

/* Header Styles */
.profile-header {
  padding: 20px 32px;
  background: white;
  border-bottom: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo-container {
  display: flex;
  align-items: center;
}

.logo-icon {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  border-radius: 10px;
  margin-right: 12px;
  position: relative;
}

.logo-icon::before {
  content: "👤";
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 20px;
}

.profile-header h1 {
  font-size: 1.75rem;
  font-weight: 600;
  color: var(--primary);
  margin: 0;
}

.user-info {
  display: flex;
  align-items: center;
  background: var(--primary-lightest);
  padding: 8px 16px;
  border-radius: 50px;
  box-shadow: 0 2px 10px rgba(79, 70, 229, 0.1);
}

.avatar {
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
  margin-right: 10px;
}

.username {
  font-weight: 500;
  color: var(--primary-dark);
}

/* Main Content Layout */
.main-content {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: 24px;
  gap: 24px;
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: var(--primary-light) var(--gray-light);
}

/* Custom scrollbar styles */
.main-content::-webkit-scrollbar {
  width: 8px;
}

.main-content::-webkit-scrollbar-track {
  background: var(--gray-light);
  border-radius: 4px;
}

.main-content::-webkit-scrollbar-thumb {
  background: var(--primary-light);
  border-radius: 4px;
}

.main-content::-webkit-scrollbar-thumb:hover {
  background: var(--primary);
}

/* Section Styles */
.info-section,
.stats-section,
.history-section {
  background: white;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: auto;
}

.section-header {
  padding: 16px 24px;
  border-bottom: 1px solid var(--gray-light);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.section-header h2 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--dark);
  margin: 0;
}

.games-count {
  background: var(--primary-lightest);
  color: var(--primary);
  font-size: 14px;
  font-weight: 500;
  padding: 4px 12px;
  border-radius: 50px;
}

.section-content {
  padding: 20px;
}

/* Info Grid Styles */
.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 16px;
}

.info-item {
  display: flex;
  flex-direction: column;
  background: var(--primary-lightest);
  padding: 16px;
  border-radius: 12px;
}

.info-label {
  font-size: 0.9rem;
  color: var(--gray);
  margin-bottom: 8px;
}

.info-value {
  font-size: 1.1rem;
  font-weight: 500;
  color: var(--primary-dark);
}

/* Stats Grid Styles */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 16px;
}

.stat-card {
  background: white;
  border: 1px solid var(--gray-light);
  border-radius: 16px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
  border-color: var(--primary-light);
}

.stat-icon {
  font-size: 28px;
  margin-bottom: 12px;
  height: 40px;
  width: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 12px;
  background: var(--primary-lightest);
}

.stat-value {
  font-size: 1.75rem;
  font-weight: 600;
  color: var(--primary);
  margin-bottom: 4px;
}

.stat-label {
  font-size: 0.9rem;
  color: var(--gray);
}

/* History Styles */
.no-history {
  height: 180px;
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

.history-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 16px;
}

.history-item {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  transition: all 0.2s ease;
  border: 1px solid var(--gray-light);
}

.history-item:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 20px rgba(79, 70, 229, 0.15);
  border-color: var(--primary-light);
}

.history-card {
  padding: 16px;
}

.history-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.game-date {
  font-size: 0.9rem;
  color: var(--gray);
}

.game-result {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--danger);
  background: rgba(239, 68, 68, 0.1);
  padding: 4px 12px;
  border-radius: 50px;
}

.game-result.win {
  color: var(--success);
  background: rgba(34, 197, 94, 0.1);
}

.history-details {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  padding: 6px 0;
  border-bottom: 1px dashed var(--gray-light);
}

.detail-item:last-child {
  border-bottom: none;
}

.detail-label {
  color: var(--gray);
  font-size: 0.9rem;
}

.detail-value {
  font-weight: 500;
  color: var(--dark);
}

/* Action Buttons */
.profile-actions {
  padding: 20px 32px;
  display: flex;
  justify-content: center;
  gap: 16px;
  background: white;
  border-top: 1px solid var(--gray-light);
}

.action-btn {
  padding: 12px 24px;
  border-radius: 12px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  border: none;
}

.action-btn.primary {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: white;
}

.action-btn.secondary {
  background: var(--primary-lightest);
  color: var(--primary);
}

.action-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

/* Responsive Adjustments */
@media (max-width: 992px) {

  .info-grid,
  .stats-grid,
  .history-list {
    grid-template-columns: 1fr;
  }

  .profile-container {
    height: auto;
    max-height: none;
    min-height: 90vh;
  }
}

@media (max-width: 768px) {
  .profile-container {
    width: 95%;
  }

  .profile-header {
    flex-direction: column;
    gap: 16px;
  }

  .profile-actions {
    flex-direction: column;
  }
}
</style>