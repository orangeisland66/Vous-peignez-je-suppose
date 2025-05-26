import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import store from './store' // ✅ 引入 Vuex store

import './styles/global.css'
import '@fortawesome/fontawesome-free/css/all.css'
import '@fortawesome/fontawesome-free/js/all.js'

const app = createApp(App)
app.use(router)
app.use(store) // ✅ 注入 store
app.mount('#app')
