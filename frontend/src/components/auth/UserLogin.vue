<template>
    <div class="login-container">
        <div class="login-card">
            <h2>登录</h2>
            <form @submit.prevent="handleLogin" class="login-form">
                <div class="form-group">
                    <label for="username">用户名</label>
                    <input
                        id="username"
                        v-model="loginForm.username"
                        type="text"
                        required
                        placeholder="请输入用户名"
                    />
                </div>
                <div class="form-group">
                    <label for="password">密码</label>
                    <input
                        id="password"
                        v-model="loginForm.password"
                        type="password"
                        required
                        placeholder="请输入密码"
                    />
                </div>
                <div class="error-message" v-if="error">{{ error }}</div>
                <button type="submit" :disabled="loading">
                    {{ loading ? '登录中...' : '登录' }}
                </button>
                <div class="register-link">
                    还没有账号？
                    <router-link to="/register">立即注册</router-link>
                </div>
            </form>
        </div>
    </div>
</template>

<script>
import { defineComponent, ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useStore } from 'vuex'

export default defineComponent({
    name: 'UserLogin',
    setup() {
        const router = useRouter()
        const store = useStore()
        const loading = ref(false)
        const error = ref('')

        const loginForm = reactive({
            username: '',
            password: ''
        })

        const handleLogin = async () => {
            try {
                loading.value = true
                error.value = ''
                
                // 调用 Vuex action 进行登录
                await store.dispatch('user/login', {
                    username: loginForm.username,
                    password: loginForm.password
                })
                
                // 登录成功后跳转到首页
                router.push('/')
            } catch (err) {
                error.value = err.message || '登录失败，请重试'
            } finally {
                loading.value = false
            }
        }

        return {
            loginForm,
            loading,
            error,
            handleLogin
        }
    }
})
</script>

<style scoped>
.login-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
    background-color: #f5f5f5;
}

.login-card {
    background: white;
    padding: 2rem;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    width: 100%;
    max-width: 400px;
}

.login-form {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
}

h2 {
    text-align: center;
    color: #333;
    margin-bottom: 1.5rem;
}

.form-group {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
}

label {
    font-size: 0.9rem;
    color: #666;
}

input {
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 1rem;
}

input:focus {
    outline: none;
    border-color: #4a90e2;
}

button {
    padding: 0.75rem;
    background-color: #4a90e2;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 1rem;
    transition: background-color 0.2s;
}

button:hover {
    background-color: #357abd;
}

button:disabled {
    background-color: #ccc;
    cursor: not-allowed;
}

.error-message {
    color: #dc3545;
    font-size: 0.9rem;
    text-align: center;
}

.register-link {
    text-align: center;
    font-size: 0.9rem;
}

.register-link a {
    color: #4a90e2;
    text-decoration: none;
}

.register-link a:hover {
    text-decoration: underline;
}
</style>