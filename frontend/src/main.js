import './assets/main.css'
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './styles/global.css'
import store from './store'
//import './assets/styles/theme.css'

const app = createApp(App)
app.use(store)
app.use(router)
app.mount('#app')