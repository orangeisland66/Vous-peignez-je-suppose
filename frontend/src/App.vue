<template>
  <div class="app">
    <nav class="navbar">
      <div class="container">
        <router-link to="/lobby" class="logo">你画我猜</router-link>
        <div class="nav-links">
          <router-link to="/lobby" class="nav-link">大厅</router-link>
          <router-link to="/room/create" class="nav-link">创建房间</router-link>
          <router-link to="/profile" class="nav-link">个人中心</router-link>
          <router-link to="/settings" class="nav-link">设置</router-link>
        </div>
      </div>
    </nav>

    <main class="main-content">
      <div class="container main-container">
        <router-view />
      </div>
    </main>

    <!-- <footer class="footer">
      <div class="container">
        <p>&copy; 2025 你画我猜. All rights reserved.</p>
      </div>
    </footer> -->
  </div>
</template>

<script>
export default {
  name: 'App'
}
</script>

<style>
:root {
  --dai-green: #426666;
  --dai-green-light: #587878;
  --dai-green-lighter: #e8f0f0;
  --text-dark: #2c3e50;
  --container-width: 100%;
  --container-padding: 0 20px;
  --max-content-width: 1200px;
}

* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
  -webkit-overflow-scrolling: touch;
}

body, html, #app {
  font-family: "Microsoft YaHei", "PingFang SC", "Helvetica Neue", Arial, sans-serif;
  height: 100%;
  margin: 0;
  padding: 0;
  overflow: hidden;
}

.app {
  height: 100vh;
  width: 100vw;
  display: flex;
  flex-direction: column;
  background-color: var(--dai-green-pale, #f8f8f8);
  overflow: hidden;
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}

.container {
  width: var(--container-width);
  max-width: var(--max-content-width);
  padding: var(--container-padding);
  margin: 0 auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-sizing: border-box;
}

.navbar {
  background-color: white;
  box-shadow: 0 4px 12px rgba(66, 102, 102, 0.1);
  position: sticky;
  top: 0;
  z-index: 1000;
  border-bottom: 1px solid var(--dai-green-lighter);
  width: 100%;
  height: 80px;
}

.navbar .container {
  height: 100%;
}

.logo {
  font-size: 24px;
  font-weight: bold;
  color: #e60000;
  text-decoration: none;
  padding: 0 10px;
}

.nav-links {
  display: flex;
  gap: 20px;
}

.nav-link {
  color: #333;
  text-decoration: none;
  font-size: 16px;
  transition: color 0.3s, transform 0.2s;
  padding: 8px 12px;
  border-radius: 4px;
}

.nav-link:hover {
  color: #e60000;
  background-color: rgba(230, 0, 0, 0.05);
  transform: translateY(-2px);
}

.main-content {
  flex: 1;
  display: flex;
  width: 100%;
  overflow: auto;
}

.main-container {
  flex-direction: column;
  width: 100%;
  height: 100%;
  justify-content: flex-start;
  overflow-y: auto;
  overflow-x: hidden;
}

.footer {
  background-color: #f9f9f9;
  color: #777;
  padding: 15px 0;
  font-size: 14px;
  border-top: 1px solid #e0e0e0;
  width: 100%;
}

.footer .container {
  justify-content: center;
}
</style>