<template>
  <div class="app">
    <nav class="navbar">
      <div class="container flex-between">
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
      <router-view />
    </main>

    <footer class="footer">
      <div class="container text-center">
        <p>&copy; 2025 你画我猜. All rights reserved.</p>
      </div>
    </footer>
  </div>
</template>

<script>
export default {
  name: 'App'
}
</script>

<style>
.app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.navbar {
  background-color: white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  padding: 15px 0;
}

.logo {
  font-size: 24px;
  font-weight: bold;
  color: #e60000;
  text-decoration: none;
}

.nav-links {
  display: flex;
  gap: 20px;
}

.nav-link {
  color: #333;
  text-decoration: none;
  font-size: 16px;
  transition: color 0.3s;
}

.nav-link:hover {
  color: #e60000;
}

.main-content {
  flex: 1;
  padding: 40px 0;
}

.footer {
  background-color: white;
  padding: 20px 0;
  margin-top: auto;
}
</style>
