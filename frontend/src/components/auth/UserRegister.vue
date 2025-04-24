<template>
    <div class="register-container">
        <h2>用户注册</h2>
        <form @submit.prevent="handleRegister" class="register-form">
            <div class="form-group">
                <label for="username">用户名</label>
                <input 
                    type="text" 
                    id="username" 
                    v-model="formData.username"
                    required
                    :class="{ 'error': errors.username }"
                >
                <span class="error-message" v-if="errors.username">{{ errors.username }}</span>
            </div>

            <div class="form-group">
                <label for="email">邮箱</label>
                <input 
                    type="email" 
                    id="email" 
                    v-model="formData.email"
                    required
                    :class="{ 'error': errors.email }"
                >
                <span class="error-message" v-if="errors.email">{{ errors.email }}</span>
            </div>

            <div class="form-group">
                <label for="password">密码</label>
                <input 
                    type="password" 
                    id="password" 
                    v-model="formData.password"
                    required
                    :class="{ 'error': errors.password }"
                >
                <span class="error-message" v-if="errors.password">{{ errors.password }}</span>
            </div>

            <div class="form-group">
                <label for="confirmPassword">确认密码</label>
                <input 
                    type="password" 
                    id="confirmPassword" 
                    v-model="formData.confirmPassword"
                    required
                    :class="{ 'error': errors.confirmPassword }"
                >
                <span class="error-message" v-if="errors.confirmPassword">{{ errors.confirmPassword }}</span>
            </div>

            <button type="submit" :disabled="isLoading">
                {{ isLoading ? '注册中...' : '注册' }}
            </button>

            <p class="login-link">
                已有账号？ <router-link to="/login">立即登录</router-link>
            </p>
        </form>
    </div>
</template>

<script>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useStore } from 'vuex'

export default {
    name: 'UserRegister',
    setup() {
        const router = useRouter()
        const store = useStore()
        const isLoading = ref(false)

        const formData = reactive({
            username: '',
            email: '',
            password: '',
            confirmPassword: ''
        })

        const errors = reactive({
            username: '',
            email: '',
            password: '',
            confirmPassword: ''
        })

        const validateForm = () => {
            let isValid = true
            // Reset errors
            Object.keys(errors).forEach(key => errors[key] = '')

            if (!formData.username || formData.username.length < 3) {
                errors.username = '用户名至少需要3个字符'
                isValid = false
            }

            if (!formData.email || !/^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$/.test(formData.email)) {
                errors.email = '请输入有效的邮箱地址'
                isValid = false
            }

            if (!formData.password || formData.password.length < 6) {
                errors.password = '密码至少需要6个字符'
                isValid = false
            }

            if (formData.password !== formData.confirmPassword) {
                errors.confirmPassword = '两次输入的密码不一致'
                isValid = false
            }

            return isValid
        }

        const handleRegister = async () => {
            if (!validateForm()) return

            isLoading.value = true
            try {
                await store.dispatch('user/register', {
                    username: formData.username,
                    email: formData.email,
                    password: formData.password
                })
                router.push('/login')
            } catch (error) {
                errors.username = error.message
            } finally {
                isLoading.value = false
            }
        }

        return {
            formData,
            errors,
            isLoading,
            handleRegister
        }
    }
}
</script>

<style scoped>
.register-container {
    max-width: 400px;
    margin: 2rem auto;
    padding: 2rem;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.register-form {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.form-group {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
}

label {
    font-weight: bold;
}

input {
    padding: 0.5rem;
    border: 1px solid #ddd;
    border-radius: 4px;
}

input.error {
    border-color: #ff4444;
}

.error-message {
    color: #ff4444;
    font-size: 0.875rem;
}

button {
    padding: 0.75rem;
    background-color: #4CAF50;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 1rem;
}

button:disabled {
    background-color: #cccccc;
    cursor: not-allowed;
}

.login-link {
    text-align: center;
    margin-top: 1rem;
}

.login-link a {
    color: #4CAF50;
    text-decoration: none;
}

.login-link a:hover {
    text-decoration: underline;
}
</style>