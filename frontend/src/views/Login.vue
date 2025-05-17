<!-- src/views/Login.vue -->
<template>
  <div class="page-outer">
    <div class="auth-container">
      <div class="tab-switch">
        <div :class="['tab', { active: !isRegister }]" @click="toLogin">登录 Login</div>
        <div :class="['tab', { active: isRegister }]" @click="toRegister">注册 Sign Up</div>
      </div>
      <div v-if="!isRegister" class="form-wrap">
        <h2>欢迎来到 D&G</h2>
        <form @submit.prevent="handleLogin">
          <div class="form-row">
            <label>用户名/邮箱：</label>
             <input v-model="username" required @input="validateUsername" />
             <span v-if="usernameError" class="error-msg">{{ usernameError }}</span>
          </div>
          <div class="form-row">
            <label>密码：</label>
             <input type="password" v-model="password" required @input="validatePassword" />
             <span v-if="passwordError" class="error-msg">{{ passwordError }}</span>
          </div>
          <div class="form-row">
            <label><input type="checkbox" v-model="remember" />记住我</label>
          </div>
          <div class="form-actions">
            <button type="submit" class="btn primary">登录</button>
            <router-link to="/register" class="btn link">注册</router-link>
          </div>
        </form>
        <div class="forgot"><router-link to="#">忘记密码？立刻重置</router-link></div>
        <div class="social-login">
          第三方登录：
          <button>微信</button>
          <button>QQ</button>
          <button>微博</button>
        </div>
        <div v-if="error" class="error-msg">{{ error }}</div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      username: '',
      password: '',
      remember: false,
      error: '',
      usernameError: '',
      passwordError: ''
    };
  },
  computed: {
    isRegister() {
      return this.$route.path === '/register';
    }
  },
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
    toLogin() {
      if (this.$route.path !== '/login') this.$router.push('/login');
    },
    toRegister() {
      if (this.$route.path !== '/register') this.$router.push('/register');
    },
    async handleLogin() {
      this.validateUsername();
      this.validatePassword();
      if (this.usernameError || this.passwordError) {
        return;
      }
      if (!this.username || !this.password) {
        this.error = '请输入用户名和密码';
        return;
      }

      try {
                const response = await this.$store.dispatch('user/login', {
                    username: this.username,
                    password: this.password
                });
                // 登录成功，跳转到大厅
                this.$router.push('/lobby');
            } catch (error) {
                if (error.response && error.response.status === 401) {
                    this.error = '用户名或密码错误';
                } else {
                    this.error = '登录失败，请稍后重试';
                }
            }
          }
  }
};
</script>

<style scoped>
.page-outer {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100vw;
  height: 100vh;
  background: #f9f9f9;
}

.auth-container {
  width: 80vw;
  max-width: 800px;
  aspect-ratio: 16/9;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  padding: 32px;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
}

.tab-switch {
  display: flex;
  border-bottom: 2px solid #eee;
  margin-bottom: 24px;
}

.tab {
  flex: 1;
  text-align: center;
  padding: 12px 0;
  cursor: pointer;
  font-size: 1.25rem;
  color: #666;
}

.tab.active {
  color: #e60000;
  border-bottom: 3px solid #e60000;
  font-weight: bold;
}

h2 {
  text-align: center;
  margin-bottom: 24px;
  color: #333;
}

.form-wrap {
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

.form-row label {
  width: 120px;
  color: #333;
  font-size: 1rem;
}

.form-row input[type="text"],
.form-row input[type="password"] {
  flex: 1;
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 1rem;
}

.form-actions {
  display: flex;
  gap: 16px;
  margin-top: 16px;
}

.btn {
  padding: 8px 24px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
}

.btn.primary {
  background: #e60000;
  color: #fff;
}

.btn.link {
  background: transparent;
  color: #e60000;
  text-decoration: underline;
}

.forgot {
  text-align: right;
  margin-top: 8px;
}

.social-login {
  text-align: center;
  margin-top: 16px;
}

.social-login button {
  margin: 0 8px;
  padding: 6px 12px;
  border: 1px solid #e60000;
  background: transparent;
  border-radius: 4px;
  cursor: pointer;
}

.error-msg {
  color: #e60000;
  text-align: center;
  margin-top: 12px;
}
</style>
