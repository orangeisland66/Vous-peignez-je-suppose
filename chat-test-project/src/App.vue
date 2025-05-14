<template>
  <div id="app">
    <Chat :isDrawer="false" :currentWord="''" @send-message="handleSendMessage" />
  </div>
</template>

<script setup>
import { ref } from 'vue';
import Chat from './components/Chat.vue';

const handleSendMessage = (message) => {
  console.log('发送的消息:', message);
  // 这里可以添加与后端通信的逻辑
};
</script>

<style scoped>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>