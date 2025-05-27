<template>
  <div class="login-background">
    <div class="login-container">
      <div class="tab-switch">
        <button :class="['tab-btn', { active: !isRegister }]" @click="toLogin">登录 Login</button>
        <button :class="['tab-btn', { active: isRegister }]" @click="toRegister">注册 Sign Up</button>
      </div>

      <div v-if="!isRegister" class="form-container">
        <h2 class="form-title">欢迎来到 D&G</h2>

        <form @submit.prevent="handleLogin" class="login-form">
          <div class="form-group">
            <label class="form-label">用户名/邮箱：</label>
            <input class="form-input" v-model="username" required @input="validateUsername" placeholder="请输入用户名或邮箱" />
            <span v-if="usernameError" class="error-message">{{ usernameError }}</span>
          </div>

          <div class="form-group">
            <label class="form-label">密码：</label>
            <input class="form-input" type="password" v-model="password" required @input="validatePassword"
              placeholder="请输入密码" />
            <span v-if="passwordError" class="error-message">{{ passwordError }}</span>
          </div>

          <div class="form-options">
            <label class="remember-me">
              <input type="checkbox" v-model="remember" />
              <span>记住我</span>
            </label>
            <router-link to="#" class="forgot-link">忘记密码？</router-link>
          </div>

          <button type="submit" class="submit-btn">登录</button>
        </form>

        <div v-if="error" class="error-message login-error">{{ error }}</div>
      </div>
    </div>
  </div>
</template>

<script>
import apiService from '@/services/apiService';
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
        console.log('handleLogin method called.');

        this.validateUsername();
        this.validatePassword();
        if (this.usernameError || this.passwordError) {
          return;
        }
        const isEmail = this.username.includes('@');
        const credentials = {
        Password: this.password,
        [isEmail ? 'Email' : 'Username']: this.username
      };
        try { 
          const response = await apiService.login(credentials); 
          console.log('Login.vue - Full API Response:', response); // 打印完整的 Axios 响应
          console.log('Login.vue - API Response Data:', response.data); // 打印后端返回的数据部分
          //检查响应中是否包含userId
          if (response && response.data && response.data.userId !== undefined) {
          const userName = response.data.userName ; // 获取用户名
          const userId = response.data.userId; // 获取 userId
          console.log('Login.vue - Extracted userId:', userId, response.data.userName);//日志输出
          // 将 userId 存储到 Local Storage**
          // 这是 Lobby.vue 主要需要的信息
          localStorage.setItem('userId', userId.toString()); // 确保存储为字符串
          localStorage.setItem('userName', response.data.userName); // 存储用户名
          localStorage.removeItem('userInfo');
          // localStorage.setItem('token', token);
          // localStorage.setItem('userInfo', JSON.stringify(userInfo));
          this.$router.push('/lobby');
        } else {
        // 登录API调用成功，但响应数据格式不符合预期 (例如没有 userId)
          let errorMessage = '登录失败，服务器响应异常。';
          if (response && response.data && (response.data.message || response.data.Message)) {
              errorMessage = response.data.message || response.data.Message || errorMessage;
          }
          this.error = errorMessage;
          console.error('Login.vue - Login successful but userId missing in response data:', response.data);
      }
      } catch (error) {
          console.error('Login.vue - Login API call error:', error);
          if (error.response && error.response.status === 401) {
            // 后端返回 Unauthorized (用户名或密码错误)
            this.error = (error.response.data && (error.response.data.message || error.response.data.Message)) || '用户名或密码错误';
          } else if (error.response && error.response.data && (error.response.data.message || error.response.data.Message)) {
            // 其他后端错误，使用后端提供的消息
            this.error = error.response.data.message || error.response.data.Message;
          } else {
            // 网络错误或其他未知错误
            this.error = '登录失败，请检查网络或稍后重试。';
          }
      }
    }
  }
};
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
.login-background {
  /* background: linear-gradient(135deg, #7877FB 0%, #EEF2FF 100%); */
  min-height: 100vh;
  width: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  color: var(--dark);
  padding: 20px 0;
}

.login-container {
  width: 90%;
  max-width: 500px;
  background: white;
  border-radius: 24px;
  box-shadow: 0 10px 30px rgba(79, 70, 229, 0.1);
  overflow: hidden;
  padding: 32px;
}

/* Tab Switch Styles */
.tab-switch {
  display: flex;
  margin-bottom: 24px;
  background: var(--primary-lightest);
  border-radius: 12px;
  padding: 6px;
}

.tab-btn {
  flex: 1;
  padding: 12px 0;
  border: none;
  background: transparent;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 500;
  color: var(--gray);
  cursor: pointer;
  transition: all 0.3s ease;
}

.tab-btn.active {
  background: white;
  color: var(--primary);
  box-shadow: 0 2px 8px rgba(79, 70, 229, 0.15);
}

/* Form Styles */
.form-container {
  padding: 10px 0;
}

.form-title {
  font-size: 24px;
  font-weight: 600;
  color: var(--primary);
  text-align: center;
  margin-bottom: 32px;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-label {
  font-size: 14px;
  font-weight: 500;
  color: var(--dark);
}

.form-input {
  padding: 12px 16px;
  border: 1px solid var(--gray-light);
  border-radius: 8px;
  font-size: 16px;
  transition: all 0.2s ease;
}

.form-input:focus {
  border-color: var(--primary-light);
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
  outline: none;
}

.form-input::placeholder {
  color: var(--gray);
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 5px;
}

.remember-me {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: var(--gray);
  cursor: pointer;
}

.remember-me input {
  accent-color: var(--primary);
}

.forgot-link {
  font-size: 14px;
  color: var(--primary);
  text-decoration: none;
  transition: all 0.2s ease;
}

.forgot-link:hover {
  color: var(--primary-dark);
  text-decoration: underline;
}

.submit-btn {
  background: linear-gradient(135deg, var(--primary) 0%, var(--primary-light) 100%);
  color: black;
  border: none;
  border-radius: 12px;
  padding: 14px;
  font-size: 16px;
  font-weight: 600;
  margin-top: 10px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.submit-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px rgba(79, 70, 229, 0.25);
}

/* Social Login Styles */
.social-login {
  margin-top: 32px;
  text-align: center;
}

.social-title {
  font-size: 14px;
  color: var(--gray);
  margin-bottom: 16px;
  position: relative;
}

.social-title::before,
.social-title::after {
  content: "";
  position: absolute;
  top: 50%;
  width: 60px;
  height: 1px;
  background: var(--gray-light);
}

.social-title::before {
  left: 40px;
}

.social-title::after {
  right: 40px;
}

.social-buttons {
  display: flex;
  justify-content: center;
  gap: 16px;
}

.social-btn {
  width: 50px;
  height: 50px;
  border-radius: 12px;
  border: 1px solid var(--gray-light);
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
}

.social-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
  border-color: var(--primary-light);
}

.social-icon {
  font-size: 14px;
  color: var(--gray);
}

/* Error Message Styles */
.error-message {
  color: var(--danger);
  font-size: 14px;
  margin-top: 4px;
}

.login-error {
  text-align: center;
  margin-top: 16px;
  padding: 8px;
  background: rgba(239, 68, 68, 0.1);
  border-radius: 8px;
}

/* Responsive Adjustments */
@media (max-width: 576px) {
  .login-container {
    width: 95%;
    padding: 24px 16px;
  }

  .social-title::before,
  .social-title::after {
    width: 30px;
  }

  .social-title::before {
    left: 20px;
  }

  .social-title::after {
    right: 20px;
  }
}
</style>