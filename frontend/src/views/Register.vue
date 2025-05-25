<template>
  <div class="register-background">
    <div class="register-container">
      <div class="tab-switch">
        <button :class="['tab-btn', { active: !isRegister }]" @click="toLogin">登录 Login</button>
        <button :class="['tab-btn', { active: isRegister }]" @click="toRegister">注册 Sign Up</button>
      </div>

      <div class="form-container">
        <h2 class="form-title">创建新账号</h2>

        <form @submit.prevent="handleRegister" class="register-form">
          <div class="form-group">
            <label class="form-label">昵称：</label>
            <input class="form-input" v-model="username" required @input="validateUsername"
              placeholder="请输入3-20个字符的昵称" />
            <span v-if="usernameError" class="error-message">{{ usernameError }}</span>
          </div>

          <div class="form-group">
            <label class="form-label">邮箱：</label>
            <input class="form-input" type="email" v-model="email" required placeholder="请输入有效的邮箱地址" />
          </div>

          <div class="form-group">
            <label class="form-label">密码：</label>
            <input class="form-input" type="password" v-model="password" required @input="validatePassword"
              placeholder="请输入6-20个字符的密码" />
            <span v-if="passwordError" class="error-message">{{ passwordError }}</span>
          </div>

          <div class="form-group">
            <label class="form-label">确认密码：</label>
            <input class="form-input" type="password" v-model="confirmPassword" required
              @input="validateConfirmPassword" placeholder="请再次输入密码" />
            <span v-if="confirmPasswordError" class="error-message">{{ confirmPasswordError }}</span>
          </div>

          <button type="submit" class="submit-btn">注册</button>
          <div class="login-redirect">
            <span>已有账号？</span>
            <button type="button" class="link-btn" @click="toLogin">去登录</button>
          </div>
        </form>

        <div v-if="error" class="error-message register-error">{{ error }}</div>
      </div>
    </div>
  </div>
</template>

<script>
// 导入 apiService
import apiService from '@/services/apiService';
export default {
  data() {
    return {
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
      error: '',
      usernameError: '',
      passwordError: '',
      confirmPasswordError: ''
    };
  },
  computed: {
    isRegister() {
      return this.$route.path === '/register'
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
    validateConfirmPassword() {
      if (this.password !== this.confirmPassword) {
        this.confirmPasswordError = '两次输入的密码不一致';
      } else {
        this.confirmPasswordError = '';
      }
    },
    toLogin() {
      this.$router.push('/login')
    },
    toRegister() {
      this.$router.push('/register')
    },
    async handleRegister() { // 标记为 async
      console.log('handleRegister method called.');

      // 清除之前的错误信息
      this.error = '';
      this.usernameError = '';
      this.passwordError = '';
      this.confirmPasswordError = '';

      // 基础前端校验
      this.validateUsername();
      this.validatePassword();
      this.validateConfirmPassword();

      if (this.usernameError || this.passwordError || this.confirmPasswordError) {
        this.error = '请检查输入信息'; // 如果有前端验证错误，显示提示
        return;
      }

      if (!this.username || !this.email || !this.password || !this.confirmPassword) {
        this.error = '请填写所有信息';
        return;
      }

      if (this.password !== this.confirmPassword) {
        this.error = '两次输入的密码不一致';
        return;
      }

      try {
        // **准备发送给后端的数据**
        // 创建一个对象，包含 username, email, 和 passwordHash (值为明文密码)
        // 这里的属性名需要与后端 User 实体中希望接收的属性名一致
        const registrationData = {
          username: this.username,
          email: this.email,
          passwordHash: this.password, // **重要：字段名是 passwordHash，但值是明文密码**
          // 确认密码不需要发送给后端
          // 注意：不要在这里发送 Id, CreatedAt, Status 等后端管理的属性
          Role: "User", // 根据业务逻辑设置默认角色
          userRole: "Player", // 根据业务逻辑设置默认用户角色
          AvatarUrl: "", // 根据业务逻辑设置默认头像 URL (例如空字符串或默认图片链接)
        };

        console.log('Sending registration data to API:', registrationData);

        // **调用后端注册接口**
        // apiService.register 会返回后端成功响应的数据，或者在失败时抛出封装好的 Error
        const result = await apiService.register(registrationData);

        console.log('Registration successful:', result);

        // **处理后端成功响应**
        // 假设后端成功时返回 { message: "注册成功", ... }
        this.$toast?.success(result.message || '注册成功，请登录') || alert(result.message || '注册成功，请登录');

        // 注册成功后跳转到登录页面
        this.$router.push('/login');

      } catch (error) {
        // **处理注册失败的情况 (包括后端返回的错误和网络错误)**
        console.error('注册失败:', error);
        // apiService 中的 register 函数已经将后端错误或网络错误封装成 Error 对象并抛出
        // 直接使用 error.message 来获取错误信息
        this.error = error.message || '注册失败，未知错误'; // 在页面上显示错误信息
        this.$toast?.error(this.error); // 如果有 toast 组件也显示
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
.register-background {
  background: linear-gradient(135deg, #F9FAFB 0%, #EEF2FF 100%);
  min-height: 100vh;
  width: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
  font-family: 'Segoe UI', 'PingFang SC', 'Microsoft YaHei', sans-serif;
  color: var(--dark);
  padding: 20px 0;
}

.register-container {
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

.register-form {
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
  border-radius: 12px;
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

.login-redirect {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
  margin-top: 16px;
  color: var(--gray);
  font-size: 14px;
}

.link-btn {
  background: none;
  border: none;
  color: var(--primary);
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  padding: 0;
}

.link-btn:hover {
  text-decoration: underline;
  color: var(--primary-dark);
}

/* Error Message Styles */
.error-message {
  color: var(--danger);
  font-size: 14px;
  margin-top: 4px;
}

.register-error {
  text-align: center;
  margin-top: 16px;
  padding: 8px;
  background: rgba(239, 68, 68, 0.1);
  border-radius: 8px;
}

/* Responsive Adjustments */
@media (max-width: 576px) {
  .register-container {
    width: 95%;
    padding: 24px 16px;
  }
}
</style>