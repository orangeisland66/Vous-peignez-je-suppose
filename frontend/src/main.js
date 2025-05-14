import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './styles/global.css'
//import './assets/styles/theme.css'
const app = createApp(App)
app.use(router)
app.mount('#app')