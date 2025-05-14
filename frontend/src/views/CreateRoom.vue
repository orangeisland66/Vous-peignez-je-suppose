<template>
  <div class="create-room-background">
    <div class="create-room-container">
      <!-- Header -->
      <header class="page-header">
        <div class="header-content">
          <div class="title-icon">🎲</div>
          <h1>创建新房间</h1>
        </div>
        <button @click="cancel" class="back-button">
          <span class="back-icon">←</span>
          <span>返回大厅</span>
        </button>
      </header>

      <!-- Form Content -->
      <div class="form-container">
        <form class="create-room-form" @submit.prevent="handleCreate">
          <!-- Left Column - Basic Settings -->
          <div class="form-column basic-settings">
            <div class="section-title">
              <div class="section-icon">📋</div>
              <h2>基本设置</h2>
            </div>
            
            <!-- Room Name Input -->
            <div class="form-group">
              <label for="room-name">房间名称</label>
              <div class="input-wrapper">
                <input 
                  id="room-name" 
                  v-model.trim="room.name" 
                  type="text" 
                  placeholder="为您的房间起个名字..." 
                  required 
                />
              </div>
            </div>
            
            <!-- Room Capacity -->
            <div class="form-group">
              <label for="max-players">玩家数量</label>
              <div class="range-selector">
                <span class="range-value">{{ room.maxPlayers }}人</span>
                <div class="slider-container">
                  <input
                    id="max-players"
                    type="range"
                    v-model.number="room.maxPlayers"
                    min="2"
                    max="12"
                    step="2"
                    class="range-slider"
                  />
                  <div class="slider-markers">
                    <span>2</span>
                    <span>4</span>
                    <span>6</span>
                    <span>8</span>
                    <span>10</span>
                    <span>12</span>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- Game Rounds -->
            <div class="form-group">
              <label for="game-rounds">游戏回合</label>
              <div class="rounds-selector">
                <div 
                  v-for="rounds in [4, 6, 8, 10]" 
                  :key="rounds"
                  @click="room.rounds = rounds"
                  class="round-option"
                  :class="{ active: room.rounds === rounds }"
                >
                  {{ rounds }}
                </div>
              </div>
            </div>
            
            <!-- Privacy Settings -->
            <div class="form-group">
              <label>隐私设置</label>
              <div class="toggle-container">
                <div 
                  class="toggle-switch"
                  :class="{ 'is-private': room.privacy === 'private' }"
                  @click="togglePrivacy"
                >
                  <div class="toggle-handle"></div>
                </div>
                <div class="toggle-labels">
                  <span :class="{ active: room.privacy === 'public' }">公开</span>
                  <span :class="{ active: room.privacy === 'private' }">私密</span>
                </div>
              </div>
            </div>
            
            <!-- Password Field (conditional) -->
            <div v-if="room.privacy === 'private'" class="form-group password-field">
              <label for="room-password">房间密码</label>
              <div class="input-wrapper">
                <input
                  id="room-password"
                  v-model.trim="room.password"
                  type="password"
                  placeholder="设置一个密码以保护房间"
                  maxlength="12"
                />
              </div>
            </div>
          </div>
          
          <!-- Right Column - Categories -->
          <div class="form-column categories-settings">
            <div class="section-title">
              <div class="section-icon">🗂️</div>
              <h2>词库分类</h2>
            </div>
            
            <div class="categories-description">
              请选择游戏中要使用的词汇类别，至少选择一项
            </div>
            
            <div class="categories-grid">
              <div 
                v-for="cat in categories" 
                :key="cat.value"
                class="category-card"
                :class="{ selected: room.categories.includes(cat.value) }"
                @click="toggleCategory(cat.value)"
              >
                <div class="category-icon">{{ cat.icon }}</div>
                <div class="category-name">{{ cat.label }}</div>
                <div class="category-check">✓</div>
              </div>
            </div>
            
            <!-- Form Actions -->
            <div class="form-actions">
              <button type="submit" class="action-button create-button">
                <span class="btn-icon">✓</span>
                <span>创建并开始</span>
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'CreateRoom',
  data() {
    return {
      room: {
        name: '',
        maxPlayers: 6,
        rounds: 6,
        categories: [],
        privacy: 'public',
        password: ''
      },
      categories: [
        { label: '动物', value: 'animal', icon: '🐼' },
        { label: '食物', value: 'food', icon: '🍜' },
        { label: '日常', value: 'daily', icon: '☂️' },
        { label: '电影', value: 'movie', icon: '🎬' },
        { label: '地名', value: 'place', icon: '🏙️' },
        { label: '自定义', value: 'custom', icon: '✏️' }
      ]
    }
  },
  methods: {
    toggleCategory(value) {
      if (this.room.categories.includes(value)) {
        this.room.categories = this.room.categories.filter(c => c !== value);
      } else {
        this.room.categories.push(value);
      }
    },
    togglePrivacy() {
      this.room.privacy = this.room.privacy === 'public' ? 'private' : 'public';
      if (this.room.privacy === 'public') {
        this.room.password = '';
      }
    },
    async handleCreate() {
      // 基础校验
      if (!this.room.name) {
        this.$toast?.error('请输入房间名称') || alert('请输入房间名称');
        return;
      }
      if (this.room.categories.length === 0) {
        this.$toast?.error('请至少选择一个词库分类') || alert('请至少选择一个词库分类');
        return;
      }
      if (this.room.privacy === 'private' && !this.room.password) {
        this.$toast?.error('请设置房间密码') || alert('请设置房间密码');
        return;
      }
      
      try {
        // TODO: 调用后端接口创建房间并获取房间ID
        // const res = await api.createRoom(this.room)
        // this.$router.push(`/room/${res.id}/waiting`)
        
        // 示例跳转：
        this.$router.push('/room/123/waiting')
      } catch (error) {
        console.error('创建房间失败:', error);
        this.$toast?.error('创建房间失败，请重试') || alert('创建房间失败，请重试');
      }
    },
    cancel() {
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
}

/* Base Layout */
.create-room-background {
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

.create-room-container {
  width: 90%;
  max-width: 1200px;
  background: white;
  border-radius: 24px;
  box-shadow: 0 10px 30px rgba(79, 70, 229, 0.1);
  overflow: hidden;
}

/* Header Styles */
.page-header {
  background: var(--primary);
  color: white;
  padding: 20px 32px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-content {
  display: flex;
  align-items: center;
}

.title-icon {
  font-size: 24px;
  background: rgba(255, 255, 255, 0.2);
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 16px;
}

.page-header h1 {
  font-size: 24px;
  font-weight: 600;
  margin: 0;
}

.back-button {
  display: flex;
  align-items: center;
  background: rgba(255, 255, 255, 0.15);
  border: none;
  border-radius: 8px;
  color: white;
  padding: 8px 16px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.back-button:hover {
  background: rgba(255, 255, 255, 0.25);
}

.back-icon {
  margin-right: 8px;
  font-size: 16px;
}

/* Form Container */
.form-container {
  padding: 32px;
}

.create-room-form {
  display: flex;
  gap: 32px;
}

.form-column {
  flex: 1;
  background: var(--light);
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

/* Section Headers */
.section-title {
  display: flex;
  align-items: center;
  margin-bottom: 24px;
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

/* Form Controls - Basic Settings */
.form-group {
  margin-bottom: 24px;
}

.form-group label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: var(--gray);
  margin-bottom: 8px;
}

.input-wrapper {
  position: relative;
}

.input-wrapper input {
  width: 100%;
  height: 46px;
  border: 1px solid var(--gray-light);
  border-radius: 10px;
  padding: 0 16px;
  font-size: 15px;
  transition: all 0.2s ease;
  background: white;
}

.input-wrapper input:focus {
  outline: none;
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
}

/* Range Slider */
.range-selector {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.range-value {
  font-size: 16px;
  font-weight: 600;
  color: var(--primary);
}

.slider-container {
  position: relative;
}

.range-slider {
  -webkit-appearance: none;
  width: 100%;
  height: 6px;
  border-radius: 3px;
  background: var(--gray-light);
  outline: none;
  margin: 10px 0 20px 0;
}

.range-slider::-webkit-slider-thumb {
  -webkit-appearance: none;
  appearance: none;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: var(--primary);
  cursor: pointer;
  box-shadow: 0 2px 6px rgba(79, 70, 229, 0.3);
}

.slider-markers {
  display: flex;
  justify-content: space-between;
  padding: 0 10px;
  margin-top: -15px;
  font-size: 12px;
  color: var(--gray);
}

/* Rounds Selector */
.rounds-selector {
  display: flex;
  gap: 12px;
}

.round-option {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  font-weight: 600;
  background: white;
  border: 1px solid var(--gray-light);
  cursor: pointer;
  transition: all 0.2s ease;
}

.round-option:hover {
  border-color: var(--primary-light);
  transform: translateY(-2px);
}

.round-option.active {
  background: var(--primary);
  color: white;
  border-color: var(--primary);
}

/* Toggle Switch */
.toggle-container {
  display: flex;
  align-items: center;
  gap: 16px;
}

.toggle-switch {
  width: 60px;
  height: 30px;
  border-radius: 15px;
  background: var(--gray-light);
  position: relative;
  cursor: pointer;
  transition: background 0.3s ease;
}

.toggle-switch.is-private {
  background: var(--primary);
}

.toggle-handle {
  position: absolute;
  top: 3px;
  left: 3px;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: white;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
  transition: transform 0.3s ease;
}

.toggle-switch.is-private .toggle-handle {
  transform: translateX(30px);
}

.toggle-labels {
  display: flex;
  gap: 12px;
  font-size: 14px;
}

.toggle-labels span {
  color: var(--gray);
  transition: color 0.2s ease;
}

.toggle-labels span.active {
  color: var(--primary);
  font-weight: 500;
}

/* Password Field */
.password-field {
  animation: slideDown 0.3s ease;
}

@keyframes slideDown {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Categories Section */
.categories-description {
  font-size: 14px;
  color: var(--gray);
  margin-bottom: 24px;
}

.categories-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  margin-bottom: 36px;
}

.category-card {
  background: white;
  border: 1px solid var(--gray-light);
  border-radius: 12px;
  padding: 16px 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  cursor: pointer;
  transition: all 0.2s ease;
  position: relative;
  overflow: hidden;
}

.category-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  border-color: var(--primary-light);
}

.category-icon {
  font-size: 28px;
  margin-bottom: 12px;
}

.category-name {
  font-size: 14px;
  font-weight: 500;
}

.category-check {
  position: absolute;
  top: 10px;
  right: 10px;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: var(--primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  opacity: 0;
  transform: scale(0);
  transition: all 0.2s ease;
}

.category-card.selected {
  border-color: var(--primary);
  background: var(--primary-lightest);
}

.category-card.selected .category-check {
  opacity: 1;
  transform: scale(1);
}

/* Action Buttons */
.form-actions {
  display: flex;
  justify-content: center;
  margin-top: auto;
}

.action-button {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 12px 32px;
  border: none;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.create-button {
  background: var(--primary);
  color: white;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.2);
}

.create-button:hover {
  background: var(--primary-dark);
  transform: translateY(-3px);
  box-shadow: 0 6px 16px rgba(79, 70, 229, 0.3);
}

.btn-icon {
  margin-right: 8px;
  font-size: 14px;
}

/* Responsive Design */
@media (max-width: 992px) {
  .create-room-form {
    flex-direction: column;
  }
  
  .categories-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}

@media (max-width: 768px) {
  .create-room-container {
    width: 95%;
  }
  
  .form-container {
    padding: 20px;
  }
  
  .categories-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 576px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
  }
  
  .back-button {
    align-self: flex-start;
  }
  
  .categories-grid {
    grid-template-columns: 1fr;
  }
  
  .rounds-selector {
    justify-content: space-between;
  }
  
  .round-option {
    flex: 1;
  }
}
</style>