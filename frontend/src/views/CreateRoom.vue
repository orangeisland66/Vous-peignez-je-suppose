<template>
  <div class="page-outer">
    <div class="create-room-container">
      <!-- Title -->
      <header class="create-room-header">
        <h1>创建新房间</h1>
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
          <div class="sub-row">
            <label>最大玩家数：</label>
            <select v-model.number="room.maxPlayers">
              <option v-for="n in [4,6,8]" :key="n" :value="n">{{ n }}</option>
            </select>
          </div>
          <div class="sub-row">
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
            <label v-for="cat in categories" :key="cat.value">
              <input type="checkbox" v-model="room.categories" :value="cat.value" /> {{ cat.label }}
            </label>
          </div>
        </div>

        <!-- Privacy -->
        <div class="form-row privacy-row">
          <label>隐私设置：</label>
          <div class="radio-group">
            <label><input type="radio" v-model="room.privacy" value="public" /> 公开房间</label>
            <label><input type="radio" v-model="room.privacy" value="private" /> 私密房间</label>
          </div>
          <div v-if="room.privacy === 'private'" class="password-row">
            <label>房间密码：</label>
            <input v-model.trim="room.password" type="password" maxlength="12" placeholder="____" />
          </div>
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
      if (!this.room.name) return;
      if (this.room.privacy === 'private' && !this.room.password) return;
      // TODO: 调用后端接口创建房间并获取房间ID
      // const res = await api.createRoom(this.room)
      // this.$router.push(`/room/${res.id}/waiting`)
      // 示例跳转：
      this.$router.push('/room/123/waiting')
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
  background: #f5f5f5;
}
.create-room-container {
  width: 80vw;
  max-width: 900px;
  aspect-ratio: 16 / 9;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  padding: 32px;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
}
.create-room-header h1 {
  text-align: center;
  font-size: 2rem;
  color: #333;
  margin-bottom: 24px;
}
.create-room-form {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}
.form-row {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
}
.double-row {
  justify-content: space-between;
}
.sub-row {
  display: flex;
  align-items: center;
  gap: 8px;
}
.form-row label {
  width: 120px;
  font-size: 1.1rem;
  color: #333;
}
.form-row input,
.form-row select {
  flex: 1;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 1rem;
}
.checkbox-group,
.radio-group {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
}
.password-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-left: 120px;
}
.form-actions {
  display: flex;
  justify-content: center;
  gap: 24px;
  margin-top: 24px;
}
.btn {
  padding: 10px 24px;
  font-size: 1.1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
.primary {
  background: #e60000;
  color: #fff;
}
.cancel {
  background: #f0f0f0;
  color: #333;
}
.primary:hover {
  background: #b80000;
}
.cancel:hover {
  background: #ddd;
}
@media (max-width: 900px) {
  .create-room-container { padding: 16px; }
  .form-row label { width: 100px; font-size: 1rem; }
  .form-row input, .form-row select { font-size: 0.9rem; }
  .primary, .cancel { font-size: 1rem; padding: 8px 16px; }
}
</style>
