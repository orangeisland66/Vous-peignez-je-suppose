/* src/views/Register.vue */
<template>
  <div class="page-outer">
    <div class="auth-container">
      <div class="tab-switch">
        <div :class="['tab', { active: isRegister }]" @click="toRegister">注册 Sign Up</div>
        <div :class="['tab', { active: !isRegister }]" @click="toLogin">登录 Login</div>
      </div>
      <div class="form-wrap">
        <h2>创建新账号</h2>
        <form @submit.prevent="handleRegister">
          <div class="form-row"><label>昵称：</label><input v-model="username" required @input="validateUsername" /></div>
          <span v-if="usernameError" class="error-msg">{{ usernameError }}</span>
          <div class="form-row"><label>邮箱：</label><input type="email" v-model="email" required /></div>
          <div class="form-row"><label>密码：</label><input type="password" v-model="password" required @input="validatePassword" /></div>
          <span v-if="passwordError" class="error-msg">{{ passwordError }}</span>
          <div class="form-row"><label>确认密码：</label><input type="password" v-model="confirmPassword" required /></div>
          <span v-if="confirmPasswordError" class="error-msg">{{ confirmPasswordError }}</span>
          <div class="form-actions">
            <button type="submit" class="btn primary">注册并登录</button>
            <button type="button" class="btn link" @click="toLogin">已有账号？去登录</button>
          </div>
        </form>
        <div v-if="error" class="error-msg">{{ error }}</div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return { username: '', email: '', password: '', confirmPassword: '', error: '',usernameError: '',passwordError: '',confirmPasswordError: '' };
  },
  computed: { isRegister() { return this.$route.path === '/register' } },
  methods: {
    validateUsername() {
      if (this.username.length < 3 || this.username.length > 20) {
        this.usernameError = '用户名长度应在3到20个字符之间';
      } else {
        this.usernameError = '';
      }
    },
    validatePassword() {
      if (this.password.length < 6 || this.password.length > 20) {
        this.passwordError = '密码长度应在6到20个字符之间';
      } else {
        this.passwordError = '';
      }
    },
    validateConfirmPassword() {
      if (this.password !== this.confirmPassword) {
        this.confirmPasswordError = '两次输入的密码不一致';
      } else {
        this.confirmPasswordError = '';
      }
    },
    toLogin() { this.$router.push('/login') },
    toRegister() { this.$router.push('/register') },
    handleRegister() {
      this.validateUsername();
      this.validatePassword();
      this.validateConfirmPassword();
      if (this.usernameError || this.passwordError || this.confirmPasswordError) {
        return;
      }
      if (!this.username || !this.email || !this.password || !this.confirmPassword) {
        this.error = '请填写所有信息'; return
      }
      if (this.password !== this.confirmPassword) {
        this.error = '两次输入的密码不一致'; return
      }
      // TODO: 注册逻辑
      // this.$router.push('/lobby')
      const userData = {
        Username: this.username,
        Email: this.email,
        PasswordHash: this.password
      };

      try {
        await this.register(userData);
        this.$router.push('/lobby');
      } catch (error) {
        this.error = '注册失败，请稍后重试';
      }
    }
  }
}
</script>

<style scoped>
/* 同 Login.vue 样式，可抽成公共 */
.page-outer {display:flex; align-items:center; justify-content:center; width:100vw; height:100vh; background:#f9f9f9;}
.auth-container {width:80vw; max-width:800px; aspect-ratio:16/9; background:#fff; border-radius:12px; box-shadow:0 4px 12px rgba(0,0,0,0.1); padding:32px; box-sizing:border-box; display:flex; flex-direction:column;}
.tab-switch {display:flex; border-bottom:2px solid #eee; margin-bottom:24px;}
.tab {flex:1; text-align:center; padding:12px 0; cursor:pointer; font-size:1.25rem; color:#666;}
.tab.active {color:#e60000; border-bottom:3px solid #e60000; font-weight:bold;}
h2 {text-align:center; margin-bottom:24px; color:#333;}
.form-wrap {flex:1; display:flex; flex-direction:column; justify-content:space-between;}
.form-row {display:flex; align-items:center; margin-bottom:16px;}
.form-row label {width:100px; color:#333; font-size:1rem;}
.form-row input {flex:1; padding:8px; border:1px solid #ccc; border-radius:4px; font-size:1rem;}
.form-actions {display:flex; gap:16px; margin-top:16px;}
.btn {padding:8px 24px; border:none; border-radius:4px; cursor:pointer; font-size:1rem;}
.btn.primary {background:#e60000; color:#fff;}
.btn.link {background:transparent; color:#e60000; text-decoration:underline;}
.error-msg {color:#e60000; text-align:center; margin-top:12px;}
</style>
