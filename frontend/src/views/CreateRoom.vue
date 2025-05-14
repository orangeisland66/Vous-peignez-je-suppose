<template>
  <div class="page-outer">
    <div class="create-room-container">
      <!-- Title -->
      <header class="create-room-header">
        <h1>创建新房间</h1>
        <div class="title-underline"></div>
      </header>

      <!-- Form -->
      <form class="create-room-form" @submit.prevent="handleCreate">
        <!-- Room Name -->
        <div class="form-row">
          <label>房间名称：</label>
          <input v-model.trim="room.name" type="text" placeholder="输入名称" required />
        </div>

        <!-- Max Players & Rounds -->
        <div class="form-row double-row">
          <div class="form-group">
            <label>最大玩家数：</label>
            <select v-model.number="room.maxPlayers">
              <option v-for="n in [4,6,8]" :key="n" :value="n">{{ n }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>回合数：</label>
            <select v-model.number="room.rounds">
              <option v-for="n in [4,6,8]" :key="n" :value="n">{{ n }}</option>
            </select>
          </div>
        </div>

        <!-- Categories -->
        <div class="form-row">
          <label>词库分类：</label>
          <div class="checkbox-group">
            <label v-for="cat in categories" :key="cat.value" class="checkbox-option">
              <input type="checkbox" v-model="room.categories" :value="cat.value" />
              <span>{{ cat.label }}</span>
            </label>
          </div>
        </div>

        <!-- Privacy -->
        <div class="form-row">
          <label>隐私设置：</label>
          <div class="radio-group">
            <label class="radio-option">
              <input type="radio" v-model="room.privacy" value="public" />
              <span>公开房间</span>
            </label>
            <label class="radio-option">
              <input type="radio" v-model="room.privacy" value="private" />
              <span>私密房间</span>
            </label>
          </div>
        </div>
          
        <!-- Password (conditional) -->
        <div v-if="room.privacy === 'private'" class="form-row password-row">
          <label>房间密码：</label>
          <input v-model.trim="room.password" type="password" maxlength="12" placeholder="请输入密码" />
        </div>

        <!-- Actions -->
        <div class="form-actions">
          <button type="submit" class="btn primary">创建房间</button>
          <button type="button" class="btn cancel" @click="cancel">取消返回</button>
        </div>
      </form>
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
        { label: '动物', value: 'animal' },
        { label: '食物', value: 'food' },
        { label: '日常', value: 'daily' },
        { label: '电影', value: 'movie' },
        { label: '地名', value: 'place' },
        { label: '自定义', value: 'custom' }
      ]
    }
  },
  methods: {
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
.page-outer {
  width: 100vw;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f8f9fa;
}

.create-room-container {
  width: 75%;
  max-width: 1400px;
  aspect-ratio: 16 / 9;
  background: white;
  border-radius: 16px;
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.06);
  padding: 28px 36px;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  border: 1px solid #e0e6e6;
  overflow: hidden;
}

.create-room-header {
  text-align: center;
  margin-bottom: 30px;
}

.create-room-header h1 {
  font-size: 28px;
  color: #333;
  font-weight: 600;
  margin-bottom: 8px;
}

.title-underline {
  height: 3px;
  width: 60px;
  background-color: #e60000;
  margin: 0 auto;
  border-radius: 2px;
}

.create-room-form {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.form-row {
  display: flex;
  align-items: center;
  margin-bottom: 24px;
}

.double-row {
  display: flex;
  justify-content: space-between;
  gap: 30px;
}

.form-row label {
  width: 120px;
  font-size: 16px;
  color: #333;
}

.form-group {
  display: flex;
  align-items: center;
  flex: 1;
}

.form-group label {
  width: auto;
  margin-right: 10px;
}

.form-row input,
.form-row select {
  flex: 1;
  height: 40px;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  padding: 0 12px;
  font-size: 15px;
  background-color: #f9f9f9;
}

.form-row input:focus,
.form-row select:focus {
  outline: none;
  border-color: #3a7563;
  box-shadow: 0 0 0 2px rgba(58, 117, 99, 0.1);
}

.checkbox-group,
.radio-group {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  flex: 1;
}

.checkbox-option,
.radio-option {
  display: flex;
  align-items: center;
  padding: 8px 16px;
  background-color: #f5f5f5;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.2s;
}

.checkbox-option:hover,
.radio-option:hover {
  background-color: #e6f0ec;
}

.checkbox-option input,
.radio-option input {
  margin-right: 6px;
  flex: none;
}

.password-row {
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}

.form-actions {
  display: flex;
  justify-content: center;
  gap: 20px;
  margin-top: auto;
  padding-top: 30px;
}

.btn {
  min-width: 120px;
  height: 40px;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.2s;
}

.primary {
  background-color: #3a7563;
  color: white;
}

.cancel {
  background-color: #f0f0f0;
  color: #333;
}

.primary:hover {
  background-color: #2c5a4c;
}

.cancel:hover {
  background-color: #e0e0e0;
}

@media (max-width: 1200px) {
  .create-room-container {
    width: 85%;
    padding: 24px 30px;
  }
}

@media (max-width: 900px) {
  .create-room-container {
    width: 90%;
    aspect-ratio: auto;
    min-height: 600px;
    padding: 20px;
  }
  
  .form-row {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .form-row label {
    margin-bottom: 8px;
  }
  
  .double-row {
    flex-direction: column;
    gap: 24px;
  }
  
  .form-group {
    width: 100%;
  }
}

@media (max-width: 600px) {
  .create-room-container {
    width: 95%;
    padding: 16px;
  }
  
  .form-actions {
    flex-direction: column;
    gap: 12px;
  }
  
  .btn {
    width: 100%;
  }
}
</style>