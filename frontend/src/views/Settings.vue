<template>
  <div class="settings-outer">
    <div class="settings-container">
      <!-- Header -->
      <header class="settings-header">
        <h2>设置</h2>
      </header>

      <!-- Content -->
      <section class="settings-content">
        <!-- Personal Info -->
        <div class="section">
          <h3>个人信息</h3>
          <div class="field-group">
            <div class="field-item">
              <label>昵称：</label>
              <span>{{ user.nickname }}</span>
              <button @click="editNickname" class="btn link">编辑</button>
            </div>
            <div class="field-item">
              <label>头像：</label>
              <img :src="user.avatarUrl" alt="avatar" class="avatar" />
              <button @click="changeAvatar" class="btn link">更换</button>
            </div>
          </div>
        </div>

        <!-- Notifications -->
        <div class="section">
          <h3>通知与声音</h3>
          <div class="field-group vertical">
            <div><input type="checkbox" v-model="settings.notifyMessage" /> 接收新消息提醒</div>
            <div><input type="checkbox" v-model="settings.soundDrawTimer" /> 画画倒计时声音</div>
            <div><input type="checkbox" v-model="settings.soundGuessHint" /> 猜词提示音</div>
          </div>
        </div>

        <!-- Privacy & Security -->
        <div class="section">
          <h3>隐私与安全</h3>
          <div class="field-group">
            <div class="field-item">
              <label>自动加入公开房间：</label>
              <button @click="settings.autoJoin ^= true" class="btn toggle">
                {{ settings.autoJoin ? '开' : '关' }}
              </button>
            </div>
            <div class="field-item">
              <label>密码修改：</label>
              <button @click="changePassword" class="btn link">修改密码</button>
            </div>
          </div>
        </div>

        <!-- Language & Region -->
        <div class="section">
          <h3>语言和地区</h3>
          <div class="field-group">
            <div class="field-item">
              <label>语言：</label>
              <select v-model="settings.language">
                <option value="zh-CN">简体中文</option>
                <option value="en-US">English</option>
              </select>
            </div>
            <div class="field-item">
              <label>时区：</label>
              <select v-model="settings.timezone">
                <option value="Asia/Taipei">Asia/Taipei</option>
                <option value="UTC">UTC</option>
              </select>
            </div>
          </div>
        </div>

        <!-- About & Help -->
        <div class="section">
          <h3>关于与帮助</h3>
          <div class="field-group vertical">
            <div><button @click="viewGuide" class="btn link">使用指南</button></div>
            <div><button @click="contactSupport" class="btn link">反馈问题</button></div>
            <div>版本号：{{ version }}</div>
          </div>
        </div>
      </section>

      <!-- Actions -->
      <footer class="action-buttons">
        <button class="btn primary" @click="save">保存设置</button>
        <button class="btn secondary" @click="reset">重置默认</button>
      </footer>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Settings',
  data() {
    return {
      user: { nickname: 'Alice', avatarUrl: '/default-avatar.png' },
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
    editNickname() { this.$router.push('/profile') },
    changeAvatar() { /* 弹出更换头像 */ },
    changePassword() { this.$router.push('/change-password') },
    viewGuide() { /* 打开指南链接 */ },
    contactSupport() { /* 打开反馈表单 */ },
    save() {
      // 调用 API 保存
      console.log('保存', this.settings)
      alert('设置已保存')
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
.settings-outer {
  width: 100vw;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f5f5;
}
.settings-container {
  width: 80vw;
  max-width: 1000px;
  aspect-ratio: 16/9;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  display: flex;
  flex-direction: column;
  padding: 24px;
  box-sizing: border-box;
}
.settings-header h2 {
  text-align: center;
  font-size: 2rem;
  margin: 0 0 16px;
  color: #333;
}
.settings-content {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 24px;
}
.section {
  background: #fff;
  padding: 16px;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
.section h3 {
  margin: 0 0 12px;
  color: #e60000;
}
.field-group {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
}
.field-group.vertical {
  flex-direction: column;
}
.field-item {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
}
.field-item label { font-weight: bold; color: #333; width: 120px; }
.field-item select { flex: 1; padding: 6px 12px; }
.field-item input[type="checkbox"] { transform: scale(1.2); }
.avatar { width: 40px; height: 40px; border-radius: 50%; }
.btn.link { background: none; border: none; color: #e60000; cursor: pointer; }
.btn.toggle { background: #f0f0f0; border: 1px solid #ccc; padding: 4px 12px; border-radius: 4px; }

.action-buttons {
  display: flex;
  justify-content: center;
  gap: 24px;
  margin-top: 16px;
}
.btn {
  padding: 10px 24px;
  font-size: 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
.primary { background: #e60000; color: #fff; }
.secondary { background: #f0f0f0; color: #333; }
.secondary:hover { background: #ddd; }

@media (max-width: 900px) {
  .settings-container { padding: 16px; }
  .field-item { flex-direction: column; align-items: flex-start; }
  .field-item label { width: auto; }
  .action-buttons { flex-direction: column; gap: 12px; }
}
</style>
