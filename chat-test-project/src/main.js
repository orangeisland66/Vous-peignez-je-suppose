import { createApp } from 'vue';
import App from './App.vue';
import store from './store'; // 引入 store
import router from './router'; // 引入路由

const app = createApp(App);
app.use(store); // 挂载 store
app.use(router); // 挂载路由
app.mount('#app');