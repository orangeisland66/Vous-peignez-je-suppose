<template>
  <div class="profile-background">
    <div class="profile-container">
      <!-- Header -->
      <header class="profile-header">
        <div class="header-content">
          <div class="logo-icon">👤</div>
          <h1>个人中心</h1>
        </div>
        <div class="user-info">
          <div class="avatar">{{ user.username?.charAt(0) || '?' }}</div>
          <span class="username">{{ user.username || '未登录' }}</span>
        </div>
      </header>

      <!-- Main Content Area -->
      <div class="main-content">
        <!-- 个人信息 -->
        <section class="info-section">
          <div class="section-header">
            <div class="section-title">
              <div class="section-icon">📋</div>
              <h2>个人信息</h2>
            </div>
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

        <!-- 游戏统计 -->
        <section class="stats-section">
          <div class="section-header">
            <div class="section-title">
              <div class="section-icon">📊</div>
              <h2>游戏统计</h2>
            </div>
          </div>
          <div class="section-content stats-grid">
            <div class="stat-card" v-for="(value, key) in statsMap" :key="key">
              <div class="stat-icon">{{ value.icon }}</div>
              <div class="stat-info">
                <span class="stat-label">{{ value.label }}</span>
                <span class="stat-value">{{ value.value }}</span>
              </div>
            </div>
          </div>
        </section>

        <!-- 最近游戏 -->
        <section class="history-section">
          <div class="section-header">
            <div class="section-title">
              <div class="section-icon">🎮</div>
              <h2>最近游戏记录</h2>
            </div>
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

      <!-- Footer -->
      <footer class="profile-actions">
        <button @click="goToLobby" class="action-btn lobby">返回大厅</button>
        <button @click="logout" class="action-btn logout">退出登录</button>
      </footer>
    </div>
  </div>
</template>

<script>
import { useRouter } from 'vue-router';

export default {
  name: 'Profile',
  data() {
    return {
      user: { username: '', email: '', createdAt: '' },
      stats: { totalGames: 0, wins: 0, totalScore: 0, rank: 0 },
      recentGames: [],
      currentUserId: null
    }
  },
  computed: {
    statsMap() {
      return {
        totalGames: { icon: '🎮', label: '总游戏数：', value: this.stats.totalGames },
        wins: { icon: '🏆', label: '胜利次数：', value: this.stats.wins },
        totalScore: { icon: '⭐', label: '总得分：', value: this.stats.totalScore },
        rank: { icon: '📊', label: '排名：', value: this.stats.rank }
      };
    }
  },
  async created() {
    const userIdString = localStorage.getItem('userId');
    if (userIdString) {
      this.currentUserId = parseInt(userIdString);
    }
    await Promise.all([
      this.fetchUser(this.currentUserId),
      this.fetchStats(),
      this.fetchRecent()
    ]);
  },
  methods: {
    async fetchUser(userId) {
      try {
        const res = await fetch(`/api/users/profile?userId=${userId}`);
        if (!res.ok) throw new Error();
        this.user = await res.json();
      } catch (e) {
        console.error(e);
        this.$toast?.error('获取用户信息失败');
      }
    },
    async fetchStats() {
      try {
        const res = await fetch('/api/users/stats');
        if (!res.ok) throw new Error();
        this.stats = await res.json();
      } catch (e) {
        console.error(e);
        this.$toast?.error('获取统计信息失败');
      }
    },
    async fetchRecent() {
      try {
        const res = await fetch('/api/users/games');
        if (!res.ok) throw new Error();
        this.recentGames = await res.json();
      } catch (e) {
        console.error(e);
        this.$toast?.error('获取游戏记录失败');
      }
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit' });
    },
    logout() {
      const router = useRouter();
      this.$store.dispatch('user/logout', router);
    },
    goToLobby() {
      this.$router.push('/lobby');
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

.profile-background {
  background: linear-gradient(135deg, var(--light) 0%, var(--primary-lightest) 100%);
  min-height: 100vh;
  height: 100vh;
  width: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  color: var(--dark);
  padding: 24px;
  box-sizing: border-box;
}

.profile-container {
  width: 100%;
  max-width: 1200px;
  height: 100%;
  max-height: calc(100vh - 48px);
  background: white;
  border-radius: 24px;
  box-shadow: 0 10px 30px rgba(79, 70, 229, 0.1);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.profile-header {
  background: var(--primary);
  padding: 20px 32px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: white;
  flex-shrink: 0;
}

.header-content {
  display: flex;
  align-items: center;
}

.header-content h1 {
  color: var(--primary-dark);
}

.logo-icon {
  width: 40px;
  height: 40px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 10px;
  margin-right: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
}

.profile-header h1 {
  font-size: 24px;
  font-weight: 600;
  margin: 0;
}

.user-info {
  display: flex;
  align-items: center;
  background: rgba(255, 255, 255, 0.15);
  padding: 8px 16px;
  border-radius: 50px;
  backdrop-filter: blur(10px);
}

.avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--primary);
  color: var(--primary-dark);
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 16px;
  margin-right: 10px;
}

.username {
  font-weight: 500;
  color: #4338CA;
}

.main-content {
  flex: 1;
  overflow-y: auto;
  padding: 32px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* Section styles */
.info-section,
.stats-section,
.history-section {
  background: var(--light);
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  flex-shrink: 0;
}

.section-header {
  padding: 20px 24px 0;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.section-title {
  display: flex;
  align-items: center;
}

.section-icon {
  width: 36px;
  height: 36px;
  background: var(--primary-lightest);
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  margin-right: 12px;
}

.section-title h2 {
  font-size: 18px;
  font-weight: 600;
  color: var(--primary-dark);
  margin: 0;
}

.games-count {
  font-size: 14px;
  color: var(--gray);
  background: white;
  padding: 4px 12px;
  border-radius: 12px;
  border: 1px solid var(--gray-light);
}

.section-content {
  padding: 24px;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
}

.info-item {
  background: white;
  padding: 20px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  border: 1px solid var(--gray-light);
  transition: all 0.2s ease;
}

.info-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  border-color: var(--primary-light);
}

.info-label {
  width: 80px;
  color: var(--gray);
  font-weight: 500;
  font-size: 14px;
}

.info-value {
  font-weight: 600;
  color: var(--primary);
  flex: 1;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
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
  transition: all 0.3s ease;
  cursor: pointer;
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
  border-color: var(--primary);
}

.stat-icon {
  font-size: 2.5rem;
  margin-bottom: 12px;
}

.stat-info {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.stat-label {
  color: var(--gray);
  font-size: 14px;
}

.stat-value {
  color: var(--primary);
  font-weight: 600;
  font-size: 18px;
}

.no-history {
  text-align: center;
  padding: 40px 20px;
  color: var(--gray);
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 16px;
}

.history-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.history-item+.history-item {
  margin-top: 16px;
}

.history-card {
  padding: 20px;
  border: 1px solid var(--gray-light);
  border-radius: 12px;
  background: white;
  transition: all 0.2s ease;
}

.history-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  border-color: var(--primary-light);
}

.history-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.game-date {
  font-size: 14px;
  color: var(--gray);
}

.game-result {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
  background: var(--gray-light);
  color: var(--gray);
}

.game-result.win {
  background: rgba(34, 197, 94, 0.1);
  color: var(--success);
}

.history-details {
  display: flex;
  gap: 24px;
}

.detail-item {
  display: flex;
  gap: 8px;
  font-size: 14px;
}

.detail-label {
  color: var(--gray);
}

.detail-value {
  font-weight: 500;
  color: var(--dark);
}

.profile-actions {
  padding: 24px 32px;
  border-top: 1px solid var(--gray-light);
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  background: var(--light);
  flex-shrink: 0;

}

.action-btn {
  padding: 12px 24px;
  border-radius: 12px;
  border: none;
  font-weight: 500;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.action-btn:hover {
  transform: translateY(-2px);
}

.logout {
  background: var(--danger);
  color: var(--warning);
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.2);
}

.logout:hover {
  background: #DC2626;
  color: white;
  box-shadow: 0 6px 16px rgba(239, 68, 68, 0.3);
}

.lobby {
  background: var(--primary);
  color: var(--primary-light);
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.2);
}

.lobby:hover {
  background: var(--primary-dark);
  color: white;
  box-shadow: 0 6px 16px rgba(79, 70, 229, 0.3);
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

/* Responsive Design */
@media (max-width: 992px) {
  .main-content {
    padding: 24px;
  }

  .info-grid {
    grid-template-columns: 1fr;
  }

  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .profile-background {
    padding: 12px;
  }

  .profile-container {
    max-height: calc(100vh - 24px);
  }

  .profile-header {
    padding: 16px 20px;
    flex-direction: column;
    gap: 16px;
    align-items: flex-start;
  }

  .main-content {
    padding: 20px;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }

  .profile-actions {
    flex-direction: column;
    gap: 8px;
  }

  .action-btn {
    width: 100%;
  }
}

@media (max-width: 576px) {
  .history-details {
    flex-direction: column;
    gap: 8px;
  }

  .section-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
}
</style>