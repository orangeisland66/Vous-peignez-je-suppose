<template>
  <div class="settings-background">
    <div class="settings-container">
      <!-- Header -->
      <header class="settings-header">
        <div class="logo-container">
          <div class="logo-icon"></div>
          <h1>用户设置</h1>
        </div>
        <button @click="goBack" class="back-btn">
          <span class="btn-icon">←</span>
          <span>返回大厅</span>
        </button>
      </header>

      <!-- Main Content Area -->
      <div class="main-content">
        <!-- Left Panel - Categories -->
        <aside class="category-panel">
          <button v-for="(section, index) in sections" :key="index" @click="activeSection = index"
            :class="['category-card', { active: activeSection === index }]">
            <div class="category-icon">{{ section.icon }}</div>
            <span class="category-text">{{ section.title }}</span>
          </button>
        </aside>

        <!-- Right Panel - Settings Content -->
        <section class="settings-panel">
          <div class="panel-header">
            <h2>{{ sections[activeSection].title }}</h2>
          </div>

          <div class="settings-list-container">
            <!-- Personal Info -->
            <div v-if="activeSection === 0" class="settings-section">
              <div class="settings-item">
                <div class="item-label">昵称</div>
                <div class="item-content">
                  <div class="item-value">{{ user.nickname }}</div>
                  <button @click="editNickname" class="edit-btn">
                    <span class="btn-icon">✎</span>
                    <span>编辑</span>
                  </button>
                </div>
              </div>

              <div class="settings-item">
                <div class="item-label">头像</div>
                <div class="item-content">
                  <div class="avatar-preview">
                    <div class="avatar">{{ user.nickname.charAt(0) }}</div>
                  </div>
                  <button @click="changeAvatar" class="edit-btn">
                    <span class="btn-icon">⟳</span>
                    <span>更换</span>
                  </button>
                </div>
              </div>
            </div>

            <!-- Notifications -->
            <div v-if="activeSection === 1" class="settings-section">
              <div class="settings-item toggle">
                <div class="item-label">接收新消息提醒</div>
                <div class="item-content">
                  <div class="toggle-switch" @click="settings.notifyMessage = !settings.notifyMessage">
                    <div :class="['toggle-slider', { 'active': settings.notifyMessage }]"></div>
                  </div>
                </div>
              </div>

              <div class="settings-item toggle">
                <div class="item-label">画画倒计时声音</div>
                <div class="item-content">
                  <div class="toggle-switch" @click="settings.soundDrawTimer = !settings.soundDrawTimer">
                    <div :class="['toggle-slider', { 'active': settings.soundDrawTimer }]"></div>
                  </div>
                </div>
              </div>

              <div class="settings-item toggle">
                <div class="item-label">猜词提示音</div>
                <div class="item-content">
                  <div class="toggle-switch" @click="settings.soundGuessHint = !settings.soundGuessHint">
                    <div :class="['toggle-slider', { 'active': settings.soundGuessHint }]"></div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Privacy & Security -->
            <div v-if="activeSection === 2" class="settings-section">
              <div class="settings-item toggle">
                <div class="item-label">自动加入公开房间</div>
                <div class="item-content">
                  <div class="toggle-switch" @click="settings.autoJoin = !settings.autoJoin">
                    <div :class="['toggle-slider', { 'active': settings.autoJoin }]"></div>
                  </div>
                </div>
              </div>

              <div class="settings-item">
                <div class="item-label">账户安全</div>
                <div class="item-content">
                  <button @click="changePassword" class="action-btn">
                    <span>修改密码</span>
                  </button>
                </div>
              </div>
            </div>

            <!-- Language & Region -->
            <div v-if="activeSection === 3" class="settings-section">
              <div class="settings-item">
                <div class="item-label">界面语言</div>
                <div class="item-content">
                  <select v-model="settings.language" class="select-input">
                    <option value="zh-CN">简体中文</option>
                    <option value="en-US">English</option>
                  </select>
                </div>
              </div>

              <div class="settings-item">
                <div class="item-label">时区设置</div>
                <div class="item-content">
                  <select v-model="settings.timezone" class="select-input">
                    <option value="Asia/Shanghai">亚洲/上海</option>
                    <option value="Asia/Taipei">亚洲/台北</option>
                    <option value="UTC">UTC</option>
                  </select>
                </div>
              </div>
            </div>

            <!-- About & Help -->
            <div v-if="activeSection === 4" class="settings-section">
              <div class="settings-item">
                <div class="item-label">使用指南</div>
                <div class="item-content">
                  <button @click="viewGuide" class="action-btn">
                    <span>查看教程</span>
                  </button>
                </div>
              </div>

              <div class="settings-item">
                <div class="item-label">反馈问题</div>
                <div class="item-content">
                  <button @click="contactSupport" class="action-btn">
                    <span>联系客服</span>
                  </button>
                </div>
              </div>

              <div class="settings-item">
                <div class="item-label">版本信息</div>
                <div class="item-content">
                  <div class="version-badge">{{ version }}</div>
                </div>
              </div>
            </div>
          </div>
        </section>
      </div>

      <!-- Footer Actions -->
      <footer class="settings-footer">
        <button @click="save" class="save-btn">保存设置</button>
        <button @click="reset" class="reset-btn">恢复默认</button>
      </footer>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Settings',
  data() {
    return {
      activeSection: 0,
      sections: [
        { title: '个人信息', icon: '👤' },
        { title: '通知与声音', icon: '🔔' },
        { title: '隐私与安全', icon: '🔒' },
        { title: '语言和地区', icon: '🌐' },
        { title: '关于与帮助', icon: 'ℹ️' }
      ],
      user: {
        nickname: 'Alice',
        avatarUrl: '/default-avatar.png'
      },
      settings: {
        notifyMessage: true,
        soundDrawTimer: true,
        soundGuessHint: true,
        autoJoin: false,
        language: 'zh-CN',
        timezone: 'Asia/Taipei'
      },
      version: '1.0.0'
    }
  },
  methods: {
    goBack() {
      this.$router.push('/lobby')
    },
    editNickname() {
      this.$router.push('/profile')
    },
    changeAvatar() {
      /* 弹出更换头像 */
    },
    changePassword() {
      this.$router.push('/change-password')
    },
    viewGuide() {
      /* 打开指南链接 */
    },
    contactSupport() {
      /* 打开反馈表单 */
    },
    save() {
      // 调用 API 保存
      console.log('保存', this.settings)
      this.$toast?.success('设置已保存')
    },
    reset() {
      if (confirm('确定要重置所有设置为默认值吗？')) {
        Object.assign(this.settings, {
          notifyMessage: true,
          soundDrawTimer: true,
          soundGuessHint: true,
          autoJoin: false,
          language: 'zh-CN',
          timezone: 'Asia/Taipei'
        })
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

/* Base and Layout Styles */
.settings-background {
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

.settings-container {
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
.settings-header {
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
  content: "⚙️";
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  font-size: 20px;
}

.settings-header h1 {
  font-size: 1.75rem;
  font-weight: 600;
  color: var(--primary);
  margin: 0;
}

.back-btn {
  display: flex;
  align-items: center;
  background: var(--primary-lightest);
  padding: 8px 16px;
  border-radius: 50px;
  border: none;
  color: var(--primary-dark);
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 2px 10px rgba(79, 70, 229, 0.1);
}

.back-btn:hover {
  background: var(--primary-light);
  color: white;
}

.back-btn .btn-icon {
  margin-right: 8px;
}

/* Main Content Layout */
.main-content {
  display: flex;
  flex: 1;
  padding: 24px;
  gap: 24px;
  overflow: hidden;
}

/* Category Panel Styles */
.category-panel {
  width: 240px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  flex-shrink: 0;
}

.category-card {
  background: white;
  border: none;
  border-radius: 16px;
  padding: 16px;
  display: flex;
  align-items: center;
  cursor: pointer;
  transition: all 0.3s ease;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid var(--gray-light);
}

.category-card.active {
  background: var(--primary);
  color: white;
  border: 1px solid var(--primary);
}

.category-card:hover:not(.active) {
  transform: translateX(5px);
  border-color: var(--primary-light);
}

.category-icon {
  font-size: 20px;
  margin-right: 12px;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.category-text {
  font-weight: 600;
  font-size: 16px;
}

/* Settings Panel Styles */
.settings-panel {
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
  flex-shrink: 0;
}

.panel-header h2 {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--dark);
  margin: 0;
}

.settings-list-container {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  scrollbar-width: thin;
  scrollbar-color: var(--primary-light) var(--gray-light);
}

/* Custom scrollbar styles */
.settings-list-container::-webkit-scrollbar {
  width: 8px;
}

.settings-list-container::-webkit-scrollbar-track {
  background: var(--gray-light);
  border-radius: 4px;
}

.settings-list-container::-webkit-scrollbar-thumb {
  background: var(--primary-light);
  border-radius: 4px;
}

.settings-list-container::-webkit-scrollbar-thumb:hover {
  background: var(--primary);
}

/* Settings Items */
.settings-section {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.settings-item {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
  gap: 16px;
  transition: all 0.2s ease;
  border: 1px solid var(--gray-light);
}

.settings-item:hover {
  border-color: var(--primary-light);
  box-shadow: 0 8px 20px rgba(79, 70, 229, 0.15);
}

.item-label {
  font-weight: 600;
  color: var(--dark);
  font-size: 16px;
}

.item-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.item-value {
  color: var(--gray);
  font-size: 16px;
}

.edit-btn {
  background: var(--primary-lightest);
  color: var(--primary);
  border: none;
  padding: 8px 16px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.edit-btn:hover {
  background: var(--primary);
  color: white;
}

.action-btn {
  background: var(--primary);
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 8px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-btn:hover {
  background: var(--primary-dark);
  transform: translateY(-2px);
}

/* Toggle Switch */
.toggle-switch {
  width: 50px;
  height: 26px;
  background-color: var(--gray-light);
  border-radius: 13px;
  position: relative;
  cursor: pointer;
  transition: background-color 0.3s ease;
}

.toggle-switch .toggle-slider {
  position: absolute;
  top: 3px;
  left: 3px;
  width: 20px;
  height: 20px;
  background-color: white;
  border-radius: 50%;
  transition: transform 0.3s ease;
}

.toggle-switch .toggle-slider.active {
  transform: translateX(24px);
}

.toggle-switch:has(.toggle-slider.active) {
  background-color: var(--primary);
}

/* Form Elements */
.select-input {
  width: 100%;
  padding: 10px;
  border-radius: 8px;
  border: 1px solid var(--gray-light);
  background-color: white;
  color: var(--dark);
  font-size: 16px;
  outline: none;
  transition: border-color 0.2s ease;
}

.select-input:focus {
  border-color: var(--primary);
}

/* Avatar */
.avatar-preview {
  display: flex;
  align-items: center;
}

.avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: var(--primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 20px;
}

/* Version Badge */
.version-badge {
  background: var(--primary-lightest);
  color: var(--primary);
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 500;
}

/* Footer */
.settings-footer {
  display: flex;
  justify-content: center;
  gap: 16px;
  padding: 20px;
  border-top: 1px solid var(--gray-light);
}

.save-btn {
  background: var(--primary);
  color: white;
  border: none;
  padding: 12px 32px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.save-btn:hover {
  background: var(--primary-dark);
  transform: translateY(-2px);
}

.reset-btn {
  background: white;
  color: var(--gray);
  border: 1px solid var(--gray-light);
  padding: 12px 32px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.reset-btn:hover {
  color: var(--danger);
  border-color: var(--danger);
}

/* Responsive Adjustments */
@media (max-width: 992px) {
  .main-content {
    flex-direction: column;
  }

  .category-panel {
    width: 100%;
    flex-direction: row;
    overflow-x: auto;
    padding-bottom: 8px;
  }

  .category-card {
    min-width: 120px;
    flex-direction: column;
    text-align: center;
    gap: 8px;
  }

  .category-icon {
    margin-right: 0;
  }

  .settings-panel {
    min-height: 400px;
  }
}

@media (max-width: 768px) {
  .settings-container {
    width: 95%;
    height: auto;
    max-height: none;
    min-height: 90vh;
  }

  .settings-header {
    flex-direction: column;
    gap: 16px;
  }

  .back-btn {
    align-self: flex-start;
  }

  .item-content {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .settings-item.toggle .item-content {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    width: 100%;
  }

  .settings-footer {
    flex-direction: column;
  }
}
</style>